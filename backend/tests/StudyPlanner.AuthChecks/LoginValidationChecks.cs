using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.RateLimiting;
using StudyPlanner.Api.Administration;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.Auth;

// Chỉ kiểm tra validation: không dùng cấu hình/kết nối database ứng dụng.
internal static class LoginValidationChecks
{
    public static async Task RunAsync()
    {
        var checks = 0;
        void Check(bool condition, string label)
        {
            if (!condition) throw new Exception("FAIL: " + label);
            checks++;
        }
        var invalid = new List<(string? Account, string? Password, string Field)>
        {
            (null, "Test1234!", "TaiKhoan"), ("", "Test1234!", "TaiKhoan"), ("   ", "Test1234!", "TaiKhoan"),
            (new string('a', 321), "Test1234!", "TaiKhoan"),
            ("0306000001", null, "MatKhau"), ("0306000001", "", "MatKhau"),
            ("0306000001", "        ", "MatKhau"),
            ("an@", "Test1234!", "TaiKhoan"), ("an@@example.com", "Test1234!", "TaiKhoan"),
            ("0306000001", new string('a', 129), "MatKhau")
        };
        foreach (var symbol in "+-.!#$%&/\\:;()[]{}<> =\n\t")
            invalid.Add(("SV" + symbol + "001", "Test1234!", "TaiKhoan"));
        foreach (var quote in "'\"`‘’“”")
        {
            invalid.Add(("SV" + quote + "001", "Test1234!", "TaiKhoan"));
        }
        foreach (var account in new[] { "0306000001", "SvABC01", "SV_01", "AN@EXAMPLE.COM", "first-last+tag@example.com", "first_last@example.com", "admin@caothang.edu.vn", " 0306000001 ", "SinhViên01", new string('a', 320) })
        foreach (var password in new[] { "a", "1234567", "12345678", new string('a', 128), " Test!@#$%&*()_+-= ", "Test1234!\n", " Test'\"`‘’“” ", "        a" })
        {
            var request = new DangNhapRequest { TaiKhoan = account, MatKhau = password };
            Check(Validator.TryValidateObject(request, new ValidationContext(request), [], true), "Valid login DTO");
            Check(request.TaiKhoan == account && request.MatKhau == password, "Validation preserves credentials");
        }

        // Độ dài tối thiểu thuộc luồng tạo mật khẩu, không chặn tài khoản cũ lúc đăng nhập.
        foreach (var password in new[] { "", "        ", "1234567", new string('a', 129), "12345678", new string('a', 128), " Test'\"`‘’“” ", "        a" })
        {
            var expected = !string.IsNullOrWhiteSpace(password) && password.Length is >= 8 and <= 128;
            var registration = new DangKyRequest { Email = "student-name+tag@example.com", HoTen = "Kiểm thử", MatKhau = password,
                MaSoSinhVien = "SV_001", TenTruong = "Trường", TenLop = "Lớp", NganhHoc = "Ngành" };
            var change = new DoiMatKhauRequest { MatKhauHienTai = "short", MatKhauMoi = password };
            Check(Validator.TryValidateObject(registration, new ValidationContext(registration), [], true) == expected, "Registration password policy");
            Check(Validator.TryValidateObject(change, new ValidationContext(change), [], true) == expected, "New password policy");
        }

        // Dependencies cố ý không có: dữ liệu lỗi phải bị từ chối trước khi đọc store/băm mật khẩu/cấp JWT.
        var service = new XacThucService(null!, null!, null!, null!, TimeProvider.System);
        foreach (var item in invalid)
        {
            var request = new DangNhapRequest { TaiKhoan = item.Account!, MatKhau = item.Password! };
            var errors = new List<ValidationResult>();
            Check(!Validator.TryValidateObject(request, new ValidationContext(request), errors, true) &&
                errors.Any(error => error.MemberNames.Contains(item.Field)), "Invalid field identified");
            try
            {
                await service.DangNhapAsync(request, CancellationToken.None);
                throw new Exception("Invalid login reached service dependencies");
            }
            catch (QuanTriException ex) { Check(ex.MaLoi == "VALIDATION_ERROR", "Application rejects invalid input"); }
        }

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing", Args = [] });
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddQuanTri();
        builder.Services.AddSingleton(service);
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
        builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter("auth", limiter =>
        {
            limiter.PermitLimit = 1000; limiter.Window = TimeSpan.FromMinutes(1);
        }));
        await using var app = builder.Build();
        app.UseExceptionHandler(); app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers();
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };
        foreach (var item in invalid)
        {
            using var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { taiKhoan = item.Account, matKhau = item.Password });
            Check(response.StatusCode == HttpStatusCode.BadRequest, "HTTP rejects invalid input before controller");
            Check(response.Content.Headers.ContentType?.MediaType == "application/problem+json", "HTTP validation uses problem details");
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Check(json.RootElement.GetProperty("code").GetString() == "VALIDATION_ERROR" &&
                json.RootElement.GetProperty("errors").TryGetProperty(item.Field, out _), "HTTP identifies invalid field");
        }
        using (var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { taiKhoan = "SV-01", matKhau = "" }))
        {
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Check(response.StatusCode == HttpStatusCode.BadRequest, "Both invalid fields return 400");
            var errors = json.RootElement.GetProperty("errors");
            Check(errors.TryGetProperty("TaiKhoan", out var accountErrors) && accountErrors[0].GetString()!.Contains("email đúng định dạng"), "Account error explains accepted identifiers");
            Check(errors.TryGetProperty("MatKhau", out var passwordErrors) && passwordErrors[0].GetString()!.Contains("Vui lòng nhập"), "Password error explains required field");
        }
        foreach (var path in new[] { "/api/v1/auth/login", "/API/V1/AUTH/LOGIN/", "/api/v1/users/me/change-password" })
        {
            var context = new DefaultHttpContext { RequestServices = app.Services };
            context.Request.Path = path;
            using var body = new MemoryStream(); context.Response.Body = body;
            Check(await new QuanTriExceptionHandler().TryHandleAsync(context, new XacThucException(), CancellationToken.None), "Authentication exception handled");
            Check(context.Response.StatusCode == 401, "Authentication failure remains 401");
            body.Position = 0;
            using var json = await JsonDocument.ParseAsync(body);
            var expected = path.Contains("change-password") ? "Thông tin xác thực không hợp lệ." : "Sai tài khoản hoặc mật khẩu.";
            Check(json.RootElement.GetProperty("title").GetString() == expected && json.RootElement.GetProperty("code").GetString() == "INVALID_CREDENTIALS", "Authentication message matches operation");
        }
        await app.StopAsync();
        Console.WriteLine($"PASS: {checks} login validation checks (DTO, Application and HTTP; no database).");
    }
}
