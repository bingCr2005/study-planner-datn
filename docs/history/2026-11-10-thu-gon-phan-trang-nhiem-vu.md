# 2026-11-10 — Thu gọn phân trang nhiệm vụ

## Yêu cầu và mục đích

Danh sách ít nhiệm vụ vẫn hiện hai nút chuyển trang mờ và lặp số nhiệm vụ đã có ở đầu danh sách. Người dùng muốn làm gọn cụm này. Xem [luồng làm mới danh sách trong nền](2026-11-10-lam-moi-danh-sach-trong-nen.md).

## Code và logic thay đổi

- Trong `Study_Planner_FE`, `src/hoc-tap/HocTapPage.tsx` tính tổng số trang từ `tongSo` và `kichThuocTrang` mà API trả về. Nếu trang đang xem biến mất sau khi số nhiệm vụ giảm, trang tự lùi về trang cuối còn hợp lệ.
- `src/hoc-tap/NhiemVuView.tsx` chỉ render phân trang khi có ít nhất hai trang. Cụm mới gồm hai nút mũi tên có nhãn cho trình đọc màn hình và chỉ báo `Trang hiện tại / Tổng số trang`. Tổng số nhiệm vụ tiếp tục nằm ở đầu danh sách. `src/hoc-tap/hoc-tap.css` định dạng riêng cụm phân trang nhiệm vụ.

## Kiểm chứng

- Đã đối chiếu JSX, CSS và cách xử lý trang vượt quá tổng sau lần tải mới. TypeScript `--noEmit`, PostCSS và `git diff --check` đạt. Chưa chạy kiểm thử trên trình duyệt theo yêu cầu người dùng tự thử.

## Việc tiếp theo

- Kiểm tra danh sách 6 nhiệm vụ và một bộ lọc có đủ nhiệm vụ để chuyển qua nhiều trang.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã và ghi kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code liên quan.
