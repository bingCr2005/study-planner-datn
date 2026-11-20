# 2026-11-17 — Gọn hộp chọn nhiệm vụ của Đồng hồ

## Yêu cầu và mục đích

- Sửa icon tìm kiếm lệch, bỏ thanh cuộn lồng nhau, tổ chức mục lọc theo ảnh TickTick và đối chiếu dấu cộng tạo nhiệm vụ với Add Timer. Bản trước [mở Đồng hồ từ nhiệm vụ và lọc việc theo hạn](2026-11-17-tap-trung-tu-nhiem-vu.md) dùng hai bộ lọc cạnh nhau và cả hộp lẫn danh sách đều có thể cuộn.

## Code và logic thay đổi

- FE `src/focus/focus-details.css`. Căn icon theo giữa chiều cao ô tìm kiếm. Hộp chọn giữ tiêu đề, tab, tìm kiếm và menu lọc phía trên. Phần danh sách bên dưới tự co theo chiều cao còn lại và là vùng cuộn duy nhất của hộp.
- FE `src/focus/FocusPage.tsx`, `TaskPicker`. Giữ tab **Gần đây** và đổi tên tab chứa danh sách đầy đủ thành **Nhiệm vụ**. Gom hai bộ lọc vào một menu. Mỗi lần chọn một phạm vi, có thể kèm tìm kiếm theo tên. Bổ sung **Ngày mai**, **7 ngày tới** và danh sách từng môn có tên học kỳ. Giữ **Học tập / Việc cá nhân** và **Quá hạn**. Lọc lại đưa danh sách về đầu và lấy từ trang đầu.
- Với **Hôm nay**, danh sách đầy đủ truy vấn hai tập dữ liệu phân trang qua API nhiệm vụ có sẵn. Một tập lấy việc đến hạn hôm nay, tập còn lại lấy việc quá hạn trước đầu ngày. Hiển thị hai nhóm riêng. **Ngày mai** dùng biên ngày theo múi giờ tài khoản và tham số `tuHan`/`denHan`; **7 ngày tới** dùng chế độ `upcoming` hiện có. Hạn hiệu lực gồm hạn kế thừa. Lọc theo từng môn dùng `maMonHoc`.
- Với **Gần đây** hoặc khi chọn bước của nhiệm vụ cha, áp dụng phạm vi ngày và môn cho danh sách tương ứng trong giao diện. Hộp chọn bước chỉ hiện các phạm vi hạn vì các bước đã thuộc cùng một nhánh.
- Đối chiếu dấu **+ Tạo nhiệm vụ** ở `src/layouts/AppLayout.tsx` và sự kiện mở form trong `src/hoc-tap/HocTapPage.tsx`. Nút **+** trong **Lịch sử tập trung** ở `FocusPage.tsx` mở form ghi bổ sung khoảng thời gian đã học. Các ảnh Add Timer của TickTick mô tả một luồng khác, gồm lưu tên, chế độ và thời lượng đồng hồ mẫu để dùng lại, cùng danh sách mẫu và thống kê riêng. Yêu cầu lần này là đánh giá mức cần thiết của nhóm đó.
- Cập nhật [module Đồng hồ](../modules/09-dong-ho-tap-trung.md). Tiếp tục trên nhánh `feat/tap-trung-tu-nhiem-vu`.

## Kiểm chứng

- Đối chiếu code/diff hộp chọn, quy tắc ngày của API nhiệm vụ và hai luồng dấu cộng. Chưa chạy ứng dụng, build hoặc bộ kiểm thử theo yêu cầu người dùng ở lượt triển khai Đồng hồ.

## Việc tiếp theo

- Người dùng kiểm tra trên trình duyệt, gồm hộp thấp với nhiều nhiệm vụ, môn trùng tên ở hai học kỳ, ngày mai và ranh giới nhóm quá hạn.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng phạm vi kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan.
