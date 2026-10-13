# Thiết kế API và Swagger — Study Planner

Ngày: 2026-10-13. Trạng thái: hợp đồng đề xuất để triển khai, không phải danh sách endpoint đã chạy.

Đọc cùng [kế hoạch chi tiết](KE-HOACH-TRIEN-KHAI-CHI-TIET.md), [ERD](erd.md) và [kiến trúc](KIEN-TRUC-BACKEND.md). Code hiện có GET /, GET /weatherforecast và GET /health/live. Swagger có trong Development nhưng chưa có API nghiệp vụ học tập.

[Đặc tả nghiệp vụ bản đầu](DAC-TA-NGHIEP-VU-BAN-DAU.md) bổ sung tình huống nghiệm thu và ý nghĩa các trường. Người dùng đã chọn gợi ý thứ tự có giải thích, chưa tự xếp giờ; endpoint recommendations phục vụ phạm vi này.

## 1. Mục đích

[Đặc tả module](modules/README.md) giải thích nghiệp vụ phía sau các endpoint, gồm luồng, giao dịch, ngoại lệ và nghiệm thu. Tài liệu API này giữ vai trò nguồn hợp đồng HTTP; những bổ sung còn đề xuất trong module phải được chốt và cập nhật hợp đồng trước khi triển khai.

Thiết kế API xác định client gửi gì, server kiểm tra gì, trả gì và ai có quyền gọi. OpenAPI mô tả hợp đồng; Swagger UI dùng tài liệu đó để hiển thị và thử HTTP. Backend đã chuyển sang ASP.NET Core 10 với Swashbuckle 10.3.0; vẫn dùng AddSwaggerGen/UseSwagger/UseSwaggerUI. Namespace model là Microsoft.OpenApi, không còn Microsoft.OpenApi.Models; đã bỏ WithOpenApi và package Microsoft.AspNetCore.OpenApi cũ vì luồng hiện tại dùng Swashbuckle. Xem [hướng dẫn chuyển Swashbuckle v10](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/migrating-to-v10.md) và [kết quả kiểm chứng](history/2026-10-13-nang-cap-dotnet-10.md).

Tài liệu này gồm danh mục endpoint, quy ước DTO/lỗi, ví dụ chi tiết và tiêu chí Swagger. Từng endpoint cần DTO và validation cụ thể khi triển khai; không coi bảng danh mục là file OpenAPI đã được sinh hoặc kiểm chứng.

## 2. Quy ước chung

| Nội dung | Quy ước đề xuất |
|---|---|
| Đường dẫn | /api/...; tên tài nguyên số nhiều, chữ thường, dấu gạch nối khi cần |
| Version | Tên tài liệu v1; bản đầu chưa cần thêm /v1 vào URL |
| JSON | camelCase; DTO riêng; không trả trực tiếp Entity/navigation |
| Enum | Chuỗi theo ERD, ví dụ InProgress, High; từ chối giá trị không định nghĩa |
| ID | Số nguyên dương; ID sai định dạng trả 400 |
| Thời điểm | ISO 8601 có offset, response chuẩn UTC; thiếu offset bị từ chối |
| Ngày/giờ học | yyyy-MM-dd; HH:mm:ss; DayOfWeek 1 = thứ Hai, 7 = Chủ nhật |
| Phân trang | page mặc định 1; pageSize mặc định 20, tối đa 100; vượt giới hạn trả 400 |
| Khoảng thời gian | from bao gồm, to không bao gồm; from phải nhỏ hơn to |
| Calendar | Khoảng truy vấn tối đa 93 ngày/lần, truy vấn dài hơn trả 400 |
| Thứ tự | sortBy/sortDirection từ danh sách cho phép, thêm Id để ổn định |
| Tạo | POST, thành công 201 với DTO và Location đến tài nguyên |
| Thay thế | PUT yêu cầu đủ các trường có thể sửa trong DTO |
| Cập nhật một phần | PATCH hồ sơ và thao tác chuyên biệt; phân biệt trường vắng với null |
| Xóa | DELETE thành công 204; trả 404 nếu tài nguyên không còn hoặc không thuộc người gọi |
| Đầu vào quản lý server | Không cho client đặt UserId, Role, CreatedAt, UpdatedAt, CompletedAt, DeliveryStatus |
| Quyền | Thiếu/hết hạn token: 401; không có role cần thiết: 403; ID cá nhân của người khác: 404 |
| Lỗi | application/problem+json; thêm code, traceId và errors khi validation |

