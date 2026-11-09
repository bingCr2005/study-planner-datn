# 2026-11-06 — Lựa chọn phạm vi buổi học rõ ràng

## Yêu cầu và mục đích

- Người dùng phản ánh ghi chú “Để trống cả hai ngày để lặp trong học kỳ của môn” trên FE khó hiểu.
- Đã đọc lịch sử, [bản ghi thu gọn bộ lọc](2026-11-06-thu-gon-bo-loc-hoc-tap.md), SuaLich trong LichView và kiểm thử hiện có trước khi sửa.

## Code và logic thay đổi

- FE tại `C:/Users/ADMIN/Desktop/Study_Planner_FE`: sửa `src/hoc-tap/LichView.tsx` và `tests/browser-hoc-tap.mjs`.
- Trước đó hiển thị hai ngày không bắt buộc cùng ghi chú để trống cả hai; người dùng phải hiểu quy tắc ngầm để biết lịch áp dụng suốt học kỳ.
- Sau thay đổi, buổi học mặc định có lựa chọn Áp dụng suốt học kỳ. Khi bật thì ẩn ngày và gửi null cho cả hai ngày theo hợp đồng API hiện có. Khi tắt thì hiện khoảng ngày, yêu cầu đủ cả hai và ngày kết thúc không trước ngày bắt đầu.
- Sửa mẫu lịch có khoảng ngày thì mở sẵn chế độ khoảng ngày; mẫu không có ngày thì mở chế độ toàn học kỳ. Giữ những ngày vừa nhập khi bật/tắt lựa chọn trong cùng biểu mẫu.
- Bỏ hoàn toàn ghi chú cũ. Giao diện giờ rảnh và các quy tắc BE không thay đổi. Không chỉnh nguồn ngoài module học tập.
- Luồng là chọn môn/thứ/giờ → chọn toàn học kỳ hoặc khoảng ngày → preview ảnh hưởng và kiểm tra BE → lưu như hợp đồng cũ.
- Cập nhật [kịch bản demo](../KICH-BAN-DEMO-NHIEM-VU-VA-THOI-KHOA-BIEU.md) theo cách thao tác mới.

## Kiểm chứng

- Build TypeScript/Vite thành công; bộ kiểm thử FE đạt 100/100.
- Chromium chạy toàn luồng thành công với 270 request giả lập. Kiểm tra tạo mặc định gửi null hai ngày; chế độ riêng chặn thiếu ngày kết thúc trước khi ghi; bật/tắt giữ ngày nhập; lưu và mở lại khoảng riêng; chuyển về toàn học kỳ gửi lại null hai ngày; không còn ghi chú cũ trong hộp.
- Một lượt dừng ở bài thử thanh kéo trước khi tới buổi học do ResizeObserver chưa cập nhật sau đổi kích thước. Bổ sung chờ hai khung hình sau resize trong kiểm thử, không sửa component thanh kéo; lượt sau đạt.
- Đã mở ảnh biểu mẫu tại `backend/obj/fe-lesson-period/lesson-whole-semester.png` để kiểm tra hiển thị. Đây là ảnh dữ liệu giả lập, được Git bỏ qua.
- Đã đọc lại code/diff, kiểm tra khoảng trắng và so hash nguồn ngoài module. Không có thay đổi nguồn ngoài module trong lượt này.
- Không thử ghi vào DB của người dùng; không thay đổi BE, không áp dụng migration.

## Việc tiếp theo

- Người dùng tải lại FE và mở Thêm buổi học để xem lựa chọn mới.
- Chưa commit/push; tài liệu giữ local.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu code/tài liệu với kết quả kiểm tra.
- [ ] Người dùng đã đọc và đối chiếu thay đổi. Chỉ người dùng xác nhận mục này.

Commit/PR cho lượt này chưa được tạo.
