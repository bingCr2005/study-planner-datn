# 2026-11-15 — Kiểm tra dữ liệu đăng nhập ở frontend và backend

## Yêu cầu và mục đích

- Người dùng yêu cầu giới hạn ký tự tên đăng nhập, mật khẩu tối thiểu 8 ký tự và cả hai ô không được chứa dấu nháy. Sau khi làm rõ email có dấu chấm, người dùng chọn tên đăng nhập nhận chữ, số, @ và dấu chấm.
- Người dùng xác nhận sửa trên nhánh hiện tại `feat/quan-ly-du-lieu-nen-tang` sau khi lệnh tạo nhánh riêng bị từ chối. Không tạo nhánh hoặc commit/push.
- Đã đọc mục lục lịch sử, bản ghi [điều hướng đăng nhập](2026-11-12-trang-mac-dinh-admin.md), [khóa tài khoản](2026-11-12-thoat-phien-khi-khoa.md), đặc tả tài khoản, code FE/BE và kiểm thử hiện tại. Hai repo sạch trước thay đổi này.
- Trước: backend chỉ bắt buộc hai trường, giới hạn tài khoản 320 và mật khẩu 128 ký tự. Frontend tự báo khi thiếu tài khoản, còn mật khẩu dựa vào kiểm tra HTML; chưa có kiểm tra ký tự tên đăng nhập và dấu nháy.

## Code và logic thay đổi

- Backend [DangNhapRequest.cs](../../backend/StudyPlanner.Application/Auth/DangNhapRequest.cs): giữ Required/StringLength, thêm tối thiểu 8 ký tự cho mật khẩu và thông báo tiếng Việt. RegularExpression từ chối nháy đơn, kép, cong và backtick trong mật khẩu. IValidatableObject kiểm tra tài khoản sau Trim, chỉ nhận Unicode chữ/số, @ và dấu chấm; báo riêng khi có dấu nháy. Tài khoản vẫn tối đa 320 ký tự, mật khẩu tối đa 128.
- XacThucService.DangNhapAsync đã gọi QuyTacTaiNguyen.Validate ngay đầu hàm; quy tắc mới có hiệu lực cả khi gọi Application trực tiếp. HTTP tự kiểm tra DTO, trả 400/VALIDATION_ERROR với lỗi theo trường, trước khi truy vấn tài khoản hoặc kiểm tra hash. Không thay đổi service, cơ chế hash, JWT, schema hoặc dữ liệu thật.
- FE repo `D:/study-planner-frontend`, `src/auth/validation.ts`: thêm loginErrors trả lỗi độc lập cho hai ô. Bỏ khoảng trắng đầu/cuối tài khoản như trước; chặn khoảng trắng ở giữa, dấu gạch ngang/gạch dưới và các ký tự ngoài nhóm đã chọn. Mật khẩu được giữ nguyên, chỉ kiểm tra bắt buộc, độ dài và dấu nháy; vẫn nhận ký tự đặc biệt khác.
- FE `src/pages/Auth/AuthPage.tsx`: kiểm tra khi submit, báo lỗi dưới từng ô, nối aria-invalid/aria-describedby, focus ô lỗi đầu tiên và không gửi API khi có lỗi. Sửa một ô xóa lỗi của ô đó. Đây là từ chối dữ liệu khi gửi, không tự xóa hoặc thay ký tự người dùng đang gõ/dán. Lỗi API vẫn hiển thị thông báo chung như trước. `src/student-simple.css` thêm màu lỗi và viền ô lỗi trong trang đăng nhập.
- FE `src/contexts/AuthProvider.tsx` và `src/api/auth.ts`: kiểm tra lại trước đăng nhập demo và trước gọi API, để các nơi gọi trực tiếp cũng tuân thủ quy tắc. Luồng xác minh hồ sơ, nhận vai trò và điều hướng giữ nguyên.
- FE `src/auth/demo.ts`, `src/api/mockAuth.ts`: cập nhật độ dài mật khẩu mẫu cho demo và các tài khoản mock tạo mới. Không đổi hash của tài khoản đã lưu trong PostgreSQL hoặc localStorage, không reset dữ liệu mock cũ.
- Kiểm thử BE: thêm [LoginValidationChecks.cs](../../backend/tests/StudyPlanner.AuthChecks/LoginValidationChecks.cs), thêm chế độ `--login-validation-only` trong Program.cs. Chế độ này dùng validation DTO thật, service với dependency cố ý không cung cấp để xác nhận dữ liệu lỗi bị chặn trước truy cập store, và HTTP controller thật trên cổng loopback ngẫu nhiên. Cấu hình HTTP trong bộ kiểm tra không dùng cấu hình DB ứng dụng. Cập nhật một mật khẩu sai trong kịch bản AuthChecks cũ thành chuỗi đủ dài để tiếp tục kiểm tra 401 thay vì bị chặn độ dài 400.
- Kiểm thử FE: thêm `tests/login-validation.test.mjs`, `tests/browser-login-validation.mjs`; thêm cờ `--login-validation` trong `tests/browser-auth.mjs`. Các fixture đăng nhập trong auth/admin-users/mock-auth/overview-mock/priority-mock/browser-auth/backend-live cập nhật độ dài mật khẩu mẫu; email fixture dùng để đăng nhập bỏ dấu gạch ngang. Không nới lỏng quy tắc sản phẩm để giữ fixture cũ.
- FE README bổ sung quy tắc, cách kiểm tra và ảnh hưởng tài khoản cũ; bỏ giá trị mật khẩu mẫu khỏi bảng tài liệu. `docs/modules/01-tai-khoan-va-ho-so.md` trước đó ghi chưa có code, chỉ đăng nhập email và lưu token trong bộ nhớ; cập nhật mục đăng nhập theo code hiện tại, đánh dấu phần đặc tả còn lại là dự thảo cũ.
- Luồng: nhập → kiểm tra từng trường trên FE → có lỗi thì hiện thông báo/focus và dừng → hợp lệ thì gọi POST /api/v1/auth/login → BE kiểm tra DTO/Application → đối chiếu tài khoản và hash → FE xác minh hồ sơ → lưu phiên/điều hướng theo vai trò.

