# 2026-11-09 — Đồng bộ và thông báo nhiệm vụ

## Yêu cầu và mục đích

Người dùng muốn có phản hồi khi tick nhiệm vụ và một nút đồng bộ ở sidebar theo hai ảnh TickTick. Luồng cũ lưu hoàn thành rồi tải lại danh sách mà chưa hiện toast. Nút tải lại nằm trong thanh bộ lọc dùng chung của các màn học tập. Xem [bản ghi tích hợp FE](2026-11-06-tich-hop-fe-nhiem-vu-va-thoi-khoa-bieu.md) và [bản ghi toast khi xóa](2026-11-06-hop-xac-nhan-va-giai-thich-giao-dien.md).

## Code và logic thay đổi

- Repository FE `Study_Planner_FE` dùng nhánh `feat/dong-bo-va-thong-bao-nhiem-vu`. `src/hoc-tap/NhiemVuView.tsx` khóa checkbox trong lúc lưu, dùng `change-preview` và phiên bản đã đọc trước khi ghi, rồi dùng toast có sẵn sau khi `complete` hoặc `reopen` thành công. Hủy xác nhận giữ trạng thái cũ.
- `src/hoc-tap/api.ts` giữ mã `DATA_VERSION_CONFLICT` cả khi phát hiện phiên bản đã đổi ngay ở phản hồi xem ảnh hưởng. `src/hoc-tap/HocTapPage.tsx` đặt nút “Tải dữ liệu mới” cạnh lỗi xung đột khi tick. Tải lại đọc phiên bản mới để người dùng kiểm tra và chủ động thao tác lại.
- `src/layouts/AppLayout.tsx` đặt icon Đồng bộ dữ liệu học tập trên sidebar của các route nhiệm vụ và lịch. Nút gọi luồng đọc lại hiện có, thay vị trí nút trong thanh bộ lọc. Danh sách vẫn có nút thử tải lại cạnh lỗi tải. Bộ lọc và nhiệm vụ đang chọn nằm trong state/URL. Màn lịch khởi tạo lại vùng lịch như trước khi chuyển vị trí nút. CSS trong `src/hoc-tap/hoc-tap.css` định dạng nút mới.
- BE tiếp tục dùng [API nhiệm vụ](../../backend/StudyPlanner.Api/Controllers/NhiemVuController.cs) và [kiểm tra phiên bản](../../backend/StudyPlanner.Application/HocTap/DungChung/HocTapDieuPhoi.cs). Việc hoàn thành có thể ảnh hưởng phiên tự học và lịch nhắc. Mở lại theo checkbox hiện tại đặt tiến độ về 0%.

## Kiểm chứng

- Đã đọc lại luồng checkbox, preview, hiển thị lỗi, tải lại và sidebar trong mã FE, đối chiếu với API BE.
- `npm run build` thành công. `npm test` đạt 100/100.
- `node tests/browser-hoc-tap.mjs` đạt với 286 request giả lập. Đã kiểm tra xung đột ở preview chặn ghi, nút tải dữ liệu mới, hai lần bấm chỉ ghi một lần, toast hoàn thành/mở lại, đồng bộ từ sidebar và thử tải lại khi lỗi. Lượt thử giữ vùng lịch qua lần tải phát hiện có thể mở phiên cũ trước khi dữ liệu mới về. Bản cuối dùng cơ chế khởi tạo lại vùng lịch sẵn có và kiểm thử phiên tự học đạt.
- Kiểm thử trên website với cơ sở dữ liệu người dùng chưa thực hiện.

## Việc tiếp theo

Hoàn tác kiểu khôi phục cần chốt cách lưu tiến độ trước khi tick và xử lý các phiên tự học hoặc lịch nhắc bị ảnh hưởng.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code và ghi kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
