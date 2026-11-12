# 2026-11-11 — Bỏ lời mô tả ở mục Chưa phân loại

## Yêu cầu và mục đích

Bỏ lời mô tả dưới tiêu đề để vào ngay danh sách. Xem [bản ghi bố cục nhiệm vụ](2026-11-10-lam-gon-giao-dien-nhiem-vu.md).

## Code và logic thay đổi

Trong `Study_Planner_FE`, bỏ đoạn JSX hiển thị lời mô tả khi `view === 'inbox'` trong `src/hoc-tap/HocTapPage.tsx`. Phần thân màn bắt đầu bằng ô thêm nhanh.

## Kiểm chứng

Đã đọc lại JSX và xác nhận dòng mô tả đã được bỏ. TypeScript `--noEmit` và `git diff --check` đạt. Phần giao diện trình duyệt để người dùng tự kiểm tra theo trao đổi trước đó.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu code và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code liên quan.
