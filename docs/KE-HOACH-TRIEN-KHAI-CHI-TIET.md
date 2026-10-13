# Kế hoạch triển khai chi tiết Study Planner

Ngày đối chiếu: 2026-10-13. Trạng thái: kế hoạch đề xuất, chưa phải các chức năng đã chạy hoặc phạm vi đã được người dùng duyệt.

Đọc cùng [ERD](erd.md), [kiến trúc backend](KIEN-TRUC-BACKEND.md), [thiết kế API và Swagger](THIET-KE-API-SWAGGER.md). Tài liệu này mở rộng [kế hoạch tổng quan](KE-HOACH-PHAT-TRIEN.md), không thay thế lịch sử kiểm chứng của các lần trước.

Đã bổ sung [đặc tả nghiệp vụ bản đầu](DAC-TA-NGHIEP-VU-BAN-DAU.md) để lần theo luồng, quy tắc và tình huống nghiệm thu. Người dùng đã chọn phần tối ưu là gợi ý việc nên làm trước, có giải thích; chưa tự xếp giờ học. Đây là xác nhận phạm vi phần tối ưu, không xác nhận toàn bộ kế hoạch/code đã được đọc hoặc duyệt.

## Mục lục

Chi tiết luồng, ngoại lệ, ảnh hưởng và nghiệm thu theo chức năng: [bộ đặc tả module](modules/README.md). Quyết định còn mở được dẫn từ mục lục module; chốt phần liên quan trước từng đầu việc code.

1. Hiện trạng và mục tiêu
2. Cách tìm hiểu nghiệp vụ thực tế
3. Phạm vi phiên bản đầu và phần mở rộng
4. Vai trò, quyền và các màn hình
5. Quy tắc cần xác định trước khi code
6. Kiến trúc, dữ liệu và môi trường
7. Thiết kế API và Swagger
8. Kế hoạch từng giai đoạn
9. Thuật toán gợi ý và cách đánh giá
10. Kiểm thử và tiêu chí nghiệm thu
11. Kịch bản demo
12. Theo dõi tiến độ, tài liệu và bàn giao

## 1. Hiện trạng và mục tiêu

### 1.1. Đã có trong code

| Hạng mục | Đã xác minh | Chưa có hoặc chưa xác minh |
|---|---|---|
| Backend | Solution .NET 10: Api, Application, Domain, Infrastructure; SDK 10.0.400 cố định bởi global.json | Chưa có nghiệp vụ trong Domain/Application |
| Dữ liệu | EF Core, provider PostgreSQL, ApplicationDbContext | DbContext chưa có DbSet; chưa có Entity nghiệp vụ |
| Migration | InitialCreate và model snapshot | Chưa định nghĩa bảng nghiệp vụ; chưa kiểm tra database đang chạy |
| API | GET /; GET /weatherforecast; GET /health/live; AddControllers và MapControllers | Chưa có controller nghiệp vụ học tập hoặc /health/ready |
| Swagger | Swashbuckle 10.3.0, tài liệu v1 và UI trong Development; health đã có schema | Chưa có API nghiệp vụ học tập hoặc Authorize |
| Thiết kế | ERD đề xuất 11 bảng, tài liệu kiến trúc và lịch sử | Chưa triển khai các quy tắc bằng code |
| Frontend/test | Chưa thấy frontend và project test trong file được kiểm kê | Chưa có luồng giao diện hoặc kiểm thử tự động nghiệp vụ |

GET / và /health/live không kiểm tra database. [Bản ghi nâng .NET 10](history/2026-10-13-nang-cap-dotnet-10.md) ghi restore/build bằng SDK 10.0.400 và kiểm chứng HTTP health/Swagger/endpoint mẫu. Các giai đoạn dưới vẫn là kế hoạch; phần nền đã làm chưa đồng nghĩa toàn bộ giai đoạn 1 đã hoàn thành.

### 1.2. Mục tiêu sản phẩm

Một sinh viên có thể trả lời bốn câu hỏi khi mở website:

1. Hôm nay mình học môn nào, lúc nào, ở đâu?
2. Mình còn những việc gì, việc nào sắp hạn hoặc đã quá hạn?
3. Mỗi bài tập đã làm đến đâu và phần nào chưa xong?
4. Nên bắt đầu việc nào tiếp theo, vì sao?

Luồng sử dụng mục tiêu:

Đăng ký/đăng nhập → tạo học kỳ → thêm môn/lịch học → tạo nhiệm vụ → chia nhỏ và cập nhật tiến độ → xem Dashboard/lịch → nhận lời nhắc → xem gợi ý việc nên làm → xem lịch sử.

## 2. Cách tìm hiểu nghiệp vụ thực tế

Không suy toàn bộ nghiệp vụ từ file đề tài hoặc ERD. File đề tài dùng kiểm tra mục tiêu/phạm vi, code dùng xác định phần đã làm; sản phẩm thực tế và người dùng giúp xác định hành vi cần thiết.

### 2.1. Nội dung cần khảo sát

