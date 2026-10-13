# Module 03 — Nhiệm vụ và tiến độ

Trạng thái: đặc tả dự thảo, chưa triển khai. Nguồn: [quy ước chung](00-quy-uoc-va-quyet-dinh.md), [ERD](../erd.md), [API — Tasks](../THIET-KE-API-SWAGGER.md). Giai đoạn 5 làm lá; giai đoạn 6 làm cây.

## 1. Mục tiêu, dữ liệu và màn hình

Sinh viên ghi việc học/cá nhân, đặt hạn và ưu tiên, chia nhỏ việc lớn và theo dõi tiến độ. Màn hình: danh sách với bộ lọc, form nhiệm vụ, chi tiết/cây và xác nhận các thao tác ảnh hưởng nhiều dòng.

TaskItem là dữ liệu chính. Course tùy chọn nhưng phải thuộc người dùng; ParentTaskId tùy chọn, không phải liên kết sang nhiệm vụ người khác. Mọi thay đổi có ý nghĩa phối hợp [Lịch sử](04-lich-su-nhiem-vu.md) và [Thông báo](06-thong-bao-nhac-han.md).

## 2. Quy tắc nhiệm vụ lá

| Thuộc tính/trạng thái | Quy tắc theo thiết kế hiện tại |
| --- | --- |
| Title | Bắt buộc, không trắng, tối đa 200 |
| Type | Assignment, Exam, Revision, Personal; đề xuất mặc định Assignment khi có môn, Personal khi không môn theo đặc tả bản đầu/API; người dùng vẫn chọn giá trị hợp lệ khác |
| Priority | Low, Medium, High; mặc định Medium |
| DueDate | Tùy chọn; nhận thời điểm có offset |
| EstimatedMinutes | NULL hoặc > 0; là tổng thời lượng ước lượng của lá |
| NotStarted | Progress = 0; CompletedAt NULL |
| InProgress | Progress từ 1–99; CompletedAt NULL |
| Completed | Progress = 100; có CompletedAt do server ghi |

Tạo nhiệm vụ khởi đầu NotStarted/0. Complete đặt 100; gọi lại khi đã hoàn thành không ghi thêm lịch sử vô ích. Reopen chọn 0–99, mặc định 0 và xóa CompletedAt. Quá hạn là giá trị tính từ hạn hiệu lực và trạng thái, không lưu thành enum Status thứ tư.

Exam chưa phải sự kiện thi có giờ/phòng. DueDate là hạn hoàn thành, không phải ngày dự định bắt đầu làm. Theo [đặc tả bản đầu](../DAC-TA-NGHIEP-VU-BAN-DAU.md), cho tạo task đã quá hạn để ghi nhận việc đang nợ; hiển thị quá hạn và không tạo lời nhắc trước hạn mới cho mốc đã qua.

## 3. Quy tắc cây và hạn hiệu lực

- Cha/con phải cùng UserId và CourseId, kể cả cùng NULL. Không tự làm cha, gắn vào hậu duệ hoặc tạo vòng ở bất kỳ độ sâu nào.
- Cha còn con không cho sửa tay tiến độ/trạng thái; EstimatedMinutes của cha để NULL. Thời lượng tổng hợp lấy từ lá, tránh cộng cả cha lẫn con.
- Tất cả con trực tiếp Completed → cha 100/Completed. Tất cả con tiến độ 0 → cha 0/NotStarted. Còn lại lấy trung bình cộng tiến độ con trực tiếp, làm tròn xuống và giới hạn 1–99 → InProgress.
- Ghi CompletedAt khi cha chuyển Completed; xóa khi rời Completed. Thêm/sửa/chuyển/xóa con phải tính lại tổ tiên; chuyển cha phải tính cả nhánh cũ và mới.
- Xóa hết con: cha trở thành lá, giữ tiến độ cuối theo ERD. Thêm con vào lá: chuyển sang tiến độ tính từ con và bỏ ước lượng nhập tay của cha; UI cảnh báo, lịch sử giữ trước/sau.
- Hạn hiệu lực = DueDate của chính node nếu có; nếu không, lấy tổ tiên gần nhất có hạn. Không có hạn ở cả chuỗi → NULL.
- Hạn con nếu tự nhập không được muộn hơn hạn hiệu lực cha. Đổi hạn/chuyển cây phải kiểm tra toàn bộ cây con bị ảnh hưởng; không tự kéo hạn của con cho vừa mà không có hợp đồng.

Ví dụ: hai con 60% và 0% → cha 30%. Hai con 1% và 0% → trung bình làm tròn là 0 nhưng giới hạn thành 1/InProgress. Cây nhiều tầng tính từ con trực tiếp, chưa phải trung bình tất cả lá hoặc tỷ lệ theo thời lượng.

## 4. Luồng xử lý

