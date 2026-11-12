# 2026-11-10 — Sửa bố cục nhiệm vụ khi cửa sổ hẹp

## Yêu cầu và mục đích

Người dùng mở Inspect ở cạnh phải và thấy khung chi tiết chồng lên danh sách cùng thanh cuộn ngang. Việc thu nhỏ vùng hiển thị kích hoạt bố cục hẹp. Xem [bản ghi thanh chia danh sách và chi tiết](2026-11-06-thanh-keo-va-menu-nhiem-vu-day-du.md).

## Code và logic thay đổi

- Trong `Study_Planner_FE/src/hoc-tap/hoc-tap.css`, bố cục cũ bỏ cột chi tiết ở dưới ngưỡng 1050px nhưng vẫn để danh sách và chi tiết chia hàng grid trong chiều cao cố định, với nội dung tràn ra ngoài hàng. Đổi vùng có chi tiết sang luồng khối, danh sách và chi tiết xếp dọc và dùng vùng cuộn chung. Ở kích thước nhỏ hơn, vùng học tập ngoài đảm nhiệm cuộn. Bố cục rộng dùng thanh chia sẵn có.
- Ghi đè chiều rộng tối thiểu 1024px của `body` riêng trên màn học tập hẹp. Cho hàng trên xuống dòng, thu sidebar chính và cho phép dòng nhiệm vụ xuống hàng. `src/hoc-tap/NhiemVuView.tsx` giới hạn phần thụt lề của dòng lồng sâu theo chiều rộng để chừa chỗ cho tên và thao tác.

## Kiểm chứng

Đã đọc lại quy tắc bố cục và phần thụt lề. `git diff --check` và Vite Oxc cho `NhiemVuView.tsx` đạt. Chưa kiểm thử bố cục trên trình duyệt theo yêu cầu để người dùng tự thử.

## Việc tiếp theo

Người dùng mở một nhiệm vụ, bật Inspect ở bên phải và kiểm tra danh sách với khung chi tiết khi kéo độ rộng qua ngưỡng 1050px và 800px.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã nguồn và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code/diff.
