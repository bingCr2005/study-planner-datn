# 2026-11-17 — Làm nhạt nhiệm vụ đã hoàn thành

## Yêu cầu và mục đích

- Áp dụng cách phân biệt nhiệm vụ đã xong bằng tông nhạt như ảnh TickTick. Ở ảnh tham khảo, danh sách mờ hơn ngay cả khi dòng được chọn, còn tên và mô tả ở chi tiết vẫn dễ đọc.
- Tiếp tục phần [hiển thị việc đã xong trên bảng](2026-11-17-hien-nhiem-vu-da-hoan-thanh-tren-bang.md) trên cùng nhánh.

## Code và logic thay đổi

- FE `src/hoc-tap/hoc-tap.css`. Dùng lớp `ht-task-completed` mà `NhiemVuView.tsx` đã gắn cho trạng thái Completed. Gom quy tắc màu của tên, môn, ngày, ô dấu tích, biểu tượng và tiến độ về tông xám trung tính. Thẻ đã xong ở bảng có nền sáng nhẹ. Dòng hoặc thẻ đang chọn có nền xám riêng.
- Trong chi tiết, chỉ giảm mức nhấn cho thông tin hạn của nhiệm vụ đã xong và các bước hoặc mục checklist đã đánh dấu. Tiêu đề, mô tả, chỗ sửa tên và điều khiển mở lại vẫn dùng kiểu và hành vi tương ứng hiện có. Không thêm opacity vào toàn bộ hàng để văn bản nhập và menu vẫn dễ đọc.
- Thay đổi ở CSS, không đổi dữ liệu hoặc cách xác định trạng thái. Cập nhật [module nhiệm vụ](../modules/03-nhiem-vu-va-tien-do.md).

## Kiểm chứng

- Đối chiếu diff và các lớp trạng thái trong JSX. Chưa chạy trình duyệt theo yêu cầu trước đó của người dùng.

## Việc tiếp theo

- Tải lại trang để xem danh sách **Đã hoàn thành**, một dòng đang chọn và thẻ tương ứng trong Bảng trạng thái.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng phạm vi kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan.
