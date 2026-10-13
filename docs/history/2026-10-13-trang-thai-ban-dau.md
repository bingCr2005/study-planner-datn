# 2026-10-13 — Trạng thái ban đầu và quy trình lưu lịch sử

## Yêu cầu và mục đích

Tạo thư mục Markdown lưu những gì đã làm và logic code để có thể đọc lại. Khi AI sửa, cần đọc tài liệu và code để hiểu thay đổi trước khi đưa code lên repository.

Thay đổi lần này chỉ thêm `AGENTS.md` và tài liệu trong `docs/history/`, không thay đổi code ứng dụng.

## Trạng thái code đã đọc

- [Solution backend](../../backend/StudyPlanner.sln) gồm các project Api, Application, Domain và Infrastructure, dùng .NET 8 theo các file `.csproj`.
- [Program.cs](../../backend/StudyPlanner.Api/Program.cs): lấy connection string theo khóa `DefaultConnection`, đăng ký `ApplicationDbContext` qua `UseNpgsql`, đăng ký controllers và Swagger. Swagger được bật trong môi trường Development; pipeline có HTTPS redirection.
- API hiện có `GET /` trả thông báo đang chạy và chuỗi `Healthy`. Endpoint này không truy vấn database nên không xác minh kết nối database.
- `GET /weatherforecast` là endpoint mẫu, tạo 5 dự báo ngẫu nhiên, chưa có nghiệp vụ lập kế hoạch học tập.
- [ApplicationDbContext.cs](../../backend/StudyPlanner.Infrastructure/Data/ApplicationDbContext.cs) nhận options qua constructor, chưa khai báo `DbSet`.
- [InitialCreate](../../backend/StudyPlanner.Infrastructure/Migrations/20261012173012_InitialCreate.cs) có `Up` và `Down` rỗng; migration này chưa định nghĩa bảng nghiệp vụ. Chưa kiểm tra database thực tế hoặc việc migration đã được áp dụng.
- Trong `Program.cs`, đăng ký `AddEndpointsApiExplorer` và `AddSwaggerGen` đang lặp lại; có `AddControllers` nhưng chưa có `MapControllers`. Đây là ghi nhận để tham khảo khi phát triển tiếp, chưa sửa trong tác vụ này.

## Luồng hiện tại

Ứng dụng khởi động → đọc cấu hình → đăng ký DbContext PostgreSQL và các dịch vụ → dựng pipeline → ánh xạ hai endpoint mẫu → chạy server. Hai endpoint hiện tại không sử dụng DbContext.

## Kiểm chứng và giới hạn

- Đã đọc cấu trúc repository, lịch sử commit gần nhất, file project, `Program.cs`, `ApplicationDbContext.cs` và migration ban đầu.
- Git trước tác vụ không báo thay đổi trong working tree.
- Chưa chạy build, API, kiểm thử hoặc kết nối database; lần này chỉ bổ sung tài liệu.
- Nội dung này là ảnh chụp trạng thái code đã đọc, không khẳng định tính năng đã hoạt động trên môi trường chạy.

## Việc tiếp theo

Mỗi lần sửa code, đọc lại bản ghi liên quan và code hiện tại; thêm bản ghi mới theo mẫu, giải thích logic trước/sau và ghi kết quả kiểm tra thực tế. Người dùng đọc bản ghi cùng code/diff để hiểu thay đổi trước khi đưa code lên.

## Đọc lại trước khi đưa code lên

- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.
