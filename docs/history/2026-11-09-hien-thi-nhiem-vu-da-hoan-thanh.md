# 2026-11-09 — Hiển thị nhiệm vụ đã hoàn thành

## Yêu cầu và mục đích

Người dùng muốn nhiệm vụ chuyển sang mục Đã hoàn thành có màu dịu như ảnh tham chiếu. Trước đây dòng đã xong vẫn dùng kiểu chữ và màu nhấn của dòng đang làm. Xem [bản ghi thao tác hoàn thành](2026-11-09-dong-bo-va-thong-bao-nhiem-vu.md).

## Code và logic thay đổi

Trong FE, `src/hoc-tap/NhiemVuView.tsx` gắn lớp `ht-task-completed` khi trạng thái từ API là `Completed`. Kiểu dòng này dùng chung cho danh sách và bảng trạng thái. `src/hoc-tap/hoc-tap.css` dùng tông xám cho tên, thông tin phụ, checkbox, tiến độ và vạch ưu tiên. Nền xám phân biệt nhiệm vụ đang được chọn. Khi bỏ tick thành công, dữ liệu tải lại có trạng thái mở và dòng trở về kiểu thông thường. Màn chi tiết tiếp tục mở được khi chọn tên nhiệm vụ.

## Kiểm chứng

Đã đối chiếu lớp CSS với trạng thái trả về và bổ sung kiểm tra lớp có mặt sau khi hoàn thành, biến mất sau khi mở lại trong `tests/browser-hoc-tap.mjs`.

`git diff --check` và `npm run build` đạt. Kịch bản Chromium đạt với 281 request giả lập. Chưa đối chiếu giao diện bằng ảnh chụp.

## Việc tiếp theo

Người dùng có thể xem kiểu mới trong mục Đã hoàn thành của FE.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code.
