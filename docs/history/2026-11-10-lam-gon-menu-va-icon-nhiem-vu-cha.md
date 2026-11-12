# 2026-11-10 — Làm gọn menu và icon nhiệm vụ cha

## Yêu cầu và mục đích

Người dùng muốn menu ⋯ có icon và nhóm thao tác như ảnh TickTick. Nút hình tam giác của nhiệm vụ cha cần dễ phân biệt với checkbox của bước nhỏ. Xem [bản ghi menu theo dòng](2026-11-10-menu-thao-tac-nhiem-vu.md).

## Code và logic thay đổi

- Trong `Study_Planner_FE`, `src/components/Icon.tsx` thêm biểu tượng ba chấm, nhóm nhiệm vụ và liên kết. `src/hoc-tap/NhiemVuView.tsx` dùng icon ở từng mục, chia nhóm bằng đường phân cách và làm nổi riêng mục xóa. Menu dùng chung ở danh sách và đầu chi tiết. Đo kích thước khi menu đã mở, đặt sát nút và chọn mở lên trên nếu phía dưới thiếu chỗ. Các mục tiếp tục gọi hành động hiện có.
- Dòng có bước nhỏ hiển thị icon nhóm bên cạnh chevron. Chevron hướng xuống khi nhánh mở. `src/hoc-tap/hoc-tap.css` điều chỉnh màu theo ưu tiên và làm dịu màu cho nhiệm vụ đã hoàn thành. Nhãn mở/thu gọn và trạng thái trợ năng được giữ để dùng với bàn phím.
- `tests/browser-hoc-tap.mjs` cập nhật bộ chọn nút đầu tiên sau khi menu có thêm tiêu đề.

## Kiểm chứng

Đã đọc lại mã nguồn và chạy `git diff --check`. Chưa chạy build hoặc test theo yêu cầu để người dùng tự kiểm tra giao diện.

## Việc tiếp theo

Người dùng thử menu của dòng sát đáy màn hình, menu ở đầu chi tiết và nút mở/thu gọn nhiệm vụ cha.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã nguồn và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code/diff.
