# 2026-11-17 — Tăng mức làm nhạt nhiệm vụ đã xong

## Yêu cầu và mục đích

- Dùng tông xám nhạt hơn theo ảnh TickTick. Sau [lượt chỉnh đầu](2026-11-17-lam-nhat-nhiem-vu-da-hoan-thanh.md), chữ xám và ô tích vẫn đậm hơn ảnh tham khảo.

## Code và logic thay đổi

- FE `src/hoc-tap/hoc-tap.css`. Đổi tên, môn, ngày và biểu tượng của việc đã xong từ xám vừa sang xám nhạt. Làm nhạt riêng ô đã tích trong danh sách, bảng và các bước nhỏ. Dòng đang chọn có nền xám riêng. Ô sửa tên giữ chữ rõ lúc lấy nét và trở về tông nhạt khi rời ô.
- Ô tích lấy nét bằng bàn phím và chế độ tương phản cao giữ mức hiển thị rõ hơn. Khung chi tiết vẫn giữ màu chữ của tên và mô tả. Dùng các lớp trạng thái hiện có, thay đổi chỉ ở CSS.

## Kiểm chứng

- Đối chiếu bộ chọn CSS với các lớp trạng thái trong JSX và kiểm tra diff. Chưa chạy thử trên trình duyệt.

## Việc tiếp theo

- Tải lại trang và so sánh màu của việc đã xong, việc đang chọn và ô đang sửa tên.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng phạm vi kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan.
