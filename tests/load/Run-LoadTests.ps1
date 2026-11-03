[CmdletBinding()]
param(
    [ValidateSet('smoke', 'load', 'stress', 'spike', 'soak')]
    [string]$Profile = 'smoke',
    [ValidateSet('all', 'reads', 'writes', 'standalone')]
    [string]$Suite = 'all',
    [ValidateRange(0, 1000000)]
    [int]$MinEndpointSamples = 0,
    [switch]$Build,
    [switch]$StopAfterRun
)

$ErrorActionPreference = 'Stop'
$workspace = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$Profile = $Profile.ToLowerInvariant()
$Suite = $Suite.ToLowerInvariant()
$runId = '{0}-{1}-{2}' -f $Profile, (Get-Date).ToUniversalTime().ToString('yyyyMMdd-HHmmss'), ([Guid]::NewGuid().ToString('N').Substring(0, 8))
$runStartedAt = [DateTime]::UtcNow.ToString('O')
$resultsPath = Join-Path $PSScriptRoot "results/$runId"
New-Item -ItemType Directory -Path $resultsPath -Force | Out-Null
$composeArgs = @('compose', '-p', 'vault-load', '-f', (Join-Path $workspace 'docker-compose.yml'), '-f', (Join-Path $PSScriptRoot 'compose.load.yml'))
$environmentNames = @('LOAD_CLIENT_SECRET','LOAD_TEST_PASSWORD','LOAD_PROFILE','LOAD_SUITE','LOAD_RUN_ID','LOAD_RESULTS_DIR','LOAD_MIN_ENDPOINT_SAMPLES','LOAD_RUNNER_UID','LOAD_RUNNER_GID','COMPOSE_PARALLEL_LIMIT')
$saved = @{}
foreach ($name in $environmentNames) { $saved[$name] = [Environment]::GetEnvironmentVariable($name, 'Process') }

function New-RandomSecret {
    $bytes = New-Object byte[] 32
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    return [Convert]::ToBase64String($bytes)
}
function Invoke-DockerChecked([string[]]$Arguments) {
    & docker @Arguments
    if ($LASTEXITCODE -ne 0) { throw "Docker command failed (exit $LASTEXITCODE): $($Arguments[0])" }
}
function Save-Diagnostics {
    $previousPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
    # No docker inspect/config dumps: those contain credentials.
    & docker @composeArgs ps --format json | Out-File (Join-Path $resultsPath 'containers.json') -Encoding utf8
    foreach ($service in @('identity-api','customer-api','ledger-api','wallet-api','transaction-api','reporting-api')) {
        $lines = & docker @composeArgs logs --no-color --since $runStartedAt $service 2>&1
        $text = ($lines | Out-String).Replace($env:LOAD_CLIENT_SECRET, '[REDACTED]').Replace($env:LOAD_TEST_PASSWORD, '[REDACTED]')
        [IO.File]::WriteAllText((Join-Path $resultsPath "$service.log"), $text)
    }
    & docker @composeArgs exec -T kafka kafka-consumer-groups --bootstrap-server kafka:9092 --all-groups --describe 2>&1 |
        Out-File (Join-Path $resultsPath 'kafka-consumer-lag.txt') -Encoding utf8
    & docker @composeArgs exec -T rabbitmq rabbitmqctl list_queues name messages_ready messages_unacknowledged consumers 2>&1 |
        Out-File (Join-Path $resultsPath 'rabbitmq-queues.txt') -Encoding utf8
    } finally { $ErrorActionPreference = $previousPreference }
}

