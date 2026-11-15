# Module 03 — Nhiệm vụ và tiến độ

Trạng thái đối chiếu mã ngày 2026-11-13. BE/FE đã chuyển sang tiến độ cha tổng hợp từ con; checklist chỉ điều khiển tiến độ nhiệm vụ lá. Xem [bản ghi chuyển quy tắc và kiểm chứng](../history/2026-11-13-tong-hop-tien-do-nhiem-vu.md). Quy tắc độc lập trong [bản ghi checklist cũ](../history/2026-11-10-tach-checklist-va-tien-do-nhiem-vu-con.md) là lịch sử, không còn là quy tắc hiện hành.

Các phần bên dưới giữ nội dung đặc tả ban đầu để đối chiếu. Tên DTO/route và hành vi hiện hành được đối chiếu với `backend/StudyPlanner.Application/HocTap` và `backend/StudyPlanner.Api/Controllers/NhiemVuController.cs`.

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
- Mỗi nhiệm vụ có một nguồn tiến độ: có con thì lấy trung bình số nguyên của mọi con trực tiếp, làm tròn xuống; không có con nhưng có checklist thì lấy tỷ lệ mục đã xong; lá không có checklist nhận tiến độ nhập tay. Tính từ dưới lên mọi cấp, không gom toàn bộ cháu vào mẫu số của gốc.
- Con đã hoàn thành vẫn thuộc cây và vẫn được tính. Cha chỉ đạt 100%/Completed khi mọi con trực tiếp đạt 100%; làm tròn không được tạo 100% giả. Ghi ngày hoàn thành khi đạt 100%, giữ mốc nếu vẫn hoàn thành, xóa mốc khi mở lại.
- Không nhập tay progress/complete/reopen cho cha và không gắn checklist công việc trực tiếp lên cha. API từ chối tương ứng bằng `TASK_PROGRESS_IS_DERIVED` và `TASK_CHECKLIST_REQUIRES_LEAF`. Mô tả cha vẫn chỉnh sửa bình thường; UI giải thích nguồn tiến độ và mở chi tiết con khi cần.
- Thêm/chuyển con vào lá có checklist giữ checklist trong con **Công việc chung**; lá có tiến độ nhập tay >0 giữ tiến độ/ước lượng trong con **Phần việc đã thực hiện**. Cha chuyển sang nguồn tổng hợp. Cả nhánh bảo toàn và nhánh mới đều góp một phần bằng nhau. Ước lượng cha để NULL, tổng thời lượng lấy từ lá.
- Preview thông báo các mã nhiệm vụ sẽ mở lại hoặc chuyển phần việc cũ thành con. Ghi children/parent cần `xacNhanDoiCauTruc=true` khi có các ảnh hưởng đó; không được chỉ dựa vào nút xác nhận FE. Metadata không đổi cấu trúc và cập nhật trực tiếp nhiệm vụ lá không cần cờ này.
- Thêm/xóa/chuyển/thùng rác/khôi phục tính lại các tổ tiên bị ảnh hưởng trong cùng giao dịch; chuyển con tính cả nhánh cũ và nhánh mới. Khi không còn con, cha cũ trở lại lá tiến độ 0, không giữ 100% dẫn xuất đã mất nguồn. Khôi phục là thao tác rõ ràng đưa việc trở lại và có thể mở lại các tổ tiên; nếu cha đã trở thành lá và có phần việc riêng mới, bảo toàn phần đó trước khi gắn lại con.
- Hạn hiệu lực = DueDate của chính node nếu có; nếu không, lấy tổ tiên gần nhất có hạn. Không có hạn ở cả chuỗi → NULL.
- Hạn con nếu tự nhập không được muộn hơn hạn hiệu lực cha. Đổi hạn/chuyển cây phải kiểm tra toàn bộ cây con bị ảnh hưởng; không tự kéo hạn của con cho vừa mà không có hợp đồng.

Ví dụ A gồm B và C; B gồm B1=100%, B2=0%, C=0% thì B=50%, A=25%. B hoàn thành thì A=50%; C hoàn thành thì A=100%. Thêm việc chưa xong vào B sẽ tính lại B/A và xóa trạng thái hoàn thành cũ sau xác nhận. Đây là tỷ lệ các nhánh công việc có trọng số bằng nhau, không phải tỷ lệ khối lượng/thời gian chính xác.

Migration dữ liệu `20261113010000_TongHopTienDoNhiemVu` chuyển checklist cha cũ thành con, tính lại cây đang dùng và nhóm thùng rác, ghi lịch sử System trước/sau và tăng revision tài khoản liên quan. Migration kiểm tra cấu trúc/giới hạn trước khi ghi, không tự loại bỏ dữ liệu. Không có Down tự động vì các con bảo toàn có thể đã được người dùng chỉnh sửa; phục hồi phải dựa trên bản sao lưu đã kiểm tra. Chưa tự áp dụng migration vào database ứng dụng.

Khi chuẩn bị nâng cấp database đã cấu hình, giữ bản sao lưu trước nâng cấp rồi chạy từ repository BE:

```powershell
dotnet ef database update --project backend/StudyPlanner.Infrastructure --startup-project backend/StudyPlanner.Api --configuration Release
```

Sau đó khởi động lại backend và tải lại frontend. Kiểm thử riêng dùng CoreChecks với `STUDYPLANNER_TEST_POSTGRES` theo quy tắc database localhost tạm; đặt `STUDYPLANNER_TEST_FRONTEND` nếu repository FE không nằm cạnh BE. Không trỏ bộ kiểm thử vào database ứng dụng.

## 4. Luồng xử lý

### Tạo hoặc cập nhật lá

Request → lấy người dùng hiện tại → kiểm tra dữ liệu/quyền Course → xác định trạng thái và hạn hiệu lực → chuẩn bị TaskHistory và lịch nhắc → lưu chung giao dịch → trả TaskResponse.

Nhiệm vụ không môn/không hạn là hợp lệ. TaskResponse trả cả dueDate và effectiveDueDate, nguồn kế thừa, hasChildren/isOverdue; không tự thêm các cột tính toán này vào ERD.

### Thêm/chuyển con

Tải node/cha/cây cần thiết có kiểm tra quyền → xem trước bảo toàn checklist/tiến độ và mở lại → kiểm tra vòng, môn và hạn → áp dụng thay đổi → tổng hợp tiến độ từ dưới lên và chuẩn hóa ước lượng → kiểm tra phiên học bị ảnh hưởng → xác nhận theo hợp đồng → ghi lịch sử User/System và đồng bộ lời nhắc → lưu chung giao dịch. Preview không ghi dữ liệu và không tăng revision.

Tạo con qua `POST /api/v1/tasks/{id}/children`, phạm vi kế thừa cha; client gửi môn/học kỳ khác bị từ chối. `PATCH /{id}/scope` chỉ nhận nhiệm vụ gốc và áp dụng cho cả cây. `PATCH /{id}/parent` không cho chuyển sang cha khác phạm vi; phải điều chỉnh phạm vi phù hợp trước, không tự đổi môn cho riêng một con.

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

Các quy tắc tiến độ, nguồn tính và xác nhận cấu trúc đã được chốt trong thay đổi ngày 13/11/2026. Đối chiếu kết quả kiểm chứng ở bản ghi lịch sử; việc đã có code/migration không đồng nghĩa database ứng dụng đã được nâng cấp. Không đếm thời lượng cha thành khối lượng bổ sung ngoài các lá.
