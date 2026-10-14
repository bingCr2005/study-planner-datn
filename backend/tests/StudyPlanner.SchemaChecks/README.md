# Kiểm tra model và SQL schema

Chạy từ thư mục gốc:

```powershell
dotnet run --project backend/tests/StudyPlanner.SchemaChecks
```

Đây là console kiểm tra không cần thêm thư viện test: lỗi điều kiện sẽ ném exception và trả exit code khác 0. Chương trình xây dựng model EF Core và sinh DDL PostgreSQL trong bộ nhớ, không kết nối database. Chuỗi cấu hình trong chương trình chỉ là giá trị giả cho provider.

Kiểm tra 11 bảng, 13 FK và hành vi xóa, không sinh cột FK ẩn do gõ sai, lịch sử không FK tới nhiệm vụ, jsonb, kiểu ngày/giờ, chỉ mục unique/partial và sentinel ưu tiên Low/giờ nhắc 0. Build solution không tự chạy các kiểm tra này; cần lệnh trên.

Các kiểm tra này không chứng minh PostgreSQL thực thi FK/CHECK, cascade hoặc rollback đúng trên dữ liệu thật. Bước tiếp theo là kiểm thử migration trên database phát triển độc lập và kiểm thử tích hợp nghiệp vụ.
