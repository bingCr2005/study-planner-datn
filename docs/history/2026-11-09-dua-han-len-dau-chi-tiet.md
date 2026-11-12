# 2026-11-09 — Đưa hạn hoàn thành lên đầu chi tiết

## Yêu cầu và mục đích

Người dùng muốn hạn dễ thấy ngay khi mở nhiệm vụ, theo ảnh tham chiếu TickTick. Trước đây hạn nằm trong bảng thông tin bên dưới tiêu đề và mô tả. Xem [bản ghi kiểu nhiệm vụ đã xong](2026-11-09-hien-thi-nhiem-vu-da-hoan-thanh.md).

## Code và logic thay đổi

`src/hoc-tap/NhiemVuView.tsx` chuyển hạn từ bảng thông tin lên đầu `ChiTietNhiemVu`, dùng chung cho khung bên cạnh danh sách và hộp thoại mở từ lịch hoặc phiên tự học. Hạn hiệu lực từ API được định dạng theo múi giờ tài khoản. Nếu nguồn hạn khác nhiệm vụ đang xem, ghi rõ “Theo hạn nhiệm vụ lớn”. Khi chưa có hạn, nút “Chưa đặt hạn” mở biểu mẫu chỉnh sửa cùng phiên bản đang đọc. CSS trong `src/hoc-tap/hoc-tap.css` dùng màu đỏ cho hạn quá hạn của nhiệm vụ còn mở và xám cho nhiệm vụ đã hoàn thành. Dữ liệu hạn dùng hợp đồng BE hiện có.

## Kiểm chứng

Đã đối chiếu hợp đồng BE và nơi gọi chi tiết trong FE. Bài Chromium dùng nút “Chưa đặt hạn” để mở và lưu biểu mẫu chỉnh sửa.

`git diff --check` và build FE đạt. Chromium đạt với 283 request giả lập. Kiểu màu quá hạn và hạn kế thừa được đối chiếu trong code, chưa kiểm tra bằng ảnh chụp.

## Việc tiếp theo

Có thể xem hàng hạn mới khi mở chi tiết từ danh sách hoặc lịch.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code.
