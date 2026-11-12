# 2026-11-11 — Căn thẳng thanh điều khiển thời khóa biểu

## Yêu cầu và mục đích

- Ảnh từ FE cho thấy thanh điều khiển lịch tháng xếp dọc dù màn hình còn rộng. Cần căn gọn theo hàng và thống nhất kích thước nút với cụm Bộ lọc. Xem [bản ghi chế độ xem thời khóa biểu](2026-11-11-cai-tien-thoi-khoa-bieu.md).

## Code và logic thay đổi

- CSS chung của `Study_Planner_FE` trong `src/hoc-tap/hoc-tap.css` đặt `width:100%` cho `input` và `select` trong không gian học tập. Vì vậy ô chọn ngày và chế độ xem trên lịch chiếm cả dòng, trong khi nút trước, sau và nút thêm chỉ rộng bằng nội dung.
- `src/hoc-tap/LichView.tsx` gom nút trước, Hôm nay, sau và ô chọn ngày thành một nhóm. Kế tiếp là chế độ xem và nút Thêm vào lịch. `src/hoc-tap/lich.css` giới hạn chiều rộng theo loại điều khiển, đặt chiều cao 36 px cho hai thanh và căn phải khi đủ chỗ. Bố cục cho phép xuống hàng theo chiều rộng còn lại và căn trái trên màn hẹp.
- `src/hoc-tap/HocTapPage.tsx` dùng nhãn Bộ lọc ổn định khi đóng hoặc mở. Trên lịch, Xóa bộ lọc xuất hiện khi đang lọc học kỳ hoặc môn. Các hành động điều hướng ngày và chọn chế độ tiếp tục dùng cùng state của lịch.

## Kiểm chứng

- Đã đối chiếu selector riêng của lịch với CSS dùng chung và đọc lại các thay đổi ở JSX. `git diff --check` cho file FE đã được Git theo dõi và kiểm tra khoảng trắng của `lich.css` đạt.
- TypeScript `--noEmit` và phân tích CSS bằng PostCSS đều đạt.
- Chưa mở trình duyệt để đo bố cục theo trao đổi trước đó.

## Việc tiếp theo

- Kiểm tra bố cục ở lịch tháng và tuần khi thu gọn hoặc mở rộng sidebar.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR điền khi đã có.
