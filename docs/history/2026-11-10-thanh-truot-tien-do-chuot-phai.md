# 2026-11-10 — Thanh trượt tiến độ bằng chuột phải

## Yêu cầu và mục đích

Người dùng muốn bấm chuột phải vào checkbox để mở thanh trượt như ảnh TickTick. Khi kéo, vòng tiến độ gần hạn và thanh ở đầu chi tiết cần phản hồi cùng lúc. Trước đó danh sách hiện phần trăm dạng nhãn và thanh trượt chỉ mở từ biểu mẫu cập nhật. Xem [bản ghi menu nhiệm vụ](2026-11-10-menu-thao-tac-nhiem-vu.md).

## Code và logic thay đổi

- Trong `Study_Planner_FE/src/hoc-tap/NhiemVuView.tsx`, click phải vào checkbox hoặc bấm vòng tiến độ của nhiệm vụ chưa chia nhỏ mở popover 0–100%. Chọn nhiệm vụ đó trong chi tiết. Click trái checkbox tiếp tục thực hiện hoàn thành hoặc mở lại.
- `src/hoc-tap/HocTapPage.tsx` giữ phần trăm xem trước chung cho vòng bên cạnh hạn, thanh trên đầu chi tiết và số phần trăm trong Thông tin nhiệm vụ. Khi thả con trỏ hoặc nhả phím điều chỉnh thanh trượt, lưu một lần qua cùng hàm gọi API của biểu mẫu tiến độ. Đóng popover hoặc hủy thao tác xóa giá trị xem trước. Nếu lưu lỗi, hiển thị lỗi và trở về dữ liệu đã tải.
- Luồng lưu giữ kiểm tra phiên bản và xác nhận tác động đến phiên tự học. Sau khi lưu, tải lại và mở lại nhánh đã thao tác nếu còn hiển thị. Nhiệm vụ cha dùng tiến độ tổng hợp từ BE sau lượt tải. [Dịch vụ BE](../../backend/StudyPlanner.Application/HocTap/NhiemVuService.cs) tiếp tục kiểm tra quy tắc này. `src/hoc-tap/hoc-tap.css` định dạng popover, vòng và thanh tiến độ.

## Kiểm chứng

Đã đọc lại luồng kéo, lưu và nguồn hiển thị. Vite Oxc, TypeScript `--noEmit` và `git diff --check` đều đạt. Chưa kiểm thử bằng trình duyệt theo yêu cầu để người dùng tự thử.

## Việc tiếp theo

Người dùng có thể thử kéo một bước nhỏ từ 0% lên một mốc giữa, kéo đến 100%, và kiểm tra tiến độ cha sau khi lưu.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã nguồn và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code/diff.
