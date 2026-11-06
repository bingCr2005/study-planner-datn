using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Application.HocTap;
namespace StudyPlanner.Api.HocTap;
[ApiController, Authorize]
public abstract class HocTapControllerBase : ControllerBase
{
    protected int Chu => int.Parse(User.FindFirstValue("sub")!);
    protected long PhienBanYeuCau()
    {
        var value = Request.Headers.IfMatch.ToString();
        if (string.IsNullOrWhiteSpace(value)) throw new HocTapException(428, "PRECONDITION_REQUIRED", "Cần If-Match từ dữ liệu vừa đọc.");
        if (!value.StartsWith("\"ht-", StringComparison.Ordinal) || !value.EndsWith('"') || !long.TryParse(value[4..^1], out var n) || n < 0)
            throw new HocTapException(400, "VALIDATION_ERROR", "If-Match phải có dạng \"ht-0\".");
        return n;
    }
    protected void GanPhienBan(long value) => Response.Headers.ETag = $"\"ht-{value}\"";
}
