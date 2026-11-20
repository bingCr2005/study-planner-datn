# 2026-11-16 — Xem trước ảnh đính kèm nhiệm vụ

## Yêu cầu và mục đích

- Người dùng gửi hình TickTick và yêu cầu hiện ảnh đính kèm ngay trong nhiệm vụ, mở ảnh lớn khi bấm. Xem [bản ghi tệp đính kèm](2026-11-15-don-bang-cu-va-tep-dinh-kem.md).
- Tiếp tục nhánh `fix/doi-ten-buoc-giu-tien-do` theo nhóm chỉnh sửa cùng màn Chi tiết nhiệm vụ.

## Code và logic thay đổi

- Trước đây cả ảnh và tài liệu đều là dòng tên tệp, bấm là tải xuống. Trong `Study_Planner_FE`, `src/hoc-tap/TepDinhKemNhiemVu.tsx` chuyển sang thẻ tệp. `AnhDinhKem.tsx` và `tep-dinh-kem.css` hiện ảnh trong chi tiết; bấm ảnh mở hộp xem trên nền tối, đóng bằng nút đóng, Escape hoặc bấm nền. Menu từng tệp có tải xuống/xóa, ảnh có thêm thu nhỏ/mở rộng.
- Ảnh có đuôi PNG, JPG/JPEG, WebP, GIF bắt đầu tải khi gần vùng hiển thị. Dùng API tải xuống hiện có với JWT và `no-store`, đối chiếu dấu hiệu nhận biết định dạng từ byte đầu, rồi gắn URL Blob vào thẻ `img`. Dừng lượt tải khi rời nhiệm vụ và thu hồi URL tạm. Nếu tệp không khớp định dạng hỗ trợ hoặc trình duyệt không giải mã được ảnh, menu tải xuống vẫn dùng được. Ảnh mới và ảnh đã có trong kho đi qua cùng luồng, không đổi Backend hay database.
- Cập nhật [hướng dẫn dùng tệp](../database-tables.md#cách-dùng-tệp-đính-kèm), fixture `tests/browser-hoc-tap.mjs` và bài kiểm tra `tests/browser-tep-dinh-kem.mjs`.

## Kiểm chứng

- `npm.cmd run build` đạt TypeScript và Vite trong thư mục tạm. Vẫn có cảnh báo chunk trên 500 kB. `git diff --check` đạt ở hai repo.
- Chrome headless với `CORE_ATTACHMENTS_ONLY=1` đạt: tải PDF/tải lại/lỗi/thử lại/xóa, xem WebP trong chi tiết, mở lớn từ dữ liệu đã đọc, đóng bằng Escape, thu nhỏ/mở rộng, tải xuống qua menu, từ chối hiển thị SVG đổi tên PNG và hộp xem ở chiều rộng 390 px. Đã xem ảnh chụp màn hình khung mở lớn dùng WebP kiểm thử. Lượt đầu chọn nút tải theo chữ chung khi có hai tệp, đã dùng nhãn gắn tên tệp và xác nhận lượt sau đạt.
- Kiểm thử trình duyệt dùng dữ liệu giả. Không ghi tệp thử vào kho hoặc DB của người dùng.

## Việc tiếp theo

- Tải lại Frontend rồi mở nhiệm vụ có ảnh để dùng. Tài liệu PDF và các tệp khác tiếp tục tải xuống.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
