# 2026-11-14 — Bỏ nút Tải lại trên các trang

## Yêu cầu và mục đích

- Người dùng gửi ảnh nút Tải lại và yêu cầu bỏ ở tất cả trang có nút này.
- Đã đọc mục lục và [bản ghi chỉnh trang danh mục](2026-11-14-bo-mo-ta-va-ghi-chu-danh-muc.md), rà soát JSX và code tải dữ liệu hiện tại của frontend. Tiếp tục nhánh nhóm chức năng hiện tại.

## Code và logic thay đổi

- Rà soát toàn bộ `src`: nút Tải lại thường trực còn ở Dữ liệu nền tảng và Quy tắc ưu tiên; trang Tổng quan đã không có nút này.
- FE `src/admin/AdminCatalogPage.tsx`, `AdminPriorityRulesPage.tsx`: bỏ hai nút trên header. Dọn import Icon không dùng của trang quy tắc và rule CSS riêng cho nút header trong `priorityRules.css`.
- Giữ cách tải dữ liệu khi mở trang, khi đổi bộ lọc/phân trang và sau thao tác lưu; các hàm reload/refresh vẫn được dùng cho cập nhật sau ghi và nút Thử lại khi lỗi.
- `tests/browser-catalog.mjs`: hai bước cần đọc lại dữ liệu giờ điều hướng lại cùng pathname/search, không gọi selector của nút đã xóa; giữ bộ lọc đang có. Không viết thêm test chỉ để kiểm tra một nút đã bị bỏ.
- Luồng: mở trang → tải tự động → thêm/sửa/xóa → đọc lại tự động như trước; nguồn lỗi vẫn có Thử lại. Không sửa API, backend hoặc file JSON.

## Kiểm chứng

- Đã đọc lại header, import và CSS liên quan; rà soát không còn nút có nội dung Tải lại trong src và không còn selector nút cũ ở bài kiểm thử danh mục.
- Build TypeScript/Vite đạt, còn cảnh báo chunk >500 kB đã có từ trước; lint hai trang và kiểm tra cú pháp `browser-catalog.mjs` đạt. `git diff --check` đạt.
- Không chạy lại kiểm thử DB/HTTP hoặc toàn bộ browser suite vì thay đổi chỉ bỏ nút giao diện; không ghi các suite này là đã chạy trong lượt này.

## Việc tiếp theo

- F5 frontend để xem thay đổi. Chưa commit/push, không khởi động lại backend.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR: chưa tạo.
