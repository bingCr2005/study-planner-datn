param([string]$OutputPath='backend/obj/student-v2', [switch]$StartOnly, [switch]$StopOnly)
$ErrorActionPreference='Stop'
$taskRoot=Split-Path -Parent $PSScriptRoot
Set-Location $taskRoot
$taskOutput=[IO.Path]::GetFullPath((Join-Path $taskRoot $OutputPath))
if (-not $taskOutput.StartsWith((Join-Path $taskRoot 'backend/obj/').Replace('/','\'), [StringComparison]::OrdinalIgnoreCase)) { throw 'Test output must stay under backend/obj.' }
New-Item -ItemType Directory -Path $taskOutput -Force | Out-Null
$taskPg='C:/Program Files/PostgreSQL/17/bin'
if ($StopOnly) {
    Set-Content -LiteralPath (Join-Path $taskOutput 'stop') -Value 'stop'
    $taskWait=[Diagnostics.Stopwatch]::StartNew()
    while (-not (Test-Path (Join-Path $taskOutput 'cleanup.json')) -and $taskWait.Elapsed.TotalSeconds -lt 30) { Start-Sleep -Milliseconds 250 }
    $taskData=Join-Path $taskOutput 'pg-data'
    if (Test-Path (Join-Path $taskData 'postmaster.pid')) {
        $taskStop=Start-Process (Join-Path $taskPg 'pg_ctl.exe') -ArgumentList @('-D', $taskData, '-m', 'fast', '-w', 'stop') -WindowStyle Hidden -PassThru -Wait -RedirectStandardOutput (Join-Path $taskOutput 'pg-stop.log') -RedirectStandardError (Join-Path $taskOutput 'pg-stop-error.log')
        if ($taskStop.ExitCode -ne 0) { throw 'Test cluster did not stop' }
    }
    Write-Output 'Stopped the isolated Student V2 cluster.'
    exit
}
if (Test-Path (Join-Path $taskOutput 'cluster.json')) { throw 'Use a fresh output directory for each run.' }
$taskData=Join-Path $taskOutput 'pg-data'
$taskListener=[Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback,0)
$taskListener.Start(); $taskPort=$taskListener.LocalEndpoint.Port; $taskListener.Stop()
$taskInit=Start-Process (Join-Path $taskPg 'initdb.exe') -ArgumentList @('-D',$taskData,'-U','postgres','-A','trust','--encoding=UTF8','--no-locale') -WindowStyle Hidden -PassThru -Wait -RedirectStandardOutput (Join-Path $taskOutput 'pg-init.log') -RedirectStandardError (Join-Path $taskOutput 'pg-init-error.log')
if ($taskInit.ExitCode -ne 0) { throw 'Cannot initialize isolated PostgreSQL' }
$taskStart=Start-Process (Join-Path $taskPg 'pg_ctl.exe') -ArgumentList @('-D',$taskData,'-l',(Join-Path $taskOutput 'postgres.log'),'-o',('"-h 127.0.0.1 -p '+$taskPort+'"'),'-w','start') -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $taskOutput 'pg-start.log') -RedirectStandardError (Join-Path $taskOutput 'pg-start-error.log')
# -Wait waits for descendants too, which includes the long-lived postgres server on Windows.
$taskStart.WaitForExit()
if ($taskStart.ExitCode -ne 0) { throw 'Cannot start isolated PostgreSQL' }
@{port=$taskPort;data=$taskData;time=[DateTimeOffset]::UtcNow} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $taskOutput 'cluster.json')
$env:STUDYPLANNER_TEST_POSTGRES="Host=127.0.0.1;Port=$taskPort;Database=postgres;Username=postgres;Pooling=false"
$env:STUDENT_V2_OUTPUT=$taskOutput
if ($StartOnly) {
    Write-Output "Started isolated PostgreSQL on localhost port $taskPort."
    exit
}
try {
    & ./scripts/test-study-core.ps1 -NoBuild -IncludeLegacy *> (Join-Path $taskOutput 'regression.log')
    if ($LASTEXITCODE -ne 0) { throw 'Regression suite failed; see regression.log' }
    Write-Output 'Regression suites passed on isolated PostgreSQL.'
} finally {
    $taskStop=Start-Process (Join-Path $taskPg 'pg_ctl.exe') -ArgumentList @('-D',$taskData,'-m','fast','-w','stop') -WindowStyle Hidden -PassThru -Wait -RedirectStandardOutput (Join-Path $taskOutput 'pg-stop.log') -RedirectStandardError (Join-Path $taskOutput 'pg-stop-error.log')
    if ($taskStop.ExitCode -ne 0) { throw 'Test cluster did not stop' }
}
