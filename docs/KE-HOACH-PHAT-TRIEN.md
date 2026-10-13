# Đánh giá và kế hoạch phát triển Study Planner

Ngày đối chiếu: 2026-10-13. Căn cứ: code hiện tại, lịch sử, [ERD](erd.md) và kết quả build. Chưa có thời hạn nộp hoặc thời gian làm việc để chia lịch theo tuần. Các mốc dưới đây là đề xuất triển khai, chưa phải phạm vi đã được người dùng duyệt.

Bản này là lộ trình tổng quan. Bản mở rộng có màn hình, phạm vi, từng bước triển khai và tiêu chí kiểm tra nằm ở [kế hoạch chi tiết](KE-HOACH-TRIEN-KHAI-CHI-TIET.md); danh mục endpoint, DTO, lỗi và cách tổ chức Swagger nằm ở [thiết kế API/Swagger](THIET-KE-API-SWAGGER.md). Các tài liệu mới không đồng nghĩa với việc các chức năng đã được code.

## Hiện trạng đã xác minh

| Hạng mục | Đã có | Giới hạn |
| --- | --- | --- |
| Backend | Solution .NET 10 gồm Api, Application, Domain, Infrastructure; SDK 10.0.400 được cố định trong global.json | Domain/Application mới có Class1 rỗng, chưa có nghiệp vụ |
| API | Swagger trong Development, HTTPS redirection; GET /, GET /weatherforecast và GET /health/live qua controller | Endpoint gốc và liveness không kiểm tra database; weatherforecast là dữ liệu mẫu |
| Dữ liệu | EF Core, provider PostgreSQL, ApplicationDbContext và đọc cấu hình DefaultConnection | Chưa có entity, DbSet hoặc cấu hình quan hệ nghiệp vụ |
| Migration | InitialCreate và model snapshot | Up/Down và model nghiệp vụ rỗng; chưa xác minh database thực tế |
| Thiết kế | ERD đề xuất 11 bảng, quy tắc cây nhiệm vụ, lịch lặp, thông báo, lịch sử, xóa và chỉ mục | Mới ở tài liệu, chưa triển khai bằng Entity/migration |
| Quy trình | AGENTS.md, mẫu và các bản ghi lịch sử; file HTTP gọi endpoint mẫu | Chưa tìm thấy frontend, project test hoặc workflow CI trong các file đã kiểm kê |

Lần đánh giá ban đầu build bằng SDK 8.0.425. Sau [nâng cấp .NET 10](history/2026-10-13-nang-cap-dotnet-10.md), đã restore và build thành công bằng SDK 10.0.400, 0 cảnh báo/0 lỗi; đã kiểm tra HTTP liveness, Swagger UI/JSON và hai endpoint mẫu. Chưa kiểm tra database, migration hoặc dựng trên máy mới; các endpoint này chưa xác minh nghiệp vụ học tập.

Code khớp bản ghi ban đầu. Lịch sử commit nhắc tạo bảng __EFMigrationsHistory nhưng chưa kiểm tra database; bảng theo dõi migration cũng chưa chứng minh có bảng nghiệp vụ. ERD xuất hiện trong lúc đánh giá và đã được đọc bổ sung; không coi 11 bảng thiết kế là 11 bảng đã triển khai.

## Phạm vi bản đầu đề xuất

Luồng chính: đăng nhập → tạo học kỳ → thêm môn và lịch học → tạo nhiệm vụ → chia nhỏ và cập nhật tiến độ → xem hạn/lịch/thông báo.

Bám ERD hiện tại: School, Faculty, Cohort, StudentClass là danh mục hồ sơ; User, Semester, Course, ClassSession, TaskItem, Notification, TaskHistory phục vụ luồng chính. Người dùng không cần chọn trường/lớp để bắt đầu.

ClassSession là lịch môn học lặp theo tuần, chưa phải buổi tự học được tự động xếp cho TaskItem. Gợi ý thứ tự công việc cũng khác tự phân bổ thời gian. Chưa đưa AI xếp lịch, email/push, thời gian học đo thực tế hoặc ngoại lệ lịch vào bản đầu nếu chưa bổ sung thiết kế.

## Thứ tự công việc và tiêu chí hoàn thành

### Mốc 1 — Chốt thiết kế và hợp đồng API

- Đối chiếu ERD với đề cương đồ án; chốt chức năng bắt buộc, màn hình và API tương ứng.
- Chốt các quyết định còn mở: phạm vi mã sinh viên duy nhất, thời hạn lưu lịch sử, nhắc hạn cha/con, có cần tắt nhắc hay ngoại lệ lịch không.
- Viết các ví dụ đầu vào/đầu ra và lỗi cho tài khoản, học kỳ, môn, nhiệm vụ, lịch, thông báo. Chốt ranh giới xử lý giữa các tầng trước khi sinh code.

Hoàn thành khi: ERD và hợp đồng API đủ rõ để tạo schema, không có yêu cầu bắt buộc nào mâu thuẫn với thiết kế. Đây là bước làm ngay tiếp theo; đã có ERD nên không cần bắt đầu thiết kế lại từ đầu.

### Mốc 2 — Khung API và schema PostgreSQL

- Gỡ đăng ký Swagger lặp trong Program.cs. Chọn cách tổ chức endpoint; nếu dùng controller thì thêm MapControllers vì hiện chỉ có AddControllers.
- Tạo entity/enum trong Domain, cấu hình EF Core và DbSet trong Infrastructure theo ERD; tạo migration có bảng, FK, CHECK và chỉ mục.
- Thống nhất validation, phản hồi lỗi và xử lý thời gian UTC/múi giờ. Tách trạng thái API đang chạy với trạng thái database truy cập được.
- Kiểm tra migration trên database phát triển đã xác định; không mặc định áp dụng lên database đang có dữ liệu.

