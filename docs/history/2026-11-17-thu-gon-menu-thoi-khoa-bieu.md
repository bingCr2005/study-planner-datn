# 2026-11-17 — Thu gọn menu để mở rộng Thời khóa biểu

## Yêu cầu và mục đích

- Thêm nút gọn để ẩn cột menu chữ khi cần nhiều chỗ cho lịch. [Thay đổi trước](2026-11-17-dong-nhat-thanh-ben-hoc-tap.md) đồng nhất bố cục và chỉ còn nút mở menu ở đường dẫn lịch riêng.

## Code và logic thay đổi

- FE `src/hoc-tap/HocTapPage.tsx` hiển thị nút **Ẩn menu / Hiện menu** cạnh **Bộ lọc** ở cả hai đường mở lịch. Lịch mở từ Nhiệm vụ mặc định hiện menu. Lịch mở từ thanh icon ngoài mặc định thu menu. Nút dùng cùng trạng thái menu của lịch và phản ánh trạng thái qua `aria-expanded`.
- Khi thu, cột menu chữ rút khỏi lưới và khung lịch lấy phần chiều rộng còn lại. Thanh icon ngoài và nội dung lịch không bị dựng lại. Khi sang mục học tập khác, cột menu chữ lại hiển thị. Không đổi ngày, chế độ xem, bộ lọc hay API.
- FE `src/hoc-tap/hoc-tap.css` và `lich.css` xử lý phần tử menu ẩn và giữ kích thước nút ổn định khi đổi nhãn. Cập nhật kỳ vọng nhãn và trạng thái ban đầu trong `tests/planner.test.mjs`.

## Kiểm chứng

- Đối chiếu JSX, CSS và diff. Hai bài kiểm tra dựng giao diện đạt, gồm lịch mở từ Nhiệm vụ và lịch mở từ thanh icon. Chưa bấm thử trên trình duyệt.

## Việc tiếp theo

- Bấm ẩn rồi hiện menu trong lịch tuần để kiểm tra vùng bảy cột. Thử lại khi mở lịch qua thanh icon ngoài.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng phạm vi kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan.
