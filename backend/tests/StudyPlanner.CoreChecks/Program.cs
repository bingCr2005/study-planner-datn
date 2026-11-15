using StudyPlanner.CoreChecks;

try
{
    ThoiGianChecks.Run();
    NhiemVuChecks.Run();
    await PhienTuHocChecks.RunAsync();
    FocusChecks.Run();
    PlanningUpgradeChecks.Run();
    var supplied = Environment.GetEnvironmentVariable("STUDYPLANNER_TEST_POSTGRES");
    if (string.IsNullOrWhiteSpace(supplied))
        Console.WriteLine("SKIP PostgreSQL/HTTP checks: set STUDYPLANNER_TEST_POSTGRES to an isolated localhost postgres database. Pure checks only; integration is not verified.");
    else
    {
        if (Environment.GetEnvironmentVariable("STUDYPLANNER_FOCUS_ONLY") == "1") await FocusHttpChecks.RunAsync(supplied);
        else { await MigrationProgressChecks.RunAsync(supplied); await IntegrationChecks.RunAsync(supplied); await AvailabilityUpgradeChecks.RunAsync(supplied); await PlanningUpgradeChecks.RunAsync(supplied); await PlanningReminderChecks.RunAsync(supplied); await FocusHttpChecks.RunAsync(supplied); }
    }
}
catch (Exception e)
{
    Console.Error.WriteLine(e);
    Environment.ExitCode = 1;
}