Response danh sách phân trang dùng items, page, pageSize, totalCount. Response chi tiết trả object trực tiếp, không thêm wrapper khác nhau tùy controller. Các thao tác complete/reopen/progress trả DTO đã cập nhật; DELETE và read trả 204. Endpoint dạng tổng quan/calendar có hợp đồng riêng rõ ràng, không bắt buộc phân trang nếu đã giới hạn khoảng/trả top N.

GET danh sách nhiệm vụ mặc định cây gốc; query leafOnly=true dùng cho thống kê/gợi ý hoặc danh sách việc có thể thực hiện độc lập. Khi lọc làm thiếu con, response không được tự suy ra hasChildren=false từ số con đã tải; hasChildren là thông tin thật ở dữ liệu. Xem cây đầy đủ qua endpoint tree riêng.

## 3. Xác thực và quyền

Đề xuất access token Bearer ngắn hạn cho bản đầu, chưa có refresh token. Response login gồm accessToken, tokenType, expiresAt, user; không trả PasswordHash. Swagger dùng security scheme HTTP bearer, bearerFormat JWT khi cơ chế này được triển khai.

Frontend lưu token trong bộ nhớ ở bản đầu; tải lại trang có thể phải đăng nhập lại. Nếu cần giữ phiên sau tải lại, phải thiết kế cookie/refresh và CSRF phù hợp trước khi đổi; không âm thầm lưu token lâu dài để có trải nghiệm giống đã có quản lý phiên hoàn chỉnh.

Đăng xuất bản đầu là xóa token ở client; không tạo endpoint logout giả vờ thu hồi JWT. Đổi mật khẩu yêu cầu mật khẩu hiện tại. Token cũ có thể còn hiệu lực đến hết hạn nếu chưa có cơ chế thu hồi; muốn vô hiệu hóa ngay phải bổ sung thiết kế dữ liệu phiên/security version. IsActive phải được kiểm tra với request bảo vệ để khóa tài khoản có hiệu lực.

API không nhận UserId để chọn chủ sở hữu khi tạo dữ liệu cá nhân. CourseId, SemesterId, ParentTaskId phải được kiểm tra thuộc người dùng. Admin cũng không mặc định được đọc mọi task. Áp dụng giới hạn tần suất cho login/register và trả 429 khi vượt ngưỡng cấu hình; kiểm tra lỗi unique của DB trong tình huống hai request đăng ký đồng thời.

## 4. Nhóm Auth, Profile và Catalogs

| Method | Endpoint | Đầu vào chính | Kết quả/quyền |
|---|---|---|---|
| POST | /api/auth/register | fullName, email, password | 201: hồ sơ đã tạo, không tự cấp Admin; khách |
| POST | /api/auth/login | email, password | 200: token, hạn token, hồ sơ; khách |
| GET | /api/users/me | Không body | 200: hồ sơ hiện tại; đăng nhập |
| PATCH | /api/users/me | fullName, studentCode, classId, major, timeZone, reminderHours | 200: hồ sơ; không sửa email/role/isActive qua đây |
| POST | /api/users/me/change-password | currentPassword, newPassword | 204; xác minh mật khẩu cũ |
| GET | /api/catalogs/schools | q, page, pageSize | 200: danh sách; đăng nhập |
| GET | /api/catalogs/faculties | schoolId, q, page, pageSize | 200: danh sách khoa |
| GET | /api/catalogs/cohorts | q, page, pageSize | 200: danh sách khóa |
| GET | /api/catalogs/student-classes | facultyId, cohortId, q, page, pageSize | 200: danh sách lớp |

