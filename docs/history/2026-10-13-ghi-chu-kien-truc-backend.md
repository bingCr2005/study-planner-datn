# 2026-10-13 — Ghi chú kiến trúc backend

## Yêu cầu và mục đích

- Người dùng yêu cầu ghi kiến trúc dự án vào một file Markdown và giải thích.
- Lưu trách nhiệm từng tầng, hướng phụ thuộc, cấu trúc thư mục, luồng nghiệp vụ và cách tách thuật toán để có thể đối chiếu khi triển khai.
- Bản ghi liên quan: [đánh giá và kế hoạch](2026-10-13-danh-gia-va-ke-hoach.md), [thiết kế ERD](2026-10-13-thiet-ke-erd.md).

## Code và logic thay đổi

- Thêm [KIEN-TRUC-BACKEND.md](../KIEN-TRUC-BACKEND.md), bản ghi này và liên kết trong [README](README.md).
- Trước: kiến trúc được trao đổi trong hội thoại; repository có khung bốn project và kế hoạch nhưng chưa có tài liệu riêng giải thích trách nhiệm các tầng.
- Sau: có tài liệu monolith/Clean Architecture 4 tầng, cấu trúc chức năng đề xuất, phân biệt hướng tham chiếu với luồng chạy, ví dụ hoàn thành nhiệm vụ, ranh giới giao dịch và kiểm chứng.
- Không thay đổi code hoặc luồng ứng dụng hiện tại. Ví dụ nghiệp vụ và tên lớp/thư mục mới được ghi rõ là định hướng chưa triển khai.
- Tài liệu bám ERD: lịch học lặp khác buổi tự học, dữ liệu thời lượng hiện là ước lượng; tự phân bổ thời gian cần mở rộng thiết kế. Worker phải đọc lại trạng thái và xử lý đồng thời, không chỉ dựa vào timer.

## Kiểm chứng

- Đã đọc README/TEMPLATE và bản ghi liên quan; đối chiếu Program.cs, ApplicationDbContext.cs, các csproj, Class1 Domain/Application, migration InitialCreate và các phần ERD liên quan.
- Hướng tham chiếu project khớp định hướng; chưa có entity/DbSet/nghiệp vụ trong code đã đọc. AddControllers chưa đi kèm MapControllers; Swagger đăng ký lặp. Ghi nhận trong tài liệu, chưa sửa code.
- Đọc lại nội dung và kiểm tra liên kết tương đối/whitespace của các tài liệu thay đổi.
- Không chạy lại build/test/API/migration vì chỉ thêm tài liệu; không xác nhận các lớp minh họa đã chạy. Kết quả build trước đó nằm trong bản ghi đánh giá, không phải kiểm chứng mới của tác vụ này.
- Sơ đồ Mermaid chưa được render bằng công cụ; đã đọc lại nguồn và các mũi tên so với csproj.

## Việc tiếp theo

- Dùng tài liệu để chốt hợp đồng API và triển khai từng chức năng; chỉ tạo abstraction khi có nhu cầu rõ ràng.
- Chốt công thức gợi ý và tiêu chí đánh giá trước khi khẳng định hiệu quả tối ưu.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu tài liệu với code/ERD và ghi giới hạn kiểm chứng.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
