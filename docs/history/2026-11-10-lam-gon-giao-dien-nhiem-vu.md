# 2026-11-10 — Làm gọn giao diện danh sách và chi tiết nhiệm vụ

## Yêu cầu và mục đích

Áp dụng bố cục gọn hơn dựa trên hai ảnh người dùng gửi, ưu tiên tên việc, hạn và thao tác tại dòng. Link TickTick chuyển về trang giới thiệu khi mở, vì vậy ảnh là tham chiếu chính cho bố cục chi tiết. Xem [quy tắc checklist](2026-11-10-tach-checklist-va-tien-do-nhiem-vu-con.md) và [bố cục cửa sổ hẹp](2026-11-10-sua-bo-cuc-nhiem-vu-khi-cua-so-hep.md).

## Code và logic thay đổi

- Trong `Study_Planner_FE`, `src/layouts/AppLayout.tsx` mặc định thu sidebar chính thành thanh icon khi vào trang nhiệm vụ. Nút đổi chiều rộng vẫn hoạt động. `src/hoc-tap/HocTapPage.tsx` đặt lối tắt **Tất cả nhiệm vụ** lên đầu và thêm icon đồng bộ cho các lối tắt. Tìm theo tên hoặc nội dung nằm trong **Mở bộ lọc**. Nút tạo toàn cục và ô thêm nhanh đảm nhiệm hai kiểu tạo hiện có.
- `src/hoc-tap/hoc-tap.css` giảm khoảng cách theo hàng và đặt môn cùng dòng với tên ở vùng đủ rộng. Khung chi tiết có thanh hạn và thao tác ở đầu, ghi chú dạng nhập nhẹ, checklist và bước con phân khu bằng đường kẻ. `KhungChiaChiTiet.tsx` dùng 400 px làm mặc định mới, ưu tiên kích thước người dùng đã lưu và giữ bố cục xếp dọc khi cửa sổ hẹp.
- `src/hoc-tap/NhiemVuView.tsx` cho sửa tên mục checklist ngay tại dòng và lưu bằng Enter hoặc nút lưu. Bước con hiển thị hạn và phần trăm nếu đang làm. Với bước con có thể hoàn thành trực tiếp, checkbox dùng chung luồng xem trước ảnh hưởng, lưu và hoàn tác của danh sách. Việc có checklist riêng hoặc chia thêm bước mở chi tiết của nó để cập nhật.
- Ghi chú vẫn có nút lưu khi đã thay đổi và hỗ trợ Ctrl/Command + Enter. Thiết kế này giữ cách xử lý bản nháp và lỗi lưu hiện có.

## Kiểm chứng

- Đã đọc lại JSX/CSS, luồng lưu và điều kiện màn hẹp. TypeScript `--noEmit`, phân tích CSS bằng PostCSS, `node --check` cho kịch bản đã chỉnh và `git diff --check` đều đạt.
- Chưa kiểm tra trực quan trên trình duyệt. Kỳ vọng trong kịch bản FE đã cập nhật vị trí ô tìm kiếm và nhãn nút thêm nhanh.

## Việc tiếp theo

- Tải lại FE, mở một nhiệm vụ có ghi chú, checklist và bước con. Kiểm tra bố cục khi đóng/mở thanh icon và khi thu nhỏ cửa sổ.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã và ghi kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code liên quan.
