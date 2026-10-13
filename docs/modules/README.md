# Đặc tả nghiệp vụ theo module

Ngày đối chiếu: 2026-10-13. Trạng thái: đặc tả dự thảo để chuẩn bị triển khai, chưa phải chức năng đã chạy hoặc quyết định đã được người dùng duyệt.

[Đặc tả nghiệp vụ bản đầu](../DAC-TA-NGHIEP-VU-BAN-DAU.md) được bổ sung trong lúc soạn bộ module và đã được đọc đối chiếu. Tài liệu đó ghi nhận người dùng đã chọn gợi ý việc nên làm trước có giải thích, chưa tự xếp giờ học. Bộ module giữ phạm vi này; các quy tắc còn lại vẫn là dự thảo, không suy ra toàn bộ tài liệu đã được duyệt.

## Mục đích và cách đọc

Các tài liệu này giải thích người dùng làm gì, hệ thống kiểm tra gì, dữ liệu thay đổi thế nào và kiểm chứng ra sao. Module ở đây là nhóm nghiệp vụ trong backend monolith, không tương ứng với một microservice, một project hoặc bắt buộc một bảng.

Đọc [quy ước và quyết định còn mở](00-quy-uoc-va-quyet-dinh.md) trước, sau đó đọc module sắp triển khai. Không cần giải quyết mọi vấn đề của phần mở rộng trước khi làm phần nền; cần chốt các quyết định ảnh hưởng trực tiếp schema hoặc hợp đồng của đầu việc đang làm.

| Module | Giá trị người dùng nhận được | Giai đoạn trong kế hoạch chi tiết |
| --- | --- | --- |
| [01 — Tài khoản và hồ sơ](01-tai-khoan-va-ho-so.md) | Đăng nhập và quản lý thông tin của mình | 3 |
| [02 — Học kỳ và môn học](02-hoc-ky-va-mon-hoc.md) | Tổ chức môn theo từng học kỳ | 4 |
| [03 — Nhiệm vụ và tiến độ](03-nhiem-vu-va-tien-do.md) | Quản lý công việc, chia nhỏ và theo dõi tiến độ | 5–6 |
| [04 — Lịch sử nhiệm vụ](04-lich-su-nhiem-vu.md) | Biết nhiệm vụ đã thay đổi gì, kể cả sau khi xóa | Bắt đầu cùng 5, hoàn thiện ở 6 |
| [05 — Lịch học và tổng quan](05-lich-hoc-va-tong-quan.md) | Xem lịch, hạn và công việc cần chú ý | 7 |
| [06 — Thông báo nhắc hạn](06-thong-bao-nhac-han.md) | Nhận và đọc lời nhắc trong ứng dụng | Chuẩn bị từ 5–6, phát ở 8 |
| [07 — Gợi ý công việc](07-goi-y-cong-viec.md) | Biết nên làm việc nào tiếp theo và lý do | 9 |
| [08 — Danh mục và quản trị](08-danh-muc-va-quan-tri.md) | Chọn thông tin trường/lớp; quản trị danh mục/tài khoản | Đọc tối thiểu khi cần cho hồ sơ, quản trị ở 10 |

Số thứ tự file để đọc và tham chiếu, không yêu cầu viết xong toàn bộ một module mới được bắt đầu module liên quan. Khung API/schema ở giai đoạn 1–2 là nền dùng chung, không tạo một module nghiệp vụ riêng.

## Quan hệ với tài liệu hiện có

- [Đặc tả nghiệp vụ bản đầu](../DAC-TA-NGHIEP-VU-BAN-DAU.md): luồng tổng thể, các nhóm NV và ca NT; bộ module diễn giải theo phạm vi chức năng, không thay thế xác nhận phạm vi đã ghi nhận.

- [ERD](../erd.md): nguồn thiết kế bảng, trường, quan hệ và ràng buộc dữ liệu.
- [Thiết kế API/Swagger](../THIET-KE-API-SWAGGER.md): nguồn method/route, DTO, mã lỗi và quy ước HTTP. Bảng endpoint trong module là phần tóm tắt để lần theo, chưa phải OpenAPI đã sinh.
- [Kiến trúc backend](../KIEN-TRUC-BACKEND.md): trách nhiệm các tầng và hướng phụ thuộc.
- [Kế hoạch chi tiết](../KE-HOACH-TRIEN-KHAI-CHI-TIET.md): thứ tự thực hiện, ưu tiên và các mốc nghiệm thu.
- [Lịch sử thay đổi](../history/README.md): ghi việc thực sự đã sửa và kiểm chứng.

