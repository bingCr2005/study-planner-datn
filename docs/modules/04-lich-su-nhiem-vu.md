# Module 04 — Lịch sử nhiệm vụ

API đọc lịch sử và màn xem hoạt động của từng nhiệm vụ đã được triển khai. Xem [bản ghi giao diện](../history/2026-11-11-lam-gon-hoat-dong-nhiem-vu.md), [quy ước chung](00-quy-uoc-va-quyet-dinh.md) và [ERD](../erd.md).

## 1. Mục tiêu và phạm vi

Sinh viên xem nhiệm vụ đã được tạo/sửa/hoàn thành/mở lại/xóa như thế nào. Đây là lịch sử nghiệp vụ, khác log chẩn đoán server. Không phải cơ chế khôi phục nhiệm vụ, event sourcing hoặc nhật ký đăng nhập.

Từ chi tiết nhiệm vụ, chọn **⋯ → Hoạt động nhiệm vụ** để xem các thao tác theo ngày. Giao diện tóm tắt những thay đổi như đổi hạn, cập nhật tiến độ và hoàn thành. API hỗ trợ tra cứu theo mã nhiệm vụ đã xóa. Lối mở trên FE hiện nằm trong chi tiết nhiệm vụ còn truy cập được.

## 2. Dữ liệu và quyền

TaskHistory lưu TaskId chụp lại không có FK, TaskTitle tại thời điểm thao tác, UserId nullable, Action, OldData/NewData và ChangedAt. Chỉ các trường nhiệm vụ được cho phép vào JSON; không serialize toàn bộ User/navigation hoặc bí mật.

User chỉ đọc dòng còn liên kết với UserId của mình. Task đã xóa không làm mất quyền đọc lịch sử còn thuộc mình. Khi User bị xóa hẳn, FK đặt UserId NULL; không hứa truy vết danh tính sau xóa. Quyền quản trị với lịch sử mất liên kết và thời hạn giữ dữ liệu là Q08, chưa mở API mặc định.

## 3. Luồng ghi và đọc

1. Application lấy giá trị trước thay đổi, áp dụng nghiệp vụ và tạo snapshot sau thay đổi trong danh sách trường cho phép.
2. Ghi hành động `Created`, `Updated`, `Completed`, `Reopened`, `Trashed`, `Restored` hoặc `Deleted`. Tạo mới chỉ có dữ liệu sau. Chuyển vào thùng rác và xóa chỉ có dữ liệu trước.
3. Lưu TaskHistory cùng giao dịch với nhiệm vụ/tổ tiên/lời nhắc. Nếu ghi lịch sử lỗi, không commit riêng phần nhiệm vụ.
4. Tính lại tổ tiên hoặc đổi môn cả cây: ghi trước/sau cho những nhiệm vụ thực sự thay đổi. No-op như complete lặp không sinh lịch sử vô ích.
5. Khi đọc, lọc UserId trước, rồi taskId/action/thời gian và phân trang. Không đòi TaskItem đang tồn tại để đọc history.

Xóa cây phải chuẩn bị sự kiện Deleted từng node trước cascade. Xóa môn/học kỳ phải ghi Updated cho CourseId bị bỏ liên kết. Xóa User nếu có luồng riêng phải ghi trước cascade và chấp nhận mất liên kết UserId sau đó. SQL xóa trực tiếp không tự tạo các sự kiện này.

## 4. API và ngoại lệ

`GET /api/v1/task-history` nhận `maNhiemVu`, `hanhDong`, `tu`, `den`, `trang`, `kichThuocTrang` và trả danh sách phân trang mới nhất trước. Đây là endpoint đọc, giới hạn bản ghi theo tài khoản trước khi áp dụng các bộ lọc.

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
