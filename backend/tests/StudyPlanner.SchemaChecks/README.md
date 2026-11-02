# Kiểm tra model và SQL schema

**Cập nhật 31/10:** model hiện có 18 bảng và 27 FK, thêm 7 Entity cùng field lập kế hoạch. [Hướng dẫn mới](../../../docs/ENTITY-VA-FIELD-LAP-KE-HOACH.md). API/worker mới chưa triển khai; phần mô tả trước đó bên dưới là tham chiếu.

Chạy từ thư mục gốc:

```powershell
dotnet run --project backend/tests/StudyPlanner.SchemaChecks
```

Đây là console kiểm tra không cần thêm thư viện test: lỗi điều kiện sẽ ném exception và trả exit code khác 0. Chương trình xây dựng model EF Core và sinh DDL PostgreSQL trong bộ nhớ, không kết nối database. Chuỗi cấu hình trong chương trình chỉ là giá trị giả cho provider.

Kiểm tra giữ đủ 11 bảng tên tiếng Việt và 13 FK, gồm danh mục/FK lớp legacy dù API của chúng đã rút khỏi bản lõi. Migration đổi bảng chỉ có 11 RenameTable ở mỗi chiều, không tái tạo dữ liệu. Kiểm tra hành vi xóa, không sinh cột FK ẩn do gõ sai, tên cột tiếng Việt snake_case, hồ sơ TenTruong/TenLop tùy chọn, lịch sử không FK tới nhiệm vụ, jsonb, kiểu ngày/giờ, chỉ mục unique/partial và sentinel ưu tiên Low/giờ nhắc 0. Build solution không tự chạy các kiểm tra này; cần lệnh trên.

Các kiểm tra này không chứng minh PostgreSQL thực thi FK/CHECK, cascade hoặc rollback đúng trên dữ liệu thật. Bước tiếp theo là kiểm thử migration trên database phát triển độc lập và kiểm thử tích hợp nghiệp vụ.