Không sao chép toàn bộ schema/DTO vào mỗi module. Khi phát hiện mâu thuẫn, kiểm tra code và bản ghi mới nhất, ghi rõ điểm khác, cập nhật các tài liệu bị ảnh hưởng; không ngầm chọn một bản rồi bỏ qua các bản còn lại.

### Đối chiếu với mã nghiệp vụ và nghiệm thu tổng thể

| Module | Nhóm nghiệp vụ | Ca nghiệm thu tiêu biểu trong đặc tả bản đầu |
| --- | --- | --- |
| 01 — Tài khoản/hồ sơ | NV-01 | NT-01, NT-11 |
| 02 — Học kỳ/môn | NV-02, phần xóa của NV-10 | NT-09, NT-11 |
| 03 — Nhiệm vụ/tiến độ | NV-04, NV-05, NV-06 | NT-02–07, NT-10–11 |
| 04 — Lịch sử | Phần lịch sử của NV-10 | NT-09–11 |
| 05 — Lịch/tổng quan | NV-03, NV-08, sử dụng NV-06 | NT-02, NT-05, NT-09, NT-11 |
| 06 — Thông báo | NV-07, sử dụng NV-06 | NT-08, NT-10–11 |
| 07 — Gợi ý | NV-09, sử dụng NV-06 | NT-12, NT-11 |
| 08 — Quản trị | Phần quản trị của NV-10, hỗ trợ NV-01 | NT-01, NT-11; thêm ca quản trị trong module |

Một ca NT có thể kiểm tra nhiều module. Bảng này để lần theo phạm vi, không ghi rằng các ca đã chạy hoặc đã đạt.

## Trạng thái code đã xác minh

Backend đã [nâng .NET 10, SDK 10.0.400](../history/2026-10-13-nang-cap-dotnet-10.md). Program.cs có hai endpoint mẫu và ánh xạ HealthController /health/live; chưa có controller nghiệp vụ học tập. Domain/Application mới có Class1 rỗng; DbContext chưa có DbSet; InitialCreate rỗng. Những luồng nghiệp vụ và kịch bản dưới đây là mục tiêu cần triển khai/kiểm tra, chưa có kết quả đạt.

## Các luồng phải kiểm tra xuyên module

| Thao tác | Phần chịu trách nhiệm chính | Ảnh hưởng cần kiểm tra |
| --- | --- | --- |
| Tạo/sửa/hoàn thành nhiệm vụ | Nhiệm vụ | Lịch sử, lời nhắc, tiến độ tổ tiên, lịch/tổng quan/gợi ý |
| Đổi hạn hoặc chuyển cha | Nhiệm vụ | Hạn hiệu lực cả cây bị ảnh hưởng, lời nhắc, lịch và thứ tự gợi ý |
| Xóa môn/học kỳ | Học kỳ/môn | Xóa mẫu lịch; giữ nhiệm vụ, bỏ CourseId và ghi lịch sử |
| Đổi giờ nhắc mặc định | Tài khoản | Tính lại lời nhắc của nhiệm vụ dùng mặc định |
| Đổi múi giờ | Tài khoản | Đổi cách diễn giải lịch địa phương và ngày thống kê, không tự đổi thời điểm deadline |
| Vô hiệu hóa tài khoản | Quản trị | Các request bảo vệ không tiếp tục được chấp nhận |
| Xóa tài khoản nếu mở rộng | Luồng xóa riêng | Ghi lịch sử trước cascade, bỏ liên kết danh tính; chưa có API bản đầu |

## Cách cập nhật sau này

Mỗi module ghi riêng mục cần chốt và kịch bản nghiệm thu. Khi triển khai, thêm liên kết tới controller/use case/entity/test thật và ghi kết quả ở lịch sử. Chỉ ghi đạt khi đã chạy kiểm chứng tương ứng; người dùng tự xác nhận phần mình đã đọc/duyệt.
