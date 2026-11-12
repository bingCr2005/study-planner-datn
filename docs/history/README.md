# Lịch sử thay đổi và logic code

Thư mục này giúp nhớ lại đã làm gì, vì sao làm và logic hoạt động thế nào. Tài liệu hỗ trợ đọc hiểu code; mỗi lần AI sửa, người dùng vẫn cần mở bản ghi và đối chiếu code/diff trước khi đưa thay đổi lên repository.

## Cách sử dụng

1. Trước khi sửa, đọc các bản ghi liên quan rồi mở code hiện tại để xác minh.
2. Sau khi sửa, tạo bản ghi theo `TEMPLATE.md`, đặt tên `YYYY-MM-DD-mo-ta-ngan.md`. Nếu cùng ngày có nhiều lần sửa, thêm hậu tố phân biệt.
3. Ghi cụ thể logic trước/sau, mục đích, file liên quan và kết quả kiểm tra; bổ sung liên kết vào danh sách dưới đây.
4. Khi đọc lại, lần theo các file được dẫn, xem diff và kiểm tra các tình huống được ghi. Điểm chưa hiểu hoặc chưa xác minh phải để rõ trong bản ghi.
5. Trước khi commit/push, đọc lại code và bản ghi. AI chỉ đánh dấu phần AI đã kiểm tra; người dùng tự xác nhận phần mình đã đọc.

Không sửa bản ghi cũ để làm mất lịch sử. Khi logic thay đổi, thêm bản ghi mới và dẫn lại bản ghi trước. Không chép cấu hình chứa bí mật vào đây.

## Danh sách bản ghi

- [2026-10-13: Trạng thái ban đầu và quy trình lưu lịch sử](2026-10-13-trang-thai-ban-dau.md)
- [2026-10-13: Thiết kế ERD và quy tắc nghiệp vụ](2026-10-13-thiet-ke-erd.md)
- [2026-10-13: Đánh giá hiện trạng và kế hoạch phát triển](2026-10-13-danh-gia-va-ke-hoach.md)
- [Mẫu cho mỗi lần sửa](TEMPLATE.md)
- [2026-10-13: Đồng bộ .NET 10 và SDK 10.0.400](2026-10-13-nang-cap-dotnet-10.md)
- [2026-10-13: Đặc tả nghiệp vụ theo module](2026-10-13-dac-ta-nghiep-vu-module.md)
- [2026-10-13: Ghi chú kiến trúc backend](2026-10-13-ghi-chu-kien-truc-backend.md)
- [2026-10-13: Kế hoạch chi tiết và thiết kế API/Swagger](2026-10-13-ke-hoach-chi-tiet-api-swagger.md)
- [2026-10-13: Đặc tả nghiệp vụ bản đầu và phạm vi gợi ý](2026-10-13-dac-ta-nghiep-vu-ban-dau.md)

