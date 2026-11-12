# 2026-11-10 — Chuyển mô tả và checklist trong chi tiết nhiệm vụ

## Yêu cầu và mục đích

Người dùng quyết định giữ checklist và muốn chuyển từng dòng mô tả thành mục đánh dấu, hoặc đưa checklist trở lại dạng văn bản. Xem [quy tắc tiến độ checklist](2026-11-10-tach-checklist-va-tien-do-nhiem-vu-con.md) và [bố cục chi tiết nhiệm vụ](2026-11-10-lam-gon-giao-dien-nhiem-vu.md).

## Code và logic thay đổi

- Trong `Study_Planner_FE/src/hoc-tap/NhiemVuView.tsx`, đặt một nút đổi chế độ cạnh tiêu đề. Từ **Mô tả**, nút lấy cả bản nháp chưa lưu, cắt khoảng trắng ngoài mỗi dòng và bỏ dòng trống. Mỗi dòng có nội dung trở thành một mục chưa đánh dấu. Mô tả trống chuyển sang ô thêm checklist tại chỗ.
- Từ **Checklist**, nút nối tiêu đề các mục vào mô tả, mỗi mục một dòng, giữ nội dung mô tả cũ nếu cả hai đã tồn tại. Phần trăm đang lưu trở thành tiến độ nhập tay. Các dấu tick thuộc chế độ checklist, nên khi tạo lại checklist từ văn bản các mục bắt đầu ở trạng thái chưa đánh dấu.
- Một lần chuyển có dữ liệu gửi mô tả và checklist trong cùng yêu cầu cập nhật, qua kiểm tra phiên bản và ảnh hưởng lịch của API hiện có. Các nội dung vượt giới hạn được báo tại chỗ. Khi đang nhập mục checklist, giao diện yêu cầu lưu hoặc hủy trước khi đổi chế độ. Bước con vẫn hiển thị ở phần riêng.
- `src/hoc-tap/LichView.tsx` tìm URL tài liệu trong mô tả và tiêu đề checklist. CSS giữ nút chuyển cạnh tên và trình chỉnh sửa tại cùng vị trí. Đã khôi phục phần BE vừa sửa dở để dùng quy tắc tiến độ checklist hiện tại. Lượt này dùng lại migration checklist đã có.

## Kiểm chứng

- Đã đối chiếu JSX, luồng lưu và các trường hợp nhiều dòng, mô tả rỗng, dữ liệu cũ có cả hai loại nội dung và bản nháp chưa lưu.
- TypeScript `--noEmit`, PostCSS và `git diff --check` đạt. Build Release backend và CoreChecks đạt với 0 lỗi, 0 cảnh báo.
- Chưa chạy trên trình duyệt hoặc database của người dùng.

## Việc tiếp theo

- Dùng một nhiệm vụ có bốn dòng mô tả, chuyển sang checklist, tick một mục rồi chuyển lại để đối chiếu nội dung và vị trí bước con.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã và ghi kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code liên quan.
