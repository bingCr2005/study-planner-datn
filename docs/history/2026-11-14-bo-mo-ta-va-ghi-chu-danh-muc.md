# 2026-11-14 — Bỏ mô tả và ghi chú ở trang Dữ liệu nền tảng

## Yêu cầu và mục đích

- Người dùng gửi ảnh và yêu cầu bỏ dòng mô tả đầu trang cùng khung ghi chú về dữ liệu tham khảo.
- Đã đọc mục lục, mẫu lịch sử, [bản ghi trang admin](2026-11-13-quan-ly-du-lieu-nen-tang.md) và code trang/CSS hiện tại. Tiếp tục nhánh chức năng đang có, không tạo nhánh khác.

## Code và logic thay đổi

- FE `src/admin/AdminCatalogPage.tsx`: bỏ dòng “Chuẩn bị học kỳ và môn học mẫu…” và toàn bộ khung `catalog-note` trong ảnh.
- FE `src/admin/catalog.css`: bỏ ba rule của ghi chú đã xóa; thêm khoảng cách trên thanh tab để giữ bố cục sau tiêu đề.
- Luồng quản lý danh mục không đổi: mở trang → tải API → chọn học kỳ/môn → thêm/sửa/xóa như trước. Không sửa backend, đường dẫn JSON hoặc dữ liệu.

## Kiểm chứng

- Đọc lại JSX/CSS đã sửa; tìm lại hai đoạn chữ và class ghi chú không còn kết quả.
- `npm.cmd run build`: đạt, vẫn có cảnh báo chunk >500 kB đã có trước. Oxlint trang admin và `git diff --check` đạt.
- Không thêm/chạy kiểm thử nghiệp vụ hoặc DB vì chỉ bỏ nội dung tĩnh và CSS. Chưa chụp lại trình duyệt ở lượt này.

## Việc tiếp theo

- Tải lại frontend để xem thay đổi. Chưa commit/push.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR: chưa tạo.
