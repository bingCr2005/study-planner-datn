# 2026-11-16 — Bỏ con trỏ dấu hỏi ở nhãn hạn

## Yêu cầu và mục đích

- Người dùng thấy con trỏ dấu hỏi khi rê vào ngày hạn của nhiệm vụ. Dùng con trỏ thường để tránh nhầm với dấu báo lỗi. Xem [bản ghi tooltip ngày giờ](2026-11-10-sua-tooltip-ngay-gio-nhiem-vu.md).
- Tiếp tục nhánh `fix/doi-ten-buoc-giu-tien-do` vì đây là chỉnh sửa giao diện nhỏ trong cùng phần Nhiệm vụ.

## Code và logic thay đổi

- `Study_Planner_FE/src/hoc-tap/hoc-tap.css`: `.ht-due-label` đổi `cursor:help` thành `cursor:default`. `cursor:help` là kiểu con trỏ do trình duyệt hiện dấu hỏi.
- Nhãn hạn tiếp tục mở tooltip giờ và ngày đầy đủ khi rê chuột hoặc focus bằng bàn phím. Thay đổi CSS chỉ tác động đến hình con trỏ.

## Kiểm chứng

- Đã đọc CSS, trình xử lý tooltip và diff. `git diff --check` đạt ở hai repo. Vite build thành công trong thư mục tạm. CSS sau build có cả `cursor:default` và lớp tooltip. Cảnh báo kích thước chunk lớn vẫn xuất hiện.
- Chưa kiểm tra rê chuột trực tiếp trong trình duyệt của người dùng.

## Việc tiếp theo

- Tải lại trang Nhiệm vụ để xem con trỏ tại nhãn hạn.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
