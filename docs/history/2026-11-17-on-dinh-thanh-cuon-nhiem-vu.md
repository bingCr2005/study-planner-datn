# 2026-11-17 — Ổn định thanh cuộn màn Nhiệm vụ

## Yêu cầu và mục đích

- Người dùng thấy thêm thanh cuộn giữa danh sách và khung chi tiết khi danh sách có nhiều nhánh mở. Cột điều hướng bên trái cũng cuộn khi vượt chiều cao cửa sổ, nên có lúc cả hai thanh cùng hiện. Xem [bản ghi bố cục thanh bên](2026-11-17-dong-nhat-thanh-ben-hoc-tap.md) và [vùng cuộn của màn Nhiệm vụ](2026-11-10-sua-khoang-trong-cuoi-man-nhiem-vu.md).

## Code và logic thay đổi

- Frontend `src/hoc-tap/hoc-tap.css` giới hạn thay đổi trong `.ht-task-workspace`. Trước đây thanh cuộn dọc của điều hướng và thanh cuộn danh sách đều lấy kiểu mặc định của trình duyệt. Điều hướng đã có khoảng dành sẵn.
- Thanh cuộn điều hướng mảnh, hiện khi rê chuột hoặc di chuyển tiêu điểm vào cột. Cuộn chuột, cảm ứng và bàn phím tiếp tục dùng cơ chế cuộn gốc. Chế độ màu cưỡng bức dùng màu hệ thống.
- Danh sách và chi tiết dùng thanh mảnh, dành chỗ bằng `scrollbar-gutter: stable` để nội dung không đổi chiều rộng khi chọn hoặc mở nhánh. Ở bố cục xếp dọc, chỗ dành cho thanh cuộn chuyển theo vùng cuộn hiện có của khung chung hoặc màn Nhiệm vụ trên cửa sổ hẹp.

## Kiểm chứng

- Ca `CORE_TASK_SCROLL_ONLY=1 node tests/browser-hoc-tap.mjs` đạt trên Chromium. Kiểm tra danh sách và chi tiết dài/ngắn, chiều rộng các cột, vị trí cuộn độc lập ở 1440 px và vùng cuộn chung ở 980 và 700 px.
- `npm.cmd run build` và `git diff --check` đạt. Vite vẫn báo chunk trên 500 kB.

## Việc tiếp theo

- Tiếp tục trong nhánh hiện tại `fix/gioi-han-nhiem-vu-nam-cap`, cùng nhóm chỉnh bố cục Nhiệm vụ.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.