| Nhóm tham khảo | Câu hỏi khảo sát | Kết quả cần ghi |
|---|---|---|
| Sản phẩm quản lý học tập, ví dụ MyStudyLife | Bắt đầu học kỳ thế nào? Môn và lịch học liên hệ ra sao? | Luồng nhập học kỳ, môn, lịch, bài tập và các giới hạn |
| Sản phẩm quản lý việc, ví dụ Todoist/TickTick | Tạo nhanh, không deadline, hoàn thành, nhiệm vụ con hoạt động thế nào? | Quy tắc thao tác và cách hiển thị việc còn lại |
| Sản phẩm lập lịch, ví dụ Motion | Cần đầu vào nào để xếp lịch? Khi bỏ lỡ thì xử lý thế nào? | Phân biệt xếp thứ tự với phân bổ thời gian |
| Sinh viên dùng thử | Hiện ghi việc ở đâu? Hay quên gì? Có sẵn lòng nhập thời lượng không? | Tình huống sử dụng, dữ liệu thực tế và điểm bất tiện |

Đây là danh sách cần khảo sát, không khẳng định đã thử trực tiếp những sản phẩm này. Dùng tài liệu chính thức và thao tác thử khi có quyền truy cập; ghi ngày, nguồn, điều quan sát được, điều suy luận và đề xuất riêng cho nhóm. Không sao chép toàn bộ sản phẩm hoặc bổ sung tính năng chỉ vì sản phẩm khác có.

### 2.2. Đầu ra của khảo sát

- Một luồng từ lúc sinh viên mới vào đến khi hoàn thành bài tập.
- Các ví dụ ngoại lệ: việc không hạn, đổi hạn, bài tập nhiều phần, quên cập nhật tiến độ, xóa môn.
- Một bảng quyết định: cần cho bản đầu / để sau / không phù hợp, có lý do.
- Xác định mức chi tiết người dùng sẵn lòng nhập; không buộc nhập mọi thông tin hồ sơ trước khi tạo nhiệm vụ.

Có thể triển khai phần nền ít phụ thuộc trong khi khảo sát. Phần cây nhiệm vụ và gợi ý cần quyết định nghiệp vụ rõ trước khi viết logic.

## 3. Phạm vi phiên bản đầu và phần mở rộng

### 3.1. Phạm vi đề xuất

| Nhóm | Chức năng bản đầu | Mức ưu tiên |
|---|---|---|
| Tài khoản | Đăng ký, đăng nhập, đăng xuất phía client, xem/sửa hồ sơ, đổi mật khẩu | P0 |
| Học kỳ | Tạo, xem, sửa, xóa; ngày bắt đầu/kết thúc | P0 |
| Môn học | Môn thuộc học kỳ, mã/tên/tín chỉ/giảng viên/màu | P0 |
| Nhiệm vụ lá | Tạo/sửa/xóa, ưu tiên, hạn tùy chọn, loại, thời lượng, tiến độ, hoàn thành/mở lại | P0 |
| Tìm và lọc | Từ khóa, môn, học kỳ, trạng thái, loại, ưu tiên, hạn, quá hạn, phân trang | P0 |
| Cây nhiệm vụ | Thêm con, chuyển cha, hạn kế thừa, tính lại tổ tiên | P1 |
| Lịch sử | Xem thay đổi, vẫn xem sự kiện sau khi nhiệm vụ bị xóa | P1 |
| Lịch học | Mẫu lặp theo tuần, xem ngày/tuần/tháng trong giới hạn học kỳ | P1 |
| Dashboard | Lịch hôm nay, việc sắp hạn/quá hạn, số liệu tiến độ theo định nghĩa rõ | P1 |
| Thông báo | Nhắc hạn trong ứng dụng, giờ nhắc, tác vụ nền, đánh dấu đã đọc | P1 |
| Gợi ý | Xếp thứ tự nhiệm vụ lá, kèm lý do và cảnh báo dữ liệu thiếu | P1 |
| Quản trị | CRUD danh mục trường/khoa/khóa/lớp; tìm tài khoản và bật/tắt hoạt động | P2 |

P0 là mốc sử dụng được đầu tiên; P1 hoàn thiện giá trị lập kế hoạch; P2 làm sau luồng cá nhân. Đây là ưu tiên triển khai, không có nghĩa P2 đã bị loại khỏi đồ án.

### 3.2. Phần mở rộng cần thiết kế thêm

- Tự chia nhiệm vụ thành các buổi tự học, lịch rảnh, chấp nhận/điều chỉnh kế hoạch.
- Lặp nhiệm vụ, tag, nghỉ lễ, tuần học luân phiên, học bù và ngoại lệ từng ngày.
- Email/push, nhiều mốc nhắc, nhật ký từng lần gửi.
- Bấm giờ thực học, báo cáo năng suất dựa trên thời gian thực tế.
- Lịch thi với giờ bắt đầu/kết thúc/phòng thi riêng. Type = Exam hiện chỉ là một nhiệm vụ.
- Chia sẻ, làm nhóm, đồng bộ lịch ngoài, tệp đính kèm.
- Quên mật khẩu qua email, xác minh email, refresh token và đăng nhập bên ngoài.

Các phần trên chưa được schema hiện tại hỗ trợ đầy đủ. Nếu bắt buộc trong yêu cầu nghiệm thu thì phải điều chỉnh phạm vi và dữ liệu trước khi triển khai; không đợi đến cuối mới phát hiện thiếu bảng/cột.

## 4. Vai trò, quyền và các màn hình

### 4.1. Quyền

