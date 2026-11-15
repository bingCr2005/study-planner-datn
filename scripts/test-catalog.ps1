param([switch]$PureOnly)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskPrevious = $env:STUDYPLANNER_TEST_POSTGRES
$taskStarted = $false
$taskPgCtl = 'C:\Program Files\PostgreSQL\17\bin\pg_ctl.exe'
$taskData = Join-Path $taskRoot ('backend/obj/catalog-test/' + [Guid]::NewGuid().ToString('N'))
Push-Location -LiteralPath $taskRoot
try {
    & dotnet build backend/tests/StudyPlanner.CatalogChecks/StudyPlanner.CatalogChecks.csproj --configuration Catalog -p:NuGetAudit=false
    if ($LASTEXITCODE -ne 0) { throw 'Catalog build failed' }
    # JSON persistence/locking tests always run; only HTTP JWT/account fixtures and
    # the legacy database-to-file transfer checks need the isolated PostgreSQL.
    if ($PureOnly) { $env:STUDYPLANNER_TEST_POSTGRES = $null }
    elseif (-not $env:STUDYPLANNER_TEST_POSTGRES) {
        $taskInitDb = Join-Path (Split-Path -Parent $taskPgCtl) 'initdb.exe'
        if (-not (Test-Path -LiteralPath $taskInitDb)) { throw 'Set STUDYPLANNER_TEST_POSTGRES to an isolated localhost postgres database, or install PostgreSQL 17.' }
        New-Item -ItemType Directory -Path $taskData -Force | Out-Null
        & $taskInitDb -D $taskData -U postgres -A trust --no-locale -E UTF8
        if ($LASTEXITCODE -ne 0) { throw 'Cannot initialize isolated test PostgreSQL' }
        & $taskPgCtl -D $taskData -l (Join-Path $taskData 'test.log') -o '-h 127.0.0.1 -p 55443' -w start
        if ($LASTEXITCODE -ne 0) { throw 'Cannot start isolated test PostgreSQL on port 55443' }
        $taskStarted = $true
        $env:STUDYPLANNER_TEST_POSTGRES = 'Host=127.0.0.1;Port=55443;Database=postgres;Username=postgres;Pooling=false'
    }
    & dotnet backend/tests/StudyPlanner.CatalogChecks/bin/Catalog/net10.0/StudyPlanner.CatalogChecks.dll
    if ($LASTEXITCODE -ne 0) { throw 'Catalog checks failed' }
} finally {
    $env:STUDYPLANNER_TEST_POSTGRES = $taskPrevious
    if ($taskStarted) { & $taskPgCtl -D $taskData -m fast -w stop }
    Pop-Location
}
