# 2026-11-09 — Hiệu ứng xoay của nút đồng bộ

## Yêu cầu và mục đích

Người dùng muốn icon Sync xoay khi thêm nhiệm vụ hoặc thực hiện thao tác học tập, theo giao diện tham chiếu TickTick. Trước đây icon ở sidebar chỉ gửi lệnh làm mới. Xem [bản ghi thêm nút đồng bộ](2026-11-09-dong-bo-va-thong-bao-nhiem-vu.md).

## Code và logic thay đổi

- Trên nhánh FE `feat/dong-bo-va-thong-bao-nhiem-vu`, `src/layouts/AppLayout.tsx` xoay icon một vòng trong 650 ms khi nhận sự kiện làm mới hoặc lưu thành công. Lần bấm tiếp theo khởi động lại hiệu ứng. Tùy chọn giảm chuyển động của hệ điều hành được tôn trọng. CSS định dạng phần tử bọc icon.
- `src/hoc-tap/api.ts` phát sự kiện lưu sau khi kiểm tra phản hồi thành công của yêu cầu ghi có phiên bản hoặc khóa chống ghi trùng. Các lệnh xem trước và phản hồi lỗi không kích hoạt hiệu ứng. Yêu cầu ghi dữ liệu tiếp tục sử dụng API BE hiện có.

## Kiểm chứng

- Đã đối chiếu các luồng ghi nhiệm vụ và áp dụng lịch với điều kiện phát sự kiện.
- Kịch bản `tests/browser-hoc-tap.mjs` kiểm tra hiệu ứng khi lưu hoàn thành, khi bấm Sync và trường hợp xung đột phiên bản.
- Build thành công, 100/100 test đạt. Kiểm thử Chromium đạt với 285 request giả lập. Chưa thử với dữ liệu thật của người dùng.

## Việc tiếp theo

Hiệu ứng là một vòng phản hồi thao tác. Thời lượng xoay cố định và độc lập với thời gian tải dữ liệu.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