- Khách: đăng ký, đăng nhập; không xem dữ liệu cá nhân.
- Student: quản lý dữ liệu của chính mình, đọc danh mục, nhận và đọc thông báo của mình.
- Admin: quản lý danh mục và trạng thái tài khoản. Không mặc định có chức năng sửa nhiệm vụ cá nhân của sinh viên; truy cập lịch sử phục vụ quản trị cần chính sách riêng trước khi mở API.
- Lấy danh tính từ phiên xác thực, không dùng UserId do client gửi làm căn cứ sở hữu.
- Áp dụng quyền ở backend cho từng endpoint, kể cả dữ liệu liên quan: CourseId, ParentTaskId, thông báo, lịch sử và truy vấn lịch.

### 4.2. Danh sách màn hình

| Màn hình | Nội dung | Thao tác và điều kiện |
|---|---|---|
| Đăng nhập | Email, mật khẩu, thông báo lỗi | Đăng nhập; ngăn gửi lặp; không tiết lộ email có tồn tại khi sai thông tin |
| Đăng ký | Họ tên, email, mật khẩu, xác nhận | Kiểm tra email trùng; không cho chọn Admin; mã sinh viên/lớp có thể nhập sau |
| Dashboard | Lịch hôm nay, việc cần chú ý, số liệu, gợi ý | Mở nhiệm vụ, chuyển sang lịch, hoàn thành nhiệm vụ lá |
| Học kỳ | Danh sách, ngày bắt đầu/kết thúc | Thêm/sửa; xem ảnh hưởng trước khi xóa |
| Chi tiết học kỳ/môn | Danh sách môn và lịch học của từng môn | Thêm/sửa/xóa môn, quản lý nhiều buổi học |
| Danh sách nhiệm vụ | Từ khóa, bộ lọc, hạn, tiến độ, ưu tiên | Tạo việc; giữ bộ lọc khi mở rồi quay lại; phân trang |
| Chi tiết nhiệm vụ | Mô tả, hạn nhập/hạn hiệu lực, con, lịch sử | Sửa; thêm/chuyển con; hoàn thành/mở lại lá; không sửa tay tiến độ cha |
| Form nhiệm vụ | Trường bắt buộc/tùy chọn, hướng dẫn thời lượng | Có thể tạo việc cá nhân không môn, không hạn; hiển thị nguồn hạn kế thừa |
| Lịch | Chế độ ngày/tuần/tháng, buổi học và deadline | Chọn ngày mở chi tiết; phân biệt buổi học với mốc hạn |
| Thông báo | Thông báo đã phát, đã/chưa đọc | Đọc một/tất cả; bấm mở nhiệm vụ còn tồn tại |
| Gợi ý công việc | Danh sách có thứ tự, lý do, dữ liệu đầu vào | Mở việc; cập nhật dữ liệu rồi tính lại; không ghi lịch tự động |
| Hồ sơ/cài đặt | Họ tên, mã SV, lớp, ngành, múi giờ, giờ nhắc | Sửa hồ sơ; đổi mật khẩu; giải thích ảnh hưởng đổi múi giờ/giờ nhắc |
| Quản trị danh mục | Trường, khoa, khóa, lớp | Lọc phụ thuộc; báo lỗi khi xóa danh mục đang được tham chiếu |
| Quản trị tài khoản | Tìm kiếm, vai trò, trạng thái | Vô hiệu hóa/kích hoạt; ngăn tự khóa tài khoản quản trị cuối cùng |
| Trang lỗi | Không tìm thấy, thiếu quyền, phiên hết hạn | Có đường quay lại/đăng nhập; không hiển thị exception nội bộ |

Mọi màn hình có trạng thái đang tải, dữ liệu rỗng, thành công, lỗi và thử lại phù hợp. Form giữ dữ liệu khi lỗi; nút lưu không gửi nhiều lần; màn hình nhỏ vẫn thao tác được. Màu ưu tiên/trạng thái phải đi kèm chữ hoặc biểu tượng có nhãn; hỗ trợ dùng bàn phím cho form cơ bản.

## 5. Quy tắc cần xác định trước khi code

ERD là thiết kế hiện có. Các quyết định dưới đây cần được ghi trong đặc tả chức năng và cập nhật ERD nếu thay đổi; không coi người dùng đã xác nhận.

