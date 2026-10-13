# 2026-10-13 — Đồng bộ backend .NET 10 và SDK 10.0.400

## Yêu cầu và mục đích

- Người dùng yêu cầu sửa trực tiếp toàn bộ dự án cho đồng bộ SDK 10.0.400. Yêu cầu này cho phép chỉnh file trong tác vụ nâng cấp, khác với các lượt hướng dẫn người dùng tự gõ trước đó.
- Nâng cả bốn tầng và dependency tương thích, giữ code người dùng đã viết cho Swagger và HealthController.
- Liên quan: [trạng thái ban đầu](2026-10-13-trang-thai-ban-dau.md), [kiến trúc](2026-10-13-ghi-chu-kien-truc-backend.md), [kế hoạch/API](2026-10-13-ke-hoach-chi-tiet-api-swagger.md).

## Code và logic thay đổi

- [global.json](../../global.json): thêm SDK 10.0.400, rollForward = disable, không dùng prerelease. Máy khác phải cài đúng SDK; lệnh dotnet chạy từ repository hoặc backend đều tìm cấu hình này. Target framework là net10.0, không phải net10.0.400.
- [API project](../../backend/StudyPlanner.Api/StudyPlanner.Api.csproj), [Application project](../../backend/StudyPlanner.Application/StudyPlanner.Application.csproj), [Domain project](../../backend/StudyPlanner.Domain/StudyPlanner.Domain.csproj), [Infrastructure project](../../backend/StudyPlanner.Infrastructure/StudyPlanner.Infrastructure.csproj): net8.0 → net10.0.
- EF Core Design 8.* → 10.0.12; Npgsql EF provider ở API/Infrastructure → 10.0.3; Swashbuckle 6.6.2 → 10.3.0. Bỏ phiên bản nổi 8.*.
- Infrastructure thêm EF Core Relational 10.0.12 để EF Core runtime/Relational đồng nhất với Design 10.0.12 trong cả hai graph, không chỉ API được nâng patch bởi dependency của Design.
- [Program.cs](../../backend/StudyPlanner.Api/Program.cs): namespace Microsoft.OpenApi.Models → Microsoft.OpenApi theo model mới; bỏ WithOpenApi đã deprecated. Bỏ package Microsoft.AspNetCore.OpenApi 8 vì không còn dùng extension này; Swashbuckle vẫn sinh schema từ ApiExplorer/metadata.
- Luồng: SDK đã cố định → restore dependency 10.x → build bốn project net10.0 → API đăng ký DbContext/Swagger/controllers → MapControllers và endpoint mẫu → trả HTTP/Swagger.
- Trước tác vụ, người dùng đã cấu hình tiêu đề Swagger, gỡ đăng ký lặp, thêm MapControllers và [HealthController](../../backend/StudyPlanner.Api/Controllers/HealthController.cs). Đây là thay đổi có sẵn trong working tree, không nhận là do tác vụ nâng cấp tạo ra.
- Không sửa migration cũ/snapshot có ProductVersion 8.0.31: đó là metadata lịch sử lúc sinh migration, không phải dependency đang chạy. Không áp dụng migration, sửa database hoặc thay đổi config bí mật.
- Cập nhật hiện trạng trong [kiến trúc](../KIEN-TRUC-BACKEND.md), [kế hoạch tổng quan](../KE-HOACH-PHAT-TRIEN.md), [kế hoạch chi tiết](../KE-HOACH-TRIEN-KHAI-CHI-TIET.md), [thiết kế API](../THIET-KE-API-SWAGGER.md) và [mục lục module](../modules/README.md); giữ nguyên bản ghi lịch sử cũ.

## Kiểm chứng

- Đã đọc code/project/history liên quan và diff Program.cs có sẵn trước sửa; xác minh SDK 10.0.400 đã cài, dependency stable từ NuGet và [hướng dẫn Swashbuckle v10](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/migrating-to-v10.md).
- `dotnet --version`: 10.0.400 từ workspace sau khi thêm global.json.
- `dotnet restore backend/StudyPlanner.sln`: thành công ngoài sandbox. Lượt trong sandbox bị từ chối đọc cấu hình NuGet của user; không phải lỗi dependency.
- `dotnet build backend/StudyPlanner.sln --no-restore`: thành công, cả bốn project net10.0, 0 cảnh báo, 0 lỗi.
- Đọc project.assets.json: API/Infrastructure cùng EF Core và Relational 10.0.12, Npgsql provider 10.0.3; API có Design 10.0.12, Swashbuckle 10.3.0, Microsoft.OpenApi 2.7.5.
- Chạy API tại loopback cổng thử 5187, Development, không dùng launch profile. GET /health/live trả 200, status Healthy, có checkedAt; GET /swagger/index.html trả 200 và HTML Swagger UI; GET /swagger/v1/swagger.json trả tài liệu OpenAPI 3.0.4, đúng title, health GET và schema LivenessResponse.
- GET /weatherforecast trả 200, 5 bản ghi và vẫn có trong OpenAPI sau bỏ WithOpenApi; GET / trả 200. Lượt kiểm tra đếm bằng PowerShell pipeline ban đầu báo sai, kiểm tra lại biến JSON đã parse xác nhận Count = 5; không có regression endpoint.
- Lượt chạy trong sandbox gặp quyền Windows Event Log; đã dừng và chạy lại ngoài sandbox, HTTP thành công. Chạy HTTP-only không cấu hình cổng HTTPS gây warning không xác định cổng redirect; đây không phải kiểm chứng HTTPS. Các tiến trình API kiểm thử đã được dừng.
- Chưa kiểm tra database hoặc áp dụng migration: các endpoint thử không dùng DbContext. Chưa render Swagger trong trình duyệt hoặc thử nút Execute, chỉ kiểm tra HTTP HTML/JSON. Chưa có project test nghiệp vụ để chạy.

## Việc tiếp theo

- Người dùng mở terminal mới ở backend, chạy dotnet --version/build/run để tiếp tục học bằng SDK đã cố định.
- Hoàn thiện /health/ready và ProblemDetails theo giai đoạn 1; nâng cấp framework chưa đồng nghĩa hoàn thành toàn bộ khung API.
- Khi triển khai schema: dùng công cụ dotnet-ef 10.x tương thích, sinh migration mới sau xác minh môi trường, không sửa migration cũ chỉ để đổi số phiên bản.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu code/dependency và kết quả build/HTTP, ghi đúng giới hạn kiểm chứng.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo; không commit/push/triển khai.
