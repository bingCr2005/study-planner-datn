# 2026-11-16 — Rút gọn nhắc tiến độ từ các bước nhỏ

## Yêu cầu và mục đích

- Người dùng muốn thông tin về tiến độ trên giao diện ngắn và dễ hiểu. Quy tắc tính được giải thích ở [bản ghi tổng hợp tiến độ](2026-11-13-tong-hop-tien-do-nhiem-vu.md).

## Code và logic thay đổi

- `Study_Planner_FE/src/hoc-tap/NhiemVuView.tsx`: dưới mục Các bước nhỏ, thay đoạn giải thích chi tiết bằng “Tiến độ tự cập nhật theo các bước nhỏ.” Phần trăm vẫn hiển thị ở thanh tiến độ. Điều kiện hiển thị và cách tính tiến độ giữ như trước.

## Kiểm chứng

- Đã đọc lại diff, TypeScript `tsc -b` và `git diff --check` đạt. Không chạy lại kiểm thử trình duyệt cho thay đổi một câu.

## Việc tiếp theo

- Tải lại chi tiết một nhiệm vụ có bước nhỏ để xem câu mới.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