| Vấn đề | Phương án dùng để lập kế hoạch | Ảnh hưởng |
|---|---|---|
| Mã sinh viên | Duy nhất trong đơn vị triển khai như ERD | Nếu nhiều trường trùng mã phải đổi unique và mô hình liên quan |
| Cây nhiệm vụ | Hỗ trợ nhiều cấp như ERD, không tự làm cha/không chu trình | Phải kiểm tra tổ tiên, cập nhật cả cây bị ảnh hưởng |
| Tiến độ cha | Trung bình con trực tiếp, theo quy tắc ERD | Không tuyên bố là tỷ lệ khối lượng/thời gian thực tế |
| Hạn của con | Có thể kế thừa tổ tiên gần nhất; không vượt hạn hiệu lực của cha | Calendar, thông báo, lọc và gợi ý cùng dùng một cách tính |
| Nhắc cha/con | Theo ERD, mọi nhiệm vụ đủ điều kiện đều có thể được nhắc | Có thể nhiều thông báo; cần khảo sát trước khi viết worker |
| Tắt nhắc | ERD hiện chưa có bật/tắt | Nếu cần, thêm trường ở User/TaskItem và sửa quy tắc trước migration |
| Xóa nhiệm vụ | Xóa cả cây con, ghi lịch sử từng dòng trước khi xóa | UI phải cho biết số nhiệm vụ bị xóa |
| Xóa môn/học kỳ | Xóa lịch học liên quan, giữ nhiệm vụ với CourseId NULL | Ghi lịch sử bỏ liên kết trong cùng giao dịch |
| Xóa tài khoản | Bản đầu quản trị ưu tiên vô hiệu hóa | Luồng xóa hẳn nếu làm phải giữ sự kiện lịch sử, đặt UserId NULL theo ERD |
| Múi giờ | Theo User.TimeZone; lưu thời điểm UTC | Lịch học là giờ địa phương, deadline là thời điểm tuyệt đối |
| Trùng lịch học | Cảnh báo nhưng cho lưu | Không tự cấm sinh viên lưu hai lớp có giờ trùng |
| Đổi email | Chưa cho đổi trong form hồ sơ bản đầu | Muốn đổi phải thiết kế xác minh/đăng nhập lại và chuẩn hóa unique |
| Đếm số liệu | Thống kê công việc dùng nhiệm vụ lá | Không cộng cha và con; danh sách vẫn hiển thị cây |
| Sửa đồng thời | Khóa/giao dịch cho cây; phát hiện bản cập nhật cũ | Chọn cơ chế phiên bản trước khi viết API sửa; nếu thêm Revision thì cập nhật ERD/migration |

Thêm con vào một nhiệm vụ từng là lá sẽ chuyển tiến độ cha sang giá trị tính từ con và đặt EstimatedMinutes của cha NULL theo ERD. Giao diện cần cảnh báo mất ước lượng nhập tay ở cha; lịch sử giữ giá trị trước/sau. Không âm thầm coi ước lượng của cha và con là hai khối lượng độc lập.

Mức ưu tiên, trạng thái và quá hạn là ba khái niệm khác nhau: Priority do người dùng chọn; Status biểu thị tiến độ; quá hạn tính từ hạn hiệu lực và thời điểm hiện tại.

## 6. Kiến trúc, dữ liệu và môi trường

### 6.1. Backend

Giữ một backend với bốn project hiện có:

- Domain: Entity/enum/quy tắc thuần, tính tiến độ, hạn hiệu lực, gợi ý.
- Application: use case, DTO, validation, quyền sở hữu, điều phối giao dịch và interface cần thiết.
- Infrastructure: EF Core, cấu hình quan hệ, migration, truy vấn, triển khai lưu trữ/xác thực.
- API: controller, xác thực/phân quyền HTTP, xử lý lỗi, Swagger và host công việc nền.

Không viết toàn bộ nghiệp vụ trong controller hoặc tạo hàng loạt lớp rỗng để đủ cấu trúc. Không tách microservices cho bản đầu. Tổ chức theo chức năng trong từng tầng, phù hợp tài liệu kiến trúc.

### 6.2. Frontend

Đề xuất React với TypeScript; đây là lựa chọn triển khai cần thống nhất, chưa có frontend trong repository. Tách màn hình/chức năng, lớp gọi API, xử lý phiên đăng nhập và component dùng chung. Phiên bản thư viện được chọn và cố định khi khởi tạo, không nâng đồng loạt backend chỉ vì bắt đầu frontend.

Tích hợp từng luồng ngay khi API tương ứng xong, không chờ backend hoàn thành toàn bộ. Có thể làm bố cục bằng dữ liệu mẫu đã gắn nhãn, nhưng không coi màn hình dùng dữ liệu mẫu là tích hợp thành công.

### 6.3. Database

11 bảng theo ERD: School, Faculty, Cohort, StudentClass, User, Semester, Course, ClassSession, TaskItem, Notification, TaskHistory.

- Tạo Entity, cấu hình từng bảng, FK, CHECK, unique và chỉ mục; phân biệt ràng buộc DB với kiểm tra qua nhiều bảng trong dịch vụ.
- Xem lại InitialCreate rỗng và lịch sử migration thực tế; không tự xóa/sửa migration đã áp dụng. Ưu tiên migration mới chứa schema nghiệp vụ sau khi xác minh môi trường.
- Kiểm tra trên PostgreSQL phát triển riêng. Không mặc định database cấu hình hiện tại là database thử nghiệm có thể xóa.
- Dữ liệu mẫu là giả lập, không dùng thông tin/mật khẩu thật; tài khoản Admin khởi tạo bằng cơ chế riêng, không mở đăng ký Admin công khai.
- Thay đổi nhiệm vụ, tổ tiên, lịch sử và lời nhắc phải cùng giao dịch. Thống nhất thứ tự khóa; giao dịch đơn thuần không tự ngăn mọi cập nhật mất dữ liệu.

### 6.4. Môi trường và vận hành

- Tách Development, Test và môi trường triển khai; lưu bí mật ngoài source/tài liệu.
- Cấu hình URL API/frontend, CORS cho origin cụ thể, HTTPS và giới hạn request.
- Health sống của API khác readiness kiểm tra database; không trả connection string khi lỗi.
- Log có traceId, thời gian, endpoint và loại lỗi; không log mật khẩu/token/toàn bộ nội dung nhạy cảm.
- Worker dùng dữ liệu bền vững trong DB; xử lý theo lô, có hủy khi tắt host và thử lại có giới hạn.
- Bản đầu giả định một API instance; vẫn cần tránh chạy trùng nếu triển khai nhiều instance. Host dừng/ngủ thì lời nhắc có thể trễ, phải kiểm tra lại khi khởi động.
- Chỉ đưa lên hosting hoặc tác động môi trường đang dùng khi người dùng yêu cầu/cho phép. Có hướng dẫn backup/khôi phục trước thay đổi schema trên dữ liệu cần giữ.

