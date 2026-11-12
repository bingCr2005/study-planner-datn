# 2026-11-12 — Giải quyết conflict mục lục lịch sử của PR 12

## Yêu cầu và mục đích

GitHub báo PR 12 xung đột tại `docs/history/README.md`. Đã xác minh nhánh đích là `main`, nhánh nguồn là `feat/loc-nhiem-vu-da-hoan-thanh`. Head local trùng head PR `1627a7e`. Main mới nhất tại thời điểm lấy về là `43e4795`.

## Code và logic thay đổi

- Đọc [bản ghi xử lý conflict trước](2026-11-06-giai-quyet-conflict-hoc-ky-mon-hoc.md) và [bản ghi thông báo nhắc hạn](2026-11-10-thong-bao-nhac-han.md).
- Gộp `origin/main` vào nhánh nguồn bằng `git merge --no-commit --no-ff origin/main`. Git chỉ báo một file xung đột.
- Hai nhánh cùng bổ sung mục lục sau dòng cuối cũ. Giữ các liên kết nhiệm vụ và lịch học từ nhánh nguồn, thêm liên kết thông báo nhắc hạn từ main trong nhóm ngày 10/11 và bỏ các dấu conflict.

## Kiểm chứng

- Đối chiếu các liên kết của bản gộp với cả hai nhánh và kiểm tra file đích tồn tại. Kiểm tra dấu conflict và khoảng trắng trong phần đã sửa.
- Kết quả kiểm tra mục lục đạt. Đủ liên kết từ cả hai nhánh, các file đích tồn tại, các mục không bị lặp và đã bỏ hết dấu conflict.
- Build API với `--no-restore` và thư mục đầu ra riêng `backend/obj/merge-pr12/bin/` đạt, 0 cảnh báo và 0 lỗi. Chưa chạy kiểm thử HTTP hoặc database trong tác vụ xử lý mục lục này.

## Việc tiếp theo

Commit bản gộp trên nhánh nguồn rồi push để GitHub cập nhật PR 12.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu nội dung gộp với hai nhánh.
- [ ] Người dùng đã đọc bản ghi và diff liên quan.
