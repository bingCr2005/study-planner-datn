using StudyPlanner.CoreChecks;

try
{
    if (args.Contains("--student-v2-host")) { await StudentV2Host.RunAsync(); return; }
    if (args.Contains("--student-v2-audit")) { await StudentV2Host.AuditAsync(); return; }
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
        if (Environment.GetEnvironmentVariable("STUDYPLANNER_ATTACHMENTS_ONLY") == "1") await AttachmentHttpChecks.RunAsync(supplied);
        else if (Environment.GetEnvironmentVariable("STUDYPLANNER_FOCUS_ONLY") == "1") await FocusHttpChecks.RunAsync(supplied);
        else if (Environment.GetEnvironmentVariable("STUDYPLANNER_PLANNING_ONLY") == "1") await PlanningUpgradeChecks.RunAsync(supplied);
        else { await MigrationProgressChecks.RunAsync(supplied); await IntegrationChecks.RunAsync(supplied); await AvailabilityUpgradeChecks.RunAsync(supplied); await PlanningUpgradeChecks.RunAsync(supplied); await PlanningReminderChecks.RunAsync(supplied); await FocusHttpChecks.RunAsync(supplied); await AttachmentHttpChecks.RunAsync(supplied); }
    }
}
catch (Exception e)
{
    Console.Error.WriteLine(e);
    Environment.ExitCode = 1;
}