## 7. Thiết kế API và Swagger

Chi tiết endpoint, DTO, lỗi, ví dụ và bố cục Swagger nằm trong [THIET-KE-API-SWAGGER.md](THIET-KE-API-SWAGGER.md).

Đây là một hạng mục triển khai riêng từ đầu, không phải bước trang trí sau cùng:

1. Thiết kế thao tác người dùng và hợp đồng request/response trước khi viết controller.
2. Cấu hình tài liệu OpenAPI, nhóm chức năng, schema enum/null, ví dụ và Authorize.
3. Với mỗi chức năng: triển khai nghiệp vụ → API → Swagger đúng hợp đồng → thử HTTP → tích hợp giao diện.
4. Chỉ endpoint đã có xử lý thực mới xuất hiện như chức năng chạy được. Endpoint dự kiến nằm trong tài liệu thiết kế, không tạo stub trả thành công giả để đủ Swagger.
5. Kiểm tra Swagger schema và quyền truy cập bằng test phù hợp; thử bằng Swagger không thay thế kiểm thử giao dịch/database/quyền sở hữu.

## 8. Kế hoạch từng giai đoạn

### Giai đoạn 0 — Nghiệp vụ, màn hình và hợp đồng

**Làm:** khảo sát các tình huống ở mục 2; xác định bản đầu; rà các quyết định mục 5; mô tả màn hình và các endpoint tương ứng; chốt từ ngữ hạn/tiến độ/gợi ý; chọn cơ chế xác thực và kiểm soát sửa đồng thời.

**Đầu ra:** ERD phù hợp phạm vi, bảng chức năng–màn hình–API, request/response mẫu, quy tắc lỗi và kịch bản nghiệm thu. Những quyết định còn mở phải ghi rõ, không coi suy đoán là yêu cầu người dùng đã duyệt.

**Hoàn thành:** lần theo được tạo nhiệm vụ, đổi hạn, hoàn thành, xóa môn và nhắc hạn; không còn mâu thuẫn ngăn viết schema/API của mốc tiếp theo.

### Giai đoạn 1 — Khung API và Swagger

**Làm:** bỏ đăng ký Swagger lặp; thêm MapControllers; chuẩn hóa DI, cấu hình và lỗi; thêm health sống/readiness; cấu hình JSON, enum, thời gian, giới hạn phân trang; dựng tên/nhóm tài liệu và cấu hình Bearer cho phần xác thực sắp triển khai. Endpoint mẫu weatherforecast được bỏ khỏi bản API sản phẩm khi endpoint nền mới thay thế.

**Đầu ra:** API khởi động; Swagger mở được trong Development; tài liệu hiển thị đúng endpoint thực có; không claim có login trước khi làm giai đoạn 3.

**Kiểm tra:** build, gọi health, kiểm tra tài liệu OpenAPI, đường dẫn controller và phản hồi lỗi. Readiness mất DB trả trạng thái không sẵn sàng mà không lộ bí mật.

### Giai đoạn 2 — Entity, cấu hình và migration

**Làm:** Entity/enum, DbSet, Fluent API, kiểu thời gian, unique/CHECK/index, quy tắc xóa; timestamps; migration schema; dữ liệu danh mục thử và hướng dẫn cấu hình. Quyết định các thay đổi schema còn mở trước khi sinh migration.

**Đầu ra:** database phát triển tạo schema đúng thiết kế, đọc/ghi dữ liệu được; có project kiểm thử cần thiết và database test độc lập.

**Kiểm tra:** email chuẩn hóa trùng, mã SV NULL/trùng, CHECK tiến độ, FK, xóa trường còn khoa, xóa môn giữ nhiệm vụ, xóa cây, UserId lịch sử SET NULL. Kiểm tra cả tạo DB mới và áp dụng nối tiếp migration rỗng trước nếu có.

### Giai đoạn 3 — Đăng ký, đăng nhập và hồ sơ

**Làm:** băm mật khẩu bằng thư viện nền tảng; chuẩn hóa email; mặc định Student; access token có hạn; xác thực/phân quyền; current user; GET/PATCH hồ sơ; đổi mật khẩu; Swagger Authorize; giao diện đăng ký/đăng nhập/hồ sơ.

**Đề xuất phiên bản đầu:** access token ngắn hạn, chưa có refresh token; đăng xuất client xóa token. Token hết hạn thì đăng nhập lại. Không quảng bá đây là thu hồi token tức thì; nếu cần thu hồi khi đổi mật khẩu/phát hiện sự cố phải bổ sung phiên bản bảo mật/phiên đăng nhập vào dữ liệu trước khi code. Tài khoản IsActive = false phải bị từ chối ở các request được bảo vệ, không chỉ lúc login.

**Kiểm tra:** email trùng, mật khẩu sai, token thiếu/hết hạn, tự gửi Role = Admin, sửa hồ sơ không thuộc quyền, tài khoản bị vô hiệu hóa. Không trả PasswordHash trong DTO hoặc Swagger.

