# Module 02 — Học kỳ và môn học

Trạng thái: đặc tả dự thảo, chưa triển khai. Nguồn: [quy ước chung](00-quy-uoc-va-quyet-dinh.md), [ERD](../erd.md), [API — Semesters/Courses](../THIET-KE-API-SWAGGER.md). Giai đoạn 4.

## 1. Mục tiêu và phạm vi

Sinh viên tổ chức môn học của mình theo học kỳ. Course là môn người dùng tự thêm, không phải lớp học phần dùng chung toàn trường. Màn hình: danh sách học kỳ, chi tiết học kỳ/môn, form tạo/sửa và xem ảnh hưởng trước khi xóa.

Module này quản lý Semester/Course; mẫu buổi học thuộc [Lịch](05-lich-hoc-va-tong-quan.md), nhiệm vụ thuộc [Nhiệm vụ](03-nhiem-vu-va-tien-do.md).

## 2. Dữ liệu và quy tắc

- Semester có UserId, Name, StartDate, EndDate; EndDate >= StartDate. Có thể có các học kỳ trùng thời gian.
- Course thuộc Semester; tên bắt buộc, code/credits/lecturer/color tùy chọn theo ERD. Credits nếu có > 0; màu nếu có đúng #RRGGBB.
- Quyền sở hữu Course lấy qua Semester.UserId. Tạo môn phải kiểm tra học kỳ thuộc người gọi; không chỉ kiểm tra học kỳ tồn tại.
- Chưa có hợp đồng chuyển Course sang học kỳ khác. Không tự thêm chức năng này vào PUT.
- Biên học kỳ giới hạn lịch học; không bắt deadline nhiệm vụ phải nằm trong học kỳ. Theo đặc tả bản đầu, có thể cảnh báo trên UI nhưng không tự từ chối vì hạn ngoài học kỳ.

## 3. Luồng tạo và cập nhật

1. Người dùng tạo học kỳ bằng tên và hai ngày, server gán chủ sở hữu từ phiên.
2. Mở học kỳ, thêm môn; server xác minh quyền và dữ liệu rồi lưu Course.
3. Danh sách/chi tiết chỉ trả dữ liệu của người dùng, danh sách có phân trang theo API.
4. Khi đổi biên học kỳ, lịch môn chỉ còn các lần xuất hiện trong biên mới. Không tự sửa deadline nhiệm vụ.

## 4. Luồng xóa và ảnh hưởng

1. Giao diện gọi deletion-preview, hiển thị số môn/buổi học bị xóa và số nhiệm vụ bị bỏ liên kết.
2. Người dùng xác nhận thao tác trong sản phẩm; server kiểm tra lại quyền và dữ liệu tại lúc xóa. Preview không khóa dữ liệu cho DELETE sau đó.
3. Xóa Course: ghi lịch sử thay đổi CourseId của các nhiệm vụ liên quan, bỏ liên kết môn, xóa ClassSession và Course trong cùng giao dịch.
4. Xóa Semester: áp dụng cho các Course liên quan rồi xóa học kỳ. Nhiệm vụ/cây vẫn tồn tại với CourseId NULL.
5. Giữ UserId, ParentTaskId, hạn và tiến độ nhiệm vụ; truy vấn lọc theo học kỳ cũ không còn lấy các nhiệm vụ đã bỏ liên kết. Danh sách toàn bộ việc của người dùng vẫn phải thấy chúng.

Cascade/SET NULL không tự tạo TaskHistory. Khi nhiệm vụ được triển khai, dịch vụ xóa cần phối hợp [Lịch sử](04-lich-su-nhiem-vu.md) trước khi database xóa/bỏ liên kết. Không công bố hoàn thành luồng xóa đầy đủ trước khi thử với nhiệm vụ thật.

## 5. API liên quan

| Nhóm | Route/thao tác |
| --- | --- |
| Học kỳ | GET/POST /api/semesters; GET/PUT/DELETE /api/semesters/{id} |
| Xem ảnh hưởng | GET /api/semesters/{id}/deletion-preview |
| Môn theo học kỳ | GET/POST /api/semesters/{id}/courses |
| Chi tiết môn | GET/PUT/DELETE /api/courses/{id} |
| Xem ảnh hưởng môn | GET /api/courses/{id}/deletion-preview |

Tạo trả 201, đọc/sửa 200, xóa 204. Đầu vào sai trả 400; ID không thuộc người gọi trả 404. Cơ chế xung đột khi dữ liệu đổi sau preview phụ thuộc Q04; không hứa preview là một ảnh chụp được khóa.

## 6. Tình huống nghiệm thu

| Tình huống | Kết quả kỳ vọng |
| --- | --- |
| EndDate trước StartDate, tên trắng, credits = 0 | Từ chối và không tạo/sửa dữ liệu |
| Hai học kỳ trùng ngày | Cho phép theo ERD |
| A tạo môn vào học kỳ B | 404, dữ liệu B không đổi |
| Tải lại sau tạo/sửa | Dữ liệu vẫn tồn tại đúng |
| Thu hẹp ngày học kỳ | Lịch thay đổi phạm vi, deadline nhiệm vụ giữ nguyên |
| Xóa môn có lịch và cây nhiệm vụ | Lịch bị xóa, cả cây giữ lại với CourseId NULL, có lịch sử |
| Lỗi ghi lịch sử khi xóa môn | Rollback, không mất môn/lịch hoặc bỏ liên kết một phần |

## 7. Điểm cần chốt và điều kiện hoàn thành

Chốt Q04 cho sửa/xóa đồng thời. Thống nhất nội dung cảnh báo xóa và cách UI tìm nhiệm vụ đã bỏ liên kết. Hoàn thành mốc cơ bản khi CRUD và kiểm tra hai tài khoản đạt; bổ sung nghiệm thu xóa xuyên module ngay khi có nhiệm vụ/lịch sử.
