# 2026-11-09 — Chỉnh chiều xoay của nút đồng bộ

## Yêu cầu và mục đích

Người dùng thấy icon Sync xoay ngược chiều mũi tên. Xem [bản ghi hiệu ứng xoay](2026-11-09-hieu-ung-nut-dong-bo.md).

## Code và logic thay đổi

Trong `src/layouts/AppLayout.tsx` của FE, đổi góc cuối từ `360deg` sang `-360deg` để icon xoay ngược chiều kim đồng hồ, cùng hướng với đầu mũi tên của icon `reset`. Sự kiện bấm Sync hoặc lưu thành công tiếp tục khởi động một vòng xoay 650 ms.

## Kiểm chứng

Đã đối chiếu đường vẽ icon và góc xoay trong code, chạy `git diff --check`, `npm run build` và `node tests/browser-hoc-tap.mjs`. Build và bộ kiểm thử Chromium với 279 request giả lập đều đạt. Bài Chromium kiểm tra hiệu ứng được kích hoạt, chiều xoay được đối chiếu trực tiếp trong code.

## Việc tiếp theo

Chờ người dùng xác nhận chiều xoay khi mở FE.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu code và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code.