**Hoàn thành:** người dùng thao tác tài khoản qua cả Swagger và UI; trạng thái hết phiên được xử lý rõ. Không log thông tin xác thực.

### Giai đoạn 4 — Học kỳ và môn học

**Làm:** CRUD học kỳ/môn; ngày hợp lệ; danh sách môn theo học kỳ; màn hình nhập/sửa; quyền sở hữu qua Semester. Cơ chế xóa ghi lịch sử bỏ CourseId khi nhiệm vụ được triển khai.

**Kiểm tra:** ngày đảo ngược, tên rỗng, truy cập học kỳ của người khác, sửa/xóa môn không thuộc mình. Khi đã có nhiệm vụ, thêm test xóa học kỳ/môn giữ nhiệm vụ và ghi lịch sử.

**Hoàn thành:** tạo học kỳ → thêm môn → tải lại vẫn thấy dữ liệu; dữ liệu hai người dùng tách biệt.

### Giai đoạn 5 — Nhiệm vụ lá và lịch sử nền

**Làm:** CRUD nhiệm vụ không cha trước, lọc/phân trang, deadline tùy chọn, ưu tiên/loại, estimated minutes, tiến độ/complete/reopen; ghi lịch sử từ lần tạo đầu tiên; chuẩn bị lời nhắc trong DB theo hợp đồng đã chốt, chưa cần phát nền. UI danh sách/form/chi tiết và lịch sử cơ bản.

**Luồng:** request → xác thực → kiểm tra Course thuộc User → quy tắc tiến độ/hạn → ghi TaskItem + TaskHistory + lịch nhắc trong giao dịch → response DTO.

**Kiểm tra:** hoàn thành 100%, mở lại xóa CompletedAt, không hạn không tạo lời nhắc; sai môn bị từ chối; lỗi lưu lịch sử khiến thay đổi nhiệm vụ rollback; bộ lọc/phân trang ổn định.

**Mốc dùng được đầu tiên:** sinh viên đăng nhập, tạo học kỳ/môn và quản lý công việc bằng UI, dữ liệu lưu thật. Không cần đợi thuật toán hoặc quản trị mới dùng thử.

### Giai đoạn 6 — Cây nhiệm vụ, hạn kế thừa và lịch sử đầy đủ

**Làm:** thêm con, chuyển cha, đổi môn cả cây, kiểm tra chu trình; tính lại tiến độ các tổ tiên; hạn hiệu lực; lịch sử từng thay đổi; ảnh hưởng xóa; đồng bộ lời nhắc cho cây; UI cây và cảnh báo thao tác ảnh hưởng nhiều dòng.

**Kiểm tra:** nhiều tầng, trung bình làm tròn, thêm con cho nhiệm vụ đã hoàn thành, đổi hạn cha sớm hơn hạn con, chuyển cha, xóa con cuối, sửa đồng thời. Giao dịch phải rollback toàn bộ nếu bất kỳ bước nào lỗi.

**Hoàn thành:** API trả cả DueDate và EffectiveDueDate; lọc, lịch, nhắc và gợi ý cùng dùng hạn hiệu lực; không tự đếm cha như một phần khối lượng bổ sung.

### Giai đoạn 7 — Lịch học, Calendar và Dashboard

**Làm:** CRUD ClassSession; sinh lịch trong khoảng ngày và học kỳ; gộp deadline; UI ngày/tuần/tháng; Dashboard việc hôm nay/sắp hạn/quá hạn/hoàn thành.

**Quy ước:** tuần bắt đầu thứ Hai; khoảng truy vấn [from, to); thời điểm truyền có offset, đổi ngày/tuần theo User.TimeZone. ClassSession lặp theo giờ địa phương. Cảnh báo giờ học trùng, không biến deadline thành buổi học kéo dài EstimatedMinutes.

**Kiểm tra:** biên đầu/cuối học kỳ, cuối tháng/năm, thứ Hai/Chủ nhật, múi giờ, việc không hạn, con kế thừa hạn, lọc học kỳ. Thống kê leafOnly và phạm vi ngày được mô tả rõ.

**Hoàn thành:** Dashboard và lịch khớp dữ liệu nhiệm vụ; không hiển thị buổi học ngoài học kỳ; UI không gọi toàn bộ dữ liệu nhiều năm cho một tháng.

### Giai đoạn 8 — Phát và đọc thông báo

**Làm:** dịch vụ lập lịch nhắc, BackgroundService, xử lý theo lô/khóa, kiểm tra lại nhiệm vụ trước phát; thử lại giới hạn; API danh sách/unread count/đã đọc; UI chuông và hộp thông báo. Quy tắc xếp lịch nhắc đã bắt đầu từ giai đoạn 5, giai đoạn này hoàn thiện cơ chế phát.

**Kiểm tra:** đổi deadline/giờ nhắc, đổi mặc định User, hoàn thành/mở lại, đổi cha, xóa cây, worker restart, xử lý trùng, lỗi lần thử, cạnh tranh giữa worker và sửa nhiệm vụ. Tại đúng hạn cho ReminderHours = 0 cần quy tắc biên nhất quán.

**Hoàn thành:** chỉ Delivered hiện trong hộp; ReadAt đổi sau khi đọc; Pending không được coi là thông báo đã gửi. Worker không phát thông báo dựa trên lịch cũ sau khi luồng sửa đã commit; xác định rõ thứ tự khi hai thao tác xảy ra đồng thời.

