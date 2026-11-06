param([switch]$NoBuild, [switch]$PureOnly, [switch]$IncludeLegacy, [string]$TestConfiguration='Release')
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
    $suites=@('StudyPlanner.SchemaChecks','StudyPlanner.PlanningChecks','StudyPlanner.CoreChecks')
    if ($IncludeLegacy) { $suites+=@('StudyPlanner.AdministrationChecks','StudyPlanner.AuthChecks') }
    $previousPostgres=$env:STUDYPLANNER_TEST_POSTGRES
    try {
        if ($PureOnly) { $env:STUDYPLANNER_TEST_POSTGRES=$null }
        foreach ($suite in $suites) {
            dotnet ("backend/tests/{0}/bin/{1}/net10.0/{0}.dll" -f $suite,$TestConfiguration)
            if ($LASTEXITCODE -ne 0) { throw "Kiểm thử thất bại: $suite" }
        }
        node frontend-preview/checks.cjs
        if ($LASTEXITCODE -ne 0) { throw 'Kiểm thử client thất bại.' }
    } finally { $env:STUDYPLANNER_TEST_POSTGRES=$previousPostgres }
} finally { Pop-Location }
