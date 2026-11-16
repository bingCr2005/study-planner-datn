# 2026-11-16 — Giải quyết xung đột khi gộp tệp đính kèm vào main

## Yêu cầu và mục đích

- Người dùng chạy `git merge feat/don-bang-cu-va-tep-dinh-kem` trên `main`. Git dừng vì hai nhánh cùng thêm mục vào cuối mục lục lịch sử.
- Điểm gộp là `main` ở `9340de2` và nhánh tệp đính kèm ở `1b8d052`. Bản ghi liên quan: [kiểm tra đăng nhập](2026-11-15-giai-quyet-conflict-kiem-tra-dang-nhap.md) và [dọn bảng cũ, tệp đính kèm](2026-11-15-don-bang-cu-va-tep-dinh-kem.md).

## Code và logic thay đổi

- [Mục lục](README.md) giữ cả hai liên kết trong vùng xung đột, bỏ dấu conflict và thêm bản ghi này. Nội dung các bản ghi cũ được giữ nguyên.
- Git đã tự gộp [AuthChecks](../../backend/tests/StudyPlanner.AuthChecks/Program.cs). Đối chiếu hai phía cho thấy chế độ `--login-validation-only` và ca đăng nhập bằng mã sinh viên có khoảng trắng của `main` vẫn có mặt. Các kiểm tra migration bảo vệ lời nhắc cũ và giữ thời điểm đọc thông báo của nhánh tệp đính kèm cũng được giữ. Không sửa phần tự gộp.

## Kiểm chứng

- Đã đối chiếu mục lục với cả hai phía. Giữ đủ 89 liên kết cũ, thêm một liên kết mới. Tất cả file đích tồn tại, không trùng mục hoặc còn dấu conflict. `git diff --check` và `git diff --cached --check` đạt.
- Build Release toàn bộ solution bằng `dotnet build backend/StudyPlanner.sln -c Release --no-restore --nologo -m:1 -nodeReuse:false -p:UseSharedCompilation=false -v:minimal` đạt 0 cảnh báo, 0 lỗi. Lượt build song song trước đó dừng mà không có lỗi chẩn đoán.
- `StudyPlanner.SchemaChecks` đạt, gồm 17 bảng và 27 khóa ngoại. `StudyPlanner.AuthChecks --login-validation-only` đạt 393 kiểm tra DTO, Application và HTTP trên localhost khi chạy lại với quyền kết nối loopback. Lượt đầu bị môi trường chặn kết nối localhost.
- Không chạy kiểm thử cần PostgreSQL, áp dụng migration hay thử Frontend. Lượt này chỉ xử lý merge ở repo Backend và không kết nối database.

## Việc tiếp theo

- Hoàn tất merge commit trên `main` sau khi xem nội dung đã gộp. Lượt xử lý xung đột không tạo commit hay push.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: `9340de2` và `1b8d052`; chưa tạo merge commit.
