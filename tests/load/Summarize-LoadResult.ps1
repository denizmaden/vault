param([Parameter(Mandatory=$true)][string]$ResultsPath)
$summaryPath = Join-Path $ResultsPath 'summary.json'
if (-not (Test-Path -LiteralPath $summaryPath)) { return }
$summary = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json
$result = Get-Content -LiteralPath (Join-Path $ResultsPath 'result.json') -Raw | ConvertFrom-Json
$reportTitle = "Vault $($result.profile)/$($result.suite) - $($result.runId)"
# k6 exports a self-contained HTML file with a compressed JSON event stream.
# Change its display title only; leave the measured samples and event data intact.
$htmlPath = Join-Path $ResultsPath 'dashboard.html'
if (Test-Path -LiteralPath $htmlPath) {
    $html = [IO.File]::ReadAllText($htmlPath)
    $html = [regex]::Replace($html, '<title>[^<]*</title>', "<title>$([Net.WebUtility]::HtmlEncode($reportTitle))</title>")
    $embedded = [regex]::Match($html, '(<script id="data"[^>]*>)([^<]*)(</script>)')
    if (-not $embedded.Success) { throw 'Unrecognized k6 HTML report: compressed data script is missing.' }
    $inputStream = [IO.MemoryStream]::new([Convert]::FromBase64String($embedded.Groups[2].Value))
    $gzip = [IO.Compression.GZipStream]::new($inputStream, [IO.Compression.CompressionMode]::Decompress)
    $reader = [IO.StreamReader]::new($gzip)
    try { $events = $reader.ReadToEnd() } finally { $reader.Dispose(); $inputStream.Dispose() }
    $titlePattern = [regex]::new('(?m)^(\{"event":"config","data":\{"title":)"[^"\\]*(?:\\.[^"\\]*)*"')
    if (-not $titlePattern.IsMatch($events)) { throw 'Unrecognized k6 HTML report: dashboard configuration title is missing.' }
    $titleJson = ConvertTo-Json -InputObject $reportTitle -Compress
    $events = $titlePattern.Replace($events, [System.Text.RegularExpressions.MatchEvaluator]{ param($match) $match.Groups[1].Value + $titleJson }, 1)
    $outputStream = [IO.MemoryStream]::new()
    $gzip = [IO.Compression.GZipStream]::new($outputStream, [IO.Compression.CompressionLevel]::Optimal, $true)
    try {
        $bytes = [Text.Encoding]::UTF8.GetBytes($events)
        $gzip.Write($bytes, 0, $bytes.Length)
    } finally { $gzip.Dispose() }
    try { $encoded = [Convert]::ToBase64String($outputStream.ToArray()) } finally { $outputStream.Dispose() }
    $html = $html.Substring(0, $embedded.Groups[2].Index) + $encoded + $html.Substring($embedded.Groups[2].Index + $embedded.Groups[2].Length)
    [IO.File]::WriteAllText($htmlPath, $html)
}
$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# $reportTitle")
$lines.Add('')
$lines.Add("Run: $($result.runId); profile/suite: $($result.profile)/$($result.suite); passed: $($result.passed)")
$lines.Add("k6 process exit: $($result.k6ExitCode). Missing exit status in older reports must be checked in runner output.")
$lines.Add('')
$lines.Add('## Successful endpoint coverage (measurement phase only)')
$lines.Add('')
$lines.Add('| Service | Endpoint | 2xx samples | Required | Covered | p95 ms | p99 ms |')
$lines.Add('|---|---|---:|---:|---|---:|---:|')
foreach ($row in $result.endpointCoverage) {
    $lines.Add(('| {0} | {1} | {2} | {3} | {4} | {5:F2} | {6:F2} |' -f $row.service, $row.endpoint, $row.successes, $row.required, $row.covered, $row.latencyMs.'p(95)', $row.latencyMs.'p(99)'))
}
$lines.Add('')
$lines.Add('Only successful 2xx calls satisfy coverage. Setup, warmup, teardown, 400/404/409 probes do not satisfy this table. Sparse samples make p95/p99 indicative; minimum coverage is not statistical confidence.')
$lines.Add('')
$lines.Add('## Successful HTTP paths intentionally outside load coverage')
$lines.Add('')
$lines.Add('| Endpoint | Reason |')
$lines.Add('|---|---|')
foreach ($item in $result.successfulEndpointExclusions) {
    $lines.Add("| $($item.endpoint) | $($item.reason) |")
}
$lines.Add('')
$lines.Add('## Scheduled business workflows')
$lines.Add('')
$lines.Add('| Workflow | Target starts/min | Planned starts (ramp + hold + down) | Minimum completed | Actual completed | p95 ms |')
$lines.Add('|---|---:|---:|---:|---:|---:|')
foreach ($flow in $result.executionPlan.workflows) {
    $countName = 'workflows_completed{workflow:' + $flow.name + '}'
    $timeName = 'workflow_duration_ms{workflow:' + $flow.name + '}'
    $actual = $summary.metrics.PSObject.Properties[$countName].Value.values.count
    $duration = $summary.metrics.PSObject.Properties[$timeName].Value.values.'p(95)'
    $lines.Add(('| {0} | {1} | {2:F2} | {3} | {4} | {5:F2} |' -f $flow.name, $flow.target, $flow.plannedStarts, $flow.minimumCompleted, $actual, $duration))
}
$lines.Add('')
$lines.Add('## Service latency (measurement phase only)')
$lines.Add('')
$lines.Add('| Service | p95 ms | p99 ms | Max ms |')
$lines.Add('|---|---:|---:|---:|')
foreach ($property in ($summary.metrics.PSObject.Properties | Sort-Object Name)) {
    if ($property.Name -match '^http_req_duration\{phase:measure,service:(.+)\}$') {
        $value = $property.Value.values
        $lines.Add(('| {0} | {1:F2} | {2:F2} | {3:F2} |' -f $Matches[1], $value.'p(95)', $value.'p(99)', $value.max))
    }
}
$lines.Add('')
$lines.Add('## Correctness and asynchronous flows')
$lines.Add('')
$lines.Add('| Metric | Values |')
$lines.Add('|---|---|')
foreach ($name in @('http_reqs{phase:measure}','http_req_failed{phase:measure}','contract_failures','correctness_failures','dropped_iterations','projection_latency_ms','saga_latency_ms')) {
    $metric = $summary.metrics.PSObject.Properties[$name]
    if ($metric) { $lines.Add("| $name | $($metric.Value.values | ConvertTo-Json -Compress) |") }
}
$lines.Add('')
$lines.Add('## Endpoint latency (measurement phase only)')
$lines.Add('')
$lines.Add('| Endpoint / variation | p95 ms | p99 ms | Max ms |')
$lines.Add('|---|---:|---:|---:|')
foreach ($property in ($summary.metrics.PSObject.Properties | Sort-Object Name)) {
    if ($property.Name -match '^endpoint_latency_ms\{phase:measure,endpoint:(.+)\}$') {
        $value = $property.Value.values
        $lines.Add(('| {0} | {1:F2} | {2:F2} | {3:F2} |' -f $Matches[1], $value.'p(95)', $value.'p(99)', $value.max))
    }
}
$lines.Add('')
$lines.Add('## Contract and behavior checks')
$lines.Add('')
$lines.Add('| Check | Passed | Failed |')
$lines.Add('|---|---:|---:|')
foreach ($item in ($summary.root_group.checks | Sort-Object name)) {
    $lines.Add("| $($item.name) | $($item.passes) | $($item.fails) |")
}
$lines.Add('')
$resourcePath = Join-Path $ResultsPath 'resources.ndjson'
if (Test-Path -LiteralPath $resourcePath) {
    $rows = Get-Content -LiteralPath $resourcePath | ForEach-Object { try { $_ | ConvertFrom-Json } catch { } }
    $lines.Add('## Container resource samples (whole run including preparation)')
    $lines.Add('')
    $lines.Add('Docker CPU percent: one fully used core = 100%. MemUsage is Docker working-set accounting.')
    $lines.Add('')
    $lines.Add('| Container | Peak CPU % | Peak memory MiB |')
    $lines.Add('|---|---:|---:|')
    foreach ($group in ($rows | Group-Object { $_.stats.Name } | Sort-Object Name)) {
        $peakCpu = 0.0; $peakMemory = 0.0
        foreach ($row in $group.Group) {
            $cpu = [double]::Parse($row.stats.CPUPerc.TrimEnd('%'), [Globalization.CultureInfo]::InvariantCulture)
            $peakCpu = [Math]::Max($peakCpu, $cpu)
            if ($row.stats.MemUsage -match '^([\d.]+)(KiB|MiB|GiB|kB|MB|GB|B)') {
                $number = [double]::Parse($Matches[1], [Globalization.CultureInfo]::InvariantCulture)
                $factor = @{ B=1; kB=1000; MB=1000000; GB=1000000000; KiB=1024; MiB=1048576; GiB=1073741824 }[$Matches[2]]
                $peakMemory = [Math]::Max($peakMemory, $number * $factor / 1048576)
            }
        }
        $lines.Add(('| {0} | {1:F2} | {2:F2} |' -f $group.Name, $peakCpu, $peakMemory))
    }
    $lines.Add('')
}
$lines.Add('## Failed thresholds')
$lines.Add('')
if ($result.failedThresholds.Count -eq 0) { $lines.Add('None.') }
else { foreach ($failure in $result.failedThresholds) { $lines.Add("- $failure") } }
$lines.Add('')
$lines.Add('## Interpretation limits')
$lines.Add('')
foreach ($limit in $result.limitations) { $lines.Add("- $limit") }
$lines.Add('- Check resources.ndjson for generator saturation and kafka-consumer-lag.txt / rabbitmq-queues.txt for pending work.')
$lines.Add('- A completed Transaction saga does not prove ledger debit/credit integration.')
[IO.File]::WriteAllText((Join-Path $ResultsPath 'report.md'), ($lines -join [Environment]::NewLine))