- [2026-11-04: API học kỳ/môn học, quyền sở hữu và xóa giữ nhiệm vụ](2026-11-04-api-hoc-ky-mon-hoc.md)
- [2026-11-06: Giải quyết conflict khi gộp main vào nhánh học kỳ/môn học](2026-11-06-giai-quyet-conflict-hoc-ky-mon-hoc.md)
- [2026-11-06 — Tích hợp FE nhiệm vụ và thời khóa biểu, chọn dữ liệu theo tên](2026-11-06-tich-hop-fe-nhiem-vu-va-thoi-khoa-bieu.md)
- [2026-11-06 — Chuyển đầy đủ chức năng frontend-preview và sửa bộ lọc ẩn việc cá nhân](2026-11-06-chuyen-day-du-frontend-preview-va-sua-bo-loc.md)
- [2026-11-06 — Chuyển menu học tập sang hàng dọc](2026-11-06-menu-hoc-tap-hang-doc.md)
- [2026-11-06 — Đồng bộ CSS và bảng chi tiết theo frontend-preview](2026-11-06-dong-bo-css-voi-frontend-preview.md)
- [2026-11-06 — Thanh kéo chia chi tiết và đầy đủ mục menu nhiệm vụ](2026-11-06-thanh-keo-va-menu-nhiem-vu-day-du.md)
- [2026-11-06 — Kịch bản demo nhiệm vụ và thời khóa biểu](2026-11-06-kich-ban-demo-nhiem-vu-va-thoi-khoa-bieu.md)
- [2026-11-06 — Hộp xác nhận và giải thích giao diện học tập](2026-11-06-hop-xac-nhan-va-giai-thich-giao-dien.md)
- [2026-11-06 — Thu gọn bộ lọc học tập](2026-11-06-thu-gon-bo-loc-hoc-tap.md)
- [2026-11-06 — Lựa chọn phạm vi buổi học rõ ràng](2026-11-06-lua-chon-pham-vi-buoi-hoc.md)
- [2026-11-09 — Chia thư mục học tập theo chức năng](2026-11-09-chia-thu-muc-hoc-tap.md)
- [2026-11-09 — Đồng bộ và thông báo nhiệm vụ](2026-11-09-dong-bo-va-thong-bao-nhiem-vu.md)
- [2026-11-09 — Hiệu ứng xoay của nút đồng bộ](2026-11-09-hieu-ung-nut-dong-bo.md)
- [2026-11-09 — Chỉnh chiều xoay của nút đồng bộ](2026-11-09-chinh-chieu-xoay-dong-bo.md)
- [2026-11-09 — Hiển thị nhiệm vụ đã hoàn thành](2026-11-09-hien-thi-nhiem-vu-da-hoan-thanh.md)
- [2026-11-09 — Đưa hạn hoàn thành lên đầu chi tiết](2026-11-09-dua-han-len-dau-chi-tiet.md)
- [2026-11-10 — Menu thao tác ngay trên dòng nhiệm vụ](2026-11-10-menu-thao-tac-nhiem-vu.md)
- [2026-11-10 — Thêm bước nhỏ ngay trong chi tiết](2026-11-10-them-buoc-nho-tai-cho.md)
- [2026-11-10 — Đổi tên và thêm bước ngay trên danh sách](2026-11-10-doi-ten-va-them-buoc-tren-danh-sach.md)
- [2026-11-10 — Ghi chú trong chi tiết và sửa tên bằng một lần bấm](2026-11-10-ghi-chu-va-sua-ten-mot-lan-bam.md)
- [2026-11-10 — Rút gọn hạn nhiệm vụ](2026-11-10-rut-gon-han-nhiem-vu.md)
- [2026-11-10 — Làm gọn menu và icon nhiệm vụ cha](2026-11-10-lam-gon-menu-va-icon-nhiem-vu-cha.md)
- [2026-11-10 — Sửa cú pháp JSX của nhãn hạn](2026-11-10-sua-cu-phap-nhan-han.md)
- [2026-11-10 — Sửa bố cục nhiệm vụ khi cửa sổ hẹp](2026-11-10-sua-bo-cuc-nhiem-vu-khi-cua-so-hep.md)
- [2026-11-10 — Sửa vùng cuộn dư cuối màn nhiệm vụ](2026-11-10-sua-khoang-trong-cuoi-man-nhiem-vu.md)
- [2026-11-10 — Hiển thị tooltip ngày giờ nhiệm vụ](2026-11-10-sua-tooltip-ngay-gio-nhiem-vu.md)
- [2026-11-10 — Thanh trượt tiến độ bằng chuột phải](2026-11-10-thanh-truot-tien-do-chuot-phai.md)
- [2026-11-10 — Làm mới danh sách nhiệm vụ trong nền](2026-11-10-lam-moi-danh-sach-trong-nen.md)
- [2026-11-10 — Lọc và nhóm nhiệm vụ đã hoàn thành theo ngày](2026-11-10-loc-nhiem-vu-da-hoan-thanh.md)
- [2026-11-10 — Thùng rác nhiệm vụ và căn icon sidebar](2026-11-10-thung-rac-nhiem-vu.md)
- [2026-11-10 — Căn toast học tập và hoàn tác khi tick nhiệm vụ](2026-11-10-toast-va-hoan-tac-hoan-thanh.md)
- [2026-11-10 — Nối luồng tự học, bấm giờ và cập nhật tiến độ](2026-11-10-luong-tu-hoc-va-bam-gio.md)
- [2026-11-10 — Thêm bước từ menu ngay dưới nhiệm vụ cha](2026-11-10-them-buoc-tu-menu-ngay-tren-danh-sach.md)
- [2026-11-10 — Thu gọn phân trang nhiệm vụ](2026-11-10-thu-gon-phan-trang-nhiem-vu.md)
- [2026-11-10 — Tách checklist và tiến độ nhiệm vụ con](2026-11-10-tach-checklist-va-tien-do-nhiem-vu-con.md)
- [2026-11-10 — Rút gọn chữ trên toast và biểu mẫu nhiệm vụ](2026-11-10-rut-gon-chu-toast-va-bieu-mau-nhiem-vu.md)
- [2026-11-10 — Làm gọn giao diện danh sách và chi tiết nhiệm vụ](2026-11-10-lam-gon-giao-dien-nhiem-vu.md)
- [2026-11-10 — Chuyển mô tả và checklist trong chi tiết nhiệm vụ](2026-11-10-chuyen-mo-ta-va-checklist.md)
- [2026-11-11 — Cải tiến chế độ xem thời khóa biểu](2026-11-11-cai-tien-thoi-khoa-bieu.md)
- [2026-11-11 — Đưa hoạt động nhiệm vụ vào menu chi tiết](2026-11-11-lam-gon-hoat-dong-nhiem-vu.md)
- [2026-11-11 — Căn thẳng thanh điều khiển thời khóa biểu](2026-11-11-can-thang-thanh-dieu-khien-lich.md)
- [2026-11-11 — Làm gọn bốn màn lập lịch học](2026-11-11-lam-gon-man-lap-lich-hoc.md)
- [2026-11-11 — Bỏ lời mô tả ở mục Chưa phân loại](2026-11-11-bo-mo-ta-chua-phan-loai.md)
- [2026-11-11 — Thiết kế thẻ riêng cho Bảng trạng thái](2026-11-11-thiet-ke-bang-trang-thai.md)
- [2026-11-11 — Kịch bản kiểm thử tự học từ tạo việc đến hoàn thành](2026-11-11-kiem-thu-luong-tu-hoc-dau-cuoi.md)