### Giai đoạn 9 — Gợi ý thứ tự công việc

**Làm:** bộ chọn nhiệm vụ lá chưa hoàn thành; ước lượng còn lại; bộ xếp hạng thuần; response có lý do; UI hiển thị thứ tự và giới hạn; bộ dữ liệu kiểm thử và đối chiếu cách sắp theo hạn.

**Kiểm tra:** việc không hạn, thiếu thời lượng, quá hạn, tiến độ tăng, đổi ưu tiên, thay deadline, cùng điểm, nhiệm vụ cha không được đưa vào như một việc độc lập.

**Hoàn thành:** cùng đầu vào/thời điểm cho cùng kết quả; giải thích được thứ tự; không gọi đó là lịch tối ưu nếu chưa có ràng buộc thời gian rảnh và tiêu chí tối ưu hóa.

### Giai đoạn 10 — Danh mục và quản trị

**Làm:** CRUD trường/khoa/khóa/lớp; lọc phụ thuộc; UI hồ sơ chọn lớp; danh sách tài khoản và active/inactive; seed Admin có kiểm soát. Không có API nâng quyền tùy ý trong bản đầu.

**Kiểm tra:** Student gọi admin nhận 403; xóa danh mục còn tham chiếu đúng RESTRICT/SET NULL; ngăn vô hiệu hóa Admin cuối cùng; user bị khóa không tiếp tục dùng token để thao tác.

**Hoàn thành:** quản trị không phá dữ liệu cá nhân qua cascade danh mục; người dùng vẫn tạo nhiệm vụ khi chưa có lớp.

### Giai đoạn 11 — Tích hợp, nghiệm thu và chuẩn bị triển khai

**Làm:** chạy toàn luồng, sửa bất nhất UI/API, kiểm tra quyền, đo các truy vấn chính, kiểm tra index theo dữ liệu thử; build backend/frontend; rà hợp đồng Swagger; hướng dẫn dựng môi trường; dữ liệu demo, backup và kịch bản khôi phục. CI tối thiểu build/test khi repository có cấu hình phù hợp.

**Hoàn thành:** chạy từ môi trường sạch theo hướng dẫn, dữ liệu bền sau tải lại/restart, không lộ bí mật, kiểm thử quan trọng qua, có báo cáo những phần chưa làm. Triển khai thực tế là thao tác tiếp theo cần yêu cầu/ủy quyền, không tự thực hiện vì kế hoạch đã có.

## 9. Thuật toán gợi ý và cách đánh giá

### 9.1. Đầu vào và đầu ra

Đầu vào: nhiệm vụ lá chưa hoàn thành của người dùng, hạn hiệu lực, Priority, EstimatedMinutes, Progress và thời điểm xét. Đầu ra: thứ tự task, lý do, thời lượng còn lại ước lượng (có thể NULL), dữ liệu thiếu và thời điểm tính.

RemainingMinutes = ceil(EstimatedMinutes × (100 − Progress) / 100) nếu có EstimatedMinutes. Nếu thiếu thì giữ NULL; không coi là 0. Progress là tự báo cáo nên kết quả thời lượng chỉ là ước lượng.

### 9.2. Bản cơ sở để so sánh

Làm bản dễ giải thích trước: việc quá hạn trước, sau đó các việc có hạn từ gần đến xa, sau đó việc không hạn; cùng hạn thì ưu tiên High/Medium/Low, cuối cùng Id tăng dần để ổn định. Đây là baseline, không phải kết luận sản phẩm bắt buộc phải dùng mãi.

Sau đó thử một phương án dùng độ gấp tương đối: khoảng thời gian đến hạn so với RemainingMinutes. Chỉ so sánh khi biết thời lượng; tách trường hợp quá hạn/không hạn/dữ liệu thiếu. Không coi toàn bộ giờ từ hiện tại đến hạn là giờ học rảnh; tỷ lệ này là tín hiệu xếp hạng, không là bằng chứng đủ thời gian hoàn thành.

Chỉ chọn trọng số/công thức sau khi có bộ ví dụ kỳ vọng. Ghi rõ quy tắc nào quyết định thứ tự khi hạn và ưu tiên xung đột; không thêm điểm số tùy ý rồi gọi là tối ưu.

### 9.3. Đánh giá

- Cố định thời điểm và đầu vào; có các ca đổi hạn, tăng tiến độ, thay ước lượng, thiếu dữ liệu và nhiều việc cạnh tranh.
- So sánh với baseline trên cùng dữ liệu; giải thích những trường hợp đổi thứ tự.
- Nhờ sinh viên dùng thử đánh giá tính dễ hiểu/hữu ích; ghi nhận quan sát thực tế, không tự khẳng định giảm trễ hạn khi chưa đo.
- Nếu muốn chứng minh khả năng hoàn thành trước hạn, cần thêm giả định năng lực/thời gian rảnh và mô phỏng rõ. Schema hiện tại chưa đủ để chứng minh một lịch khả thi.

## 10. Kiểm thử và tiêu chí nghiệm thu

