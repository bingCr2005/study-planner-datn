# 2026-11-10 — Rút gọn chữ trên toast và biểu mẫu nhiệm vụ

## Yêu cầu và mục đích

Bỏ dấu chấm cuối các toast thành công và gỡ dòng hướng dẫn dán liên kết dưới ô **Nội dung**. Xem [bản ghi toast](2026-11-10-toast-va-hoan-tac-hoan-thanh.md) và [bản ghi phiên tự học](2026-11-10-luong-tu-hoc-va-bam-gio.md).

## Code và logic thay đổi

- Trong `Study_Planner_FE`, chỉnh chuỗi thành công tại nơi phát toast trong `src/hoc-tap/NhiemVuView.tsx`, `LapLichView.tsx`, `LichView.tsx` và `ThungRacView.tsx`.
- `SuaNhiemVu` bỏ dòng hướng dẫn dưới ô nội dung. Phiên tự học vẫn đọc và mở các liên kết có trong nội dung. Bỏ câu nhắc cộng tiến độ từ bước con trong cùng biểu mẫu để khớp [quy tắc checklist hiện tại](2026-11-10-tach-checklist-va-tien-do-nhiem-vu-con.md).

## Kiểm chứng

- Đã đối chiếu nội dung trước/sau và cập nhật hai kỳ vọng toast trong `tests/browser-hoc-tap.mjs`.
- TypeScript `--noEmit`, `node --check` và `git diff --check` đạt. Chưa chạy kịch bản trên trình duyệt.

## Việc tiếp theo

- Tải lại giao diện và thử hoàn thành, mở lại hoặc mở biểu mẫu sửa nhiệm vụ.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã và ghi kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code liên quan.
