using System.Text.RegularExpressions;
using StudyPlanner.Application.HocTap;

namespace StudyPlanner.Infrastructure.TepDinhKem;

/// <summary>Kho riêng ngoài wwwroot; tên tệp người dùng không bao giờ được dùng làm đường dẫn.</summary>
public sealed partial class KhoTepNhiemVu(string thuMuc)
{
    public const long GioiHanByte = 10 * 1024 * 1024;
    private readonly string goc = Path.GetFullPath(thuMuc);
    public string ThuMuc => goc;

    public async Task<string> GhiAsync(Stream noiDung, long kichThuoc, CancellationToken ct)
    {
        if (kichThuoc <= 0 || kichThuoc > GioiHanByte)
            throw new HocTapException(400, "ATTACHMENT_SIZE_INVALID", "Tệp phải có dữ liệu và không vượt quá 10 MB.");
        Directory.CreateDirectory(goc);
        var khoa = Guid.NewGuid().ToString("N");
        try
        {
            await using var tep = new FileStream(DuongDan(khoa), FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
            var dem = 0L; var buffer = new byte[81920];
            int n;
            while ((n = await noiDung.ReadAsync(buffer, ct)) > 0)
            {
                dem += n;
                if (dem > GioiHanByte || dem > kichThuoc)
                    throw new HocTapException(400, "ATTACHMENT_SIZE_INVALID", "Kích thước tệp không hợp lệ hoặc vượt 10 MB.");
                await tep.WriteAsync(buffer.AsMemory(0, n), ct);
            }
            if (dem != kichThuoc) throw new HocTapException(400, "ATTACHMENT_SIZE_INVALID", "Tệp chưa được tải lên đầy đủ.");
            await tep.FlushAsync(ct);
            return khoa;
        }
        catch { Xoa(khoa); throw; }
    }

    public FileStream Mo(string khoa)
    {
        try { return new FileStream(DuongDan(khoa), FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, 81920, true); }
        catch (FileNotFoundException) { throw new HocTapException(404, "ATTACHMENT_CONTENT_MISSING", "Nội dung tệp không còn trong kho. Hãy tải lên lại."); }
        catch (DirectoryNotFoundException) { throw new HocTapException(404, "ATTACHMENT_CONTENT_MISSING", "Nội dung tệp không còn trong kho. Hãy tải lên lại."); }
    }
    public void Xoa(string khoa) => File.Delete(DuongDan(khoa));
    private string DuongDan(string khoa)
    {
        if (!KhoaHopLe().IsMatch(khoa)) throw new HocTapException(409, "ATTACHMENT_KEY_INVALID", "Khóa lưu trữ tệp không hợp lệ.");
        return Path.Combine(goc, khoa);
    }
    [GeneratedRegex("\\A[a-f0-9]{32}\\z", RegexOptions.CultureInvariant)]
    private static partial Regex KhoaHopLe();
}
