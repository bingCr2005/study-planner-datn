using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using StudyPlanner.Application.HocTap;

namespace StudyPlanner.Infrastructure.HocTap;

// Khóa phải được cấp từ cấu hình runtime; payload chỉ chứa lịch của người đã xác thực.
public sealed class HmacPreviewLapLichToken : IPreviewLapLichToken
{
    private readonly byte[] key;
    public HmacPreviewLapLichToken(byte[] key)
    {
        if (key.Length < 32) throw new ArgumentException("Khóa ký preview cần ít nhất 32 byte.", nameof(key));
        this.key = key.ToArray();
    }

    public string Tao(NoiDungPreviewLapLich noiDung)
    {
        var payload = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(noiDung));
        return payload + "." + Convert.ToBase64String(HMACSHA256.HashData(key, Encoding.ASCII.GetBytes(payload)));
    }

    public NoiDungPreviewLapLich Doc(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > 2_000_000)
            throw Sai();
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 2) throw Sai();
            var signature = Convert.FromBase64String(parts[1]);
            var expected = HMACSHA256.HashData(key, Encoding.ASCII.GetBytes(parts[0]));
            if (!CryptographicOperations.FixedTimeEquals(signature, expected)) throw Sai();
            var data = JsonSerializer.Deserialize<NoiDungPreviewLapLich>(Convert.FromBase64String(parts[0]));
            if (data is null || data.Phien is null || data.Phien.Count > 2000) throw Sai();
            return data;
        }
        catch (Exception e) when (e is FormatException or JsonException or ArgumentException) { throw Sai(); }
    }

    private static HocTapException Sai() => new(400, "INVALID_PREVIEW_TOKEN", "Phiếu gợi ý không hợp lệ hoặc đã bị sửa.");
}
