# 2026-11-12 — Gộp main vào nhánh quy tắc ưu tiên

## Yêu cầu và mục đích

- Người dùng đồng ý xử lý merge/conflict và kiểm tra backend, chưa yêu cầu commit hoặc push.
- Nhánh hiện tại: `feat/quy-tac-uu-tien`, HEAD `6cbf076`. Main mới nhất sau fetch: `b047b24` (PR #12).
- Khi bắt đầu đã có merge đang dở với `MERGE_HEAD = b047b24`, chỉ `docs/history/README.md` còn unmerged. Lệnh merge thử không thực hiện thêm thay đổi vì Git phát hiện phiên merge có sẵn; tiếp tục giải quyết phiên này.
- Đã đọc mục lục, mẫu lịch sử, [xử lý conflict học kỳ/môn](2026-11-06-giai-quyet-conflict-hoc-ky-mon-hoc.md) và [xử lý mục lục PR #12](2026-11-12-giai-quyet-conflict-lich-su-pr-12.md).

## Code và logic thay đổi

- File xử lý thủ công: [README.md](README.md). Hai bên cùng nối thêm liên kết sau mục thông báo ngày 10/11 nên Git không tự ghép được đoạn cuối.
- Trước: đoạn conflict chứa nhóm quản trị từ HEAD và nhóm lịch học/nhiệm vụ từ main.
- Sau: giữ toàn bộ liên kết hai phía, đặt nhóm ngày 11/11 trước nhóm ngày 12/11, bỏ dấu conflict và thêm bản ghi này.
- Luồng xử lý: xác minh HEAD/main và merge hiện có → đối chiếu hai bản mục lục → giữ hợp các liên kết → kiểm tra trùng/thiếu/file đích → build backend → stage file đã giải quyết, dừng trước commit.
- Các thay đổi backend, migration, tài liệu và việc xóa `frontend-preview` đã có trong phiên merge từ main được giữ nguyên; không tự chỉnh sửa logic nghiệp vụ trong lượt xử lý mục lục này.

## Kiểm chứng

- Đã đọc diff mục lục với cả hai phía; kiểm tra tất cả liên kết từ HEAD và MERGE_HEAD đều còn, không trùng liên kết.
- `git diff --check` và `git diff --cached --check` đạt; không còn dấu conflict trong backend/docs.
- `dotnet build backend/StudyPlanner.sln --no-restore -p:BaseOutputPath=D:/study-planner-datn/backend/obj/merge-priority/bin/ -p:CopyRetryCount=0`: thành công, 0 cảnh báo, 0 lỗi. Thư mục output riêng tránh ghi đè DLL API đang chạy.
- `dotnet backend/obj/merge-priority/bin/Debug/net10.0/StudyPlanner.SchemaChecks.dll`: năm kiểm tra đầu đạt, dừng ở kiểm tra tên cột snake_case (`Program.cs:31`). Test yêu cầu mọi tên cột suy ra trực tiếp từ tên property, nhưng `NhiemVu.ChecklistJson` được main chủ động ánh xạ thành cột `checklist` kiểu jsonb, thay vì `checklist_json`. Đã đối chiếu diff HEAD/MERGE_HEAD: ánh xạ này được thêm từ main, còn file SchemaChecks không thay đổi giữa hai phía. Đây là điểm chưa đồng bộ test/model từ main; không sửa schema hay test ngoài phạm vi xử lý conflict.
- Chưa chạy HTTP/PostgreSQL hoặc toàn bộ project kiểm thử nằm ngoài solution; phạm vi kiểm chứng là build solution và schema, không xác nhận toàn bộ nghiệp vụ sau merge.

## Việc tiếp theo

- Merge còn chờ commit. Cần commit bản merge và push nhánh nguồn để GitHub cập nhật PR; lượt này chưa thực hiện hai thao tác đó.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo commit merge.
