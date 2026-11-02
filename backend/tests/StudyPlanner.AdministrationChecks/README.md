# PostgreSQL và HTTP checks cho tài khoản và bản lõi

AuthChecks còn kiểm tra register bắt buộc bảy field, login bằng taiKhoan email/MSSV, giữ số 0 đầu, trùng email/MSSV, tài khoản cũ, định danh mơ hồ và schema/example Swagger. Fixture HTTP dùng IP riêng cho các ca chức năng; ca rate limit dùng cùng IP để xác minh quota thật. Chỉ test host có cơ chế tách IP, không thêm header tin cậy vào production.

**Cập nhật 31/10:** model hiện có 18 bảng và 27 FK, thêm 7 Entity cùng field lập kế hoạch. [Hướng dẫn mới](../../../docs/ENTITY-VA-FIELD-LAP-KE-HOACH.md). API/worker mới chưa triển khai; phần mô tả trước đó bên dưới là tham chiếu.

Cập nhật 31/10: CRUD suite tập trung quản trị người dùng, hồ sơ tự khai `tenTruong`/`tenLop`, trạng thái, giao dịch và lịch sử. Các API danh mục trường/khoa/khóa/lớp đã rút phải trả 404; bảng và FK legacy vẫn được giữ để tương thích dữ liệu. AuthChecks kiểm tra JWT thật, các vai trò và migration/backfill hồ sơ. Tên C#/JSON/cột theo tiếng Việt. Script hỗ trợ `-TestConfiguration Release`; chạy DLL trực tiếp nên không phụ thuộc apphost .exe. Chỉ dùng `-NoBuild` sau khi build đúng cấu hình đó.

Chương trình console kiểm tra API qua HTTP thật, dùng PostgreSQL thật, không thêm framework test. Không tự chạy khi build solution hoặc khi gọi dotnet test.

CRUD suite dùng actor Admin giả trong host Development và khách trong host Production; AuthChecks riêng dùng JWT thật trên host Production để kiểm tra Student/Admin. Script chạy cả hai. API quản trị người dùng thiếu xác thực trả 401; API danh mục đã rút trả 404 cho cả khách, Student và Admin. Khi API đang chạy khóa file, có thể build output riêng rồi đặt STUDYPLANNER_TEST_ARTIFACTS trỏ thư mục DLL cùng -NoBuild để chạy bản mới nhất.

## Cách chạy khuyến nghị

Cần Docker Desktop đang hoạt động và SDK trong global.json. Từ thư mục gốc repository:

```powershell
powershell -NoProfile -File scripts/test-administration.ps1
```

Script build solution, tạo container PostgreSQL 17 tạm với cổng localhost ngẫu nhiên, chạy checks và dọn container trong finally. Cấu hình chỉ truyền qua biến môi trường trong bộ nhớ, không sửa appsettings hoặc kết nối Neon. Image có thể được tải ở lần đầu và được giữ trong Docker cache; container không dùng volume dữ liệu bền.

Nếu đã build đúng code mới nhất, thêm `-NoBuild`. Không dùng cờ này sau khi sửa code mà chưa build lại.

## Chạy trực tiếp với PostgreSQL cục bộ

Đặt STUDYPLANNER_TEST_POSTGRES trong phiên shell bằng cấu hình PostgreSQL kiểm thử cục bộ có quyền tạo database; không chép bí mật vào tài liệu. Chương trình chỉ chấp nhận Host localhost/127.0.0.1 và database quản trị tên postgres, tạo database riêng có tên ngẫu nhiên rồi xóa đúng database đó trong finally.

```powershell
dotnet run --project backend/tests/StudyPlanner.AdministrationChecks
```

Không có biến môi trường thì chương trình dừng, không lấy DefaultConnection của API làm mặc định.

## Phạm vi kiểm tra

- Áp dụng migration hiện có vào database mới; HTTP tạo/đọc/sửa/xóa người dùng và kích hoạt/vô hiệu hóa.
- Required/độ dài/email/ID, trim, chuẩn hóa email, unique, tên trường/lớp tự khai; lọc, phân trang và Location khi tạo.
- Hash mật khẩu xác minh bằng Identity; không đưa hash hoặc mật khẩu vào DTO/OpenAPI; client không tự cấp Admin.
- DTO/OpenAPI không còn `maLop`; sửa hồ sơ mới không âm thầm xóa FK lớp legacy trong DB.
- Xóa người dùng cascade nhiệm vụ/thông báo nhưng giữ history với MaNguoiDung NULL; chặn dữ liệu cây sai chủ.
- Ép lỗi ghi history và lỗi xóa ở bước sau để kiểm tra rollback xuyên nhiều lần SaveChanges.
- Tạo email trùng đồng thời, xóa hai Admin đồng thời, bảo vệ Admin hoạt động cuối cùng.
- Các method của bốn controller quản trị danh mục và GET tra cứu không còn route thực, kể cả Admin; không chỉ kiểm tra ẩn trên Swagger.
- AuthChecks: schema cũ → đổi tên 98 cột → schema cũ → đổi cột lần hai → backfill hồ sơ → đổi tên 11 bảng Up/Down/Up. So toàn bộ 100 cột, OID bảng và identity sequence; giữ hash/snapshot/tên trường lớp, kiểm tra EF đọc quan hệ sau nâng cấp và dọn fixture trước các ca HTTP.

Kết quả PASS là điều kiện/khẳng định của chương trình, không phải số test case xUnit. CRUD suite không thay thế kiểm thử JWT thật trong AuthChecks; các suite không chứng minh giao diện, dữ liệu Neon hoặc mọi lịch cạnh tranh có thể xảy ra.
