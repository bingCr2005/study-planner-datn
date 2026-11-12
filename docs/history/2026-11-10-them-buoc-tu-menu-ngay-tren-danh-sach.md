# 2026-11-10 — Thêm bước từ menu ngay dưới nhiệm vụ cha

## Yêu cầu và mục đích

Khi chọn **Thêm bước nhỏ** từ menu ⋯ trên dòng nhiệm vụ, người dùng muốn nhập ngay sau các bước con trong danh sách. Trước đó mục này mở khung chi tiết, còn ô nhập trên danh sách chủ yếu xuất hiện sau khi bấm Enter ở dòng đang đổi tên. Xem [luồng đổi tên và thêm bước trên danh sách](2026-11-10-doi-ten-va-them-buoc-tren-danh-sach.md).

## Code và logic thay đổi

- Trong `Study_Planner_FE`, `src/hoc-tap/NhiemVuView.tsx` mở nhánh được chọn và hiển thị ô nhập sau các bước hiện có. Nếu chưa có bước nào, ô nhập nằm ngay dưới nhiệm vụ. Focus chuyển đến ô nhập khi nó xuất hiện. Enter lưu bằng API tạo bước và mở lại ô nhập ở cuối nhánh để tiếp tục. Escape đóng ô nhập.
- `src/hoc-tap/HocTapPage.tsx` giữ cơ chế khôi phục nhánh qua lần tải dữ liệu sau khi lưu. Đường mở chi tiết vẫn dùng khi sửa tên một nhiệm vụ con đang hiển thị riêng và không có dòng cha trong danh sách. Menu nằm trong chi tiết tiếp tục mở ô nhập của chính khung đó.
- Luồng lưu dùng lại hàm tạo bước hiện có, quy tắc hạn mặc định và kiểm tra phiên bản của BE.

## Kiểm chứng

- Đã đọc lại JSX và các trường hợp nhánh thu gọn, nhánh chưa có con, thêm tiếp sau Enter và giữ cách chèn bước từ luồng sửa tên.
- TypeScript `--noEmit` và `git diff --check` đạt. Chưa chạy trình duyệt theo yêu cầu người dùng tự thử giao diện.

## Việc tiếp theo

- Thử menu ⋯ trên một nhánh có con đang thu gọn và một nhiệm vụ chưa có con, rồi bấm Enter để kiểm tra vị trí ô nhập tiếp theo.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã và ghi kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code liên quan.
