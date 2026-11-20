# 2026-11-17 — Đánh dấu nhiệm vụ và thu gọn hạn trên lịch

## Yêu cầu và mục đích

- Phân biệt việc chưa xong và đã xong bằng màu thẻ, tham khảo ảnh TickTick. Cho phép tích trực tiếp với việc đủ điều kiện. Giữ hạn dễ tìm khi cuộn lịch giờ nhưng giảm chiều cao dải hạn lúc cần. Tận dụng thêm chiều cao màn hình để xem lịch.
- Bản ghi liên quan [dải hạn lịch tuần](2026-11-16-lam-ro-han-trong-lich-tuan.md) và [thu gọn menu lịch](2026-11-17-thu-gon-menu-thoi-khoa-bieu.md).

## Code và logic thay đổi

- BE [SuKienLichResponse](../../backend/StudyPlanner.Application/HocTap/ThoiKhoaBieu/SuKienLichResponse.cs) và [ThoiKhoaBieuService](../../backend/StudyPlanner.Application/HocTap/ThoiKhoaBieu/ThoiKhoaBieuService.cs). API lịch vốn có trạng thái nhưng chưa cho biết nhiệm vụ có thể tích trực tiếp hay cần tổng hợp tiến độ. Thêm `coTheDanhDauHoanThanh` và `tienDoNhiemVu` cho mốc hạn và sự kiện từ nhiệm vụ. Chỉ bật thao tác với nhiệm vụ lá chưa có checklist. Lịch lớp, giờ rảnh và phiên tự học không có thao tác này.
- FE `src/hoc-tap/LichKhungNhin.tsx` và `lich.css`. Dùng thẻ chung cho dải hạn, lịch giờ, lịch tháng và hai tuần, danh sách và khung mở thêm hạn. Mỗi loại lịch có màu nền riêng. Việc đã xong dùng nền và chữ nhạt hơn. Tên và ô tích là hai điều khiển độc lập. Với việc đã xong cần cập nhật qua chi tiết, dấu tích chỉ biểu thị trạng thái.
- FE `src/hoc-tap/LichView.tsx`, `types.ts`, `NhiemVuView.tsx`. Khi tích hoặc bỏ tích, dùng luồng hoàn thành/mở lại có sẵn cùng phiên bản từ lịch. Giữ kiểm tra xung đột, xác nhận khi ảnh hưởng phiên tự học và hoàn tác ở trường hợp phù hợp. Chặn thao tác song song trong lúc lưu. Sau khi API xác nhận, tải lại lịch để mốc hạn và khung giờ của cùng nhiệm vụ cùng đổi trạng thái. Không yêu cầu migration.
- Giữ dải hạn ghim phía trên lưới giờ. Nút **Thu gọn hạn** giữ nút đếm theo ngày và mở danh sách cũ ngay tại cột. Bấm **Hiện dải hạn** để thấy hai thẻ xem trước như trước. Chế độ Ngày chuyển thẻ hạn sang một hàng nếu đủ chỗ. Thu hay mở dải giữ ngày, chế độ lịch và bộ lọc đang dùng.
- Khung lịch trên desktop giãn theo chiều cao còn lại. Lưới giờ, lưới tháng hoặc danh sách là vùng cuộn chính. Trang vẫn có thể cuộn nếu phần điều khiển chiếm nhiều chỗ. Trục giờ dùng 48 px mỗi giờ thay cho 60 px. `lich.ts` và `LichKhungNhin.tsx` đổi tỷ lệ hiển thị, giữ chiều cao tối thiểu của khối ngắn và không đổi thời gian lưu.

## Kiểm chứng

- Backend build Release không lỗi hoặc cảnh báo. Toàn bộ CoreChecks đạt trên PostgreSQL tạm, gồm 585 kiểm tra HTTP lõi và kiểm tra quyền tích của hạn cha/con.
- TypeScript và kịch bản trình duyệt lịch đạt. Dữ liệu giả lập kiểm tra tích và mở lại, màu đồng bộ của hai mốc cùng nhiệm vụ, giới hạn ô tích, mở thêm hạn và xem chi tiết trên màn nhỏ. Với kích thước 1280×635, đo được trang ngoài không cần cuộn, nút đếm hạn giữ trong vùng ghim khi cuộn và hai thẻ xem trước của lịch Ngày cùng một hàng.

## Việc tiếp theo

- Khởi động lại backend local để có dữ liệu cho ô tích rồi tải lại trang. Đối chiếu thêm với dữ liệu sinh viên thực tế ở lịch tuần và tháng.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng phạm vi kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan.
