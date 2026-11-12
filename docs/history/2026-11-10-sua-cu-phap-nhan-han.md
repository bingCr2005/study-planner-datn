# 2026-11-10 — Sửa cú pháp JSX của nhãn hạn

## Yêu cầu và mục đích

Người dùng báo Vite không mở được màn nhiệm vụ. Xem [bản ghi rút gọn hạn nhiệm vụ](2026-11-10-rut-gon-han-nhiem-vu.md).

## Code và logic thay đổi

Trong `Study_Planner_FE/src/hoc-tap/NhiemVuView.tsx`, thêm dấu `}` đóng biểu thức `className` của nhãn hạn trong dòng nhiệm vụ. Dấu bị thiếu khiến trình phân tích JSX đọc tiếp nội dung của nhãn như một phần của thuộc tính.

## Kiểm chứng

Đã đối chiếu dòng lỗi với ảnh Vite. `transformWithOxc` của Vite đọc được `NhiemVuView.tsx`, `HocTapPage.tsx` và `Icon.tsx`. Kiểm tra kiểu TypeScript bằng `tsc --noEmit -p tsconfig.app.json` và `git diff --check` đều đạt. Chưa kiểm thử bằng trình duyệt.

## Việc tiếp theo

Người dùng tải lại màn Nhiệm vụ trên Vite để xem các thay đổi giao diện.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã nguồn và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code/diff.
