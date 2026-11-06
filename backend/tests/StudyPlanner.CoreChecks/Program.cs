using StudyPlanner.CoreChecks;

try
{
    ThoiGianChecks.Run();
    NhiemVuChecks.Run();
    var supplied = Environment.GetEnvironmentVariable("STUDYPLANNER_TEST_POSTGRES");
    if (string.IsNullOrWhiteSpace(supplied))
        Console.WriteLine("SKIP PostgreSQL/HTTP checks: set STUDYPLANNER_TEST_POSTGRES to an isolated localhost postgres database. Pure checks only; integration is not verified.");
    else
        await IntegrationChecks.RunAsync(supplied);
}
catch (Exception e)
{
    Console.Error.WriteLine(e);
    Environment.ExitCode = 1;
}
