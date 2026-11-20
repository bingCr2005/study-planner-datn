# 2026-11-18 — Thu gọn log API trong terminal backend

## Yêu cầu và mục đích

- Người dùng muốn terminal BE gọn, nhìn được GET/POST và API khi thao tác trên giao diện; đã yêu cầu sửa trực tiếp trên nhánh hiện tại, không tạo nhánh mới.
- Cấu hình thực tế trước khi sửa chỉ hạ `Microsoft.AspNetCore` xuống Warning. EF Core vẫn kế thừa Information nên log SQL từ request và worker có thể xuất liên tục. Chưa có dòng tóm tắt request riêng.
- Bản ghi liên quan: [kiến trúc backend](2026-10-13-ghi-chu-kien-truc-backend.md), [giải phóng cổng backend](2026-11-13-giai-phong-cong-backend-5059.md). Bản ghi kiến trúc mô tả khung ban đầu; code hiện tại đã có controller, authentication, worker và exception handler.

## Code và logic thay đổi

- [ApiRequestLoggingMiddleware.cs](../../backend/StudyPlanner.Api/Logging/ApiRequestLoggingMiddleware.cs): thêm middleware dùng logger tên `API`. Mỗi request có path bắt đầu bằng segment `/api` ghi một dòng `{Method} {Path} -> {StatusCode} | {ElapsedMs} ms` khi kết thúc. Thời gian đo bằng Stopwatch; số ms được làm tròn.
- [Program.cs](../../backend/StudyPlanner.Api/Program.cs): đặt middleware trước exception handler, authentication, authorization và rate limiter để ghi cả request bị từ chối và mã lỗi sau khi exception handler chuyển thành HTTP response. Exception thoát khỏi pipeline vẫn được ném tiếp, dòng tóm tắt ghi 500.
- [appsettings.json](../../backend/StudyPlanner.Api/appsettings.json): log console một dòng, có giờ `HH:mm:ss`; EF Core và Npgsql chỉ ghi Warning trở lên.
- [appsettings.Development.json](../../backend/StudyPlanner.Api/appsettings.Development.json): cũng hạ EF Core và Npgsql xuống Warning; giữ nguyên các cấu hình khác.
- Luồng: request HTTP → middleware bắt đầu đo → pipeline xử lý → đọc status cuối → Information với mã dưới 400, Warning với 4xx, Error với 5xx → ghi một dòng tóm tắt. Không ghi query string, header/token hoặc body.
- Log khởi động, cảnh báo/lỗi của framework và worker vẫn giữ; lỗi có thể có thêm log chẩn đoán ngoài dòng tóm tắt. Swagger không tạo dòng tóm tắt API. Không thay đổi nghiệp vụ hay tần suất gọi API: một lần bấm có thể gọi nhiều request, request tự làm mới trong nền cũng hiện.

## Kiểm chứng

- Đọc lại middleware, vị trí đăng ký trong Program, cấu hình Logging ở cả hai môi trường và diff các phần vừa sửa; đối chiếu với exception handler hiện tại.
- Build Release API bằng `dotnet build backend/StudyPlanner.Api/StudyPlanner.Api.csproj --no-restore -c Release -o backend/obj/api-log-build -p:CopyRetryCount=0 -m:1 -nr:false`: thành công, 0 cảnh báo, 0 lỗi. Output riêng tránh ghi đè BE đang chạy. Lần build mặc định ban đầu thất bại không báo chẩn đoán; chạy đơn luồng như trên đã qua, gồm lượt build sau chỉnh nhãn logger cuối cùng.
- Harness tạm trong `backend/obj/api-log-check` (được Git bỏ qua) dùng đúng source middleware, cấu hình Logging thực và server localhost cổng ngẫu nhiên; không đăng ký worker hoặc kết nối database.
- `dotnet run --project backend/obj/api-log-check/LogChecks.csproj --no-restore`: đạt sau lần sửa cuối. HTTP 200/201/400/401/404/500 trả đúng mã, mỗi API có đúng một dòng, đúng mức log và có thời gian; query mẫu không bị ghi; request Swagger không tạo dòng API. Cấu hình kế thừa Development có EF Warning và console SingleLine. Server thử được dừng trong finally.
- Lượt harness đầu sai đường dẫn tương đối do thư mục chạy của dotnet, đã sửa đường dẫn trong harness. Sandbox chặn kết nối socket localhost ở lượt sau; kiểm tra HTTP đã được cấp quyền chạy ngoài sandbox và qua.
- Chưa chạy toàn bộ test nghiệp vụ/database hoặc thử giao diện với tài khoản thật vì chỉ sửa log. Chưa thay thế tiến trình BE đang chạy trong terminal người dùng; cần khởi động lại để nạp code/cấu hình mới.

## Việc tiếp theo

- Khởi động lại BE trong terminal hiện tại rồi thao tác giao diện để xem log mới.
- Chưa commit/push. Các thay đổi khác đã có sẵn trên nhánh được giữ nguyên.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
