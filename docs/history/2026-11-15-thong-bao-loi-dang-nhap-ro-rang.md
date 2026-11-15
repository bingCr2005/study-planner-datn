# 2026-11-15 — Thông báo lỗi đăng nhập rõ ràng theo trường

## Yêu cầu và mục đích

- Người dùng yêu cầu sai tài khoản/mật khẩu phải báo rõ Sai tài khoản hoặc mật khẩu; dữ liệu vi phạm quy tắc phải nói rõ trường và lý do, ví dụ không được nhập ký tự đặc biệt.
- Đã đọc mục lục, [bản ghi kiểm tra dữ liệu đăng nhập](2026-11-15-kiem-tra-du-lieu-dang-nhap.md), mẫu lịch sử, DTO/service/exception handler, validation/form/API/demo/mock và các kiểm thử liên quan.
- Tiếp tục cùng nhóm công việc và nhánh hiện tại theo xác nhận trước của người dùng. Giữ các thay đổi chưa commit từ lượt trước.
- Đối chiếu thực tế: backend trả Thông tin xác thực không hợp lệ khi sai thông tin đăng nhập, frontend hiển thị nguyên văn; lỗi cục bộ chỉ hiện sau submit. Backend IValidatableObject không chạy khi thuộc tính mật khẩu đã vi phạm annotation, nên request có cả hai ô sai chưa báo đủ hai trường.

## Code và logic thay đổi

- [QuanTriExceptionHandler.cs](../../backend/StudyPlanner.Api/Administration/QuanTriExceptionHandler.cs): XacThucException ở POST /api/v1/auth/login trả 401/INVALID_CREDENTIALS với thông báo Sai tài khoản hoặc mật khẩu. So đường dẫn không phân biệt hoa/thường, nhận dấu / cuối. Thông báo của các thao tác xác thực khác giữ hành vi hiện có.
- [DangNhapRequest.cs](../../backend/StudyPlanner.Application/Auth/DangNhapRequest.cs): chuyển kiểm tra tài khoản từ IValidatableObject sang attribute trên thuộc tính, để tài khoản và mật khẩu đều được kiểm tra dù ô kia có lỗi. Phân biệt dấu nháy, khoảng trắng giữa tài khoản và ký tự đặc biệt ngoài @/dấu chấm. Tách thông báo mật khẩu quá ngắn và quá dài; bộ ký tự, độ dài và cách trim giữ như quy tắc đã chốt.
- FE `D:/study-planner-frontend/src/auth/validation.ts`: đồng bộ nội dung lỗi, bổ sung serverLoginErrors ánh xạ TaiKhoan/MatKhau hoặc dạng camelCase sang đúng trường trên form; bỏ qua trường không nhận diện và lấy thông báo không rỗng đầu tiên.
- FE `src/pages/Auth/AuthPage.tsx`: kiểm tra thêm khi rời ô đăng nhập; lỗi hiện dưới ô trước khi submit. Lỗi validation từ API được đưa vào từng ô thay vì chỉ hiện thông báo chung. Khi submit vẫn kiểm tra cả hai ô, focus ô lỗi đầu và không gửi dữ liệu lỗi. Không tự lọc ký tự gõ/dán.
- FE `src/api/auth.ts`: chuẩn hóa lỗi 401 từ riêng request POST đăng nhập thành Sai tài khoản hoặc mật khẩu để cả backend đang chạy bản cũ cũng có nội dung dễ hiểu. Giữ status/code/errors, không đổi lỗi ACCOUNT_DISABLED hoặc lỗi của GET hồ sơ sau khi nhận token. Lỗi mạng, server, quyền và giới hạn tần suất tiếp tục có nội dung riêng.
- FE `src/auth/demo.ts`, `src/api/mockAuth.ts`: dùng cùng thông báo sai thông tin đăng nhập cho demo/mock. Không tách câu trả lời theo tài khoản có tồn tại hay mật khẩu có đúng; kết quả xác thực sai dùng một câu chung theo yêu cầu.
- Kiểm thử FE `tests/auth.test.mjs`, `tests/login-validation.test.mjs`, `tests/browser-login-validation.mjs`, `tests/browser-auth.mjs`: kiểm tra thông báo chính xác, lỗi API theo trường, blur, đăng nhập sai tài khoản/sai mật khẩu, giữ lỗi mạng/khóa/hết phiên riêng; fixture HTTP cũ vẫn có thông báo cũ để xác minh frontend chuẩn hóa đúng. Nhánh fixture báo lỗi theo trường chỉ bật cho cờ --login-validation.
- [LoginValidationChecks.cs](../../backend/tests/StudyPlanner.AuthChecks/LoginValidationChecks.cs): thêm request HTTP sai đồng thời hai ô, xác nhận nội dung lỗi, và kiểm tra exception handler trả thông báo theo thao tác, kể cả URL hoa/thường hoặc có / cuối.
- [Đặc tả module tài khoản](../modules/01-tai-khoan-va-ho-so.md) cập nhật thời điểm báo lỗi và nội dung thông báo theo code hiện tại. Bản ghi trước giữ lại mô tả IValidatableObject như lịch sử trước lần sửa này.
- Luồng: nhập/rời ô → kiểm tra và báo lỗi tương ứng → submit hợp lệ → API kiểm tra lại → lỗi 400 đưa về từng trường; sai xác thực 401 hiện Sai tài khoản hoặc mật khẩu; thành công tiếp tục xác minh hồ sơ và điều hướng như trước.

## Kiểm chứng

- Backend build AuthChecks cấu hình LoginValidation, --no-restore: đạt, 0 warnings/errors; không ghi đè DLL Debug đang chạy.
- `dotnet backend/tests/StudyPlanner.AuthChecks/bin/LoginValidation/net10.0/StudyPlanner.AuthChecks.dll --login-validation-only`: 303 kiểm tra DTO/Application/HTTP/exception handler đạt; không kết nối database.
- FE TypeScript/Vite build và lint các file thay đổi đạt. Cảnh báo chunk trên 500 kB vẫn có như trước.
- FE `npm.cmd test`: 157/157 đạt. Có cảnh báo cổng WebSocket của một helper Vite đang được dùng; toàn bộ test vẫn kết thúc thành công.
- Chrome `npm.cmd run test:auth:browser -- --login-validation`: đạt; kiểm tra blur, hai ô cùng lỗi, hai kiểu sai xác thực, lỗi server theo trường, dấu nháy/dán, không gửi dữ liệu lỗi và email hợp lệ gửi bằng Enter.
- Chrome `npm.cmd run test:auth:browser`: đạt cả fixture API (168 request trong test) và mock adapter, bao gồm đăng nhập/đăng ký, demo admin/sinh viên, hồ sơ/đổi mật khẩu và quản trị tài khoản.
- Đã đọc lại diff, DTO, handler, form và API; git diff --check đạt cả hai repo. Không đổi schema, dữ liệu tài khoản thật hoặc cơ chế cấp phiên.
- Chưa đăng nhập tài khoản thật hoặc chạy lại toàn bộ kiểm thử backend có PostgreSQL; bộ kiểm tra lần này dùng HTTP và fixture độc lập database.

## Việc tiếp theo

- Khởi động lại backend đang chạy để nạp thông báo/validation mới; chưa tự dừng hoặc khởi động lại tiến trình của người dùng.
- Các giới hạn tài khoản cũ và việc thống nhất chính sách với đăng ký/đổi mật khẩu vẫn như bản ghi trước. Không tự đổi mật khẩu hoặc dữ liệu đã lưu.
- Chưa commit/push hoặc triển khai.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