PATCH me: trường vắng giữ nguyên; studentCode/classId/major có thể gửi null để bỏ; fullName/timeZone/reminderHours không được null. reminderHours thay đổi phải cập nhật các lời nhắc dùng mặc định tài khoản trong cơ chế nhất quán; không trả thành công trước khi dữ liệu liên quan đã được xử lý theo phạm vi giao dịch đã chốt.

Mật khẩu có giới hạn độ dài và chính sách được xác định khi làm Auth; không đưa mật khẩu dùng thật vào ví dụ Swagger. Bản đầu chưa có forgot-password, email verification, đổi email hoặc refresh token.

## 5. Nhóm Semesters, Courses, ClassSessions

| Method | Endpoint | Chức năng/đầu vào | Kết quả |
|---|---|---|---|
| GET | /api/semesters | Danh sách của tôi; q, page, pageSize | 200 phân trang |
| POST | /api/semesters | name, startDate, endDate | 201 |
| GET | /api/semesters/{id} | Chi tiết học kỳ thuộc tôi | 200 |
| PUT | /api/semesters/{id} | name, startDate, endDate | 200 |
| GET | /api/semesters/{id}/deletion-preview | Số môn/buổi học bị xóa, số task bỏ liên kết | 200 |
| DELETE | /api/semesters/{id} | Xóa học kỳ và môn/lịch; giữ task | 204 |
| GET | /api/semesters/{id}/courses | Danh sách môn của học kỳ; page, pageSize | 200 phân trang |
| POST | /api/semesters/{id}/courses | code, name, credits, lecturer, color | 201 |
| GET | /api/courses/{id} | Chi tiết môn, semesterId | 200 |
| PUT | /api/courses/{id} | code, name, credits, lecturer, color | 200; chưa hỗ trợ chuyển học kỳ |
| GET | /api/courses/{id}/deletion-preview | Số buổi học bị xóa, số task bỏ liên kết | 200 |
| DELETE | /api/courses/{id} | Xóa môn/lịch; task.CourseId = NULL | 204 |
| GET | /api/courses/{id}/class-sessions | Các mẫu lịch hằng tuần | 200 mảng |
| POST | /api/courses/{id}/class-sessions | dayOfWeek, startTime, endTime, room | 201 |
| PUT | /api/class-sessions/{id} | dayOfWeek, startTime, endTime, room | 200 |
| DELETE | /api/class-sessions/{id} | Xóa một mẫu lặp | 204 |

Chỉ dữ liệu thuộc người gọi được đọc/ghi. Deletion-preview là thông tin để UI xác nhận, không khóa dữ liệu cho tới DELETE; DELETE phải kiểm tra lại và trả xung đột nếu cơ chế phiên bản phát hiện dữ liệu đã thay đổi. Việc xác nhận xóa là thao tác sản phẩm, không thay kiểm tra quyền backend.

Đổi biên học kỳ làm thay đổi các lần xuất hiện của lịch; deadline task không bị tự sửa. Buổi học trùng trả warning có mã trong DTO sau lưu, không dùng lỗi để cấm theo phạm vi đề xuất.

## 6. Nhóm Tasks và TaskHistory