$sampler = $null
$runLock = [System.Threading.Mutex]::new($false, 'VaultLoadTests')
$lockAcquired = $false
$exitCode = 1
try {
    try { $lockAcquired = $runLock.WaitOne(0) }
    catch [System.Threading.AbandonedMutexException] { $lockAcquired = $true }
    if (-not $lockAcquired) { throw 'Another Vault load-test runner is already using the isolated environment.' }
    $env:LOAD_CLIENT_SECRET = New-RandomSecret
    $env:LOAD_TEST_PASSWORD = 'Lt!9-' + (New-RandomSecret)
    $env:LOAD_PROFILE = $Profile
    $env:LOAD_SUITE = $Suite
    $env:LOAD_RUN_ID = $runId
    $env:LOAD_RESULTS_DIR = $resultsPath.Replace('\', '/')
    $env:LOAD_MIN_ENDPOINT_SAMPLES = if ($MinEndpointSamples -gt 0) { "$MinEndpointSamples" } else { '' }
    # Match the bind-mount owner on Linux CI; avoid chmod 777 or a root k6 process.
    if ([Environment]::OSVersion.Platform -eq [PlatformID]::Unix) {
        $env:LOAD_RUNNER_UID = (& id -u).Trim()
        $env:LOAD_RUNNER_GID = (& id -g).Trim()
    } else {
        $env:LOAD_RUNNER_UID = '12345'
        $env:LOAD_RUNNER_GID = '12345'
    }
    $env:COMPOSE_PARALLEL_LIMIT = '1'
    # Keep the run identity even when startup fails before measurement begins.
    @{ runId=$runId; profile=$Profile; suite=$Suite; startedAt=$runStartedAt; project='vault-load' } |
        ConvertTo-Json | Out-File (Join-Path $resultsPath 'environment.json') -Encoding utf8
    Invoke-DockerChecked -Arguments @('info', '--format', '{{.ServerVersion}}')
    Invoke-DockerChecked ($composeArgs + @('config', '--quiet'))
    $apis = @('identity-api','customer-api','ledger-api','wallet-api','transaction-api','reporting-api')
    $upArgs = @('up', '-d', '--remove-orphans')
    if ($Build) { $upArgs += '--build' }
    Invoke-DockerChecked ($composeArgs + $upArgs + $apis + @('prometheus','grafana'))
    # Wait for application migrations, not just container startup.
    # The pinned k6 image executes a short readiness script instead of requiring curl in API images.
    $ready = $false
    $startupDeadline = [DateTime]::UtcNow.AddMinutes(3)
    while ([DateTime]::UtcNow -lt $startupDeadline) {
        try {
            $ErrorActionPreference = 'Continue'
            & docker @composeArgs run --rm --no-deps -T k6 run --quiet /work/helpers/readiness.js 2>&1 |
                Out-File (Join-Path $resultsPath 'readiness.log') -Encoding utf8 -Append
            $readinessExit = $LASTEXITCODE
        } finally { $ErrorActionPreference = 'Stop' }
        if ($readinessExit -eq 0) { $ready = $true; break }
        Start-Sleep -Seconds 2
    }
    if (-not $ready) { throw 'Service readiness did not pass within the startup window.' }
    $sampler = Start-Job -ArgumentList (Join-Path $resultsPath 'resources.ndjson') -ScriptBlock {
        param($OutputPath)
        while ($true) {
            $timestamp = [DateTime]::UtcNow.ToString('O')
            $ContainerIds = @(& docker ps --filter label=com.docker.compose.project=vault-load -q)
            if ($ContainerIds.Count -eq 0) { Start-Sleep -Seconds 3; continue }
            & docker stats --no-stream --format '{{json .}}' @ContainerIds 2>$null | ForEach-Object {
                try {
                    @{ timestamp = $timestamp; stats = ($_ | ConvertFrom-Json) } | ConvertTo-Json -Compress -Depth 4 | Add-Content -LiteralPath $OutputPath -Encoding utf8
                } catch { }
            }
            Start-Sleep -Seconds 3
        }
    }
    $metadata = @{
        runId=$runId; profile=$Profile; suite=$Suite
        startedAt=[DateTime]::UtcNow.ToString('O'); project='vault-load'; gitRevision=(& git -c "safe.directory=$workspace" -C $workspace rev-parse HEAD 2>$null)
        images=((& docker @composeArgs images --format json | Out-String) | ConvertFrom-Json); os=[Environment]::OSVersion.ToString()
        dockerResources=(& docker info --format 'CPUs={{.NCPU}} MemoryBytes={{.MemTotal}}')
        loadModel='Open arrival-rate HTTP business workflows'; location='Local Docker Desktop/shared host'
        minEndpointSamplesOverride=$MinEndpointSamples
    }
    $sourceHashes = @{}
    Get-ChildItem -LiteralPath $PSScriptRoot -Recurse -File | Where-Object {
        $_.FullName -notmatch '[/\\](results|node_modules|bin|obj)[/\\]'
    } | ForEach-Object {
        $sourceHashes[$_.FullName.Substring($PSScriptRoot.Length + 1).Replace('\','/')] = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
    }
    $metadata.testSourceSha256 = $sourceHashes
    $metadata | ConvertTo-Json -Depth 5 | Out-File (Join-Path $resultsPath 'environment.json') -Encoding utf8
    Write-Host "Run $runId ($Profile/$Suite). Grafana: http://localhost:3300"
    try {
        # Windows PowerShell 5.1 represents native stderr as ErrorRecord objects.
        # k6/Compose use stderr for normal progress too; the native exit code is authoritative.
        $ErrorActionPreference = 'Continue'
        & docker @composeArgs run --rm --no-deps -T k6 run --quiet --log-output stdout --tag "run_id=$runId" --out experimental-prometheus-rw /work/all.js 2>&1 |
            Tee-Object -FilePath (Join-Path $resultsPath 'k6.log')
        $exitCode = $LASTEXITCODE
    } finally { $ErrorActionPreference = 'Stop' }
    # Process exit is authoritative, including setup/script failures with no failing threshold.
    $resultFile = Join-Path $resultsPath 'result.json'
    if (Test-Path -LiteralPath $resultFile) {
        $runResult = Get-Content -LiteralPath $resultFile -Raw | ConvertFrom-Json
        $runResult | Add-Member -NotePropertyName k6ExitCode -NotePropertyValue $exitCode -Force
        $runResult.passed = $runResult.passed -and ($exitCode -eq 0)
        [IO.File]::WriteAllText($resultFile, ($runResult | ConvertTo-Json -Depth 15))
        if (-not $runResult.passed -and $exitCode -eq 0) { $exitCode = 99 }
    } elseif ($exitCode -eq 0) { $exitCode = 1; throw 'k6 produced no result.json' }
    if ($exitCode -ne 0) { Write-Warning "k6 failed with exit code $exitCode. See $resultsPath" }
}
catch {
    $exitCode = 1
    Write-Warning $_.Exception.Message
    [IO.File]::WriteAllText((Join-Path $resultsPath 'runner-error.txt'), $_.Exception.ToString())
}
finally {
    if ($sampler) { Stop-Job $sampler; Remove-Job $sampler -Force }
    if ($lockAcquired) {
        try { Save-Diagnostics } catch { Write-Warning "Could not capture all diagnostics: $_" }
        try { & (Join-Path $PSScriptRoot 'Summarize-LoadResult.ps1') -ResultsPath $resultsPath } catch { Write-Warning "Could not create markdown report: $_" }
        if ($StopAfterRun) { & docker @composeArgs stop | Out-Null }
        $runLock.ReleaseMutex()
    }
    $runLock.Dispose()
    foreach ($name in $environmentNames) { [Environment]::SetEnvironmentVariable($name, $saved[$name], 'Process') }
    Write-Host "Reports: $resultsPath"
}
exit $exitCode
