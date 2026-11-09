# 2026-11-06 — Menu học tập theo chiều dọc

## Yêu cầu và mục đích

- Người dùng yêu cầu đưa bảy mục Nhiệm vụ, Bảng trạng thái, Lịch ngày/tuần, Lịch học theo môn, Giờ rảnh, Gợi ý lịch tự học và Các phiên tự học thành hàng dọc. Người dùng hỏi có ràng buộc trong AppLayout hoặc component khiến nhóm nút bị cố định hay không.
- Đã đọc lịch sử, [bản ghi chuyển đổi đầy đủ](2026-11-06-chuyen-day-du-frontend-preview-va-sua-bo-loc.md), AppLayout, App, HocTapPage và CSS của module hiện hành.

## Code và logic thay đổi

- Nguyên nhân là `.ht-tabs` dùng flex theo hướng mặc định và nằm trong luồng phía trên nội dung của `HocTapPage.tsx`. `AppLayout.tsx` cung cấp khung trang cùng Outlet, hướng nhóm nút được quyết định trong module học tập.
- `C:/Users/ADMIN/Desktop/Study_Planner_FE/src/hoc-tap/HocTapPage.tsx` đưa navigation thành cột riêng cạnh vùng nội dung. Các handler chọn màn, trạng thái đang chọn và tham số URL tiếp tục dùng cùng logic hiện có.
- `C:/Users/ADMIN/Desktop/Study_Planner_FE/src/hoc-tap/hoc-tap.css` đặt lưới hai cột, menu rộng 155–190px, flex-direction column, nút đủ chiều rộng và trạng thái chọn. Menu bám đầu vùng cuộn trên desktop. Với màn hình rộng tối đa 800px, menu nằm trên nội dung và các nút vẫn xếp dọc.
- Phạm vi cập nhật FE gồm hai file module và `tests/browser-hoc-tap.mjs`. Các file AppLayout, App, Academic, xác thực, hồ sơ, quản trị và package đã được kiểm tra dấu nội dung với bản đầu vào, kết quả trùng khớp.

## Kiểm chứng

- Build TypeScript/Vite đạt.
- Bộ kiểm thử FE đạt 100/100 bài.
- Chuỗi UI bằng phản hồi API giả lập đạt. Có thêm kiểm tra thực tế `flexDirection = column`, đủ bảy nút, tọa độ các nút lần lượt từ trên xuống, menu nằm bên trái vùng nội dung. Các luồng nhiệm vụ, lịch, giờ rảnh và tự học trong chuỗi tiếp tục đạt.
- Đã mở ảnh desktop có menu dọc và chi tiết nhiệm vụ để kiểm tra bố cục. Ảnh trung gian nằm trong `backend/obj/fe-navigation-vertical`, được Git bỏ qua.
- Đã đọc lại phần JSX/CSS thay đổi và chạy git diff --check cho FE.

## Việc tiếp theo

- Người dùng tải lại FE để xem menu dọc. Chế độ mobile giữ các nút dọc ở phía trên nhằm dành chiều rộng cho nội dung.
- Thay đổi là local trên nhánh tích hợp FE hiện có, chờ người dùng test/đọc và quyết định commit/push.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã, vị trí nút trên trình duyệt và kết quả kiểm thử.
- [ ] Người dùng đã đọc bản ghi và code liên quan. Chỉ người dùng xác nhận mục này.
