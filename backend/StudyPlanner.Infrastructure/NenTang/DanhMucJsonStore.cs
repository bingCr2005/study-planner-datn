using System.Text.Json;
using StudyPlanner.Application.NenTang;

namespace StudyPlanner.Infrastructure.NenTang;

public sealed record HocKyJson(int Ma, string Ten, int ThuTu, int PhienBan);
public sealed record MonHocJson(int Ma, int MaHocKyMau, string KyHieu, string Ten, int SoTinChi, int PhienBan);
public sealed class DanhMucJson
{
    [System.Text.Json.Serialization.JsonRequired] public int PhienBanSchema { get; set; } = 1;
    [System.Text.Json.Serialization.JsonRequired] public int MaHocKyTiepTheo { get; set; } = 1;
    [System.Text.Json.Serialization.JsonRequired] public int MaMonTiepTheo { get; set; } = 1;
    [System.Text.Json.Serialization.JsonRequired] public List<HocKyJson> HocKy { get; set; } = [];
    [System.Text.Json.Serialization.JsonRequired] public List<MonHocJson> MonHoc { get; set; } = [];
}

// OS file lock serializes requests across instances/processes. Temp+replace avoids partial JSON.
public sealed class DanhMucJsonStore(string filePath, string seedPath)
{
    public string FilePath { get; } = Path.GetFullPath(filePath);
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    public async Task<FileStream> KhoaAsync(CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            try { return new FileStream(FilePath + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch (IOException) when (DateTime.UtcNow < deadline) { await Task.Delay(40, ct); }
        }
    }
    public async Task<T> DocAsync<T>(Func<DanhMucJson, T> read, CancellationToken ct)
    {
        await using var handle = await KhoaAsync(ct);
        return read(await DocTrongKhoaAsync(ct));
    }
    public async Task<T> GhiAsync<T>(Func<DanhMucJson, T> change, CancellationToken ct)
    {
        await using var handle = await KhoaAsync(ct);
        var data = await DocTrongKhoaAsync(ct);
        var result = change(data);
        await LuuTrongKhoaAsync(data, ct);
        return result;
    }
    // Migration tools also hold KhoaAsync while exporting/verifying/removing legacy data.
    public async Task<DanhMucJson> DocTrongKhoaAsync(CancellationToken ct)
    {
        if (!File.Exists(FilePath))
        {
            var seed = Parse(await File.ReadAllTextAsync(seedPath, ct));
            await LuuTrongKhoaAsync(seed, ct); return seed;
        }
        // Never reset an existing damaged file to the default catalog.
        return Parse(await File.ReadAllTextAsync(FilePath, ct));
    }
    public static DanhMucJson Parse(string text)
    {
        var data = JsonSerializer.Deserialize<DanhMucJson>(text, Json) ?? throw new InvalidDataException("Danh mục JSON rỗng.");
        KiemTra(data); return data;
    }
    public static void KiemTra(DanhMucJson data)
    {
        if (data.PhienBanSchema != 1 || data.HocKy is null || data.MonHoc is null || data.MaHocKyTiepTheo < 1 || data.MaMonTiepTheo < 1)
            throw new InvalidDataException("Schema danh mục JSON không hợp lệ.");
        if (data.HocKy.Any(x => x is null || x.Ma < 1 || x.PhienBan < 1 || x.ThuTu is < 1 or > 100 || x.Ten != QuyTacDanhMucMau.Ten(x.Ten)) ||
            data.MonHoc.Any(x => x is null || x.Ma < 1 || x.PhienBan < 1 || x.SoTinChi is < 1 or > 30 || x.KyHieu != QuyTacDanhMucMau.KyHieu(x.KyHieu) || x.Ten != QuyTacDanhMucMau.Ten(x.Ten)))
            throw new InvalidDataException("Dữ liệu mẫu JSON không hợp lệ.");
        if (data.HocKy.Select(x => x.Ma).Distinct().Count() != data.HocKy.Count || data.MonHoc.Select(x => x.Ma).Distinct().Count() != data.MonHoc.Count ||
            data.HocKy.Select(x => x.Ten.ToUpperInvariant()).Distinct().Count() != data.HocKy.Count ||
            data.MonHoc.Select(x => (x.MaHocKyMau, x.KyHieu)).Distinct().Count() != data.MonHoc.Count ||
            data.MonHoc.Any(x => !data.HocKy.Any(t => t.Ma == x.MaHocKyMau)) ||
            data.HocKy.Any(x => x.Ma >= data.MaHocKyTiepTheo) || data.MonHoc.Any(x => x.Ma >= data.MaMonTiepTheo))
            throw new InvalidDataException("Mã, quan hệ hoặc bộ đếm danh mục JSON không hợp lệ.");
    }
    public async Task LuuTrongKhoaAsync(DanhMucJson data, CancellationToken ct)
    {
        KiemTra(data);
        var temp = FilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await using (var output = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(output, data, Json, ct);
                await output.FlushAsync(ct); output.Flush(true);
            }
            ct.ThrowIfCancellationRequested();
            if (File.Exists(FilePath)) File.Replace(temp, FilePath, FilePath + ".bak");
            else File.Move(temp, FilePath);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
}
