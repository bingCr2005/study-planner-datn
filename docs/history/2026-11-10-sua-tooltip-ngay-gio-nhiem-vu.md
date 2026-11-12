# 2026-11-10 — Hiển thị tooltip ngày giờ nhiệm vụ

## Yêu cầu và mục đích

Người dùng xác nhận rê chuột vào hạn rút gọn không thấy ngày giờ đầy đủ. Nhãn cũ dựa vào thuộc tính `title` và cách trình duyệt hiển thị tooltip mặc định. Xem [bản ghi rút gọn hạn](2026-11-10-rut-gon-han-nhiem-vu.md).

## Code và logic thay đổi

- Trong `Study_Planner_FE/src/hoc-tap/NhiemVuView.tsx`, nhãn hạn mở popover riêng khi rê chuột hoặc focus bằng bàn phím. Dùng lớp hiển thị phía trên vùng cuộn để tooltip xuất hiện trong danh sách và trong khung chi tiết, kể cả chi tiết dạng hộp thoại. Đo kích thước rồi đặt sát nhãn trong phạm vi viewport. Rời nhãn, nhấn Escape, cuộn hoặc đổi kích thước sẽ đóng tooltip.
- `src/hoc-tap/hoc-tap.css` định dạng tooltip nền tối. Ngày giờ đầy đủ vẫn theo múi giờ tài khoản và vẫn có văn bản dành cho trình đọc màn hình. Hạn rút gọn giữ quy tắc trước đó.

## Kiểm chứng

Đã đọc lại nơi gọi nhãn và mã định vị tooltip. Vite Oxc, TypeScript `--noEmit` và `git diff --check` đều đạt. Chưa kiểm thử rê chuột trên trình duyệt.

## Việc tiếp theo

Người dùng tải lại và rê chuột lên hạn trong danh sách hoặc đầu khung chi tiết để kiểm tra ngày giờ đầy đủ.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã nguồn và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code/diff.
