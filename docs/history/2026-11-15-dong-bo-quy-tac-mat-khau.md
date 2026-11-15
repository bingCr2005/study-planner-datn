# 2026-11-15 — Đồng bộ quy tắc mật khẩu đăng nhập và mật khẩu mới

## Yêu cầu và mục đích

- Người dùng yêu cầu rà soát quy tắc username/mật khẩu, bổ sung phần thiếu và giữ phần đã có. Quy tắc username 5–30 ký tự chỉ áp dụng nếu hệ thống dùng username; hiện sản phẩm dùng email/MSSV nên giữ mô hình này và giới hạn trường hiện có.
- Thay thế quy tắc chặn dấu nháy và tối thiểu 8 ký tự ở đăng nhập trong [bản ghi cũ](2026-11-15-kiem-tra-du-lieu-dang-nhap.md). Đã đọc mục lục lịch sử, bản ghi này, bản ghi lỗi đăng nhập, đặc tả và code FE/BE. Tiếp tục trên nhánh hiện tại cùng nhóm công việc; không commit/push.

## Code và logic thay đổi

- Backend `DangNhapRequest.cs`: bỏ MinLength và regex cấm nháy của mật khẩu đăng nhập, giữ Required và tối đa 128. Email nhận các ký tự hợp lệ như dấu cộng/gạch ngang/gạch dưới; MSSV nhận thêm gạch dưới. Giữ chặn dấu nháy/khoảng trắng trong tài khoản và trim tài khoản.
- Đăng ký và đặt mật khẩu mới đã có Required, tối thiểu 8, tối đa 128, không cấm ký tự đặc biệt; giữ nguyên DTO này. FE bổ sung từ chối mật khẩu mới chỉ gồm khoảng trắng để đồng nhất với BE/mock.
- FE `PasswordInput.tsx`/CSS dùng chung ở `AuthPage.tsx` và `ProfilePage.tsx`: mặc định ẩn, mỗi ô có nút Hiện/Ẩn riêng với aria-controls/aria-pressed; nút type=button, không submit. Đăng nhập bỏ minLength, đăng ký/đổi mật khẩu giữ minLength=8; xác nhận mật khẩu giữ kiểm tra khớp.
- FE `validation.ts`: đồng bộ quy tắc đăng nhập, giữ nguyên mật khẩu khi gửi. Lỗi theo trường, xác thực với CSDL/hash, xử lý tài khoản không tồn tại/bị khóa, JWT và thu hồi phiên sau đổi mật khẩu giữ nguyên.
- Luồng: nhập → FE kiểm tra bắt buộc/định dạng/độ dài → API kiểm tra DTO/Application → truy vấn email/MSSV và xác minh hash → nhận phiên hoặc thông báo chung sai tài khoản/mật khẩu. Không tự sửa dữ liệu hoặc hash đã lưu.
- `LoginValidationChecks.cs` kiểm tra DTO/Application/HTTP, biên mật khẩu mới 7/8/128/129, khoảng trắng, dấu nháy, MSSV gạch dưới và email có dấu cộng/gạch ngang. Kiểm tra độc lập DB. FE bổ sung test xác thực mật khẩu chứa nháy/khoảng trắng qua đăng ký/đăng nhập/đổi mật khẩu và kiểm tra nút hiện/ẩn trên trình duyệt.

## Kiểm chứng

- FE build TypeScript/Vite và lint file thay đổi đã đạt; Vite còn cảnh báo kích thước chunk có sẵn.
- FE `npm.cmd test`: 159/159 đạt. Các kiểm tra mật khẩu mới bao gồm trống/chỉ khoảng trắng, 7/8/128/129 ký tự, xác nhận không khớp và ký tự đặc biệt; kiểm tra mock xác nhận mật khẩu chứa dấu nháy/khoảng trắng được giữ nguyên qua đăng ký/đăng nhập/đổi mật khẩu.
- Chrome `npm.cmd run test:auth:browser -- --login-validation`: đạt; kiểm tra gạch dưới MSSV, dấu cộng/gạch ngang email, đăng nhập mật khẩu ngắn, dán dấu nháy, hiện/ẩn không đổi giá trị hoặc submit, lỗi theo trường và Enter. Lần chạy trong sandbox không kết nối CDP được, chạy ngoài sandbox đạt.
- Chrome `npm.cmd run test:auth:browser`: đạt cả lượt HTTP fixture (168 request) và mock adapter thật; kiểm tra nút Hiện/Ẩn độc lập ở đăng ký/đổi mật khẩu và các luồng tài khoản/quản trị/thu hồi phiên có sẵn.
- Backend đã cài bản cập nhật. `dotnet build backend/tests/StudyPlanner.AuthChecks/StudyPlanner.AuthChecks.csproj --configuration LoginValidation -p:NuGetAudit=false --no-restore`: đạt, 0 cảnh báo/lỗi; cấu hình riêng tránh ghi đè DLL Debug đang chạy.
- `dotnet backend/tests/StudyPlanner.AuthChecks/bin/LoginValidation/net10.0/StudyPlanner.AuthChecks.dll --login-validation-only`: 393 kiểm tra đạt ở DTO/Application/HTTP, không dùng CSDL. Đã đối chiếu code/diff FE/BE; `git diff --check` cả hai repo đạt.
- Chưa kiểm tra tài khoản/DB thật; kịch bản trình duyệt dùng HTTP fixture/mock. Không ghi dữ liệu người dùng.

## Việc tiếp theo

- API đang chạy cần khởi động lại để nạp DTO mới; không tự dừng tiến trình người dùng.
- Không thêm trường username, thay schema, reset mật khẩu hoặc thay hash tài khoản cũ.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
