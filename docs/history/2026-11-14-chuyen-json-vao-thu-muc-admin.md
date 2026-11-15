# 2026-11-14 — Chuyển JSON danh mục vào thư mục admin

## Yêu cầu và mục đích

- Người dùng yêu cầu đặt JSON trong folder admin. Đã đọc mục lục, [bản ghi lưu JSON](2026-11-14-luu-danh-muc-mau-trong-json.md), code đăng ký store, công cụ chuyển dữ liệu, seed, quy tắc publish và kiểm thử hiện có.
- Tiếp tục nhánh nhóm chức năng `feat/quan-ly-du-lieu-nen-tang`, giữ các thay đổi khác chưa commit.

## Code và logic thay đổi

- Trước: file runtime ở `StudyPlanner.Api/App_Data/danh-muc-mau.json`, seed ở `StudyPlanner.Api/Data/danh-muc-mau.seed.json`.
- Sau: cả hai cùng nằm trong `StudyPlanner.Api/Administration/Data/`; `backups` và `.bak` của runtime cũng chuyển cùng. Đây là dữ liệu backend do admin quản lý; không đưa file ghi được vào bundle frontend.
- `Program.cs`, `tools/StudyPlanner.CatalogTransfer/Program.cs` dùng đường dẫn mặc định mới; `Catalog:FilePath` cấu hình riêng vẫn được tôn trọng. Không đổi API, schema JSON, quyền admin/student hoặc database.
- `.csproj`: đóng gói seed theo đường dẫn mới, loại runtime/khóa/file tạm/backup khỏi publish. `.gitignore` chỉ bỏ runtime và backups, giữ seed trong source; vẫn bỏ App_Data cũ để tránh đưa dữ liệu máy cũ vào Git.
- `CatalogChecks/Program.cs`, `JsonChecks.cs`: đọc seed output từ đường dẫn mới. Module 09 cập nhật theo code; bản ghi cũ giữ đúng lịch sử vị trí trước thay đổi.
- Luồng: kiểm tra đường dẫn tuyệt đối nằm trong workspace và đích chưa tồn tại → giữ khóa file nguồn → di chuyển runtime, `.bak`, toàn bộ backup và seed → so SHA256 trước/sau → cập nhật backend đọc/ghi tại đường dẫn mới.

## Kiểm chứng

- Đã chuyển và so hash của 4 file dữ liệu/backup: tất cả giữ nguyên; runtime hiện có 2 học kỳ, 12 môn. Không tái tạo từ seed hoặc ghi đè nội dung admin.
- Build API/CatalogChecks đạt, 0 cảnh báo/lỗi; toàn bộ kiểm tra validate và lưu/đọc/khóa/cancel/ghi lỗi/khôi phục instance của JSON đạt theo seed đường dẫn mới. Phần DB/HTTP được báo SKIP đúng phạm vi, không ghi là đã chạy.
- Publish đạt; kiểm tra output có `Administration/Data/danh-muc-mau.seed.json`, không chứa file runtime hoặc backup. Đã đọc lại diff và kiểm tra khoảng trắng bằng `git diff --check`.
- Không sửa database, không dừng/khởi động backend hoặc mở thêm tiến trình server nền. Không chạy lại kiểm thử DB/HTTP vì chỉ di chuyển đường dẫn file và giữ nguyên API/service nghiệp vụ.

## Việc tiếp theo

- Nếu backend còn chạy bản dùng App_Data, người dùng Ctrl+C và chạy lại trong terminal của mình để nạp đường dẫn mới. Máy khác có file cũ cần chuyển file/backup trước, hoặc cấu hình đường dẫn riêng đến file cũ.
- Chưa commit/push.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
