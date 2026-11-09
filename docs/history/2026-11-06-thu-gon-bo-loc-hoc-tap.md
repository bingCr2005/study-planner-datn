# 2026-11-06 — Thu gọn bộ lọc học tập

## Yêu cầu và mục đích

- Người dùng yêu cầu có nút thu gọn bộ lọc để tối ưu vùng nội dung.
- Trước khi sửa đã đọc danh mục lịch sử, [bản ghi hộp xác nhận](2026-11-06-hop-xac-nhan-va-giai-thich-giao-dien.md), HocTapPage, boLoc, CSS và các kiểm thử FE hiện tại.

## Code và logic thay đổi

- File FE trong repository `C:/Users/ADMIN/Desktop/Study_Planner_FE`: `src/hoc-tap/HocTapPage.tsx`, `src/hoc-tap/hoc-tap.css`, `tests/browser-hoc-tap.mjs`.
- Trước đó ô tìm kiếm và các bộ lọc học kỳ/môn, hiển thị, ưu tiên, loại, trạng thái, sắp xếp luôn chiếm nhiều hàng phía trên danh sách.
- Sau thay đổi, ô tìm nhiệm vụ luôn hiển thị. Các lựa chọn còn lại gom vào vùng có Mở bộ lọc/Thu gọn bộ lọc, mặc định đóng. Xóa bộ lọc và Tải lại luôn hiển thị.
- Dùng state mở/đóng riêng với query và URL. Thu gọn chỉ ẩn vùng chọn bằng hidden, không gỡ điều kiện hay đổi trang/kết quả; không phát request chỉ vì thu/mở. Không ghi tùy chọn này vào localStorage. F5 mặc định thu gọn, điều kiện trên URL vẫn có hiệu lực.
- Khi đóng có số tùy chọn và các nhãn tóm tắt dùng tên học kỳ/môn, ưu tiên, loại, trạng thái, chế độ xem và thứ tự đang chọn. Không tóm tắt phạm vi học kỳ/môn bị bỏ qua ở danh sách việc cá nhân, hoặc trạng thái bị danh sách Đã hoàn thành ghi đè. Tìm kiếm không tính vào nhãn vì ô tìm luôn thấy.
- Nút có aria-expanded và aria-controls nối với vùng chọn bằng useId. CSS cho phép các nút và nhãn xuống dòng, thu các trường thành một cột ở màn nhỏ, hỗ trợ tắt chuyển động.
- Luồng xử lý là mở vùng chọn → đổi điều kiện như trước → API lọc dữ liệu → thu gọn giữ điều kiện và hiển thị tóm tắt → Xóa bộ lọc gỡ điều kiện theo hành vi hiện có.
- Cập nhật [kịch bản demo](../KICH-BAN-DEMO-NHIEM-VU-VA-THOI-KHOA-BIEU.md) về vị trí và cách giới thiệu bộ lọc. Không đổi BE hoặc phần nguồn của thành viên khác.

## Kiểm chứng

- Build TypeScript/Vite thành công. Bộ kiểm thử FE đạt 100/100.
- Kiểm thử Chromium chạy thành công với 261 request giả lập trong toàn luồng. Đã kiểm tra mặc định thu gọn, ô tìm vẫn có kích thước hiển thị, mở vùng chọn đẩy danh sách xuống, thu/mở không đổi URL hoặc số request danh sách, giữ điều kiện và hiện tên tóm tắt, xóa bộ lọc khi đang đóng trả lại danh sách chung.
- Bộ kiểm thử trình duyệt được cập nhật để mở vùng chọn trước khi thao tác với trường đang ẩn. Luồng nhiệm vụ, chi tiết/thanh kéo, lịch lớp, giờ rảnh, gợi ý/áp dụng, phiên và hộp xóa tiếp tục đạt.
- Đã mở ảnh desktop thu gọn và đang có điều kiện để kiểm tra bố cục. Ảnh được Git bỏ qua tại `backend/obj/fe-collapsible-filters/`.
- Đã đọc lại code, kiểm tra git diff --check và so hash nguồn ngoài module học tập với đầu lượt. Không thay đổi các file nguồn đó.
- Kiểm tra dùng dữ liệu giả lập, không thay đổi database của người dùng. CSS màn nhỏ có điều chỉnh, chưa thực hiện kiểm tra thiết bị di động thật.

## Việc tiếp theo

- Người dùng tải lại FE, dùng Mở bộ lọc/Thu gọn bộ lọc phía trên danh sách.
- Chưa commit/push; tài liệu tiếp tục giữ local.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu code/tài liệu và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc và đối chiếu thay đổi. Chỉ người dùng xác nhận mục này.

Commit/PR cho thay đổi này chưa được tạo.
