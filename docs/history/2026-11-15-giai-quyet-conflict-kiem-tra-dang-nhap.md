# 2026-11-15 — Giải quyết conflict phần kiểm tra đăng nhập với main

## Yêu cầu và mục đích

- Người dùng vừa push phần kiểm tra nhập liệu đăng nhập và yêu cầu sửa hết conflict ở cả frontend/backend. Giữ nhánh `feat/quan-ly-du-lieu-nen-tang`; không tạo commit hoặc push khi chưa được yêu cầu.
- Đã đọc AGENTS, mục lục lịch sử, [bản ghi mật khẩu](2026-11-15-dong-bo-quy-tac-mat-khau.md), [lần gộp main trước](2026-11-15-dong-bo-main-be-fe.md), code và diff hai nhánh. Hai working tree sạch trước khi bắt đầu.
- Sau fetch: frontend HEAD `837c854`, main `85a774b`; backend HEAD `bcb550e`, main `463d54f`. Main đã chứa phần danh mục trước đó và bổ sung đồng hồ, lập kế hoạch, tiến độ cây, thông báo. Mô phỏng merge xác định hai conflict frontend và một conflict tài liệu backend.

## Code và logic thay đổi

- Dùng `git merge --no-commit --no-ff origin/main` trong cả hai repo, giữ nguyên lịch sử đã push và dừng trước commit. Git tự gộp các thay đổi không chồng lấn; không chọn toàn bộ một phía.
- Frontend `src/components/Icon.tsx`: hợp kiểu IconName và các SVG của hai phía. Giữ `eye`/`eye-off` phục vụ hiện/ẩn mật khẩu và `dots`/`task-group`/`link`/`check-square`/`undo` phục vụ nhiệm vụ mới từ main.
- Frontend `src/pages/Auth/AuthPage.tsx`: giữ PasswordInput, lỗi từng trường, aria-invalid/aria-describedby, kiểm tra khi blur/submit, focus ô lỗi đầu và ánh xạ lỗi server; giữ thay đổi placeholder đăng ký rỗng của main. Không bỏ nút con mắt hoặc giới hạn mật khẩu mới khi gộp.
- Frontend `validation.ts`, `api/auth.ts`, `AuthProvider.tsx`, PasswordInput/CSS, hồ sơ và kiểm thử của nhánh đăng nhập được giữ. Thông báo tài khoản ngắn vẫn là “Vui lòng nhập email đúng định dạng hoặc MSSV.” theo lần chỉnh giao diện mới nhất.
- Backend `docs/history/README.md`: hợp đủ ba mục về nhập liệu/thông báo lỗi/mật khẩu của nhánh hiện tại với mục đồng bộ BE/FE của main; thêm bản ghi này. Code đăng nhập backend không có conflict; Program.cs của AuthChecks tự gộp và vẫn giữ chế độ `--login-validation-only` cùng các thay đổi mới từ main.
- Luồng sau gộp: nhập thông tin → kiểm tra cục bộ và hiển thị lỗi → backend kiểm tra DTO/Application → đối chiếu tài khoản/hash → trả phiên hoặc thông báo xác thực. Các tính năng mới từ main tiếp tục nằm trong kết quả merge.
- Đã đánh dấu các file conflict được giải quyết bằng git add. Không đổi schema, chạy migration hoặc ghi vào database người dùng.

## Kiểm chứng

- FE `npm.cmd run build` đạt; Vite còn cảnh báo kích thước chunk trên 500 kB. Lint Icon/AuthPage/validation/PasswordInput đạt.
- FE `npm.cmd test`: 175/175 đạt, gồm các kiểm tra mới từ main và nhánh đăng nhập.
- Chrome `npm.cmd run test:auth:browser -- --login-validation`: đạt, gồm lỗi nhập liệu, MSSV gạch dưới, email có ký tự hợp lệ, mật khẩu gửi nguyên văn, nút con mắt và submit bằng Enter.
- Chrome `npm.cmd run test:auth:browser`: đạt lượt API fixture với 216 request và lượt mock adapter thật, gồm đăng ký/đăng nhập, hồ sơ, đổi mật khẩu, điều hướng quản trị và thu hồi phiên. Không dùng tài khoản/CSDL thật.
- BE solution build Release đạt 0 cảnh báo/0 lỗi, output và restore artifacts riêng trong workspace frontend để không ghi đè DLL API đang chạy. Lệnh thử đầu dùng cấu hình LoginValidation ở cấp solution bị MSB4126 vì solution chỉ khai báo Debug/Release; đã đổi sang Release và chạy lại đạt.
- BE `StudyPlanner.AuthChecks.dll --login-validation-only`: 393 kiểm tra DTO/Application/HTTP đạt, không kết nối DB.
- BE SchemaChecks đạt 22 bảng/34 khóa ngoại, mapping/constraint/migration SQL; không mở kết nối DB.
- Đã đọc lại diff so với cả HEAD và origin/main; không còn file unmerged hoặc marker conflict trong source/test/tài liệu. Kiểm tra whitespace đạt; FE dùng `core.whitespace=cr-at-eol` để nhận đúng CRLF đã có trên main.
- Chưa chạy các bộ integration cần PostgreSQL hoặc thử trên tài khoản thật; phạm vi xác nhận lần này gồm build, unit, browser fixture/mock, validation HTTP độc lập DB và schema.

## Việc tiếp theo

- Cả hai repo còn trạng thái merge chờ commit, mọi conflict đã giải quyết và nội dung đã stage. Chưa commit, push hoặc merge PR vào main. GitHub chỉ nhận kết quả giải quyết sau khi tạo merge commit và push nhánh tương ứng.
- Các file mới từ main có thể yêu cầu khởi động lại API Development và áp dụng migration theo quy trình riêng; lượt sửa conflict này không tự thao tác database hoặc dừng API người dùng.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo commit cho lượt giải quyết này.
