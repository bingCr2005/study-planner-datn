param([switch]$NoBuild, [string]$TestConfiguration = 'Debug')

$ErrorActionPreference = 'Stop'
$ArtifactsDirectory = $env:STUDYPLANNER_TEST_ARTIFACTS
$testContainerName = 'studyplanner-admin-check-' + [guid]::NewGuid().ToString('N')
$testContainerStarted = $false
$previousTestPostgres = $env:STUDYPLANNER_TEST_POSTGRES
Push-Location (Split-Path -Parent $PSScriptRoot)
try {
    docker info --format '{{.ServerVersion}}' | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Hãy mở Docker Desktop và chờ engine hoạt động.' }
    if (-not $NoBuild) {
        dotnet build backend/StudyPlanner.sln --configuration $TestConfiguration
        if ($LASTEXITCODE -ne 0) { throw 'Build thất bại.' }
    }
    # Chỉ PostgreSQL tạm, không volume, không Neon; cổng ngẫu nhiên chỉ trên loopback.
    docker run --detach --rm --name $testContainerName --publish '127.0.0.1::5432' --env 'POSTGRES_HOST_AUTH_METHOD=trust' postgres:17-alpine | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Không khởi tạo được PostgreSQL kiểm thử.' }
    $testContainerStarted = $true
    $testPostgresReady = $false
    for ($testAttempt = 0; $testAttempt -lt 30; $testAttempt++) {
        docker exec $testContainerName pg_isready -U postgres 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) { $testPostgresReady = $true; break }
        Start-Sleep -Seconds 1
    }
    if (-not $testPostgresReady) { throw 'PostgreSQL kiểm thử chưa sẵn sàng.' }
    $testPortMapping = docker port $testContainerName 5432/tcp
    if ($LASTEXITCODE -ne 0) { throw 'Không đọc được cổng kiểm thử.' }
    $testPostgresPort = [int]($testPortMapping.Trim().Split(':')[-1])
    # Chỉ truyền cấu hình trong bộ nhớ, không ghi ra log/file hoặc sửa appsettings.
    $env:STUDYPLANNER_TEST_POSTGRES = "Host=127.0.0.1;Port=$testPostgresPort;Database=postgres;Username=postgres;Pooling=false"
    if ($ArtifactsDirectory) { dotnet (Join-Path $ArtifactsDirectory 'StudyPlanner.AdministrationChecks.dll') }
    else { dotnet ("backend/tests/StudyPlanner.AdministrationChecks/bin/{0}/net10.0/StudyPlanner.AdministrationChecks.dll" -f $TestConfiguration) }
    if ($LASTEXITCODE -ne 0) { throw 'Kiểm thử CRUD thất bại.' }
    if ($ArtifactsDirectory) { dotnet (Join-Path $ArtifactsDirectory 'StudyPlanner.AuthChecks.dll') }
    else { dotnet ("backend/tests/StudyPlanner.AuthChecks/bin/{0}/net10.0/StudyPlanner.AuthChecks.dll" -f $TestConfiguration) }
    if ($LASTEXITCODE -ne 0) { throw 'Auth checks failed.' }
}
finally {
    $env:STUDYPLANNER_TEST_POSTGRES = $previousTestPostgres
    if ($testContainerStarted) {
        # Tên duy nhất do script này sinh; --rm tự dọn container sau khi dừng.
        docker stop --time 5 $testContainerName | Out-Null
    }
    Pop-Location
}
