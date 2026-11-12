# 2026-11-11 — Thiết kế thẻ riêng cho Bảng trạng thái

## Yêu cầu và mục đích

- Cải thiện Bảng trạng thái theo ảnh FE. Bố cục một hàng của danh sách làm tên nhiệm vụ và môn bị cắt. Nền cột giãn theo cột dài nhất làm phần trống chiếm nhiều diện tích.
- Đã đọc [bản ghi bố cục nhiệm vụ](2026-11-10-lam-gon-giao-dien-nhiem-vu.md), mã hiển thị bảng và cách API lọc trạng thái trong `NhiemVuService.DanhSachAsync`.

## Code và logic thay đổi

- Trong `Study_Planner_FE`, `src/hoc-tap/NhiemVuView.tsx` dùng thẻ riêng cho bảng. Hàng trên có tên việc, nút hoàn thành hoặc mở bước nhỏ và menu ba chấm. Môn học nằm dưới tên. Hạn, ưu tiên và phần trăm tiến độ nằm ở chân thẻ. Các thẻ bước con có thụt lề. Cả thẻ và dòng danh sách dùng chung các điều khiển và logic lưu hiện có.
- `src/hoc-tap/hoc-tap.css` cho tên và môn xuống dòng, dùng nền trung tính và chấm màu ở đầu cột. Cột dừng ở cuối nội dung. Khi chiều rộng thiếu, chỉ vùng bảng cuộn ngang để giữ chiều rộng tối thiểu cho thẻ.
- `src/hoc-tap/HocTapPage.tsx` dùng chung thao tác mở danh sách cho lối tắt sidebar và nút Xem danh sách đã hoàn thành. Nút mở từ cột trống, giữ phạm vi học kỳ và môn, về trang đầu và bỏ điều kiện trạng thái cũ. Số ở đầu cột đếm các nhiệm vụ của trang đang xem.
- Cập nhật [kịch bản demo](../KICH-BAN-DEMO-NHIEM-VU-VA-THOI-KHOA-BIEU.md) theo bố cục và đường mở mới.

## Kiểm chứng

- Đã đọc lại JSX, kiểm tra vùng cuộn và đối chiếu các điều khiển với cách hiển thị danh sách. TypeScript `--noEmit`, PostCSS và `git diff --check` đạt.
- Chưa xem bố cục trên trình duyệt theo trao đổi người dùng tự kiểm tra giao diện.

## Việc tiếp theo

- Mở bảng với tên nhiệm vụ dài và mở chi tiết để kiểm tra chiều rộng khi hai vùng cùng hiển thị.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu code và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code liên quan.

Commit/PR điền khi đã có.
