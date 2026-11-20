param([switch]$NoBuild, [switch]$PureOnly, [switch]$IncludeLegacy, [string]$TestConfiguration='Release', [string]$FrontendPath = (Join-Path (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)) 'Study_Planner_FE'))
$ErrorActionPreference='Stop'
Push-Location (Split-Path -Parent $PSScriptRoot)
try {
    if (-not $PureOnly -and -not $env:STUDYPLANNER_TEST_POSTGRES) {
        throw 'Đặt STUDYPLANNER_TEST_POSTGRES tới PostgreSQL kiểm thử localhost, database postgres; hoặc chọn -PureOnly. Script không dùng cấu hình database thật của API.'
    }
    if ($PureOnly -and $IncludeLegacy) { throw '-IncludeLegacy cần PostgreSQL kiểm thử; không dùng cùng -PureOnly.' }
    if (-not $NoBuild) {
        dotnet build backend/StudyPlanner.sln --configuration $TestConfiguration --no-restore
        if ($LASTEXITCODE -ne 0) { throw 'Build thất bại; restore dependencies trước khi chạy.' }
    }
    $suites=@('StudyPlanner.SchemaChecks','StudyPlanner.PlanningChecks','StudyPlanner.PriorityChecks','StudyPlanner.CoreChecks')
    if ($IncludeLegacy) { $suites+=@('StudyPlanner.AdministrationChecks','StudyPlanner.AuthChecks') }
    $previousPostgres=$env:STUDYPLANNER_TEST_POSTGRES
    try {
        if ($PureOnly) { $env:STUDYPLANNER_TEST_POSTGRES=$null }
        foreach ($suite in $suites) {
            dotnet ("backend/tests/{0}/bin/{1}/net10.0/{0}.dll" -f $suite,$TestConfiguration)
            if ($LASTEXITCODE -ne 0) { throw "Kiểm thử thất bại: $suite" }
        }
        if (-not (Test-Path (Join-Path $FrontendPath 'package.json'))) { throw 'Không tìm thấy frontend.' }
        Push-Location $FrontendPath
        try { npm.cmd test } finally { Pop-Location }
        if ($LASTEXITCODE -ne 0) { throw 'Kiểm thử client thất bại.' }
    } finally { $env:STUDYPLANNER_TEST_POSTGRES=$previousPostgres }
} finally { Pop-Location }
