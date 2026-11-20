# 2026-11-17 — Đưa đính kèm vào menu nhiệm vụ

## Yêu cầu và mục đích

- Đưa nút đính kèm vào menu ⋯ để bỏ khối trống và dòng giới hạn luôn hiện trong chi tiết. Thực hiện tiếp trên nhánh `fix/gioi-han-nhiem-vu-nam-cap`, cùng nhóm chỉnh khung chi tiết. [Hướng dẫn của TickTick](https://help.ticktick.com/articles/7055792921664028672)

## Code và logic thay đổi

- Trong repo frontend, `src/hoc-tap/NhiemVuView.tsx` thêm mục **Đính kèm tệp** có biểu tượng kẹp giấy vào menu ⋯ của khung chi tiết. Dòng danh sách vẫn dùng cùng menu cơ sở, với mục đính kèm chỉ được truyền từ khung chi tiết.
- `src/hoc-tap/TepDinhKemNhiemVu.tsx` dùng cùng trạng thái để điều khiển hộp chọn tệp, danh sách và nút trong menu. Lúc danh sách đã tải nhưng rỗng, ô chọn tệp ẩn vẫn sẵn sàng cho menu, còn phần hiển thị thu gọn hoàn toàn. Tệp có sẵn vẫn hiện giữa nội dung và cây bước nhỏ. Trong lúc xử lý, khi có lỗi hoặc cần tải lại phiên bản, khung hiển thị phản hồi tương ứng.
- Giới hạn 10 MB mỗi tệp và 20 tệp mỗi nhiệm vụ hiện trong chú thích của mục menu. Kiểm tra dung lượng, số lượng, phiên bản và API giữ nguyên. Xóa giá trị ô chọn ngay sau khi đọc tệp để lần chọn lại cùng một tệp vẫn phát sinh sự kiện.
- Cập nhật [hướng dẫn sử dụng](../database-tables.md#cách-dùng-tệp-đính-kèm), ca đính kèm trên Chromium và ca cây nhiệm vụ khi khung tệp rỗng.

## Kiểm chứng

- Đã đọc luồng menu ⋯, tải/đọc/xóa tệp và chọn lại tệp trong frontend.
- `npm.cmd run build` đạt, Vite báo chunk trên 500 kB. `git diff --check` đạt ở hai repo.
- `CORE_ATTACHMENTS_ONLY=1 node tests/browser-hoc-tap.mjs` đạt với Chromium và API giả lập. Xác nhận khung rỗng được ẩn, mục menu kích hoạt ô chọn tệp, tải lên/tải xuống, xóa, lỗi/thử lại, giới hạn dung lượng, xem WebP và chế độ di động.
- `CORE_TASK_DEPTH_ONLY=1 node tests/browser-hoc-tap.mjs` đạt, gồm vị trí cây khi khung tệp được thu gọn. Backend dùng API hiện có, kiểm tra lần này tập trung vào frontend.

## Việc tiếp theo

- Thay đổi giao diện giữ cùng API và cấu trúc dữ liệu.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.
