# 2026-11-10 — Ghi chú trong chi tiết và sửa tên bằng một lần bấm

## Yêu cầu và mục đích

Người dùng muốn bấm tên một lần để chỉnh ngay và nhấn Enter để tiếp tục với bước mới. Khung chi tiết cần ưu tiên chỗ ghi chú có thể nhập trực tiếp. Xem [bản ghi đổi tên trên danh sách](2026-11-10-doi-ten-va-them-buoc-tren-danh-sach.md).

## Code và logic thay đổi

- Trong `Study_Planner_FE`, `src/hoc-tap/NhiemVuView.tsx` mở ô sửa tên và chọn chi tiết ngay khi bấm tên. Với dòng con trong cây đang mở, Enter lưu và mở ô nhập bước mới cùng cha ngay dưới nhánh vừa sửa, kể cả khi bên dưới đã có bước khác. Sau khi tạo thành công, ô nhập tiếp theo nằm sau bước vừa tạo. Ở danh sách phẳng, Enter từ một bước con mở ô thêm trong chi tiết của cha. Bước trống chỉ là ô nhập. Hạn mặc định tiếp tục áp dụng quy tắc hôm nay và hạn của cha.
- `src/hoc-tap/HocTapPage.tsx` giữ mã cha và vị trí đặt ô nhập qua lần tải lại. Ô nháp nằm theo thứ tự đang hiển thị. Vị trí bước đã lưu do thứ tự của cây từ API quyết định.
- Chi tiết đặt trình ghi chú dưới tiêu đề và đưa tiến độ, ưu tiên, loại việc, ước lượng và mốc sự kiện vào phần “Thông tin nhiệm vụ” có thể mở ra. Ghi chú lưu vào trường mô tả có sẵn qua API cập nhật nhiệm vụ và giữ nguyên những trường khác. Nút Lưu ghi chú hoặc Ctrl+Enter gửi thay đổi. Gõ dấu `/` ở đầu dòng mở mẫu văn bản như gạch đầu dòng, đánh số và ô đánh dấu. Các mẫu được lưu dưới dạng văn bản. CSS trong `src/hoc-tap/hoc-tap.css` định dạng ô ghi chú và con trỏ ở tên nhiệm vụ.

## Kiểm chứng

Đã đọc lại source FE, so hợp đồng cập nhật mô tả của BE và chạy `git diff --check`. Chưa chạy build hoặc test để người dùng tự kiểm tra như đã yêu cầu.

## Việc tiếp theo

Người dùng có thể thử Enter từ một bước ở giữa nhánh, nhập thêm hai bước liên tiếp và lưu ghi chú qua cả nút và phím tắt.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã nguồn và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code/diff.
