# 2026-11-10 — Thùng rác nhiệm vụ và căn icon sidebar

## Yêu cầu và mục đích

- Cho phép khôi phục nhiệm vụ đã xóa và dùng icon để phân biệt **Đã hoàn thành** với **Thùng rác**. Ảnh phản hồi cho thấy icon mới đẩy chữ lệch sang phải, khiến **Đã hoàn thành** xuống dòng.
- Xem [menu học tập theo chiều dọc](2026-11-06-menu-hoc-tap-hang-doc.md) và [hộp xác nhận cũ](2026-11-06-hop-xac-nhan-va-giai-thich-giao-dien.md).

## Code và logic thay đổi

- Trước đây FE gọi API xóa trực tiếp sau khi xác nhận. Nay `NhiemVuView.tsx` chuyển nhiệm vụ và các bước vào thùng rác bằng `POST /api/v1/tasks/{id}/trash`. FE chỉ hỏi xác nhận nếu cần hủy phiên tự học dự kiến. Phiên đang học chặn thao tác.
- [NhiemVuService](../../backend/StudyPlanner.Application/HocTap/NhiemVu/NhiemVuService.cs) đánh dấu nhánh bằng thời điểm và mã nhóm xóa. Truy vấn thường bỏ qua nhiệm vụ trong thùng rác. Phiên dự kiến cùng nhắc hạn và thông báo chưa gửi của nhánh được hủy. [Controller](../../backend/StudyPlanner.Api/Controllers/NhiemVuController.cs) cung cấp API xem các nhóm đã xóa của chính người dùng, khôi phục và xóa vĩnh viễn.
- `ThungRacView.tsx` dùng các API này để hiển thị danh sách, khôi phục hoặc yêu cầu xác nhận khi xóa vĩnh viễn. Khi khôi phục, nhánh về dưới cha cũ nếu còn hợp lệ, nếu không sẽ thành nhiệm vụ độc lập. Phiên đã hủy vẫn ở trạng thái đã hủy. Xóa vĩnh viễn xóa dữ liệu liên quan theo ràng buộc hiện có và giữ lịch sử thao tác.
- Trong repo `Study_Planner_FE`, `HocTapPage.tsx` bọc nhãn sidebar trong `span`. `hoc-tap.css` dùng hai cột chung cho các lối tắt. Icon rộng 16 px nằm ở cột đầu. Mọi nhãn bắt đầu ở cột thứ hai và nằm trên một dòng, kể cả các mục chỉ có chữ. Khoảng đệm trái của nhãn vẫn là 34 px tính từ mép nút.
- [Migration](../../backend/StudyPlanner.Infrastructure/Migrations/20261110093934_ThemThungRacNhiemVu.cs) thêm dữ liệu đánh dấu xóa và các giá trị lịch sử mới. Khi hạ migration, phải xử lý hết nhiệm vụ trong thùng rác trước để tránh đưa nhiệm vụ ẩn trở lại danh sách ngoài ý muốn. Các bản ghi lịch sử loại mới được chuyển thành `Updated` để phù hợp ràng buộc cũ.

## Kiểm chứng

- Đã đối chiếu JSX/CSS, logic chuyển vào thùng rác và các endpoint. `tsc --noEmit -p tsconfig.app.json` và `git diff --check` trên hai file sidebar đạt.
- Build Release của BE API đạt với 0 cảnh báo và 0 lỗi. EF sinh thành công SQL áp dụng và hạ migration, không kết nối cơ sở dữ liệu.
- Chưa áp dụng migration hoặc kiểm tra HTTP với database. Người dùng sẽ tự kiểm tra giao diện trong trình duyệt.

## Việc tiếp theo

- Áp dụng migration cho môi trường cần dùng Thùng rác. Kiểm tra căn icon, nhãn và trạng thái chọn ở chiều rộng như ảnh phản hồi.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code liên quan.
