# 2026-11-17 — Làm mảnh đường chia chi tiết nhiệm vụ

## Yêu cầu và mục đích

- Tạo một đường chia mảnh giữa danh sách và chi tiết theo ảnh tham chiếu. Dải nền có vạch ngắn ở giữa khiến vùng kéo trông giống một thanh cuộn. Xem [thao tác kéo ban đầu](2026-11-06-thanh-keo-va-menu-nhiem-vu-day-du.md) và [bản ghi vùng cuộn](2026-11-17-on-dinh-thanh-cuon-nhiem-vu.md).

## Code và logic thay đổi

- Frontend `src/hoc-tap/hoc-tap.css` thay nền và vạch ngắn bằng một đường 1 px xuyên suốt chiều cao. Vùng tương tác 8 px trong suốt. Khi rê chuột, kéo hoặc dùng bàn phím tới vùng chia, đường đổi màu. Chế độ màu cưỡng bức dùng màu hệ thống.
- `src/hoc-tap/KhungChiaChiTiet.tsx` bỏ phần tử trang trí và giữ `role="separator"`. Kéo chuột hoặc bấm phím mũi tên thực hiện đổi độ rộng như trước. Bố cục xếp dọc tiếp tục ẩn đường chia tại ngưỡng 1050 px. Code hiện đặt mặc định 400 px, thay cho 310 px ghi trong bản ghi cũ.
- `tests/browser-hoc-tap.mjs` bổ sung kiểm tra đường vẽ, vùng bắt chuột, thao tác kéo, phím mũi tên và bố cục hẹp vào ca kiểm tra cuộn.

## Kiểm chứng

- Đã đối chiếu CSS hiện tại và luồng xử lý trong `KhungChiaChiTiet.tsx`.
- `CORE_TASK_SCROLL_ONLY=1 node tests/browser-hoc-tap.mjs` đạt trên Chromium. Xác nhận đường 1 px, vùng kéo 8 px, kéo chuột sang trái 32 px, bấm mũi tên phải 16 px và ẩn đường chia khi xếp dọc.
- `npm.cmd run build` và `git diff --check` đạt. Vite vẫn báo chunk trên 500 kB.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.
