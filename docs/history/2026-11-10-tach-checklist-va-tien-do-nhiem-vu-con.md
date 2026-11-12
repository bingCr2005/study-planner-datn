# 2026-11-10 — Tách checklist và tiến độ nhiệm vụ con

## Yêu cầu và mục đích

Người dùng chọn hành vi như ví dụ TickTick, trong đó thanh tiến độ của nhiệm vụ theo các ô checklist và phần trăm của nhiệm vụ con thay đổi độc lập. Đồng thời đối chiếu trường hợp bước nhỏ mới hiển thị hạn hiệu lực của cha.

## Code và logic thay đổi

- [NhiemVuService](../../backend/StudyPlanner.Application/HocTap/NhiemVu/NhiemVuService.cs) lưu checklist qua API tạo/cập nhật hiện có và tính tỷ lệ mục đã đánh dấu. Khi có checklist, API tiến độ nhập tay trả lỗi dẫn về checklist. Khi xóa mục cuối, giữ giá trị đang có làm tiến độ nhập tay. API cập nhật nhiệm vụ cũ gửi thiếu `checklist` vẫn giữ checklist đã lưu.
- [QuyTacCay](../../backend/StudyPlanner.Application/HocTap/NhiemVu/QuyTacCay.cs) giữ quy tắc tổng hợp thời lượng từ lá. Tiến độ được lưu riêng trên mỗi nhiệm vụ, kể cả nhiệm vụ cha. Nhiệm vụ cũ giữ phần trăm hiện tại. Cha đã hoàn thành xuất hiện ở mục hoàn thành, còn con chưa xong của cha vẫn có dòng riêng ở danh sách đang làm.
- Trong `Study_Planner_FE`, `NhiemVuView.tsx` thêm vùng **Checklist** trong chi tiết, tách khỏi **Các bước nhỏ**, với thao tác thêm, tick và xóa mục. Vòng tiến độ mở checklist nếu đã có hoặc mở thanh kéo nếu tiến độ nhập tay. `LichView.tsx` đưa nhiệm vụ có checklist về chi tiết khi cập nhật kết quả học.
- [Migration](../../backend/StudyPlanner.Infrastructure/Migrations/20261110162327_ThemChecklistNhiemVu.cs) thêm cột `checklist` kiểu JSON, mặc định rỗng theo nghĩa `NULL`. Dữ liệu nhiệm vụ trước đó được giữ. Thay đổi checklist dùng giao dịch và kiểm tra ảnh hưởng phiên học hiện có.
- `hanBuocMacDinh` của FE vẫn dùng 23:59 hôm nay khi phù hợp. Nếu cha có hạn hiệu lực sớm hơn hoặc bằng mốc đó, bước mới để hạn riêng trống và kế thừa. Giờ hiển thị trên dòng là hạn, ví dụ 22:00 ngày 10 tháng 11, còn giờ tạo là dữ liệu riêng.

## Kiểm chứng

- Đã đối chiếu DTO, JSX, quy tắc hoàn thành, logic lập lịch và migration. Cập nhật CoreChecks để kiểm tra tiến độ độc lập, checklist 1/3 và việc con chưa xong vẫn hiện khi cha hoàn thành.
- Build Release đạt 0 lỗi và 0 cảnh báo. CoreChecks thuần đạt nhóm thời gian và 13 kiểm tra cây/hạn/tiến độ. TypeScript, PostCSS và `git diff --check` đạt. EF sinh thành công SQL thêm cột `checklist` mà không kết nối database.
- Chưa áp dụng migration hoặc kiểm tra với trình duyệt/database đang dùng.

## Việc tiếp theo

- Áp dụng migration và khởi động lại BE. Thử nhiệm vụ có ba mục checklist và hai nhiệm vụ con. Tick một mục rồi chỉnh tiến độ hai con để kiểm tra cha vẫn ở 33%.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã và ghi kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code liên quan.