| Method | Endpoint | Chức năng/đầu vào | Kết quả |
|---|---|---|---|
| GET | /api/tasks | q, semesterId, courseId, status, priority, type, dueFrom, dueTo, overdue, leafOnly, page, pageSize, sortBy, sortDirection | 200 phân trang |
| POST | /api/tasks | title, description, courseId, parentTaskId, type, dueDate, priority, estimatedMinutes, reminderHours | 201; status/progress khởi tạo server |
| GET | /api/tasks/{id} | Chi tiết task và thông tin tính toán | 200 |
| PUT | /api/tasks/{id} | title, description, courseId, type, dueDate, priority, estimatedMinutes, reminderHours | 200; không đổi parent/status qua DTO này |
| GET | /api/tasks/{id}/tree | Cây từ task, không phân trang cắt mất cấu trúc | 200; có giới hạn kích thước cấu hình |
| POST | /api/tasks/{id}/children | DTO tạo task, không nhận parentTaskId khác route | 201; gán chủ/môn theo cha |
| PATCH | /api/tasks/{id}/parent | parentTaskId có thể null | 200; di chuyển cây, kiểm tra chu trình/hạn/môn |
| PATCH | /api/tasks/{id}/progress | progress | 200; chỉ lá; đồng bộ status/completedAt |
| POST | /api/tasks/{id}/complete | Không body | 200; chỉ lá; gọi lại không sinh lịch sử trùng |
| POST | /api/tasks/{id}/reopen | progress tùy chọn, mặc định 0, trong 0–99 | 200; chỉ lá; xóa completedAt |
| GET | /api/tasks/{id}/deletion-preview | Số task trong cây bị xóa, thông báo liên quan | 200 |
| DELETE | /api/tasks/{id} | Xóa cây, ghi lịch sử từng dòng | 204 |
| GET | /api/task-history | taskId, action, from, to, page, pageSize | 200 phân trang; lịch sử thuộc tôi kể cả task đã xóa |

Đối với POST task có parentTaskId, CourseId được kế thừa từ cha; nếu client gửi giá trị khác thì 400, không tự đổi môn cha. Endpoint children là cách thuận tiện cho UI; cả hai dùng chung nghiệp vụ tạo con.

Type nếu không gửi: mặc định Assignment khi task có môn (kể cả môn kế thừa từ cha), Personal khi không môn; client có thể chọn giá trị enum hợp lệ khác. Đây là mặc định đề xuất theo NV-04, chưa triển khai.

PUT task là thay thế đầy đủ các trường trong DTO sửa; null có ý nghĩa bỏ giá trị tùy chọn. Với cha còn con, estimatedMinutes phải null. Đổi courseId cập nhật cả cây và lịch sử theo ERD; không cho con tự đổi môn khác cha. Việc chuyển sang cha có môn khác phải cập nhật cả cây chuyển hoặc trả xung đột theo hợp đồng triển khai đã chốt; phương án đề xuất là cập nhật cả cây về môn của cha mới và ghi rõ trước/sau.

Response task tối thiểu: id, title, description, courseId, parentTaskId, type, dueDate, effectiveDueDate, inheritedDueDateFromTaskId, priority, status, estimatedMinutes, progress, reminderHours, effectiveReminderHours, completedAt, hasChildren, isOverdue, createdAt, updatedAt. Các trường effective/hasChildren/isOverdue là giá trị tính, không mặc định thêm cột cùng tên vào ERD.

dueFrom/dueTo lọc hạn hiệu lực; task không hạn không khớp bộ lọc hạn. overdue=true chỉ lấy chưa Completed có hạn hiệu lực < thời điểm xét. Không biến overdue thành Status thứ tư. Thêm includeUndated riêng nếu sau này cần kết hợp nhiệm vụ có hạn và không hạn trong một truy vấn; chưa tự hiểu dueTo là bao gồm task không hạn.

GET history lọc theo UserId hiện tại trước khi lọc taskId, không yêu cầu task đang tồn tại. GET task đã xóa vẫn 404; lịch sử không bị mất chỉ vì endpoint chi tiết không còn.

## 7. Nhóm Calendar, Dashboard, Notifications và Planning

| Method | Endpoint | Đầu vào/kết quả | Quyền |
|---|---|---|---|
| GET | /api/calendar | from, to, semesterId tùy chọn; trả events với eventType, sourceId, start/end hoặc dueAt, timeZone | Dữ liệu của tôi |
| GET | /api/dashboard | date dạng yyyy-MM-dd, semesterId tùy chọn; trả lịch ngày, top việc cần chú ý, số liệu có phạm vi rõ | Dữ liệu của tôi |
| GET | /api/notifications | unreadOnly, page, pageSize; chỉ Delivered, mới nhất trước | Dữ liệu của tôi |
| GET | /api/notifications/unread-count | Trả unreadCount | Dữ liệu của tôi |
| PATCH | /api/notifications/{id}/read | Không body; ghi ReadAt nếu chưa đọc; lặp lại không đổi lần đọc đầu | 204; của tôi |
| POST | /api/notifications/read-all | Không body; đánh dấu các Delivered hiện có chưa đọc | 204; của tôi |
| GET | /api/planning/recommendations | semesterId tùy chọn, limit mặc định 10 tối đa 50; trả generatedAt, strategy, items, warnings | Nhiệm vụ lá của tôi |