### Tạo hoặc cập nhật lá

Request → lấy người dùng hiện tại → kiểm tra dữ liệu/quyền Course → xác định trạng thái và hạn hiệu lực → chuẩn bị TaskHistory và lịch nhắc → lưu chung giao dịch → trả TaskResponse.

Nhiệm vụ không môn/không hạn là hợp lệ. TaskResponse trả cả dueDate và effectiveDueDate, nguồn kế thừa, hasChildren/isOverdue; không tự thêm các cột tính toán này vào ERD.

### Thêm/chuyển con

Tải node/cha/cây cần thiết có kiểm tra quyền → kiểm tra vòng, môn và hạn → áp dụng thay đổi → tính lại nhánh/tổ tiên → ghi lịch sử và đồng bộ lời nhắc → lưu chung giao dịch.

POST có parentTaskId hoặc POST children cùng dùng một nghiệp vụ tạo con. Course của con kế thừa cha; client gửi môn khác khi tạo con trả 400. Đổi môn của root phải cập nhật cả cây. Chuyển sang cha khác môn đang có phương án đồng bộ cây chuyển về môn cha mới, cần chốt Q09 trước code; con không tự đổi môn khác cha.

### Xóa cây

Preview số nhiệm vụ/thông báo → UI xác nhận → kiểm tra lại quyền và dữ liệu → ghi Deleted cho từng node → xóa cây/thông báo → tính lại tổ tiên còn lại và các thay đổi liên quan → commit. Lỗi bất kỳ bước nào phải rollback toàn bộ.

## 5. API và truy vấn

| Nhóm | Route/thao tác |
| --- | --- |
| Danh sách/tạo | GET/POST /api/tasks |
| Chi tiết/sửa/xóa | GET/PUT/DELETE /api/tasks/{id} |
| Cây/tạo con | GET /api/tasks/{id}/tree; POST /api/tasks/{id}/children |
| Chuyển cha/tiến độ | PATCH /api/tasks/{id}/parent; PATCH /api/tasks/{id}/progress |
| Hoàn thành/mở lại | POST /api/tasks/{id}/complete; POST /api/tasks/{id}/reopen |
| Xem ảnh hưởng xóa | GET /api/tasks/{id}/deletion-preview |

PUT thay thế đủ trường trong DTO sửa, không sửa parent/status qua đó. Danh sách mặc định lấy root; leafOnly=true lấy lá. Bộ lọc hạn dùng hạn hiệu lực; task không hạn không khớp khoảng hạn. hasChildren phải lấy theo dữ liệu thật, không suy từ số con lọc được. Cây có giới hạn Q05 nhưng không được trả cây bị cắt mà giả vờ đầy đủ.

400 cho dữ liệu sai; 404 cho tài nguyên/quan hệ cá nhân không thuộc người gọi; 409 cho chu trình, sửa tiến độ cha hoặc xung đột theo hợp đồng API. Quy tắc ràng buộc DB và kiểm tra nghiệp vụ phải bổ trợ nhau.

## 6. Tình huống nghiệm thu

| Tình huống | Kết quả kỳ vọng |
| --- | --- |
| Tạo việc không môn/không hạn | Thành công, không tạo nhắc hạn nếu không có hạn hiệu lực |
| Progress = 100 rồi reopen = 60 | Completed rồi InProgress; CompletedAt được ghi rồi xóa |
| Complete lặp lại | Trạng thái đúng, không lịch sử hoàn thành trùng |
| Sửa tiến độ cha còn con | 409 TASK_PROGRESS_IS_DERIVED |
| Con tự làm cha/cha chuyển xuống cháu | 409 TASK_CYCLE, dữ liệu không đổi |
| Cha đổi hạn sớm hơn hạn nhập của con | Từ chối xung đột, không cập nhật một phần |
| Con NULL DueDate | Calendar/filter/nhắc/gợi ý cùng dùng hạn kế thừa |
| Chuyển con và xóa con cuối | Nhánh cũ/mới tính lại đúng; cha hết con thành lá |
| A dùng Course/ParentTask của B | Không đọc/ghi được dữ liệu B |
| Lỗi lưu history/lời nhắc | Task và mọi tổ tiên rollback |
| Hai cập nhật cây đồng thời | Không mất cập nhật hoặc phá cấu trúc theo cơ chế Q04 đã chọn |

## 7. Điểm cần chốt và điều kiện hoàn thành

Chốt Q04–Q06 và Q09 trước phần phụ thuộc. Triển khai lá kèm lịch sử/lịch nhắc nền trước; cây làm sau nhưng dùng cùng quy tắc hạn và quyền. Hoàn thành khi các ca trên có kết quả kiểm chứng, UI thể hiện nguồn hạn kế thừa và ảnh hưởng thao tác, không đếm cha thành khối lượng bổ sung.
