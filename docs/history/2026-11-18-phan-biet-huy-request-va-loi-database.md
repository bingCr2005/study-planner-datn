# 2026-11-18 — Phân biệt hủy request và lỗi database

## Yêu cầu và mục đích

- Người dùng gửi log API 500/499 xen kẽ 200 và yêu cầu xử lý sau khi xác định không phải trùng cổng. Tiếp tục sửa trực tiếp trên nhánh hiện tại theo yêu cầu trong phiên.
- Log có lỗi Npgsql bọc SocketException trong InvalidOperationException và truy vấn xác thực bị hủy tại `XacThucRegistration.cs`. Không đủ bằng chứng kết luận database hoặc DNS mất kết nối kéo dài.
- Bản ghi trước: [thu gọn log API](2026-11-18-thu-gon-log-api.md). Log một dòng vẫn đúng định dạng, nhưng phân loại lỗi và xử lý hủy request còn thiếu.

## Code và logic thay đổi

- [RequestFailure.cs](../../backend/StudyPlanner.Api/Logging/RequestFailure.cs): duyệt chuỗi InnerException để nhận diện lỗi kết nối Npgsql, bỏ qua PostgresException (lỗi SQL). Chỉ coi OperationCanceledException, IOException hoặc lỗi kết nối Npgsql là hủy phía client khi `RequestAborted` đã được kích hoạt.
- [QuanTriExceptionHandler.cs](../../backend/StudyPlanner.Api/Administration/QuanTriExceptionHandler.cs): trước đây chỉ xét lớp ngoài và GetBaseException nên bỏ sót Npgsql ở giữa wrapper EF và SocketException. Sau sửa, lỗi kết nối bọc nhiều lớp trả 503/SERVICE_UNAVAILABLE. Hủy request hợp lệ đặt 499, không cố ghi JSON vào kết nối đã ngắt. Lỗi ứng dụng và timeout khi client chưa hủy vẫn là lỗi; không tự biến thành 499.
- [XacThucRegistration.cs](../../backend/StudyPlanner.Api/Auth/XacThucRegistration.cs): bắt lỗi do hủy truy vấn xác thực khi client ngắt, gọi NoResult thay vì ném lỗi lên JwtBearerHandler. Challenge không ghi body khi RequestAborted. Không thay đổi kiểm tra tài khoản bị khóa, token stamp, role hiện tại hoặc cho phép request vượt xác thực.
- [ApiRequestLoggingMiddleware.cs](../../backend/StudyPlanner.Api/Logging/ApiRequestLoggingMiddleware.cs): hủy request hợp lệ ghi 499 ở mức Information; xử lý cả lỗi hủy thoát khỏi exception handler. Request hoàn tất sau khi client ngắt cũng ghi 499. Lỗi không thuộc nhóm hủy vẫn được ném tiếp và ghi Error/500, kể cả khi token hủy đã kích hoạt. Các 4xx khác vẫn Warning và 5xx vẫn Error.
- [RequestFailureChecks.cs](../../backend/tests/StudyPlanner.AuthChecks/RequestFailureChecks.cs), [Program.cs](../../backend/tests/StudyPlanner.AuthChecks/Program.cs): thêm chế độ `--request-failures-only`, chạy độc lập DB/cổng bằng HttpContext và callback JWT thực; interceptor luôn chặn trước khi mở kết nối.
- Luồng: client hủy → token RequestAborted kích hoạt → truy vấn bị hủy → xác thực trả NoResult/không ghi challenge → middleware ghi 499 Information. Nếu client còn kết nối và PostgreSQL không truy cập được → lỗi đi tới handler → JSON 503. Không thêm retry tự động cho request ghi dữ liệu.
- Đã đọc `Study_Planner_FE/src/hooks/useNotifications.ts` và `usePlanningReminders.ts`: effect cleanup hủy request cũ; refresh theo focus, visibility, sự kiện saved/refresh và chu kỳ 60 giây có thể tạo request thay thế. Chưa tái hiện để xác nhận FE có vòng lặp gọi API; không sửa FE hoặc tắt hủy request.

## Kiểm chứng

- Build Release AuthChecks và API phụ thuộc với output riêng: `dotnet build backend/tests/StudyPlanner.AuthChecks/StudyPlanner.AuthChecks.csproj --no-restore -c Release -o backend/obj/request-failure-build -m:1 -nr:false -p:CopyRetryCount=0` đạt 0 cảnh báo, 0 lỗi.
- `dotnet backend/obj/request-failure-build/StudyPlanner.AuthChecks.dll --request-failures-only`: đạt 20 kiểm tra sau lần sửa cuối. Bao gồm Npgsql bọc SocketException, lỗi SQL không bị coi là mất kết nối, mã/code 503 và 500, request hủy không ghi body, 499 Information, lỗi ứng dụng không bị nuốt, mã 500 đã xử lý không bị ghi nhầm thành 499, callback JWT thật khi hủy và khi kết nối lỗi với client còn hoạt động.
- `dotnet backend/obj/request-failure-build/StudyPlanner.AuthChecks.dll --login-validation-only`: đạt 393 kiểm tra DTO/Application/HTTP, không dùng database. Server HTTP thử dùng cổng ngẫu nhiên và đã dừng; chạy ngoài sandbox với quyền được cấp do giới hạn socket localhost.
- Đọc lại diff và code liên quan; `git diff --check` đạt. Không ghi đè các thay đổi có sẵn của các tác vụ khác.
- Chưa kiểm tra database Development thật, trình duyệt thật hoặc nguyên nhân mạng/DNS tại thời điểm log. Bản sửa phân loại và xử lý lỗi/hủy request; không khẳng định đã khắc phục sự cố mạng. Stack trace lỗi thật vẫn được giữ để chẩn đoán.

## Việc tiếp theo

- Khởi động lại BE trong terminal người dùng để nạp code. Không chạy API nền giữ cổng 5059 trong tác vụ này.
- Nếu sau sửa vẫn có 503 hoặc 499 dày dù đứng yên ở một trang, cần tái hiện trên giao diện và đối chiếu request initiator/cancellation với kết nối database thực.
- Chưa commit/push.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