Hoàn thành khi: build thành công, tạo schema và đọc/ghi thử được; ràng buộc dữ liệu và các hành vi xóa quan trọng đúng thiết kế. Có hướng dẫn cấu hình không chứa bí mật.

### Mốc 3 — Xác thực và quyền sở hữu

- Làm đăng ký, đăng nhập, thông tin tài khoản; dùng cơ chế băm mật khẩu của thư viện xác thực phù hợp nền tảng.
- Chuẩn hóa email, kiểm tra tài khoản bị vô hiệu hóa và ngăn người dùng tự cấp quyền Admin.
- Lấy danh tính từ phiên xác thực; kiểm tra quyền sở hữu học kỳ, môn, nhiệm vụ, thông báo và lịch sử trước mọi đọc/ghi.

Hoàn thành khi: hai tài khoản không thể đọc/sửa/xóa dữ liệu của nhau; đăng nhập sai và tài khoản bị vô hiệu hóa bị từ chối. Viết kiểm thử tích hợp cho các trường hợp này.

### Mốc 4 — Học kỳ, môn và lịch học lặp

- CRUD học kỳ và môn; quản lý nhiều buổi học trong tuần của một môn.
- Sinh lịch trong khoảng ngày yêu cầu và giới hạn trong ngày hiệu lực học kỳ; xử lý múi giờ nhất quán.
- Bổ sung danh mục hồ sơ và màn hình quản trị theo phạm vi đã chốt, không chặn người dùng mới vì thiếu trường/lớp.

Hoàn thành khi: tạo học kỳ → môn → buổi học và xem lịch đúng ngày; giờ kết thúc phải sau giờ bắt đầu; xóa môn/học kỳ giữ nhiệm vụ theo ERD.

### Mốc 5 — Nhiệm vụ, cây tiến độ và lịch sử

- Làm nhiệm vụ lá trước: CRUD, lọc, phân trang, hạn, ưu tiên, loại, hoàn thành/mở lại.
- Thêm cha–con: chống chu trình, kiểm tra cùng chủ sở hữu/môn, kế thừa hạn, tính lại tiến độ tổ tiên trong giao dịch.
- Ghi TaskHistory cùng giao dịch; khi xóa cây ghi từng sự kiện trước cascade; khi xóa môn ghi việc bỏ liên kết. Kiểm tra xóa tài khoản giữ lịch sử nhưng bỏ liên kết danh tính.

Hoàn thành khi: tiến độ/trạng thái nhất quán, sửa hạn hoặc chuyển cây không làm sai con, dữ liệu và lịch sử cùng thành công hoặc cùng rollback. Kiểm thử cây nhiều tầng, xóa con cuối, đổi môn và các trường hợp xóa liên quan.

### Mốc 6 — Nhắc hạn, tổng quan và gợi ý thứ tự

- Tạo/cập nhật lời nhắc theo hạn hiệu lực và thiết lập tài khoản; làm tác vụ nền phát thông báo trong ứng dụng và API đánh dấu đã đọc.
- Kiểm tra hoàn thành/mở lại, đổi hạn, thử lại lỗi và xử lý đồng thời giữa cập nhật nhiệm vụ với phát thông báo.
- Làm tổng quan công việc còn lại, hoàn thành, quá hạn; ghi rõ công thức thống kê để tránh đếm lặp cha và con.
- Nếu thuộc phạm vi đồ án, thiết kế công thức gợi ý thứ tự nhiệm vụ lá từ hạn, ưu tiên, thời lượng còn lại; kiểm tra bằng ví dụ có kết quả kỳ vọng. Chưa gọi đây là tự xếp lịch.

Hoàn thành khi: lời nhắc không phát theo dữ liệu cũ sau khi đổi hạn/hoàn thành; chỉ thông báo đã phát hiện trong hộp thông báo; thống kê và gợi ý có kết quả kiểm chứng được.

### Mốc 7 — Giao diện, tích hợp và demo

- Chốt công nghệ frontend theo yêu cầu đồ án và kỹ năng hiện tại. Có thể dựng giao diện sau mốc 1, tích hợp từng chức năng khi backend tương ứng hoàn thành.
- Làm đăng nhập, học kỳ/môn, lịch, cây nhiệm vụ, tổng quan và thông báo; có trạng thái tải/rỗng/lỗi và xử lý phiên hết hạn.
- Kiểm thử toàn luồng, viết hướng dẫn chạy từ đầu, tài liệu API và kịch bản demo; chuẩn bị dữ liệu mẫu. Chỉ triển khai khi người dùng yêu cầu hoặc cho phép.

Hoàn thành khi: người dùng thực hiện được toàn bộ luồng chính bằng giao diện; tải lại vẫn thấy dữ liệu đúng; có thể dựng môi trường theo hướng dẫn và trình bày demo.

## Cách theo dõi

Tách mỗi mốc thành thay đổi nhỏ có thể đọc và kiểm chứng; thêm bản ghi lịch sử theo mẫu sau mỗi thay đổi có ý nghĩa. Ưu tiên hoàn thành một luồng xuyên suốt trước khi mở rộng. Không dùng tỷ lệ phần trăm hoàn thành khi chưa chốt phạm vi; chưa ước lượng theo tuần khi chưa biết hạn nộp và thời gian có thể dành cho dự án.
