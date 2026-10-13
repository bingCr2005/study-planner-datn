# Module 06 — Thông báo nhắc hạn

Trạng thái: đặc tả dự thảo, chưa triển khai. Nguồn: [quy ước chung](00-quy-uoc-va-quyet-dinh.md), [ERD](../erd.md), [API — Notifications](../THIET-KE-API-SWAGGER.md). Chuẩn bị lịch nhắc khi làm nhiệm vụ, phát nền ở giai đoạn 8.

## 1. Mục tiêu và phạm vi

Nhắc sinh viên về nhiệm vụ có hạn và cho phép đọc thông báo trong ứng dụng. Màn hình: chuông/số chưa đọc, hộp thông báo và cài đặt giờ nhắc trong hồ sơ/nhiệm vụ.

Chỉ có DeadlineReminder, một bản ghi tối đa cho mỗi (TaskId, Type); chưa email/push, nhiều mốc, nút tắt nhắc hoặc lịch sử từng lần phát. ERD hiện nhắc cả cha và con đủ điều kiện, có thể tạo nhiều thông báo cho một bài tập chia nhỏ; cần trình bày rõ hoặc chốt thay đổi Q06.

## 2. Dữ liệu và tính lịch nhắc

Notification thuộc User và TaskItem; hai chủ sở hữu phải trùng. Server quản lý trạng thái, SentAt, ReadAt, AttemptCount và NextAttemptAt; client không tự đánh dấu thông báo đã phát.

Giờ nhắc hiệu lực = TaskItem.ReminderHours nếu có, nếu NULL thì User.ReminderHours. Giá trị 0 nghĩa là nhắc đúng hạn, không có nghĩa tắt nhắc. Hạn dùng hạn hiệu lực, kể cả hạn kế thừa.

RemindAt = hạn hiệu lực − giờ nhắc hiệu lực. Nếu mốc nhắc đã qua và hạn còn ở tương lai, lên lịch ngay. Không tạo lời nhắc trước hạn mới cho nhiệm vụ đã quá hạn. Quy tắc tại đúng hạn và cách xử lý backlog sau downtime phải chốt Q07.

Không hạn hiệu lực/đã hoàn thành thì không có lời nhắc chờ hợp lệ. Khi bỏ hạn làm lời nhắc cũ mất điều kiện, luồng cập nhật phải vô hiệu hóa lời nhắc chờ; chi tiết chuẩn hóa trạng thái cần chốt cùng Q07 để worker không phát lịch cũ.

## 3. Vòng đời và ảnh hưởng thao tác

| Sự kiện | Xử lý theo thiết kế hiện tại |
| --- | --- |
| Tạo task đủ điều kiện | Tạo Pending với lịch nhắc |
| Đổi hạn/cha/giờ nhắc/mặc định User | Tính lại những lời nhắc bị ảnh hưởng trong cơ chế nhất quán |
| Tạo lịch nhắc mới hợp lệ cho bản ghi đã Delivered | Chuyển Pending, xóa SentAt/ReadAt, đặt lại bộ đếm; thay thế lần nhắc cũ |
| Hoàn thành | Pending/Failed → Cancelled; Delivered vẫn đọc được |
| Mở lại | Tính lại điều kiện và lịch nhắc |
| Phát thành công | Delivered và ghi SentAt; ReadAt NULL |
| Đọc | Ghi ReadAt lần đầu; gọi lại không đổi thời điểm đọc đầu |
| Lỗi xử lý | Tăng AttemptCount, lên NextAttemptAt; tối đa 3 lần thử, hết lượt giữ Failed/NextAttemptAt NULL |
| Xóa task/cây | Notification liên quan bị xóa theo FK |

SentAt trong bản đầu nghĩa là đã có thể hiển thị trong ứng dụng, không phải xác nhận người dùng đã thấy hoặc email đã gửi. Hộp thông báo chỉ hiển thị Delivered; Pending/Failed/Cancelled không tính là thông báo chưa đọc.

## 4. Luồng tác vụ nền

1. BackgroundService đang chạy chọn Pending đến RemindAt hoặc Failed đến NextAttemptAt còn lượt, theo lô.
2. Tạo scope; trong giao dịch nhận/khóa dòng và đọc lại task, hạn hiệu lực, thiết lập tài khoản liên quan.
3. Xác minh còn hợp lệ trước khi phát; phối hợp thứ tự khóa với luồng sửa task/hồ sơ theo Q04/Q07.
4. Cập nhật trạng thái/thời điểm hoặc thông tin thử lại, commit; không phát dựa trên ảnh chụp đã lỗi thời sau khi sửa task commit.
5. Lần chạy sau tiếp tục từ trạng thái database. Backend dừng/ngủ thì worker không chạy; khởi động lại phải xét backlog theo chính sách đã chốt.

Nếu nhiều instance chạy, phải có cơ chế nhận việc chống xử lý trùng. Thử lại không được tạo nhiều bản ghi cho cùng (TaskId, Type). Khoảng retry và đồng bộ khi đổi mặc định User cần thiết kế trước worker, không chỉ thêm timer.

## 5. API và quyền

| Method | Route | Hành vi |
| --- | --- | --- |
| GET | /api/notifications | Danh sách Delivered của mình, unreadOnly và phân trang |
| GET | /api/notifications/unread-count | Đếm Delivered với ReadAt NULL của mình |
| PATCH | /api/notifications/{id}/read | 204, đánh dấu đã đọc theo hợp đồng |
| POST | /api/notifications/read-all | 204, chỉ Delivered chưa đọc hiện có của mình |

Chỉ cho đọc dữ liệu của mình; read không được biến Pending thành đã phát. ID tài khoản khác trả 404. Chi tiết lỗi cho read một bản ghi chưa Delivered cần chốt trước code (đề xuất 409); đây chưa phải quy ước đã có trong bảng API. Không mở endpoint phát thử công khai.

## 6. Tình huống nghiệm thu

| Tình huống | Kết quả kỳ vọng |
| --- | --- |
| Task con kế thừa hạn; ReminderHours NULL | Lấy đúng hạn tổ tiên và mặc định User |
| Đổi hạn/giờ nhắc trước khi worker phát | Không phát theo lịch cũ sau commit sửa |
| Complete rồi reopen | Hủy chờ rồi tính lại nếu đủ điều kiện |
| Lời nhắc đã Delivered được đặt lịch mới | Bản ghi thay thế có Pending/SentAt NULL/ReadAt NULL |
| Chỉ có Pending | Hộp thông báo và unread count không tính Pending |
| Đọc lặp/read-all | Không đổi lần đọc đầu, không sửa task người khác hoặc Pending |
| Worker restart/hai worker/lỗi retry | Không trùng xử lý, đúng giới hạn thử và chính sách backlog |
| Mốc đúng hạn với giờ nhắc 0 | Khớp chính sách biên Q07 đã chốt |

## 7. Điểm cần chốt và điều kiện hoàn thành

Chốt Q04, Q06, Q07; bổ sung hợp đồng read chưa Delivered. Lập lịch nhắc là một phần giao dịch nhiệm vụ từ đầu; worker đến sau. Hoàn thành khi kiểm thử cạnh tranh/khởi động lại/thử lại đạt và UI phân biệt đã phát với đã đọc.
