using System.Text.Json;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.NenTang;
using StudyPlanner.Infrastructure.NenTang;

public static class JsonChecks
{
    public static async Task RunAsync()
    {
        var folder = Path.Combine(Path.GetTempPath(), "catalog-json-test-" + Guid.NewGuid().ToString("N"));
        var file = Path.Combine(folder, "catalog.json");
        var seed = Path.Combine(AppContext.BaseDirectory, "Administration/Data/danh-muc-mau.seed.json");
        void Check(bool ok, string label) { if (!ok) throw new Exception("FAIL: " + label); Console.WriteLine("PASS: " + label); }
        var store = new DanhMucJsonStore(file, seed);
        var service = new DuLieuNenTangService(store);
        try
        {
            Check((await service.HocKyAsync(default)).Count == 2, "New JSON file initializes once with the tracked seed");
            var created = await service.LuuHocKyAsync(null, new("Kỳ file", 3, null), default);
            var tasks = Enumerable.Range(0, 20).Select(i => new DuLieuNenTangService(new(file, seed)).LuuMonHocAsync(null, new(created.Ma, "CODE" + i, "Môn " + i, 3, null), default));
            var saved = await Task.WhenAll(tasks);
            Check(saved.Select(x => x.Ma).Distinct().Count() == 20 && (await service.MonHocAsync(created.Ma, null, 1, default)).TongSo == 20, "Independent file stores serialize concurrent writes without losing IDs/rows");
            var target = saved[0];
            async Task<bool> Update(string name)
            {
                try { await new DuLieuNenTangService(new(file, seed)).LuuMonHocAsync(target.Ma, new(created.Ma, target.KyHieu, name, 5, target.PhienBan), default); return true; }
                catch (QuanTriException ex) when (ex.LoaiLoi == LoaiLoiNghiepVu.Conflict) { return false; }
            }
            Check((await Task.WhenAll(Update("Một"), Update("Hai"))).Count(x => x) == 1, "Stale edits rejected after file lock acquired");
            var deleted = saved[^1]; await service.XoaAsync(false, deleted.Ma, deleted.PhienBan, default);
            var after = await new DuLieuNenTangService(new(file, seed)).LuuMonHocAsync(null, new(created.Ma, "NEXT", "Môn tiếp", 3, null), default);
            Check(after.Ma > deleted.Ma, "Restart/deletion never reuses the highest deleted ID");
            Check(File.Exists(file + ".bak") && !Directory.EnumerateFiles(folder, "*.tmp").Any(), "Atomic replacement keeps previous backup and no temporary leftovers");
            var good = await File.ReadAllTextAsync(file);
            File.Move(file + ".bak", file + ".saved-backup"); Directory.CreateDirectory(file + ".bak");
            try { await service.LuuHocKyAsync(null, new("Không ghi được", 8, null), default); throw new Exception("Must reject failed file replacement"); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
            Check(await File.ReadAllTextAsync(file) == good && !Directory.EnumerateFiles(folder, "*.tmp").Any(), "Failed atomic replacement preserves original file and cleans temporary output");
            Directory.Delete(file + ".bak"); File.Move(file + ".saved-backup", file + ".bak");
            try { DanhMucJsonStore.Parse("{}"); throw new Exception("Incomplete JSON must fail"); } catch (JsonException) { }
            await File.WriteAllTextAsync(file, "{broken");
            try { await service.HocKyAsync(default); throw new Exception("Corrupt file must fail"); } catch (JsonException) { }
            Check(await File.ReadAllTextAsync(file) == "{broken", "Corrupt JSON is reported without resetting existing admin data");
            await File.WriteAllTextAsync(file, good);
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            try { await service.LuuHocKyAsync(null, new("Không lưu", 9, null), cancellation.Token); throw new Exception("Must cancel"); } catch (OperationCanceledException) { }
            Check(await File.ReadAllTextAsync(file) == good, "Cancelled mutation cannot replace the JSON file");
            var restart = new DuLieuNenTangService(new(file, seed));
            Check((await restart.MonHocAsync(created.Ma, null, 1, default)).TongSo == 20, "Saved admin changes persist across fresh service/store instances");
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
    }
}