Calendar eventType gồm ClassSessionOccurrence và TaskDeadline. Buổi học có start/end; deadline dùng dueAt và không tạo thời lượng giả. Không trả toàn bộ Entity. Nếu UI hiện cả cha/con, DTO phải giúp nhận ra quan hệ; không dùng số event làm số khối lượng công việc.

Dashboard đề xuất: lịch học ngày chọn; tối đa 10 việc cần chú ý; counts gồm totalLeaves, completedLeaves, overdueLeaves, dueTodayLeaves, undatedLeaves theo học kỳ đã chọn hoặc toàn bộ. Total/completed/undated xét toàn phạm vi học kỳ, dueToday theo date và TimeZone, overdue theo generatedAt; không mô tả chúng như cùng một bộ lọc ngày. Trả generatedAt và scope để UI giải thích đúng. Không gọi tỷ lệ hoàn thành nhiệm vụ là tỷ lệ thời gian học.

Read-all chỉ áp dụng thông báo đã phát tại lúc xử lý. Không đánh dấu Pending, không sửa dữ liệu người khác. Khi schema một bản ghi/mỗi task được dùng lại cho lịch nhắc mới, notification cũ có thể được thay thế như ERD đã nêu; chưa có lịch sử từng lần gửi.

Recommendations là truy vấn, không tự đổi Priority/Status/DueDate hoặc ghi buổi học. Mỗi item có rank, taskId, title, effectiveDueDate, priority, remainingMinutes (nullable), reasons, missingInputs. limit là top N, không đại diện tổng số việc.

## 8. Nhóm Admin và Health

| Method | Endpoint | Chức năng/quyền |
|---|---|---|
| GET | /api/admin/users | q, isActive, role, page, pageSize; chỉ Admin; DTO không có hash/token |
| PATCH | /api/admin/users/{id}/active | isActive; chỉ Admin; ngăn tự khóa/quản trị cuối cùng |
| GET | /health/live | API host còn xử lý; không kiểm tra DB |
| GET | /health/ready | Kiểm tra dependency cần thiết; DB lỗi trả 503 với mô tả tối thiểu |

CRUD danh mục Admin có các route đề xuất cụ thể theo bảng dưới. Mỗi tên tài nguyên tương ứng bốn method POST collection, GET item, PUT item và DELETE item; GET danh sách đọc dùng Catalogs ở mục 4, không nhân đôi một API danh sách quản trị nếu chưa có nhu cầu.

| Collection | Item | Trường tạo/sửa chính |
|---|---|---|
| /api/admin/schools | /api/admin/schools/{id} | code, name |
| /api/admin/faculties | /api/admin/faculties/{id} | schoolId, code, name |
| /api/admin/cohorts | /api/admin/cohorts/{id} | name, startYear, endYear |
| /api/admin/student-classes | /api/admin/student-classes/{id} | facultyId, cohortId, name |

POST trả 201, GET/PUT trả 200, DELETE trả 204. Trùng unique hoặc xóa RESTRICT trả 409. Đổi quan hệ của danh mục đã được dùng cần kiểm tra tác động hồ sơ; không tự đổi trường/khoa của người dùng mà UI không thể hiện.

Chưa có endpoint đổi Role, đọc mọi nhiệm vụ, xóa hẳn tài khoản hoặc phát thông báo thử công khai. Nếu bổ sung, cần đặc tả quyền/kiểm chứng riêng. Health không được trả tên host DB, connection string hoặc stack trace.

