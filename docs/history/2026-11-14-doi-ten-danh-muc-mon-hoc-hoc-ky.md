# 2026-11-14 — Đổi tên trang thành Danh mục môn học & học kỳ

## Yêu cầu và mục đích

- Người dùng chọn tên hiển thị mới: “Danh mục môn học & học kỳ”.
- Đã đọc mục lục lịch sử, [bản ghi giao diện liên quan](2026-11-14-bo-nut-tai-lai-tren-cac-trang.md) và code tiêu đề/menu/breadcrumb hiện tại. Tiếp tục nhánh nhóm chức năng đang làm.

## Code và logic thay đổi

- FE `src/admin/AdminCatalogPage.tsx`: đổi h1 từ Dữ liệu nền tảng thành tên người dùng chọn.
- FE `src/admin/AdminLayout.tsx`: đổi nhãn sidebar và tiêu đề breadcrumb cùng tên.
- FE `tests/browser-catalog.mjs`: cập nhật kỳ vọng chữ trên breadcrumb; không thêm test mới cho thay đổi nội dung tĩnh.
- Module 09 và mục lục module cập nhật tên hiện hành; các bản ghi cũ giữ tên tại thời điểm đã triển khai.
- Luồng mở `/admin/catalog` → tải danh mục → thêm/sửa/xóa giữ nguyên. Chỉ đổi tên hiển thị, không đổi route, API, JSON hoặc backend runtime.

## Kiểm chứng

- Đã đọc lại các vị trí đổi tên; tìm kiếm trong src và bài kiểm thử danh mục không còn tên hiển thị cũ.
- Build TypeScript/Vite, lint hai component và kiểm tra cú pháp bài browser đạt. Build còn cảnh báo chunk >500 kB đã có trước. `git diff --check` đạt ở hai repo.
- Chưa chạy lại browser hoặc kiểm thử DB/API vì chỉ đổi tên hiển thị.

## Việc tiếp theo

- F5 frontend để xem tên mới. Chưa commit/push; không khởi động lại backend.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR: chưa tạo.