| Nhóm | Kiểm chứng chính | Vì sao cần |
|---|---|---|
| Build | Backend và frontend khi có | Phát hiện lỗi biên dịch/cấu hình |
| Unit | Tiến độ cha, hạn kế thừa, trạng thái, công thức gợi ý, thời gian cố định | Quy tắc phải kiểm tra lặp lại được |
| Integration PostgreSQL | FK/CHECK/unique, cascade/SET NULL, rollback, khóa/cập nhật đồng thời | Database giả không chứng minh hành vi PostgreSQL |
| HTTP/API | Status code, DTO, phân trang, token, role, quyền sở hữu | Kiểm tra hợp đồng công khai |
| OpenAPI | Schema, required/null, enum, security, phản hồi lỗi | Swagger phải mô tả đúng API |
| Toàn luồng UI | Đăng nhập → môn → việc → con → lịch → nhắc → hoàn thành | Xác minh người dùng thực hiện được công việc |
| Worker | Clock cố định, restart, retry, xử lý trùng/cạnh tranh | Tránh nhắc sai hoặc mất nhắc |
| Dữ liệu riêng tư | Hai Student, một Admin; thử ID của nhau | Chặn truy cập chéo người dùng |

Không cần viết test chỉ để lặp lại nội dung tài liệu hoặc mọi thay đổi trình bày. Tập trung vào quy tắc, ràng buộc, quyền và luồng có rủi ro. Mỗi giai đoạn chạy kiểm tra phù hợp; không chờ cuối mới test.

Một chức năng được coi là hoàn thành khi: nghiệp vụ rõ; code thực thi; schema phù hợp; Swagger đúng; UI tích hợp nếu thuộc màn hình; validation/quyền/lỗi đã kiểm tra; dữ liệu không mất khi tải lại; lịch sử thay đổi ghi đúng kết quả và giới hạn. Mở được Swagger hoặc build thành công riêng lẻ chưa đủ.

## 11. Kịch bản demo

1. Đăng ký tài khoản sinh viên, đăng nhập; chưa chọn lớp vẫn dùng được.
2. Tạo học kỳ; thêm CSDL và Lập trình Web; mỗi môn thêm lịch hằng tuần.
3. Tạo một việc cá nhân không hạn và một bài tập có deadline.
4. Chia bài tập thành hai con; cập nhật một con 60%, con kia 0%; cha hiển thị 30%.
5. Xem Calendar/Dashboard; phân biệt lịch học, hạn nhập và hạn kế thừa.
6. Đổi hạn cha; xem lịch sử và lịch nhắc được cập nhật.
7. Dùng dữ liệu thử có mốc nhắc gần để worker phát; đọc thông báo.
8. Xem gợi ý; hoàn thành một việc; tính lại danh sách.
9. Xóa môn; lịch học biến mất, nhiệm vụ còn với CourseId NULL.
10. Dùng tài khoản thứ hai thử ID dữ liệu tài khoản đầu, bị từ chối.
11. Admin vô hiệu hóa tài khoản thử; tài khoản đó không tiếp tục dùng API bảo vệ.

Demo không phụ thuộc chờ nhiều ngày; dùng clock có thể kiểm soát trong test và dữ liệu deadline gần ở môi trường demo. Không đưa chức năng đổi giờ hệ thống hoặc phát thông báo tùy ý vào API công khai.

## 12. Theo dõi tiến độ, tài liệu và bàn giao

### 12.1. Mốc bàn giao

| Mốc | Sau giai đoạn | Có thể kiểm chứng |
|---|---|---|
| A | 0–2 | Thiết kế rõ, Swagger nền, schema có ràng buộc |
| B | 3–5 | Đăng nhập, học kỳ/môn, quản lý nhiệm vụ thật trên UI |
| C | 6–8 | Cây tiến độ, lịch, Dashboard, thông báo và lịch sử |
| D | 9–10 | Gợi ý có giải thích, quản trị |
| E | 11 | Kiểm thử, tài liệu chạy và demo hoàn chỉnh |

Chưa có hạn nộp, số thành viên và giờ làm/tuần nên không gán lịch ngày/tuần giả định. Khi có thông tin, chia từng giai đoạn thành công việc nhỏ và ước lượng, chừa thời gian sửa lỗi/tích hợp. Hạng mục phụ thuộc phải xong trước; UI bố cục và dữ liệu mẫu có thể chuẩn bị sớm sau hợp đồng, không cần chờ giai đoạn 11.

### 12.2. Mỗi công việc cần ghi

- Yêu cầu/tình huống người dùng và lý do làm.
- File/chức năng thay đổi, dữ liệu đọc/ghi, logic trước/sau.
- Request/response/lỗi và kiểm tra quyền.
- Kiểm chứng đã chạy, kết quả thực tế, phần chưa chạy và lý do.
- Việc tiếp theo; không tự ghi người dùng đã đọc/duyệt.

### 12.3. Thứ tự bắt đầu cụ thể

1. Rà quyết định mục 5 và hợp đồng trong tài liệu API.
2. Chuẩn hóa Program.cs, routing controller, lỗi và Swagger nền.
3. Viết Entity/Fluent API/migration sau khi thống nhất những điểm ảnh hưởng schema.
4. Triển khai tài khoản → học kỳ/môn → nhiệm vụ lá và lịch sử, tích hợp UI theo từng luồng.
5. Mở rộng cây/lịch/thông báo/gợi ý và quản trị theo các giai đoạn trên.

Tác vụ tạo tài liệu này không thực hiện các bước code, migration, commit, push hoặc triển khai.
