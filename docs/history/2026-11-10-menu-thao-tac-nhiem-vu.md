# 2026-11-10 — Menu thao tác ngay trên dòng nhiệm vụ

## Yêu cầu và mục đích

Người dùng muốn áp dụng các thao tác phù hợp từ ảnh TickTick vào source FE, ưu tiên thêm bước nhỏ lần đầu và chỉnh tiến độ tại danh sách. Trước đó các thao tác này nằm trong chi tiết. Xem [bản ghi nút đồng bộ và thao tác hoàn thành](2026-11-09-dong-bo-va-thong-bao-nhiem-vu.md).

## Code và logic thay đổi

- Trong repository `Study_Planner_FE`, `src/hoc-tap/NhiemVuView.tsx` thêm nút ⋯ dùng popover của trình duyệt cho từng dòng. Menu có thêm bước nhỏ, chỉnh sửa hạn và ưu tiên qua biểu mẫu hiện có, cập nhật tiến độ cho nhiệm vụ lá, chuyển vào nhiệm vụ lớn hoặc tách riêng, đổi học kỳ/môn cho nhiệm vụ gốc, xem phiên tự học, sao chép liên kết và xóa. `src/hoc-tap/hoc-tap.css` định vị menu trong viewport và giữ màu phân biệt hạn quá hạn/đã hoàn thành.
- Khi API tạo bước nhỏ thành công, `src/hoc-tap/HocTapPage.tsx` chọn nhiệm vụ vừa được thêm bước và ghi đường dẫn của nhánh cho lượt tải lại tiếp theo. Danh sách tải lại cây và tự mở nhánh nếu nhiệm vụ gốc còn hiển thị theo bộ lọc. Nếu bộ lọc ẩn nhánh, bước mới vẫn xuất hiện trong chi tiết. Khung chi tiết đặt nút thêm ngay dưới mục Các bước nhỏ, kể cả khi danh sách bước còn trống.
- Biểu mẫu tiến độ đồng bộ ô số với thanh trượt 0–100. Lưu qua API và kiểm tra ảnh hưởng sẵn có. Luồng xóa dùng chung bước xem ảnh hưởng, xác nhận và phiên bản dữ liệu ở cả menu lẫn chi tiết. Các tùy chọn ghim, nhãn, chuyển ghi chú và chế độ bỏ qua trong ảnh chưa được thêm.

## Kiểm chứng

- Bản đầu build thành công và 100/100 test đạt. Lượt Chromium dừng ở bước tìm nút Sao chép liên kết vì bộ chọn lấy cả nút trong menu đang ẩn. Đã cập nhật bộ chọn để chỉ nhận nút hiển thị.
- Bản cuối đã đối chiếu mã nguồn. Theo yêu cầu mới của người dùng, dừng chạy build và test, để người dùng tự thử trên FE.

## Việc tiếp theo

Người dùng tự kiểm tra thêm bước đầu tiên từ ⋯, tự mở nhánh, thanh trượt tiến độ và các hành động có xác nhận.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu thay đổi với mã nguồn và ghi kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code.

Commit/PR liên quan: chưa tạo.
