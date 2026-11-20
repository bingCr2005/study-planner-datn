# 2026-11-17 — Tách bộ chuyển chế độ và rút gọn câu chữ trên Đồng hồ

## Yêu cầu và mục đích

- Tên nhiệm vụ che bộ chuyển **Pomodoro / Đếm lên** sau khi lưu phiên trên desktop thấp. Rút gọn các câu ở Tổng quan, thẻ đã lưu và cài đặt. Tránh dấu chấm phẩy trong câu chữ hiển thị cho sinh viên.
- Bản ghi liên quan [Căn giữa nút Bắt đầu và gọn mặt đồng hồ](2026-11-17-can-giua-nut-bat-dau-dong-ho.md).

## Code và logic thay đổi

- FE `src/focus/focus.css`. Trước đó khối đồng hồ co theo chiều cao của cột. Khi có thêm thẻ đã lưu, các phần tử vẫn được căn giữa theo chiều dọc nên đầu khối có thể tràn lên bộ chuyển chế độ. Giữ chiều cao theo nội dung, cho cột cuộn khi thiếu chỗ và căn bộ chuyển theo trục giữa trên desktop.
- FE `src/focus/FocusPage.tsx`. Dùng nhãn **Pomodoro hoàn tất hôm nay** và **Tổng Pomodoro hoàn tất** thay đoạn chú thích dưới số liệu. Bỏ lời giải thích dư ở thẻ đã lưu và dòng thời gian. Giữ tên nhiệm vụ, thời gian đã lưu, nút cập nhật tiến độ và nút nghỉ. Câu xác nhận bỏ phiên nói rõ thời gian sẽ không được lưu.
- FE `src/focus/FocusStatistics.tsx` và `focus-details.css`. Rút gọn chú thích biểu đồ, khoảng ngày và thông báo trống. Giữ nguyên truy vấn và cách tính các số liệu.
- FE `src/hoc-tap/StudySettings.tsx` viết rõ thời gian học/nghỉ dùng cho Pomodoro mới và gợi ý lịch mới, còn giới hạn ngày dùng khi xếp lịch. Rà thêm câu hướng dẫn Nhắc hạn trong `NotificationSettings.tsx` và thông báo nhập học kỳ trong `AcademicEditors.tsx`. Các thay đổi của lượt này ở giao diện.

## Kiểm chứng

- Đối chiếu CSS cho cửa sổ thấp và JSX các trạng thái sau khi lưu, tổng quan, thống kê, cài đặt. Đọc lại diff. Chưa chạy build hoặc trình duyệt theo yêu cầu trước đó của người dùng.

## Việc tiếp theo

- Khi thử trên trình duyệt, kết thúc và lưu một phiên ở chiều cao tương tự ảnh. Kiểm tra tên nhiệm vụ vẫn nằm dưới bộ chuyển và bấm được cả hai chế độ.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng phạm vi kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan.
