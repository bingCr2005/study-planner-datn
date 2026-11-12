using StudyPlanner.Application.HocTap;
using StudyPlanner.Application.UuTien;

QuyTacCauHinhUuTien.KiemTra(QuyTacCauHinhUuTien.MacDinh());
QuyTacCauHinhUuTien.KiemTra(new() { PhienBanSchema = 1, TieuChi = ["Importance", "Deadline"] });
foreach (var config in new CauHinhUuTien?[] { null, new() { PhienBanSchema = 2, TieuChi = ["Deadline"] },
    new() { PhienBanSchema = 1, TieuChi = [] }, new() { PhienBanSchema = 1, TieuChi = ["Importance"] },
    new() { PhienBanSchema = 1, TieuChi = ["Deadline", "Deadline"] }, new() { PhienBanSchema = 1, TieuChi = ["Deadline", "script"] } })
{
    try { QuyTacCauHinhUuTien.KiemTra(config); throw new Exception("Invalid config accepted"); }
    catch (HocTapException error) when (error.Status == 400) { }
}
Console.WriteLine("PASS: priority config schema, required deadline, criteria allow-list and uniqueness.");
await IntegrationChecks.RunAsync();
