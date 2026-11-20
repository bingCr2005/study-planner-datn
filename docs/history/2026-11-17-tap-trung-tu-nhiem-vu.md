# 2026-11-17 — Mở Đồng hồ từ nhiệm vụ và lọc việc theo hạn

## Yêu cầu và mục đích

- Thêm lối tắt Pomodoro/Đếm lên từ menu nhiệm vụ, và bổ sung lọc **Đến hạn hôm nay / Quá hạn** trong hộp chọn của Đồng hồ. Hiện thêm thông tin để phân biệt những bước cùng tên. Người dùng sẽ tự chạy thử.
- Trước đây người dùng mở màn Tập trung rồi chọn nhiệm vụ, hoặc bắt đầu từ một phiên tự học đã đặt lịch. Hộp chọn có tìm tên, **Tất cả / Gần đây**, bộ lọc học tập/cá nhân chạy trên các trang đã tải và thông tin loại việc cùng tiến độ.
- Bản ghi liên quan [Màn Đồng hồ Tập trung](2026-11-12-man-dong-ho-tap-trung-rieng.md) và [Sửa nhiệm vụ tại Đồng hồ](2026-11-12-sua-nhiem-vu-tai-trang-tap-trung.md).

## Code và logic thay đổi

- FE `src/hoc-tap/NhiemVuView.tsx`: menu nhiệm vụ chưa hoàn thành có **Tập trung ngay**. Chọn chế độ dẫn đến `/focus?task=...&mode=...`. Lối tắt **Xem phiên tự học** tiếp tục mở luồng buổi theo kế hoạch.
- FE `src/focus/FocusPage.tsx`: xác định phiên đang mở trước khi chuẩn bị nhiệm vụ từ menu. Nếu đã có phiên, hiện đồng hồ của phiên đó. Nếu chưa có, đọc cây qua `GET /tasks/{id}/tree` để lấy nhiệm vụ hiện hành. Nhiệm vụ lá được chọn sẵn, còn nhiệm vụ cha mở hộp chọn các bước lá chưa hoàn thành trong nhánh. Sau khi chọn, nút **Bắt đầu** dùng API `/focus/start` hiện có với mã nhiệm vụ. Chỉ thao tác này mới ghi dữ liệu phiên.
- Hộp chọn dùng `cheDo=today|overdue` và `phamVi=academic|personal` cho danh sách đầy đủ, lọc trước khi phân trang. Tab **Gần đây** và hộp chọn bước của một nhiệm vụ lọc trên danh sách tương ứng đã lấy đủ. **Đến hạn hôm nay** xét ngày theo múi giờ tài khoản, **Quá hạn** xét hạn hiệu lực đã qua. Việc hết hạn từ đầu ngày có thể thuộc cả hai lựa chọn.
- FE `src/hoc-tap/types.ts` và `src/focus/focus-details.css` hỗ trợ hiển thị tên cha, tên môn và hạn. Bộ lọc theo hạn dùng cả hạn đặt trực tiếp và hạn kế thừa.
- BE [DanhSachNhiemVuRequest](../../backend/StudyPlanner.Application/HocTap/NhiemVu/DanhSachNhiemVuRequest.cs) và [NhiemVuService](../../backend/StudyPlanner.Application/HocTap/NhiemVu/NhiemVuService.cs): thêm `phamVi` tùy chọn vào truy vấn nhiệm vụ. Việc học tập có học kỳ hoặc môn và việc cá nhân không có cả hai, theo cách phân loại cũ của Đồng hồ. [NhiemVuResponse](../../backend/StudyPlanner.Application/HocTap/NhiemVu/NhiemVuResponse.cs) trả tên cha từ cây hiện có, cùng phạm vi chủ sở hữu. Thay đổi không thêm bảng hay migration.
- Cập nhật [module Đồng hồ](../modules/09-dong-ho-tap-trung.md) theo luồng mới. Làm trên nhánh `feat/tap-trung-tu-nhiem-vu` của hai repo, giữ các thay đổi local có sẵn.

## Kiểm chứng

- Đã đọc lại diff và đối chiếu đường dẫn màn Đồng hồ, API nhiệm vụ, truy vấn hạn hiện có và lệnh bắt đầu phiên.
- Chưa chạy ứng dụng, build hoặc bộ kiểm thử theo yêu cầu người dùng.

## Việc tiếp theo

- Khởi động lại backend local để nạp trường tên cha và tham số lọc phạm vi mới. Khi thử giao diện, kiểm tra nhiệm vụ độc lập, nhiệm vụ có bước con, phiên đang mở và hai bộ lọc kết hợp.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng phạm vi kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan.
