# 2026-11-11 — Làm gọn bốn màn lập lịch học

## Yêu cầu và mục đích

- Làm đồng nhất bố cục Lịch học theo môn, Giờ rảnh của tôi, Gợi ý lịch học và Phiên tự học. Thay các đoạn hướng dẫn kiểu “Tiếp theo” bằng điều khiển và thông tin có ích ngay tại chỗ. Nút tạo nhiệm vụ xuất hiện ở nơi phù hợp với việc quản lý nhiệm vụ.
- Đã đọc [luồng tự học và bấm giờ](2026-11-10-luong-tu-hoc-va-bam-gio.md), [bản ghi căn thanh lịch](2026-11-11-can-thang-thanh-dieu-khien-lich.md), mã FE và cách API chọn nhiệm vụ trong `LapLichService.XemTruocAsync`.

## Code và logic thay đổi

- Trong `Study_Planner_FE`, `src/hoc-tap/HocTapPage.tsx` đặt ô chọn học kỳ và môn ngay trên Lịch học theo môn. Đổi môn nạp đúng danh sách mới. Giờ rảnh theo tài khoản nên ẩn bộ lọc học kỳ và môn ở màn đó. Gợi ý và Phiên tự học vẫn có bộ lọc trên hàng tiêu đề khi lọc được.
- `src/hoc-tap/LichView.tsx` thay các đoạn giới thiệu dài bằng danh sách gọn, nút chính theo từng màn và trạng thái trống ngắn. Lịch học và giờ rảnh sắp theo thứ, giờ. Phiên tự học dùng cụm điều hướng ngày giống thời khóa biểu, gom phiên theo ngày và có nút Mở phiên. Các thao tác lưu lịch, xem ảnh hưởng, áp dụng gợi ý và bấm giờ dùng luồng hiện có.
- `src/hoc-tap/LapLichView.tsx` thay ô chọn nhiều bằng hai chế độ rõ ràng. Tất cả nhiệm vụ phù hợp gửi danh sách mã rỗng theo hợp đồng API. Chọn nhiệm vụ hiển thị tìm kiếm và checkbox, giữ lựa chọn khi đổi từ khóa trong cùng phạm vi và chỉ gửi các mã đã đánh dấu. Kết quả nhóm theo ngày và tách cảnh báo thiếu dữ liệu hoặc thiếu giờ khỏi các phiên có thể lưu. Thử lưu lại sau khi mất kết nối vẫn dùng cùng khóa và nội dung yêu cầu.
- `src/layouts/AppLayout.tsx` nhận trạng thái `showCreate` từ tab đang mở. Nút trên thanh chung chỉ hiện ở danh sách nhiệm vụ và bảng trạng thái, trừ danh sách Đã hoàn thành. Hôm nay có nút riêng trong nội dung. Tạo nhiệm vụ từ Thời khóa biểu nằm trong Thêm vào lịch. Bốn màn lập lịch có nút theo đúng công việc của mình.
- `src/hoc-tap/study-planning.css` thống nhất chiều cao nút, cách trình bày dòng dữ liệu, nhãn trạng thái và vùng trống. `lich.css` dùng chung kiểu điều hướng ngày. Cập nhật [kịch bản demo](../KICH-BAN-DEMO-NHIEM-VU-VA-THOI-KHOA-BIEU.md) và nhãn nút trong `tests/browser-hoc-tap.mjs`.

## Kiểm chứng

- Đã đọc lại JSX, CSS và đối chiếu API xem trước. TypeScript `--noEmit`, phân tích CSS bằng PostCSS, `node --check` cho kịch bản FE và `git diff --check` đạt.
- Chưa chạy trình duyệt với dữ liệu thật theo trao đổi người dùng tự kiểm tra giao diện.

## Việc tiếp theo

- Kiểm tra bốn màn khi đổi học kỳ và môn. Thử tạo một gợi ý từ các nhiệm vụ được đánh dấu rồi áp dụng và mở phiên đã lưu.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR điền khi đã có.