## 9. Ví dụ hợp đồng tạo nhiệm vụ

Các giá trị bên dưới là dữ liệu minh họa, không phải dữ liệu đã có trong database. ID phải lấy từ response API thật khi thử; không dùng số ví dụ làm bằng chứng API đã chạy.

### Request

POST /api/tasks

```json
{
  "title": "Hoàn thành báo cáo CSDL",
  "description": "Viết phần thiết kế và kiểm thử truy vấn",
  "courseId": 12,
  "parentTaskId": null,
  "type": "Assignment",
  "dueDate": "2026-10-20T17:00:00+07:00",
  "priority": "High",
  "estimatedMinutes": 300,
  "reminderHours": 24
}
```

Server xác thực → lấy UserId → kiểm tra môn 12 thuộc User → kiểm tra dữ liệu → tính trạng thái/hạn/giờ nhắc → ghi task + history + lịch nhắc trong giao dịch → trả 201 và Location tới /api/tasks/35.

### Response 201

```json
{
  "id": 35,
  "title": "Hoàn thành báo cáo CSDL",
  "description": "Viết phần thiết kế và kiểm thử truy vấn",
  "courseId": 12,
  "parentTaskId": null,
  "type": "Assignment",
  "dueDate": "2026-10-20T10:00:00Z",
  "effectiveDueDate": "2026-10-20T10:00:00Z",
  "inheritedDueDateFromTaskId": null,
  "priority": "High",
  "status": "NotStarted",
  "estimatedMinutes": 300,
  "progress": 0,
  "reminderHours": 24,
  "effectiveReminderHours": 24,
  "completedAt": null,
  "hasChildren": false,
  "isOverdue": false,
  "createdAt": "2026-10-13T03:00:00Z",
  "updatedAt": "2026-10-13T03:00:00Z"
}
```

Client không gửi CompletedAt hoặc UserId. Response không chứa PasswordHash, thông tin bảo mật hoặc toàn bộ quan hệ User/Course. Nếu chọn cơ chế phiên bản sửa đồng thời, bổ sung ETag/header hoặc version vào hợp đồng và thiết kế trước khi triển khai, không tự dùng updatedAt như một khóa chống mất cập nhật hoàn chỉnh.

### Ví dụ cập nhật tiến độ

PATCH /api/tasks/35/progress

```json
{ "progress": 60 }
```

Trả 200 với TaskResponse: Progress = 60, Status = InProgress, CompletedAt = NULL; tính lại tổ tiên, lịch sử và lời nhắc liên quan trong giao dịch. Nếu là cha còn con thì 409 với code TASK_PROGRESS_IS_DERIVED. Nếu progress = 100 thì chuyển Completed và hủy lời nhắc chưa phát.

### Ví dụ danh sách

