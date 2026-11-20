param([switch]$NoBuild)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskArtifacts = Join-Path $taskRoot 'backend/obj/admin-accounts-checks'
$taskData = Join-Path $taskRoot ('backend/obj/admin-accounts-pg-' + [guid]::NewGuid().ToString('N'))
$taskPg = 'C:/Program Files/PostgreSQL/17/bin'
$taskStarted = $false
$taskPrevious = $env:STUDYPLANNER_TEST_POSTGRES
Push-Location $taskRoot
try {
    if (-not $NoBuild) {
        dotnet build backend/tests/StudyPlanner.AdministrationChecks/StudyPlanner.AdministrationChecks.csproj --artifacts-path $taskArtifacts --ignore-failed-sources -v minimal
        if ($LASTEXITCODE -ne 0) { throw 'Admin accounts checks build failed.' }
    }
    # Cụm kiểm thử chỉ nghe localhost và tạo database tên ngẫu nhiên.
    # Không đọc cấu hình kết nối của ứng dụng, không dùng database sinh viên thật.
    $taskListener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, 0)
    $taskListener.Start()
    $taskPort = $taskListener.LocalEndpoint.Port
    $taskListener.Stop()
    & (Join-Path $taskPg 'initdb.exe') -D $taskData -U postgres -A trust --encoding=UTF8 --no-locale | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Cannot initialize isolated PostgreSQL.' }
    # pg_ctl sinh postgres chạy nền. Tách stdout/stderr khỏi pipe của terminal
    # để tiến trình con không giữ pipe mở và làm PowerShell chờ vô hạn trên Windows.
    $taskStart = Start-Process -FilePath (Join-Path $taskPg 'pg_ctl.exe') -ArgumentList @('-D', $taskData, '-l', (Join-Path $taskData 'test.log'), '-o', "`"-h 127.0.0.1 -p $taskPort`"", '-w', 'start') -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $taskData 'start.out') -RedirectStandardError (Join-Path $taskData 'start.err')
    # Chỉ chờ pg_ctl; Start-Process -Wait sẽ chờ cả postgres chạy nền và bị treo.
    $taskStart.WaitForExit()
    if ($taskStart.ExitCode -ne 0) { throw 'Cannot start isolated PostgreSQL.' }
    $taskStarted = $true
    $env:STUDYPLANNER_TEST_POSTGRES = "Host=127.0.0.1;Port=$taskPort;Database=postgres;Username=postgres;Pooling=false"
    Write-Host 'Running admin accounts HTTP checks on isolated PostgreSQL...'
    dotnet (Join-Path $taskArtifacts 'bin/StudyPlanner.AdministrationChecks/debug/StudyPlanner.AdministrationChecks.dll')
    if ($LASTEXITCODE -ne 0) { throw 'Admin accounts HTTP/PostgreSQL checks failed.' }
} finally {
    $env:STUDYPLANNER_TEST_POSTGRES = $taskPrevious
    if ($taskStarted) { & (Join-Path $taskPg 'pg_ctl.exe') -D $taskData -m fast -w stop | Out-Null }
    Pop-Location
}
