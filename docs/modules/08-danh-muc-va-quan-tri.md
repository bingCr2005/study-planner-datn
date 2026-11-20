# Module 08 — Danh mục và quản trị

## Cập nhật hiện hành 19/11/2026

- Đã tích hợp giao diện thêm/sửa tài khoản sinh viên và theo dõi học tập vào checkout gốc, nhánh `feat/tich-hop-quan-tri-sinh-vien`. Hai nhánh chức năng riêng vẫn được giữ trong worktree. Xem [bản ghi tích hợp](../history/2026-11-19-tich-hop-admin-vao-ban-dang-chay.md).
- Admin có danh sách/chi tiết, tạo tài khoản Student, sửa hồ sơ và khóa/mở khóa. API xóa tài khoản đã tồn tại trước thay đổi này nhưng không bổ sung nút xóa vào giao diện. Sửa hồ sơ gửi mốc cập nhật để backend chặn ghi đè dữ liệu cũ bằng 409.
- [Theo dõi học tập](10-admin-theo-doi-hoc-tap.md) chỉ đọc nhiệm vụ học tập, kế hoạch tự học và tổng hợp thời gian; không mở quyền sửa/xóa nhiệm vụ hoặc đọc dữ liệu Personal, mô tả, checklist, ghi chú, tệp và giờ rảnh.
- Danh mục mẫu môn/học kỳ lưu JSON; hồ sơ dùng tên trường/lớp/ngành tự khai. Không triển khai lại danh mục tổ chức cũ. Tổng quan tài khoản và quản lý quy tắc ưu tiên là chức năng có sẵn.

## Nội dung lịch sử

Các mô tả phân cấp trường/khoa/khóa/lớp và trạng thái chưa triển khai dưới đây thuộc đặc tả cũ, không phản ánh đầy đủ code hiện hành.

**Hiện hành 15/11/2026:** đã bỏ bốn bảng tổ chức trường/khoa/khóa/lớp và khóa `nguoi_dung.ma_lop`. Hồ sơ dùng tên tự khai; quản trị tài khoản đã có. Danh mục mẫu môn/học kỳ dùng JSON. Nội dung về phân cấp tổ chức bên dưới là đặc tả lịch sử đã rút khỏi phạm vi hiện tại. Xem [bảng đang sử dụng](../database-tables.md).

Trang quản lý học kỳ/môn học mẫu của admin được triển khai riêng trong [Dữ liệu nền tảng](09-du-lieu-nen-tang.md). Các danh mục tổ chức trường/khoa/khóa/lớp mô tả dưới đây vẫn là phạm vi dự thảo; không dùng chúng để thay thế mẫu hoặc dữ liệu học tập riêng của sinh viên.

Trạng thái: đặc tả dự thảo, chưa triển khai. Nguồn: [quy ước chung](00-quy-uoc-va-quyet-dinh.md), [ERD](../erd.md), [API — Catalogs/Admin](../THIET-KE-API-SWAGGER.md). CRUD quản trị ở giai đoạn 10; đọc danh mục tối thiểu khi hồ sơ cần.

## 1. Mục tiêu và phạm vi

Sinh viên chọn thông tin lớp nếu muốn. Admin quản lý trường/khoa/khóa/lớp và kích hoạt/vô hiệu hóa tài khoản. Màn hình: danh mục phụ thuộc và danh sách tài khoản quản trị.

Chưa có API tự đổi Role, đọc mọi nhiệm vụ cá nhân, xóa hẳn tài khoản hoặc quản trị lịch sử không còn UserId. Không bắt người dùng mới điền hết danh mục trước khi tạo việc.

## 2. Dữ liệu và quy tắc

| Dữ liệu | Ràng buộc theo ERD |
| --- | --- |
| School | Code unique |
| Faculty | Thuộc School; (SchoolId, Code) unique |
| Cohort | Name unique toàn ứng dụng; EndYear >= StartYear |
| StudentClass | Thuộc Faculty và Cohort; (FacultyId, CohortId, Name) unique |
| User.ClassId | Có thể NULL; chọn lớp tồn tại, các thông tin trường/khoa suy theo quan hệ |

