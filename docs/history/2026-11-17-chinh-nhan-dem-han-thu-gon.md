# 2026-11-17 — Chỉnh nhãn đếm hạn thu gọn

## Yêu cầu và mục đích

- Làm đẹp nhãn số hạn dưới ngày để sinh viên nhận ra có thể bấm xem danh sách.
- Bản ghi liên quan [Mở rộng vùng lịch khi cuộn xuống](2026-11-17-mo-rong-vung-lich-khi-cuon.md).

## Code và logic thay đổi

- `Study_Planner_FE/src/hoc-tap/LichKhungNhin.tsx`, hàm `HanTrongNgay`, hiển thị số hạn bằng chữ đậm và thêm mũi tên ở chế độ thu gọn.
- `Study_Planner_FE/src/hoc-tap/lich.css` tăng kích thước tối thiểu của nút từ 20px lên 24px theo chiều cao, dùng nền vàng nhạt với viền mảnh. Mũi tên hướng xuống khi đóng và hướng lên khi danh sách mở.
- Trước đây nút chỉ có dòng chữ nhỏ như “4 hạn”. Bấm nhãn vẫn mở danh sách hạn theo ngày qua `popoverTarget` hiện có. Thay đổi chỉ áp dụng cho nhãn thu gọn.

## Kiểm chứng

- Đã rà soát JSX, CSS và chạy `git diff --check` cho hai tệp frontend.
- Đã chạy `CORE_CALENDAR_DEADLINES_ONLY=1` với `node tests/browser-hoc-tap.mjs` ở kích thước 1280 × 635. Kiểm tra đạt, gồm thao tác cuộn và mở danh sách hạn thu gọn. Đã xem ảnh chụp từ lần chạy.
- Lần kiểm tra dùng dữ liệu mẫu của bộ kiểm tra trình duyệt.

## Việc tiếp theo

- Có thể tải lại trang Thời khóa biểu và chọn “Thu gọn hạn” để xem nhãn với dữ liệu thực.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code liên quan.