## Kiểm chứng

- BE: `dotnet build backend/tests/StudyPlanner.AuthChecks/StudyPlanner.AuthChecks.csproj --configuration LoginValidation -p:NuGetAudit=false --no-restore` đạt, 0 warnings/errors. Build dùng cấu hình riêng để không ghi đè DLL Debug đang chạy.
- BE: `dotnet backend/tests/StudyPlanner.AuthChecks/bin/LoginValidation/net10.0/StudyPlanner.AuthChecks.dll --login-validation-only` đạt 291 kiểm tra DTO/Application/HTTP: null/trống/chỉ khoảng trắng, ký tự cấm, các loại nháy, độ dài biên 7/8/128/129 và 320/321, email có dấu chấm, MSSV giữ số 0 đầu, chữ có dấu, mật khẩu có ký tự đặc biệt khác; không kết nối DB. Lần đầu helper HTTP xóa hết configuration provider nên không đặt được URL; thêm provider trong bộ nhớ rồi chạy lại đạt.
- FE build TypeScript/Vite và lint đạt. Cảnh báo chunk trên 500 kB vẫn có như trước.
- FE `npm.cmd test`: 155/155 đạt. Lần đầu 4 fixture admin dùng email có gạch ngang bị từ chối đúng quy tắc mới; sửa fixture rồi chạy lại toàn bộ đạt.
- Chrome `npm.cmd run test:auth:browser -- --login-validation`: đạt; xác nhận lỗi dưới ô, aria-invalid/focus, không tự lọc dữ liệu, thao tác dán có dấu nháy bị từ chối, không gửi request lỗi hoặc lưu phiên, email hợp lệ được trim/chuẩn hóa và gửi bằng Enter. Lần đầu helper Enter thiếu ký tự carriage return nên chưa submit; sửa thao tác mô phỏng và chạy lại đạt.
- Chrome `npm.cmd run test:auth:browser`: đạt cả lượt API fixture (170 request được chặn trong test) và mock adapter thực tế; bao gồm đăng ký/đăng nhập email/MSSV, demo sinh viên/admin, điều hướng, sửa hồ sơ, đổi mật khẩu, hết hạn và quản trị tài khoản. Không dùng tài khoản hoặc DB thật.
- Đã đọc lại diff BE/FE, code xác thực và các file kiểm thử mới; git diff --check đạt cả hai repo.
- Chưa chạy lại toàn bộ bộ BE AuthChecks có PostgreSQL hoặc đăng nhập vào ứng dụng thật; kiểm chứng BE lần này tập trung validation độc lập DB và HTTP thật. Chưa chạy backend-live vì cần API/tài khoản thật.

## Việc tiếp theo

- Người dùng cần khởi động lại backend đang chạy để nạp DTO mới. Không tự dừng/khởi động lại tiến trình của người dùng.
- Tài khoản cũ có mật khẩu dưới 8 ký tự/chứa dấu nháy, hoặc email/MSSV chứa ký tự đã cấm, sẽ bị từ chối khi đăng nhập. Không tự đổi dữ liệu hoặc mật khẩu tài khoản.
- Phạm vi là đăng nhập. Đăng ký/đổi mật khẩu vẫn giữ bộ quy tắc cũ, nên hiện còn khả năng tạo email hoặc mật khẩu mà quy tắc đăng nhập mới không chấp nhận; cần thống nhất các luồng nếu người dùng yêu cầu mở rộng. Dữ liệu mock đã lưu vẫn giữ hash cũ.
- Chưa commit/push hoặc triển khai.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