Cohort là danh mục khóa dùng chung, không tự diễn giải thành khóa riêng của từng trường. StudentCode unique theo giả định Q01, không dùng trường/khoa hiển thị để ngầm đổi quy tắc unique.

Student đã đăng nhập được đọc danh mục; chỉ Admin được tạo/sửa/xóa hoặc xem danh sách tài khoản quản trị. Không chỉ ẩn nút ở UI mà phải kiểm tra quyền backend.

## 3. Luồng quản lý danh mục

1. Admin chọn danh mục và nhập dữ liệu/quan hệ → kiểm tra role, trường bắt buộc và FK → xử lý unique → lưu/trả DTO.
2. Student chọn trường/khoa/khóa để lọc lớp → gửi ClassId trong PATCH hồ sơ → server kiểm tra lớp tồn tại; không nhận các ID trường/khoa độc lập gây hồ sơ mâu thuẫn.
3. Xóa School còn Faculty, Faculty còn StudentClass hoặc Cohort còn StudentClass bị RESTRICT, trả 409.
4. Xóa StudentClass bỏ ClassId của User về NULL, giữ tài khoản và dữ liệu học tập.
5. Sửa quan hệ của danh mục đang dùng có thể đổi trường/khoa hiển thị của nhiều hồ sơ. Cần chốt cảnh báo/giới hạn trước phần sửa đó; không âm thầm coi là sửa nhãn vô hại.

## 4. Luồng trạng thái tài khoản

Admin tìm tài khoản → chọn bật/tắt hoạt động → server kiểm tra role và quy tắc ngăn tự khóa/quản trị cuối cùng → cập nhật IsActive → các request bảo vệ kiểm tra lại trạng thái theo module Auth.

Vô hiệu hóa không xóa tài khoản/nhiệm vụ và không thay thế luồng xóa hẳn. Nếu mở rộng xóa hẳn, phải thiết kế xác nhận riêng và giữ lịch sử theo ERD; chưa có endpoint tương ứng trong bản đầu.

## 5. API và ngoại lệ

- GET /api/catalogs/schools, /api/catalogs/faculties, /api/catalogs/cohorts, /api/catalogs/student-classes với lọc/phân trang theo API.
- POST collection và GET/PUT/DELETE item tại /api/admin/schools, /api/admin/faculties, /api/admin/cohorts, /api/admin/student-classes. Danh sách đọc dùng Catalogs; không mặc định nhân đôi endpoint danh sách.
- GET /api/admin/users; PATCH /api/admin/users/{id}/active.

Student gọi Admin trả 403; không phiên trả 401; unique/RESTRICT/xung đột nghiệp vụ trả 409; đầu vào sai trả 400. DTO tài khoản không có PasswordHash/token. Seed Admin có kiểm soát theo môi trường, không ghi thông tin xác thực vào tài liệu.

## 6. Tình huống nghiệm thu

| Tình huống | Kết quả kỳ vọng |
| --- | --- |
| Student gọi CRUD Admin bằng request trực tiếp | 403, không thay đổi dữ liệu |
| Code khoa trùng cùng trường/khác trường | Cùng trường bị chặn; khác trường theo unique ghép |
| Lớp trùng trong cùng khoa/khóa | Bị chặn, không tạo dòng trùng |
| Xóa danh mục còn tham chiếu | RESTRICT đúng các quan hệ theo ERD |
| Xóa lớp đang có User | User.ClassId NULL, không mất tài khoản/học tập |
| Khóa tài khoản sau khi đã login | Token hiện có không tiếp tục cho request bảo vệ |
| Tự khóa hoặc khóa Admin cuối cùng, kể cả đồng thời | Bị chặn theo chính sách, không mất đường quản trị |
| User chưa chọn lớp | Vẫn quản lý việc được |

## 7. Điểm cần chốt và điều kiện hoàn thành

Chốt Q01, cách seed Admin, chính sách bảo vệ quản trị cuối cùng trong giao dịch và xử lý thay quan hệ danh mục đang dùng. Hoàn thành khi CRUD đúng FK/unique/xóa, quyền được thử qua API và cơ chế khóa tài khoản phối hợp Auth hoạt động.