GET /api/tasks?status=InProgress&leafOnly=true&page=1&pageSize=20&sortBy=effectiveDueDate&sortDirection=asc

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 0
}
```

Đây là ví dụ danh sách rỗng hợp lệ. sortBy cho phép createdAt, updatedAt, title, effectiveDueDate, priority; NULL deadline xếp cuối khi sort theo hạn. Không ghép trực tiếp sortBy thành SQL chưa kiểm tra. totalCount được tính sau bộ lọc và trước phân trang.

## 10. Lỗi và mã phản hồi

| HTTP | Trường hợp | Ví dụ code |
|---|---|---|
| 400 | Kiểu/định dạng sai, thiếu trường, ngoài khoảng | VALIDATION_ERROR |
| 401 | Thiếu/sai/hết hạn token hoặc phiên không còn được chấp nhận | UNAUTHORIZED |
| 403 | Đã xác thực nhưng không có role/quyền endpoint | FORBIDDEN |
| 404 | Không tồn tại hoặc không thuộc người gọi | RESOURCE_NOT_FOUND |
| 409 | Email/mã trùng, thao tác xung đột nghiệp vụ, sửa bản cũ | EMAIL_ALREADY_USED, TASK_CYCLE, TASK_PROGRESS_IS_DERIVED, CONCURRENCY_CONFLICT |
| 429 | Vượt giới hạn tần suất | RATE_LIMIT_EXCEEDED |
| 500 | Lỗi server ngoài dự kiến | INTERNAL_ERROR |
| 503 | Dependency cần thiết không sẵn sàng, ví dụ readiness DB | SERVICE_UNAVAILABLE |

Quy ước này chọn 400 cho lỗi validation và 409 cho xung đột trạng thái/nghiệp vụ; không để controller tự chọn 400/422/409 không nhất quán. Các lỗi khóa/unique được chuyển về mã phù hợp, không trả lỗi PostgreSQL thô.

Ví dụ application/problem+json:

```json
{
  "type": "about:blank",
  "title": "Dữ liệu không hợp lệ",
  "status": 400,
  "detail": "Vui lòng kiểm tra các trường được đánh dấu.",
  "instance": "/api/tasks",
  "code": "VALIDATION_ERROR",
  "traceId": "example-trace-id",
  "errors": {
    "title": ["Tiêu đề không được để trống."],
    "estimatedMinutes": ["Thời lượng phải lớn hơn 0 khi có giá trị."]
  }
}
```

Lỗi 500 trả mô tả chung và traceId; chi tiết exception ghi ở log có kiểm soát. Tham khảo [Microsoft: xử lý lỗi API và Problem Details](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-8.0). Không log body chứa password/token hoặc sao chép bí mật vào lỗi.

## 11. Thiết kế trang Swagger UI

### 11.1. Bố cục

```text
Study Planner API — v1
Mô tả phạm vi, quy tắc thời gian, cách đăng nhập và mã lỗi

[Authorize]

Auth
Profile
Catalogs
Semesters
Courses
ClassSessions
Tasks
TaskHistory
Calendar
Dashboard
Notifications
Planning
Admin
Health

Schemas
  RegisterRequest / LoginRequest / LoginResponse
  ProfileResponse / UpdateProfileRequest
  SemesterRequest / SemesterResponse
  CourseRequest / CourseResponse
  ClassSessionRequest / ClassSessionResponse
  CreateTaskRequest / UpdateTaskRequest / TaskResponse
  UpdateProgressRequest / ChangeParentRequest / ReopenTaskRequest
  TaskHistoryResponse / NotificationResponse
  CalendarResponse / DashboardResponse / RecommendationResponse
  ProblemDetails / ValidationProblemDetails
  PagedResponse của các nhóm có phân trang
