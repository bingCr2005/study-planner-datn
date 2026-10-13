# Module 04 — Lịch sử nhiệm vụ

Trạng thái: đặc tả dự thảo, chưa triển khai. Nguồn: [quy ước chung](00-quy-uoc-va-quyet-dinh.md), [ERD](../erd.md), [API — TaskHistory](../THIET-KE-API-SWAGGER.md). Bắt đầu cùng nhiệm vụ lá, không để đến cuối mới thêm ghi lịch sử.

## 1. Mục tiêu và phạm vi

Sinh viên xem nhiệm vụ đã được tạo/sửa/hoàn thành/mở lại/xóa như thế nào. Đây là lịch sử nghiệp vụ, khác log chẩn đoán server. Không phải cơ chế khôi phục nhiệm vụ, event sourcing hoặc nhật ký đăng nhập.

Màn hình: tab lịch sử trong chi tiết nhiệm vụ và danh sách lịch sử có lọc; cần đường xem lịch sử sau khi nhiệm vụ không còn tồn tại.

## 2. Dữ liệu và quyền

TaskHistory lưu TaskId chụp lại không có FK, TaskTitle tại thời điểm thao tác, UserId nullable, Action, OldData/NewData và ChangedAt. Chỉ các trường nhiệm vụ được cho phép vào JSON; không serialize toàn bộ User/navigation hoặc bí mật.

User chỉ đọc dòng còn liên kết với UserId của mình. Task đã xóa không làm mất quyền đọc lịch sử còn thuộc mình. Khi User bị xóa hẳn, FK đặt UserId NULL; không hứa truy vết danh tính sau xóa. Quyền quản trị với lịch sử mất liên kết và thời hạn giữ dữ liệu là Q08, chưa mở API mặc định.

## 3. Luồng ghi và đọc

1. Application lấy giá trị trước thay đổi, áp dụng nghiệp vụ và tạo snapshot sau thay đổi trong danh sách trường cho phép.
2. Chọn Action: Created/Updated/Completed/Reopened/Deleted; Created có OldData NULL, Deleted có NewData NULL.
3. Lưu TaskHistory cùng giao dịch với nhiệm vụ/tổ tiên/lời nhắc. Nếu ghi lịch sử lỗi, không commit riêng phần nhiệm vụ.
4. Tính lại tổ tiên hoặc đổi môn cả cây: ghi trước/sau cho những nhiệm vụ thực sự thay đổi. No-op như complete lặp không sinh lịch sử vô ích.
5. Khi đọc, lọc UserId trước, rồi taskId/action/thời gian và phân trang. Không đòi TaskItem đang tồn tại để đọc history.

Xóa cây phải chuẩn bị sự kiện Deleted từng node trước cascade. Xóa môn/học kỳ phải ghi Updated cho CourseId bị bỏ liên kết. Xóa User nếu có luồng riêng phải ghi trước cascade và chấp nhận mất liên kết UserId sau đó. SQL xóa trực tiếp không tự tạo các sự kiện này.

## 4. API và ngoại lệ

GET /api/task-history với taskId, action, from, to, page, pageSize theo hợp đồng API; trả 200 danh sách phân trang. Không có POST/PUT/DELETE lịch sử dành cho client. TaskId là bộ lọc snapshot, không phải bằng chứng người gọi sở hữu mọi dòng có ID đó.

Thiếu xác thực trả 401; query sai trả 400. Lọc taskId của người khác không được trả dòng của người đó. GET chi tiết task đã xóa vẫn 404, độc lập với lịch sử còn đọc được.

## 5. Tình huống nghiệm thu

| Tình huống | Kết quả kỳ vọng |
| --- | --- |
| Tạo/sửa/hoàn thành/mở lại | Action và snapshot đúng, ChangedAt do server ghi |
| Xóa cây 3 node | Có Deleted cho cả 3; đọc lại theo snapshot TaskId được |
| Xóa môn | Có lịch sử trước/sau CourseId; nhiệm vụ vẫn còn |
| Lỗi ghi một dòng history | Toàn bộ nghiệp vụ liên quan rollback |
| User B lọc TaskId của A | Không nhận history A |
| Xóa hẳn User bằng luồng riêng | History còn, UserId NULL; không còn hứa gắn danh tính |
| Kiểm tra snapshot và DTO | Không có password/hash/token hoặc dữ liệu User ngoài phạm vi |

## 6. Điểm cần chốt và điều kiện hoàn thành

Chốt Q08 và danh sách trường snapshot trước khi ghi dữ liệu; không nhận JSON lịch sử do client tự gửi. Hoàn thành khi lịch sử phản ánh đúng thay đổi/rollback và đọc được sau khi xóa task; các ca xóa xuyên module đã thử thực tế.
