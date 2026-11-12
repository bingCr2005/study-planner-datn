# 2026-11-10 — Thêm bước nhỏ ngay trong chi tiết

## Yêu cầu và mục đích

Ảnh của người dùng cho thấy các thao tác đã có trong ⋯ còn lặp lại dưới thông tin nhiệm vụ. Mở biểu mẫu đầy đủ để thêm một bước có tên khiến thao tác mất thêm thời gian. Xem [bản ghi menu theo dòng](2026-11-10-menu-thao-tac-nhiem-vu.md).

## Code và logic thay đổi

- Trong repo `Study_Planner_FE`, `src/hoc-tap/NhiemVuView.tsx` dùng cùng menu ⋯ cho dòng và đầu chi tiết. Bỏ hàng nút trùng ở thân chi tiết. Các điều kiện hiển thị tiến độ, tách riêng và đổi phạm vi vẫn đi theo loại nhiệm vụ. Thao tác xóa giữ xác nhận ảnh hưởng.
- “+ Thêm bước nhỏ” mở ô nhập ngay dưới các bước hiện có. Chọn từ menu theo dòng cũng mở đúng chi tiết và đưa focus vào ô nhập. Enter hoặc nút Thêm gửi một tiêu đề đã cắt khoảng trắng đến API tạo con. [BE](../../backend/StudyPlanner.Application/HocTap/NhiemVuService.cs) tự gắn mã nhiệm vụ cha, lấy học kỳ/môn từ cha và dùng hạn kế thừa khi thiếu hạn riêng. Nội dung và các trường tùy chọn có thể sửa sau. Đường dẫn “Thêm nhiều bước hoặc tùy chọn” giữ biểu mẫu đã có cho nhu cầu nhập thêm.
- Dùng chung hàm tạo bước cho nhập nhanh và biểu mẫu đầy đủ. Hai đường đều xem ảnh hưởng đến lịch và gửi phiên bản dữ liệu. Sau khi lưu, `src/hoc-tap/HocTapPage.tsx` tải lại danh sách, đánh dấu nhánh cần mở nếu vẫn hiện theo bộ lọc. Khung chi tiết đọc cây theo lần tải mới và giữ ô nhập để thêm liên tiếp. Khi có xung đột, giữ tiêu đề và hiện nút tải dữ liệu mới.
- `src/hoc-tap/hoc-tap.css` định dạng nhóm nút trên đầu chi tiết và ô nhập tại chỗ. `tests/browser-hoc-tap.mjs` cập nhật vị trí nút, đường thêm nhanh và cách mở biểu mẫu nhiều bước.

## Kiểm chứng

Đã đối chiếu mã FE sau sửa, hợp đồng POST children của BE và `git diff --check`. Chưa chạy build hoặc bộ kiểm thử theo yêu cầu để người dùng tự kiểm tra.

## Việc tiếp theo

Người dùng thử thêm bước bằng Enter từ cả menu dòng và chi tiết, thêm hai bước liên tiếp và mở menu chi tiết từ thời khóa biểu.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã nguồn và ghi kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code/diff.