```

Tên schema là định hướng; chỉ xuất hiện sau khi DTO thật được triển khai. Tên tag không quyết định cấu trúc database và không bắt buộc mỗi tag phải tương ứng một bảng.

### 11.2. Mỗi operation phải hiển thị

- Summary ngắn: thao tác người dùng nhận được.
- Description: quy tắc cần biết, ví dụ xóa task sẽ xóa cây con; sửa cha sẽ đổi tiến độ; deadline NULL có thể kế thừa.
- Path/query/body đúng tên, kiểu, required, nullable, default, enum, giới hạn và ví dụ.
- Schema response thành công; phản hồi lỗi thực tế có thể xảy ra, không liệt kê mọi HTTP code cho mọi endpoint.
- Biểu tượng bảo vệ đúng operation cần đăng nhập; register/login và health không bị yêu cầu token vì global security cấu hình nhầm.
- operationId duy nhất, ổn định; tên request/response dễ hiểu, không dùng Entity làm schema công khai.
- Giải thích quyền Admin khác quyền sở hữu; đổi ID của người khác không có quyền chỉ vì có token.

### 11.3. Cấu hình và kiểm tra

1. Giữ một lần đăng ký Swagger trong Program.cs; cấu hình tên tài liệu/title/version và XML comments/metadata DTO phù hợp.
2. MapControllers; ánh xạ API thật; bổ sung mô tả response và lỗi.
3. Cấu hình HTTP Bearer security, security requirement theo từng operation và thử nút Authorize sau khi Auth chạy.
4. Cấu hình serializer và schema enum/null nhất quán; ví dụ không dùng mật khẩu/token thật hoặc UserId của dữ liệu thật.
5. Mở /swagger trong Development và kiểm tra tài liệu /swagger/v1/swagger.json theo cấu hình dự kiến. URL/port cụ thể lấy từ môi trường chạy, không đoán một localhost cố định trong tài liệu.
6. Thử một request thành công, một validation lỗi và một request thiếu quyền; đối chiếu body/header/status với tài liệu.
7. Kiểm tra tài liệu OpenAPI không có route trùng, operationId trùng hoặc PasswordHash.
8. Giữ Swagger Development theo hiện trạng; khi cần tài liệu ở môi trường ngoài thì cấu hình quyền truy cập riêng, không bật công khai chỉ để demo tiện.

Swagger là tài liệu tương tác và công cụ thử thủ công. Một lần Try it out thành công không xác nhận mọi trường hợp quyền, giao dịch và worker đều đúng.

## 12. Kịch bản thử xuyên suốt bằng Swagger

1. Register Student A và B bằng dữ liệu thử; login A, lấy access token thực tế và Authorize.
2. Tạo học kỳ, lưu ID từ response; dùng ID đó tạo môn và lịch học.
3. Tạo task có môn, có hạn; kiểm tra 201 và Location; GET lại.
4. Tạo task không môn/không hạn; kiểm tra NULL được chấp nhận đúng trường.
5. Thêm hai con cho một task; cập nhật một con 60%, con kia 0%; GET cha thấy 30%.
6. Đổi hạn cha; GET cây/calendar thấy hạn hiệu lực đồng nhất; xem history.
7. Gửi self-parent hoặc chuyển cha thành hậu duệ; nhận 409 TASK_CYCLE, dữ liệu không đổi.
8. Complete một lá; gửi lại không thêm lịch sử vô ích; reopen làm completedAt NULL.
9. Xem recommendations và Dashboard; kiểm tra cha không bị cộng khối lượng thêm lần nữa.
10. Khi worker được triển khai, tạo mốc nhắc gần trong dữ liệu thử; xem Delivered, unread count và read. Không giả lập đã gửi chỉ bằng response tạo task.
11. Login B, Authorize bằng token B; thử ID học kỳ/task/notification của A: 404, không có dữ liệu A trong body hoặc danh sách.
12. Student gọi Admin: 403. Thử không Authorize: 401. Thử token hết hạn: 401.
13. Xóa môn bằng A: lịch bị xóa, task còn với courseId NULL; history thể hiện bỏ liên kết.
14. Xóa task cha: các con không còn, history vẫn truy vấn được theo taskId chụp lại.

Tạo dữ liệu độc lập cho từng test tự động; không phụ thuộc vào việc người dùng đã click đúng thứ tự trên Swagger. Với DB và worker, dùng integration test bổ sung.

## 13. Những hợp đồng cần hoàn thiện trước từng phần code

- Giới hạn độ dài/validation từng trường theo ERD, chính sách password và khoảng giờ nhắc phù hợp.
- Cơ chế phiên bản sửa đồng thời; nếu thêm cột thì cập nhật ERD và migration, nếu thêm header thì mô tả trong OpenAPI.
- Bật/tắt nhắc, nhắc cha/con và luồng thay đổi mặc định tài khoản nếu quyết định khác ERD hiện tại.
- Cơ chế thu hồi token nếu yêu cầu đăng xuất/đổi mật khẩu vô hiệu hóa phiên ngay.
- Kích thước cây tối đa/query budget và lỗi khi vượt giới hạn, không trả cây bị cắt mà giả vờ đầy đủ.
- Chính sách lưu lịch sử và quyền quản trị dữ liệu cá nhân.

Không triển khai những phần phụ thuộc bằng giả định ngầm. Các endpoint trong tài liệu được chia theo giai đoạn của kế hoạch; đây không phải yêu cầu viết toàn bộ API cùng lúc.
