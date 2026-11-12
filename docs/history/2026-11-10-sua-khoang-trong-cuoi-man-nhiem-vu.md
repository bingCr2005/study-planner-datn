# 2026-11-10 — Sửa vùng cuộn dư cuối màn nhiệm vụ

## Yêu cầu và mục đích

Ảnh người dùng cho thấy trang cuộn ra ngoài khung ứng dụng và lộ một khoảng nền trống phía dưới. Danh sách và chi tiết đã có vùng cuộn riêng. Xem [bản ghi bố cục cửa sổ hẹp](2026-11-10-sua-bo-cuc-nhiem-vu-khi-cua-so-hep.md).

## Code và logic thay đổi

- Trong `Study_Planner_FE/src/hoc-tap/hoc-tap.css`, tạo điểm neo cho chữ ẩn dành cho trình đọc màn hình bên trong nhãn hạn. Chữ này dùng vị trí tuyệt đối nên cần nằm trong hệ tọa độ của nhãn để tránh tràn khỏi khung cuộn chứa nhiệm vụ.
- Giới hạn khung ứng dụng học tập trong chiều cao viewport và chặn cuộn tài liệu bên ngoài. Danh sách, chi tiết và vùng xếp dọc khi cửa sổ hẹp tiếp tục dùng quy tắc cuộn của từng bố cục. Phạm vi CSS gắn với màn có `ht-workspace`.

## Kiểm chứng

Đã đọc lại quy tắc khung trang, vùng cuộn và nhãn hạn. `git diff --check` đạt. Chưa tái hiện trên trình duyệt, nên chưa xác minh được phần tử nào tạo ra toàn bộ độ dài cuộn trong ảnh. Không chạy bộ test theo yêu cầu để người dùng tự kiểm tra.

## Việc tiếp theo

Người dùng tải lại trang, mở nhiệm vụ có nhiều bước và cuộn hết vùng danh sách hoặc chi tiết để kiểm tra phần nền bên dưới.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã nguồn và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code/diff.
