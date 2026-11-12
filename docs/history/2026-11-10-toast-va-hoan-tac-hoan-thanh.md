# 2026-11-10 — Căn toast học tập và hoàn tác khi tick nhiệm vụ

## Yêu cầu và mục đích

- Dựa vào ảnh TickTick do người dùng gửi, đưa toast về giữa mép dưới và giảm độ nổi của màu nền. Hiển thị mũi tên hoàn tác rõ ràng khi có thể mở lại nhiệm vụ vừa tick.
- Nối tiếp [đồng bộ và thông báo nhiệm vụ](2026-11-09-dong-bo-va-thong-bao-nhiem-vu.md) và [hộp xác nhận học tập](2026-11-06-hop-xac-nhan-va-giai-thich-giao-dien.md).

## Code và logic thay đổi

- Trước đây toast thành công của module học tập dùng nền xanh nhạt, cố định ở góc phải và chỉ có nút đóng. Nay `Study_Planner_FE/src/hoc-tap/hoc-tap.css` căn giữa mép dưới, dùng nền xám đậm, chữ trắng và icon vàng cho hoàn tác. Chiều rộng giới hạn theo màn hình, tên nhiệm vụ dài rút gọn trên một dòng và vẫn đọc được đầy đủ trong nội dung của thông báo. Lỗi vẫn hiển thị tại vùng thao tác hiện có.
- `src/hoc-tap/HocTapPage.tsx` lưu nội dung và hành động tùy chọn của toast, giữ thời gian tự ẩn 7 giây. `src/components/Icon.tsx` có mũi tên quay lại và nút được đặt tên để dùng bằng bàn phím hoặc trình đọc màn hình.
- `src/hoc-tap/NhiemVuView.tsx` giữ tiến độ trước khi tick và phiên bản từ phản hồi hoàn thành. Khi người dùng bấm hoàn tác, FE xem trước ảnh hưởng rồi gọi API `reopen` với tiến độ cũ. Ví dụ, nhiệm vụ đang 40% sẽ về 40%. Nếu phiên bản đã đổi, FE dùng luồng báo xung đột hiện có. Nếu hoàn thành đã hủy phiên tự học dự kiến, toast chỉ hiện nút đóng. Hoàn tác qua API này khôi phục tiến độ và trạng thái nhiệm vụ, còn lịch nhắc đã hủy vẫn tuân theo nghiệp vụ BE hiện tại.

## Kiểm chứng

- Đã đối chiếu diff JSX/CSS và nhánh `reopen` trong [NhiemVuService](../../backend/StudyPlanner.Application/HocTap/NhiemVu/NhiemVuService.cs).
- TypeScript `tsc --noEmit -p tsconfig.app.json` và `git diff --check` trên bốn file FE đều đạt. Chưa kiểm tra bằng trình duyệt theo yêu cầu người dùng tự thử.

## Việc tiếp theo

- Khi xem trên giao diện, có thể tick một nhiệm vụ đang có tiến độ rồi bấm mũi tên để kiểm tra vị trí, màu và phần trăm được khôi phục.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code liên quan.
