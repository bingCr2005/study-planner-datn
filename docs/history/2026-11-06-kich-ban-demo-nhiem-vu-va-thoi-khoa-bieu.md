# 2026-11-06 — Kịch bản demo nhiệm vụ và thời khóa biểu

## Yêu cầu và mục đích

- Người dùng yêu cầu đối chiếu commit gộp PR #8, liệt kê chức năng, ràng buộc và soạn thứ tự thao tác cùng lời trình bày, nối tiếp phần tài khoản/học kỳ/môn do thành viên khác trình bày.
- Người dùng gửi thêm 10 ảnh FE hiện tại để kịch bản dùng đúng màn hình, nhãn nút và tình trạng dữ liệu.
- Đã đọc [lịch sử menu và thanh kéo](2026-11-06-thanh-keo-va-menu-nhiem-vu-day-du.md), các bản ghi tích hợp liên quan và code hiện tại.

## Code và logic thay đổi

- Thêm [kịch bản demo](../KICH-BAN-DEMO-NHIEM-VU-VA-THOI-KHOA-BIEU.md), cập nhật danh mục lịch sử.
- Trước đó chưa có tài liệu tập trung cho việc trình bày module nối tiếp phần của thành viên khác.
- Sau thay đổi có danh sách chức năng, dữ liệu minh họa, 11 bước thao tác/lời nói, bảng ràng buộc, câu hỏi thường gặp, giải thích ảnh hiện tại và đường dẫn lần theo code.
- Luồng minh họa là chọn môn có sẵn → tạo nhiệm vụ → chia nhỏ → cập nhật tiến độ/lịch sử → khai báo lịch lớp và giờ rảnh → tạo gợi ý → áp dụng → xem và xử lý phiên → minh họa thay đổi có ảnh hưởng.
- Phân biệt commit BE/frontend-preview trên GitHub với các chỉnh sửa React local trong repository FE riêng. Không sửa code nghiệp vụ hoặc phần thành viên khác trong tác vụ này.
- Nêu đúng cách tính tiến độ trung bình các bước trực tiếp, thời lượng còn lại tính theo ước lượng từng việc chưa chia tiếp, và hoàn thành phiên không tự tăng tiến độ nhiệm vụ.
- Các ảnh gợi ý 0 phút có cảnh báo thiếu ước lượng. Code loại các việc này trước khi cộng tổng nên không được hiểu bốn ô 0 là không còn việc. Giờ rảnh thứ Hai đầu tiên trong phạm vi ảnh là 09/11, sau hạn ky01 ngày 07/11; cần chuẩn bị lại dữ liệu trước khi demo.

## Kiểm chứng

- Đã lấy nội dung commit `fd670fd6c506f1cf797319e9575c629e2766955f` qua công cụ GitHub, đối chiếu danh sách file và các nhóm API.
- Đã đọc service/DTO nhiệm vụ, quy tắc phân cấp, lịch lớp, giờ rảnh, thời khóa biểu, gợi ý, quy tắc đặt phiên, trạng thái phiên và điều phối ghi; đối chiếu FE HocTapPage, NhiemVuView, LichView, LapLichView cùng ảnh người dùng gửi.
- Đã kiểm tra lại tài liệu, ví dụ số học, tên nút và liên kết local; dùng git diff --check để kiểm tra khoảng trắng.
- Không chạy lại build hoặc kiểm thử ứng dụng vì lượt này chỉ thêm tài liệu. Chưa diễn tập trên tài khoản/dữ liệu demo của người dùng; ảnh và đọc code không chứng minh toàn luồng đã chạy thành công trên tài khoản đó.

## Việc tiếp theo

- Người dùng chuẩn bị dữ liệu theo kịch bản, chạy thử theo thứ tự trước buổi trình bày; chọn đúng ngày học kỳ, thời gian trước hạn và ước lượng.
- Tài liệu lưu local, không commit hoặc push.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu tài liệu với code, ảnh và phạm vi thay đổi.
- [ ] Người dùng đã đọc tài liệu và code liên quan. Chỉ người dùng xác nhận mục này.

Commit tham chiếu là bản gộp PR #8 nêu trên; tác vụ tài liệu này chưa tạo commit mới.
