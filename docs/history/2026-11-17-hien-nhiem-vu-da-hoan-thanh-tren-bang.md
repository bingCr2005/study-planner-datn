# 2026-11-17 — Hiện nhiệm vụ đã hoàn thành trên Bảng trạng thái

## Yêu cầu và mục đích

- Thay nút chuyển trang trong cột **Đã hoàn thành** bằng thẻ của nhiệm vụ đã xong. [Bản thiết kế cũ](2026-11-11-thiet-ke-bang-trang-thai.md) chia nhóm từ danh sách mặc định. API mặc định bỏ trạng thái Completed nên cột thường trống và chỉ hiện lối tắt.

## Code và logic thay đổi

- FE `src/hoc-tap/HocTapPage.tsx`. Khi xem bảng và chưa lọc riêng trạng thái, tải thêm một trang bằng API nhiệm vụ hiện có với `trangThai=Completed`. Dùng chung phạm vi học kỳ, môn học, từ khóa, loại việc, độ ưu tiên và chế độ đang xem. Mặc định xếp theo ngày hoàn thành mới nhất. Lựa chọn sắp xếp trên bảng vẫn áp dụng nếu có.
- Cột hoàn thành có trang riêng và kết quả gắn với cùng phiên bản dữ liệu với các cột còn lại. Thao tác cập nhật gọi luồng tải lại hiện có. Nếu hai lượt đọc khác phiên bản, cột báo tải lại để đồng bộ. Khi đang lọc riêng một trạng thái, dùng kết quả của truy vấn chính. Khi chuyển từ danh sách hoàn thành sang bảng, xóa bộ lọc riêng của danh sách đó và giữ phạm vi học kỳ/môn.
- FE `src/hoc-tap/NhiemVuView.tsx` và `hoc-tap.css`. Dùng thẻ nhiệm vụ chung để mở hoặc thao tác trên việc đã xong. Ngày hoàn thành xuất hiện cạnh dấu tích. Khi truy vấn trả về rỗng, hiện **Chưa có nhiệm vụ đã hoàn thành**. Cột hiển thị trạng thái tải hoặc nút thử lại nếu có lỗi. Khi tải riêng, số ở đầu cột là tổng sau lọc và có phân trang ngay dưới thẻ. Các mục đã hoàn thành hiển thị phẳng như danh sách hoàn thành. Nhánh chưa xong vẫn mở theo cây.
- Cập nhật [module nhiệm vụ](../modules/03-nhiem-vu-va-tien-do.md) và [kịch bản demo](../KICH-BAN-DEMO-NHIEM-VU-VA-THOI-KHOA-BIEU.md). Nhánh `fix/hien-nhiem-vu-da-hoan-thanh-tren-bang` giữ các thay đổi local có sẵn.

## Kiểm chứng

- Đã đối chiếu diff FE và quy tắc lọc, phân trang, phiên bản dữ liệu trong API nhiệm vụ. Chưa chạy build hoặc trình duyệt theo yêu cầu trước đó của người dùng.

## Việc tiếp theo

- Trên trình duyệt, hoàn thành một việc đang mở rồi xem việc chuyển cột. Thử lọc theo môn và kiểm tra phân trang khi có hơn 20 việc đã xong.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng phạm vi kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan.
