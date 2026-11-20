# 2026-11-17 — Đồng nhất thanh bên của Nhiệm vụ và Lập kế hoạch

## Yêu cầu và mục đích

- Giữ chiều rộng, vị trí, kích thước mục và màu đang chọn đồng nhất khi chuyển giữa toàn bộ mục Nhiệm vụ, Thùng rác, Bảng trạng thái và Lập kế hoạch. [Menu dọc ban đầu](2026-11-06-menu-hoc-tap-hang-doc.md) và [căn icon thùng rác](2026-11-10-thung-rac-nhiem-vu.md) có liên quan.
- Trước sửa, lớp riêng của màn nhiệm vụ đặt thanh icon thu gọn 56 px và cột menu 184 px. Thùng rác và các màn lập kế hoạch rơi vào kiểu mặc định với thanh icon 68 px và cột menu 165 px. Kích thước nút, màu chọn và lề nội dung cũng khác theo màn.

## Code và logic thay đổi

- FE `src/layouts/AppLayout.tsx` thêm lớp ổn định theo nhóm đường dẫn học tập. `src/hoc-tap/hoc-tap.css` áp dụng kiểu thanh icon thu gọn theo lớp đó thay cho kiểm tra màn con. Giữ lựa chọn thu/mở thanh chính của người dùng.
- FE dùng một kiểu chung cho cột menu, canh icon và nhãn bằng hai cột, dành chỗ cho thanh cuộn và thống nhất màu đang chọn dù dùng `aria-current` hay `aria-pressed`. Mục **Nhiệm vụ** dẫn về nhóm danh sách, còn lối tắt tương ứng thể hiện trạng thái đang xem. Ở kích thước hẹp, các màn tiếp tục dùng cùng ngưỡng chuyển bố cục một cột.
- FE `src/hoc-tap/HocTapPage.tsx` giữ menu khi mở Thời khóa biểu bên trong Nhiệm vụ để chuyển mục mà không dời mép nội dung. Đường dẫn lịch riêng vẫn có nút **Công cụ học tập** để mở menu. Xóa lề trang và kiểu tiêu đề riêng trong `lich.css` và `study-planning.css` để dùng lề chung.
- Sửa kỳ vọng hiện có của trường hợp `/tasks?layout=calendar` trong `tests/planner.test.mjs` theo hành vi mới. Cập nhật [kịch bản demo](../KICH-BAN-DEMO-NHIEM-VU-VA-THOI-KHOA-BIEU.md). Tiếp tục trên nhánh sửa Bảng trạng thái vì cùng khu vực Nhiệm vụ.

## Kiểm chứng

- Đối chiếu diff và các bộ chọn theo màn, trạng thái đang chọn và kích thước hẹp.
- TypeScript `tsc --noEmit -p tsconfig.app.json` và bài kiểm tra dựng giao diện các đường dẫn Nhiệm vụ đạt. Chưa so sánh tọa độ thực tế trên trình duyệt.

## Việc tiếp theo

- So sánh trên trình duyệt khi chuyển lần lượt giữa các mục ở cùng chiều rộng cửa sổ. Quan sát mép cột, nhãn **Lịch học theo môn** và vị trí cuộn của menu.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng phạm vi kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan.
