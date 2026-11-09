# 2026-11-06 — Đồng bộ cách trình bày với frontend-preview

## Yêu cầu và mục đích

- Người dùng đối chiếu ảnh FE React với `frontend-preview` và phản ánh cách trình bày lệch nhau. Lượt trước đã có CSS nhưng dùng nhóm nút viền và các đoạn thông tin liên tiếp, chưa chuyển đầy đủ cách trình bày từ bản cũ.
- Đã đọc lịch sử, [bản ghi menu dọc](2026-11-06-menu-hoc-tap-hang-doc.md), `frontend-preview/styles.css`, các phần task row/detail của bản cũ và module học tập React hiện tại.

## Code và logic thay đổi

- FE thuộc `C:/Users/ADMIN/Desktop/Study_Planner_FE`, nhánh tích hợp hiện hành.
- `src/hoc-tap/HocTapPage.tsx` dùng biểu tượng từ component Icon hiện có cho menu dọc và thêm dòng nhãn đầu trang.
- `src/hoc-tap/NhiemVuView.tsx` chuyển chi tiết thành vùng tiêu đề phạm vi, trạng thái, tên nhiệm vụ, mô tả, danh sách tên–giá trị, nhóm thao tác và các mục con/lịch sử. Các trường hạn, tiến độ, ưu tiên, loại, ước lượng và phần việc còn lại căn thành hai cột. Hạn kế thừa vẫn có giải thích riêng.
- Dòng nhiệm vụ có nền khi được chọn, tên/nhãn môn, tiến độ và dấu ưu tiên theo màu. Menu dùng biểu tượng và nền trạng thái chọn thay cho các nút viền tách rời. Các thao tác phụ trong chi tiết được trình bày dạng chữ, thao tác chính vẫn có viền và thao tác xóa có nền nhạt.
- `src/hoc-tap/hoc-tap.css` đồng bộ màu xanh tím của module, cỡ chữ, khoảng cách, đường chia nhẹ, bộ lọc ba cột, ô thêm nhanh và panel chi tiết rộng 310px. Vùng module dùng chiều cao được cấp bởi khung FE, danh sách và chi tiết có vùng cuộn riêng. Selector áp cho main chỉ khi main chứa trực tiếp module học tập.
- Quyết định thiết kế trong lượt này là lấy cách trình bày/màu nhấn từ frontend-preview cho module học tập, giữ bố cục menu dọc đã được yêu cầu. File khung chung AppLayout/App, dữ liệu Academic, xác thực và package được đối chiếu dấu nội dung với bản đầu vào, kết quả trùng khớp.
- Luồng API và các handler tạo/sửa/chia nhỏ/chuyển/tách/phiên tự học tiếp tục dùng mã nghiệp vụ đã có. Hai kiểm tra render cũ được cập nhật để tìm nhãn nút đang chọn khi có thêm biểu tượng.

## Kiểm chứng

- Build TypeScript/Vite thành công sau lượt sửa cuối.
- Bộ kiểm thử FE đạt 100/100 bài sau thay đổi cấu trúc thông tin và biểu tượng.
- Chuỗi browser core đạt ở desktop 1440px và 1280×720. Lượt cuối ở 1280×720 có 241 request giả lập, kiểm các luồng tạo/sửa/xóa, nhiệm vụ con, lọc, phiên học, lịch và áp dụng lịch, bao gồm khôi phục yêu cầu chờ sau tải lại.
- Đã mở ảnh chi tiết tại hai kích thước để so khoảng cách, hàng thông tin, menu, nhiệm vụ đang chọn và cuộn. Ảnh trung gian nằm trong `backend/obj/fe-style-preview`, được Git bỏ qua.
- Đã đọc lại JSX/CSS thay đổi. Git diff --check của FE đạt. Công cụ sao chép kiểm tra các file ngoài phạm vi module bằng dấu nội dung trước mỗi lần áp dụng.
- Bố cục mobile dùng quy tắc CSS hiện có để chuyển về một cột, lượt này kiểm trực tiếp giao diện desktop theo ảnh người dùng.

## Việc tiếp theo

- Người dùng tải lại FE để xem cách trình bày mới. Các thay đổi đang là local, chờ người dùng đọc/test và quyết định thời điểm commit/push.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu source, ảnh tham chiếu và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code liên quan. Chỉ người dùng xác nhận mục này.
