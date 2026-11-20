# 2026-11-17 — Bảng chọn nổi và cây nhiệm vụ trên Đồng hồ

## Yêu cầu và mục đích

- Bám sát các ảnh TickTick do người dùng cung cấp. [Bản trước](2026-11-17-gon-hop-chon-nhiem-vu-dong-ho.md) có hộp lớn giữa màn hình, điều khiển lọc dạng select mặc định và danh sách nhiệm vụ phẳng. Ảnh tham khảo dùng bảng nổi cạnh đồng hồ, menu lọc nhỏ và cây nhiệm vụ.

## Code và logic thay đổi

- FE `src/focus/FocusDialog.tsx` và `FocusPage.tsx`. Bảng chọn mở từ nút **Tập trung** có kích thước gọn và bám theo nút. Ưu tiên neo bên dưới, đổi phía hoặc căn giữa khi vùng trống không đủ. Backdrop trong suốt. Có thể đóng bằng Escape hoặc bấm bên ngoài. Bộ chọn bước từ nhiệm vụ cha và bộ chọn trong form nhập lịch sử vẫn mở bằng hộp thoại có tiêu đề.
- FE `src/focus/FocusPage.tsx` và `focus-details.css`. Nút lọc hiển thị biểu tượng và phạm vi hiện tại. Menu riêng đánh dấu lựa chọn bằng dấu tích, nhóm môn học ở cuối và hiển thị học kỳ kèm tên môn. Khi menu mở, danh sách nhiệm vụ bên dưới dừng cuộn để chỉ cuộn phần menu. Dòng nhiệm vụ tách tên và hạn thành hai cột, hạn quá hạn có màu đỏ.
- FE dựng lại cây từ các nhiệm vụ lá khớp bộ lọc. Các hàng cha làm nút bung/thu, các bước nhỏ hiển thị theo cấp và chọn trực tiếp để bấm giờ. Mỗi nhóm **Hôm nay / Quá hạn** dựng cây từ phần dữ liệu của chính nhóm. Nếu backend còn ở bản trước, giao diện dùng tên cha trực tiếp khi có.
- BE [NhiemVuService](../../backend/StudyPlanner.Application/HocTap/NhiemVu/NhiemVuService.cs) và [NhiemVuResponse](../../backend/StudyPlanner.Application/HocTap/NhiemVu/NhiemVuResponse.cs) trả `duongDanNhiemVuCha` từ cây đã đọc theo chủ sở hữu. Các phần tử đi từ cha cao nhất đến cha trực tiếp và mang mã, tên, hạn đặt trực tiếp. FE khai báo trường bổ sung tại `src/hoc-tap/types.ts`. Thêm trường phản hồi, không cần đổi dữ liệu hay schema.
- Phạm vi triển khai là bảng chọn nhiệm vụ. Các lựa chọn của menu dựa trên nhiệm vụ, môn và hạn hiện có của dự án. Tiếp tục trên nhánh `feat/tap-trung-tu-nhiem-vu` và cập nhật [module Đồng hồ](../modules/09-dong-ho-tap-trung.md).

## Kiểm chứng

- Đã đọc lại code/diff dựng cây, neo bảng chọn, điều khiển cuộn và phạm vi dữ liệu từ backend. `git diff --check` qua cho các file code liên quan.
- Chưa chạy build, ứng dụng hoặc kiểm thử trình duyệt. Người dùng kiểm thử trực tiếp theo yêu cầu trước đó.

## Việc tiếp theo

- Khởi động lại backend local và tải lại trang để xem đủ cây nhiều cấp. Kiểm tra menu ở màn hình thấp, chọn nhiều bộ lọc và mở/thu các cấp cha.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng phạm vi kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan.
