# 2026-11-17 — Mở rộng vùng lịch khi cuộn xuống

## Yêu cầu và mục đích

- Giữ các hàng tiêu đề, điều hướng và bộ lọc tách riêng. Thanh cuộn bên phải phải cho phép đưa các hàng đó lên khỏi tầm nhìn để lưới lịch rộng và cao hơn. [Bản trước](2026-11-17-danh-dau-nhiem-vu-tren-lich.md) giới hạn chiều cao lưới giờ trong phần còn lại của cửa sổ, nên lưới chỉ cuộn trong vùng thấp khi dải hạn mở.

## Code và logic thay đổi

- FE `src/hoc-tap/lich.css`. Với cửa sổ rộng từ 1051 px, chuyển vùng cuộn dọc sang cột nội dung Thời khóa biểu và giảm lề hai bên. Lưới giờ, lưới tháng và danh sách cao theo nội dung và dùng chung vùng cuộn đó. Hàng ngày và hạn của lịch giờ cùng hàng thứ của lịch tháng bám đầu vùng cuộn.
- FE `src/hoc-tap/LichKhungNhin.tsx`. Chỉ tự cuộn đến khung giờ đầu khi lưới còn là vùng cuộn riêng, như ở cửa sổ hẹp. Desktop mở từ phần điều khiển để người dùng có thể chọn ngày, kiểu xem và bộ lọc trước khi kéo xuống.
- Không gộp tiêu đề, khoảng ngày và bộ lọc. Nút **Ẩn menu** và **Thu gọn hạn** vẫn có tác dụng tương ứng. Không thay API hoặc dữ liệu. Tiếp tục trên nhánh `feat/danh-dau-nhiem-vu-tren-lich`.

## Kiểm chứng

- TypeScript, `git diff --check` và kịch bản lịch trên trình duyệt đều đạt. Ở 1280×635, đã đo phần điều khiển cuộn lên khỏi tầm nhìn, hàng ngày bám sát đầu khung và nút đếm hạn vẫn mở được. Đã xem ảnh chụp từ dữ liệu giả lập sau khi cuộn.

## Việc tiếp theo

- Tải lại trang và kéo thanh cuộn ở mép phải. Kiểm tra hàng ngày và nút đếm hạn còn đọc được khi tiêu đề đã cuộn lên.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng phạm vi kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan.
