# Đặc tả nghiệp vụ bản đầu — Study Planner

Ngày: 2026-10-13. Mục đích: xác định website phục vụ ai, người dùng thao tác gì, hệ thống xử lý thế nào và điều kiện nào chứng minh đã làm đúng.

**Người dùng đã xác nhận trong hội thoại:** phần tối ưu của bản đầu là **gợi ý việc nên làm trước, có giải thích**. Chưa triển khai tự phân bổ công việc vào giờ rảnh. Xác nhận này không đồng nghĩa người dùng đã đọc hoặc duyệt toàn bộ các quy tắc, tài liệu hoặc code.

**Trạng thái những phần còn lại:** phương án nghiệp vụ đề xuất để triển khai từng chức năng, cần rà lại khi chức năng đó bắt đầu. Chưa có code nghiệp vụ tương ứng. Không cần chờ chốt công thức thuật toán mới chuẩn bị khung API/Swagger; các quyết định ảnh hưởng dữ liệu phải được giải quyết trước migration liên quan.

Đọc cùng [ERD](erd.md), [API/Swagger](THIET-KE-API-SWAGGER.md) và [kế hoạch triển khai](KE-HOACH-TRIEN-KHAI-CHI-TIET.md). Tài liệu này mô tả hành vi; ERD mô tả lưu trữ; tài liệu API mô tả hợp đồng HTTP. Nếu thay hành vi, cập nhật cả tài liệu có liên quan và lịch sử, không để ba tài liệu tự mâu thuẫn.

## 1. Căn cứ và giới hạn nghiên cứu

Đã đối chiếu code: Domain còn Class1 rỗng, DbContext chưa có DbSet; Program.cs có Swagger Development nhưng chưa có API nghiệp vụ. Những hành vi dưới đây chưa tồn tại trong ứng dụng.

Ngày 2026-10-13 đã xem nguồn chính thức để phân biệt các khái niệm:

| Điều được nguồn mô tả | Ý nghĩa đối với thiết kế của nhóm |
|---|---|
| MyStudyLife giới thiệu quản lý lịch học, bài tập, tiến độ một phần và lời nhắc. [Nguồn: trang tính năng](https://mystudylife.com/tour/) | Luồng học tập cần nối lịch, nhiệm vụ và tiến độ, không chỉ CRUD danh mục |
| Todoist phân biệt ngày dự định làm với deadline phải hoàn thành. [Nguồn: hướng dẫn deadline](https://www.todoist.com/help/todoist/features/introduction-to-deadlines-in-todoist-uMqbSLM6U) | DueDate của nhóm được định nghĩa là hạn hoàn thành; không dùng nó như thời điểm bắt đầu tự học |

Đây là khảo sát tài liệu chính thức, chưa thử trực tiếp tài khoản sản phẩm và chưa phỏng vấn sinh viên. Công thức tiến độ, kế thừa hạn, cách nhắc và xếp thứ tự bên dưới là lựa chọn riêng cho dự án, không gán cho sản phẩm tham khảo. Chưa có bằng chứng thực nghiệm website giúp giảm trễ hạn hoặc tăng kết quả học tập.

## 2. Bài toán, người dùng và phạm vi

Sinh viên có lịch học cố định và nhiều công việc với hạn/khối lượng khác nhau. Website giúp xem lịch, ghi việc, chia nhỏ, cập nhật tiến độ, nhận nhắc và hiểu nên làm việc nào trước.

| Vai trò | Được làm | Giới hạn |
|---|---|---|
| Khách | Đăng ký, đăng nhập | Không đọc dữ liệu cá nhân |
| Student | Quản lý hồ sơ, học kỳ, môn, lịch, nhiệm vụ của mình; đọc thông báo/lịch sử/gợi ý | Không quản lý dữ liệu sinh viên khác hoặc tự cấp quyền Admin |
| Admin | Quản lý danh mục trường/khoa/khóa/lớp và trạng thái tài khoản | Không mặc định có quyền sửa nhiệm vụ cá nhân hoặc đọc mọi lịch sử |

Bản đầu có: tài khoản, học kỳ, môn, lịch hằng tuần, nhiệm vụ, nhiệm vụ con, tiến độ, lịch sử, Dashboard, nhắc trong ứng dụng, gợi ý có lý do và quản trị danh mục.

Để sau: tự xếp giờ học, thời gian rảnh, bấm giờ thực học, nhiệm vụ lặp, ngoại lệ lịch, email/push, chia sẻ nhóm, tệp đính kèm, đồng bộ lịch ngoài. Exam trong bản đầu là loại nhiệm vụ có deadline, chưa phải sự kiện thi đầy đủ giờ/phòng.

## 3. Từ ngữ thống nhất

| Khái niệm | Nghĩa trong dự án |
|---|---|
| Học kỳ | Khoảng ngày do sinh viên tự tạo, chứa các môn của riêng mình |
| Môn | Một môn cá nhân thuộc một học kỳ; không phải lớp học phần chung toàn trường |
| Buổi học | Mẫu học theo thứ/giờ lặp trong học kỳ; không là buổi tự học của nhiệm vụ |
| Deadline / DueDate | Thời điểm phải hoàn thành, có thể để trống |
| Hạn hiệu lực | Hạn nhập trên nhiệm vụ, hoặc hạn kế thừa từ tổ tiên gần nhất có hạn |
| Nhiệm vụ lá | Nhiệm vụ không có con; người dùng cập nhật tiến độ trực tiếp |
| Nhiệm vụ cha | Có con; tiến độ được tính từ con, không sửa tay |
| Ưu tiên | Low/Medium/High do người dùng chọn |
| Trạng thái | NotStarted/InProgress/Completed phản ánh tiến độ |
| Quá hạn | Chưa hoàn thành và hạn hiệu lực đã qua; là giá trị tính, không là trạng thái thứ tư |
| Thời lượng ước lượng | Tổng số phút dự kiến cho nhiệm vụ lá, không phải thời gian đo thực tế |
| Gợi ý | Danh sách việc nên làm trước có lý do; không tự sửa kế hoạch hoặc tạo buổi học |

## 4. Luồng chính từ đầu đến cuối

1. Sinh viên đăng ký và đăng nhập; chưa có lớp/mã sinh viên vẫn được dùng.
2. Tạo học kỳ, thêm môn và nhập các buổi học hằng tuần.
3. Tạo bài tập theo môn hoặc việc cá nhân không môn; deadline/ước lượng có thể để trống.
4. Nếu việc lớn, tạo các phần con. Website hiển thị hạn nhập và hạn kế thừa rõ ràng.
5. Cập nhật tiến độ ở lá; website tính lại cha, ghi lịch sử và xử lý lời nhắc liên quan.
6. Xem Dashboard/lịch: biết lịch học, việc đến hạn, việc quá hạn.
7. Mở gợi ý: thấy thứ tự, lý do, thời lượng ước lượng còn lại và dữ liệu thiếu.
8. Hoàn thành việc: bỏ khỏi danh sách việc chưa xong/gợi ý, hủy lời nhắc chưa phát; vẫn xem lại lịch sử.

## 5. Đặc tả từng chức năng

### NV-01 — Tài khoản và hồ sơ

- Người dùng nhập họ tên, email, mật khẩu để đăng ký; đăng ký không tự đăng nhập trong hợp đồng đề xuất hiện tại.
- Email được chuẩn hóa thống nhất và kiểm tra duy nhất; mật khẩu chỉ lưu hash. Role do server gán Student.
- Login sai thông tin báo lỗi chung; tài khoản bị vô hiệu hóa không tiếp tục dùng API bảo vệ.
- Hồ sơ cho sửa tên, mã SV, lớp, ngành, múi giờ, giờ nhắc; không tự sửa role, isActive hoặc email trong bản đầu.
- Đổi mật khẩu cần mật khẩu cũ. Xác thực bản đầu và giới hạn thu hồi token theo tài liệu API, không coi logout client là đã vô hiệu hóa mọi token.
- Mã SV là tùy chọn; giả định duy nhất trong một đơn vị triển khai như ERD. Nếu triển khai nhiều trường phải xem lại quy tắc trước nhập dữ liệu thật.

**Đạt khi:** đăng ký → login → đọc/sửa me thành công; email trùng bị chặn; không nhận PasswordHash trong response; Student không tự nâng quyền.

### NV-02 — Học kỳ và môn

- Sinh viên tạo học kỳ bằng tên, ngày bắt đầu/kết thúc; ngày kết thúc không trước ngày bắt đầu. Cho phép các học kỳ trùng thời gian.
- Tạo môn trong học kỳ của mình bằng tên; mã, tín chỉ, giảng viên, màu có thể bổ sung sau. Tín chỉ nếu nhập phải lớn hơn 0.
- Không tự giới hạn nhiệm vụ phải có deadline trong học kỳ: học kỳ giới hạn lịch học; deadline nhiệm vụ có thể ngoài khoảng đó và giao diện có thể cảnh báo.
- Xóa môn/học kỳ phải báo ảnh hưởng; xóa lịch tương ứng, giữ task và bỏ CourseId; ghi lịch sử task bị bỏ liên kết.

**Đạt khi:** người dùng chỉ thao tác học kỳ/môn của mình; xóa môn không làm mất bài tập; tải lại vẫn thấy thay đổi.

### NV-03 — Lịch học hằng tuần

- Nhập thứ 1–7 (thứ Hai đến Chủ nhật), giờ bắt đầu/kết thúc và phòng tùy chọn cho mỗi mẫu lịch của môn.
- Một môn có nhiều mẫu; giờ kết thúc sau giờ bắt đầu, chưa hỗ trợ qua nửa đêm.
- Khi xem ngày/tuần/tháng, sinh các lần xuất hiện theo mẫu trong biên học kỳ. Tuần bắt đầu thứ Hai; lịch hiển thị theo múi giờ tài khoản.
- Lịch trùng có cảnh báo nhưng được lưu; không hỗ trợ nghỉ lễ/học bù/tuần luân phiên trong bản đầu.

**Đạt khi:** không sinh buổi ngoài học kỳ; thứ/giờ hiển thị đúng; deadline không bị vẽ như một buổi học kéo dài theo EstimatedMinutes.

### NV-04 — Tạo và sửa nhiệm vụ

- Bắt buộc tiêu đề không rỗng. Type mặc định Personal nếu không gắn môn, Assignment nếu có môn; client có thể chọn một giá trị Type hợp lệ khác. Đây là quy tắc mặc định đề xuất cần đồng bộ DTO khi làm API.
- Priority mặc định Medium; Progress = 0, Status = NotStarted khi tạo. Course, mô tả, deadline và ước lượng là tùy chọn.
- CourseId phải thuộc người dùng. Tạo con thì kế thừa môn/chủ sở hữu của cha; không nhận môn khác cha.
- Cho tạo task đã quá hạn để ghi nhận việc đang nợ; hiển thị quá hạn, không tạo lời nhắc trước hạn mới cho mốc đã qua.
- Sửa nội dung không tự thay đổi tiến độ; thao tác tiến độ/hoàn thành/mở lại có endpoint riêng. Ghi lịch sử và cập nhật lịch nhắc khi dữ liệu liên quan đổi.

**Đạt khi:** tạo được việc không môn/không hạn; dữ liệu không hợp lệ bị từ chối; client không tự gán owner hoặc thời điểm hoàn thành.

### NV-05 — Tiến độ và nhiệm vụ con

- Lá: 0% → NotStarted; 1–99% → InProgress; 100% → Completed và ghi CompletedAt. Mở lại chọn 0–99%, mặc định 0, xóa CompletedAt.
- Cha: tất cả con xong → 100%/Completed; tất cả con 0% → 0%/NotStarted; còn lại trung bình con trực tiếp, làm tròn xuống và giới hạn 1–99%/InProgress.
- Ví dụ hai con 60% và 0% thì cha 30%. Các con cùng trọng số; không gọi đây là tỷ lệ theo khối lượng thực tế. Cây nhiều tầng tính từ dưới lên.
- Không đánh dấu hoàn thành cha bằng tay khi còn con. Nếu muốn hoàn thành toàn cây bằng một thao tác, đó là chức năng bổ sung cần hợp đồng riêng.
- Không cho tự làm cha, gắn vào hậu duệ hoặc dùng task người khác. Các thành viên trong cây cùng môn, kể cả cùng NULL.
- Thêm con làm cha chuyển từ tiến độ nhập tay sang tính toán, EstimatedMinutes cha thành NULL; phải báo trước trên UI và giữ lịch sử giá trị cũ.
- Xóa hết con: giữ tiến độ cuối của cha và chuyển thành lá theo ERD. Khi đó người dùng được sửa tiến độ/ước lượng lại.
- Chuyển cha: cập nhật môn cả cây chuyển theo cha mới nếu khác môn, kiểm tra hạn và tính lại các tổ tiên cũ/mới; giao diện thể hiện tác động trước khi gửi.

**Đạt khi:** không tạo chu trình; cập nhật một lá tính lại đúng tổ tiên; lỗi ở một bước rollback dữ liệu, lịch sử và lời nhắc liên quan.

### NV-06 — Hạn và quá hạn

- Nhiệm vụ tự nhập hạn thì dùng hạn đó; không nhập thì tìm tổ tiên gần nhất có hạn; không có tổ tiên có hạn thì không có deadline.
- Hạn nhập của con không được sau hạn hiệu lực cha. Đổi hạn/chuyển cha phải kiểm tra toàn cây bị ảnh hưởng; nếu vi phạm thì từ chối và chỉ ra task cần sửa, không âm thầm dời deadline con.
- Bỏ DueDate của con nghĩa là trở lại kế thừa, không có nghĩa chắc chắn không hạn. UI phải giải thích rõ.
- Quá hạn tại thời điểm xét nếu EffectiveDueDate < now và chưa Completed. Bằng đúng hạn chưa tính quá hạn theo phép so sánh này.
- Lưu thời điểm UTC; người dùng nhập/đọc theo múi giờ tài khoản. Không trộn ngày dự định làm với hạn hoàn thành.

**Đạt khi:** list, calendar, Dashboard, notifications và recommendations dùng chung quy tắc hạn hiệu lực; không có chỗ dùng DueDate riêng khiến con kế thừa bị bỏ sót.

### NV-07 — Thông báo

- Bản đầu chỉ thông báo trong ứng dụng, một mốc DeadlineReminder/mỗi task. Giờ nhắc riêng nếu có, nếu NULL dùng User.ReminderHours, mặc định 24 giờ; 0 nghĩa là đúng hạn.
- Có hạn hiệu lực, chưa hoàn thành → tính mốc nhắc. Nếu mốc đã qua mà hạn chưa qua, nhắc sớm nhất có thể. Nếu hạn đã qua, không tạo mới nhắc trước hạn.
- Theo ERD hiện tại, cha và con đủ điều kiện đều có thể được nhắc; chưa có bật/tắt nhắc riêng. Đây là giới hạn cần người dùng nhận biết, có thể sửa trước khi làm module thông báo nếu gây phiền.
- Đổi deadline/hạn kế thừa/giờ nhắc/default User thì tính lại; complete hủy Pending/Failed; Delivered vẫn đọc được. Mở lại tính lời nhắc theo hạn hiện hành.
- Worker phát khi đến giờ và còn hợp lệ; trạng thái/đọc/thử lại theo ERD. Host dừng có thể làm nhắc trễ; khởi động lại phải xử lý các bản ghi đang chờ sau khi xác minh lại.
- Một bản ghi được dùng lại khi đổi lịch nhắc, nên chưa lưu lịch sử mọi lần phát; không hứa hộp thông báo giữ nguyên thông báo cũ sau mọi thay đổi lịch.

**Đạt khi:** Pending không hiện như đã gửi; chỉ người nhận đọc được; đổi kế hoạch đã commit không bị worker phát theo dữ liệu cũ; đọc lại không sửa thời điểm đọc đầu.

### NV-08 — Dashboard, tìm kiếm và lịch

- Có danh sách việc với bộ lọc rõ, phân trang, danh sách rỗng và lỗi tải.
- Dashboard có lịch ngày chọn, việc cần chú ý và thống kê theo phạm vi môn/học kỳ đã chọn hoặc toàn bộ.
- Thống kê khối lượng dùng lá để tránh cộng thêm cha; dueToday xét ngày theo múi giờ; overdue xét thời điểm hiện tại; mọi con số trả scope/generatedAt.
- Chưa gọi danh sách deadline hôm nay là lịch các việc đã được xếp làm hôm nay. Task không hạn có khu vực riêng, không mất khỏi ứng dụng.

**Đạt khi:** bộ lọc và số liệu có định nghĩa; con kế thừa hạn xuất hiện đúng; UI không dùng tổng số event làm tổng số việc độc lập.

### NV-09 — Gợi ý việc nên làm trước (phạm vi đã được người dùng chọn)

- Chỉ xét lá chưa hoàn thành của người dùng trong phạm vi chọn; không tự sửa task hoặc tạo buổi học.
- Trả thứ tự, lý do, hạn hiệu lực, ưu tiên, ước lượng còn lại nếu biết và dữ liệu thiếu. Ước lượng còn lại = ceil(EstimatedMinutes × (100 − Progress)/100); thiếu thời lượng thì NULL, không là 0.
- Bản cơ sở đề xuất: quá hạn trước → việc có hạn từ gần đến xa → việc không hạn; cùng hạn theo High/Medium/Low, cuối cùng Id để ổn định. Với nhóm quá hạn cũng sắp theo hạn tăng dần.
- Việc không hạn vẫn xuất hiện ở nhóm cuối và sắp theo ưu tiên/Id. Nói rõ đây là chính sách bản cơ sở; nhiệm vụ quan trọng không hạn có thể bị xếp sau mọi việc có hạn, cần đánh giá trước khi chọn thuật toán cuối.
- Cùng dữ liệu và thời điểm cho cùng kết quả. Khi hoàn thành/đổi hạn/đổi ưu tiên/tiến độ, lần tính tiếp theo phản ánh dữ liệu mới.
- Sau bản cơ sở mới thử thêm tín hiệu lượng việc còn lại, so sánh trên bộ ví dụ có đáp án kỳ vọng; không chốt trọng số thiếu căn cứ. Lý do hiển thị phải đúng yếu tố thực sự dùng trong xếp hạng.

**Đạt khi:** giải thích được thứ tự từng việc và giới hạn; không tuyên bố bảo đảm kịp hạn khi chưa có thời gian rảnh; có kiểm thử và đánh giá so với baseline.

### NV-10 — Xóa, lịch sử và quản trị

- Xóa task cha xóa cả cây con và thông báo của các task; ghi lịch sử từng dòng trước cascade. Xóa con tính lại tổ tiên còn sống.
- Xóa môn/học kỳ giữ task, bỏ CourseId; lịch sử ghi các thay đổi task; việc ghi lịch sử không tự xảy ra chỉ vì DB có cascade.
- History lưu TaskId chụp lại không FK, TaskTitle, Action, trước/sau và thời điểm; chỉ chứa trường cho phép. Người dùng xem được sự kiện của mình sau khi task đã bị xóa.
- Quản trị bản đầu dùng vô hiệu hóa tài khoản; nếu làm xóa hẳn, UserId của History thành NULL và không hứa giữ được danh tính người đã xóa.
- Danh mục trường/khoa/khóa/lớp theo quy tắc xóa ERD. Student không có quyền quản trị; ngăn vô hiệu hóa Admin cuối cùng.

**Đạt khi:** không mất lịch sử vì xóa task; không truy cập lịch sử của người khác; xóa danh mục không kéo theo xóa tài khoản/dữ liệu cá nhân ngoài quy tắc đã nêu.

## 6. Nối nghiệp vụ với màn hình, API và dữ liệu

| Nghiệp vụ | Màn hình | API tiêu biểu | Dữ liệu chính |
|---|---|---|---|
| NV-01 | Đăng ký/login/hồ sơ | /api/auth/register, /api/auth/login, /api/users/me | User |
| NV-02 | Học kỳ, môn | /api/semesters, /api/semesters/{id}/courses | Semester, Course |
| NV-03 | Form lịch môn, Calendar | /api/courses/{id}/class-sessions, /api/calendar | ClassSession và biên Semester |
| NV-04 | Form/list/chi tiết task | /api/tasks, /api/tasks/{id} | TaskItem, TaskHistory, lịch nhắc |
| NV-05 | Cây task, tiến độ | children, parent, progress, complete, reopen của task | TaskItem và tổ tiên, TaskHistory |
| NV-06 | Form deadline, lịch | TaskResponse.effectiveDueDate và các truy vấn có hạn | TaskItem.DueDate, quan hệ cha |
| NV-07 | Cài đặt, hộp thông báo | /api/notifications, unread-count, read | User, TaskItem, Notification |
| NV-08 | Dashboard/list/Calendar | /api/dashboard, /api/tasks, /api/calendar | Tổng hợp, không cần bảng Dashboard |
| NV-09 | Gợi ý | /api/planning/recommendations | Nhiệm vụ lá và dữ liệu tính toán, không cần bảng lưu thứ tự |
| NV-10 | Xác nhận xóa/lịch sử/Admin | deletion-preview, DELETE, /api/task-history, /api/admin/... | Các bảng liên quan trong giao dịch |

Các suffix như children/progress ở bảng này là cách gọi ngắn; method/route đầy đủ nằm trong tài liệu API. Một thao tác người dùng có thể cập nhật nhiều bảng, không thiết kế API đơn thuần theo mỗi bảng.

## 7. Tình huống nghiệm thu tiêu biểu

| Mã | Trước/thao tác | Kết quả cần có |
|---|---|---|
| NT-01 | Đăng ký không mã SV/lớp | Tạo tài khoản Student, dùng được chức năng cá nhân |
| NT-02 | Tạo việc không môn/không hạn | Lưu được, không có deadline event hoặc nhắc trước hạn |
| NT-03 | Lá từ 0% lên 60%, rồi complete | InProgress/60%; sau đó Completed/100% có CompletedAt |
| NT-04 | Hai con 60% và 0% | Cha 30%/InProgress; không sửa tay tiến độ cha |
| NT-05 | Con NULL deadline, cha hạn 20/10 | Con dùng hạn hiệu lực cha; UI chỉ rõ kế thừa |
| NT-06 | Cha đang hạn 20/10, con nhập 19/10; đổi cha về 18/10 | Từ chối do con vượt hạn mới, giữ nguyên cả cây |
| NT-07 | Gắn cha vào một hậu duệ | Từ chối chu trình, không thay dữ liệu |
| NT-08 | Complete task trước lúc worker phát | Nhắc chưa phát bị hủy, không hiện như đã gửi |
| NT-09 | Xóa môn đang có task và lịch | Lịch bị xóa; task giữ lại, CourseId NULL; history ghi bỏ liên kết |
| NT-10 | Xóa cây task | Task/con/notification bị xóa; history của người dùng vẫn đọc được |
| NT-11 | Student B dùng ID của A | Không đọc/sửa được; API trả theo quy ước 404/403 tương ứng |
| NT-12 | Hoàn thành một lá trong danh sách gợi ý | Lần tính mới không còn lá đó; các mục còn lại có lý do đúng |

Ví dụ ngày trong NT-05/06 chỉ nhằm kiểm tra quan hệ hạn, khi chạy test dùng thời điểm cố định đầy đủ năm/múi giờ. Nghiệm thu không phụ thuộc ngày hiện tại của máy.

## 8. Quyết định đã có và việc cần hoàn thiện

| Nội dung | Trạng thái | Khi nào xử lý |
|---|---|---|
| Gợi ý có giải thích, chưa tự xếp giờ | Người dùng đã chọn trong hội thoại | Áp dụng cho bản đầu |
| Quy tắc NV-01–10 | Đề xuất cụ thể, chưa tự đánh dấu người dùng đã duyệt | Rà theo từng chức năng trước code |
| Tắt nhắc/nhắc cha-con | Giữ hành vi ERD hiện tại trong bản đề xuất | Quyết định trước schema/logic nhắc nếu muốn thay đổi |
| Đăng nhập và sửa đồng thời | Có lựa chọn/giới hạn trong tài liệu API; cần chốt thiết kế kỹ thuật cụ thể | Trước Auth/API sửa và migration liên quan |
| Chính sách lịch sử, nhiều trường trùng mã | Chưa có yêu cầu vận hành đầy đủ | Trước sử dụng dữ liệu thật tương ứng |
| Công thức gợi ý cuối | Có baseline đề xuất; chưa có đánh giá thực nghiệm | Trước hoàn thiện module gợi ý |
| Khảo sát sinh viên và dùng thử sản phẩm | Chưa thực hiện | Bổ sung bằng quan sát/phản hồi thực tế, không giả ghi đã khảo sát |

Không yêu cầu người dùng quyết định các chi tiết kỹ thuật như tên lớp/repository để bắt đầu. Các lựa chọn triển khai thường lệ được giải quyết bằng code, kiểm chứng và ghi lịch sử; hỏi khi thay đổi hành vi hoặc phạm vi đáng kể.

## 9. Giai đoạn này hoàn thành khi nào?

- Có phạm vi đủ rõ, đặc biệt ý nghĩa của phần tối ưu đã được chọn.
- Giải thích được luồng chính, trường bắt buộc/tùy chọn và các ngoại lệ ở mục 7.
- Màn hình, API, ERD cùng phản ánh một quy tắc, không có điểm mâu thuẫn ngăn chức năng sắp triển khai.
- Ghi rõ những điều chưa xác nhận/chưa khảo sát, không lấy tài liệu thay cho kiểm chứng thực tế.
- Sau rà soát, bắt đầu phần nền API/Swagger; quyết định ảnh hưởng schema phải được giải quyết trước khi sinh migration đó. Chưa coi toàn bộ thiết kế là đã nghiệm thu chỉ vì tài liệu được tạo.

**Việc triển khai kế tiếp:** chuẩn hóa Program.cs/routing/lỗi/Swagger nền theo kế hoạch; rồi Entity, cấu hình và migration phù hợp những quy tắc đã thống nhất. Tác vụ đặc tả này chưa viết code ứng dụng hoặc áp dụng migration.
