# 2026-10-13 — Thiết kế ERD Study Planner

## Yêu cầu và mục đích

- Người dùng yêu cầu vẽ lại ERD/cơ sở dữ liệu sau khi rà soát sơ đồ đã gửi. Đếm lại có 11 bảng, không phải 12 như phần mô tả trước đó.
- Mục đích: đưa ra thiết kế có thể lần theo nghiệp vụ, giải quyết mâu thuẫn giữ lịch sử khi xóa User và quy định rõ tiến độ cây nhiệm vụ, hạn kế thừa, vòng đời thông báo.
- Bản ghi trước: [Trạng thái ban đầu](2026-10-13-trang-thai-ban-dau.md).

## Code và logic thay đổi

- Thêm [docs/erd.md](../erd.md); thêm bản ghi này và liên kết trong [README](README.md). Không sửa code ứng dụng.
- Trước: chưa có docs/erd.md; code Domain có Class1 rỗng, DbContext chưa có DbSet, migration InitialCreate rỗng.
- Sau: tài liệu thiết kế 11 bảng với Mermaid, nullability, enum, kiểu dữ liệu đề xuất, khóa/chỉ mục, xóa và các quy tắc dịch vụ. Code vẫn ở trạng thái nền, chưa triển khai schema.
- User có NormalizedEmail để áp dụng duy nhất nhất quán; StudentCode tùy chọn với giả định duy nhất trong đơn vị triển khai. TaskHistory.UserId nullable và SET NULL để giữ sự kiện khi xóa tài khoản; TaskId vẫn không có FK.
- Nhiệm vụ không hạn được phép; con kế thừa hạn từ tổ tiên gần nhất. Tiến độ cha tính từ con, giữ nhất quán với ba trạng thái. Các kiểm tra qua bảng và chống chu trình phải thực hiện trong dịch vụ/giao dịch, không được coi FK đơn là đủ.
- Notification dùng ReadAt thay IsRead, thêm trạng thái phát và thông tin thử lại. Luồng: tạo/sửa nhiệm vụ → tính hạn và giờ nhắc → cập nhật bản ghi nhắc duy nhất → tác vụ nền xác minh lại và phát trong ứng dụng → đọc ghi ReadAt.
- Xóa môn/học kỳ giữ nhiệm vụ; xóa cây ghi lịch sử từng nhiệm vụ trước khi cascade; xóa tài khoản giữ lịch sử nhưng bỏ liên kết danh tính. Xóa bằng SQL trực tiếp không tự tạo lịch sử.
- Giới hạn: lịch lặp hằng tuần không có ngoại lệ; thông báo trong ứng dụng một mốc, có thể nhắc cả cha và con; chưa tự xếp buổi học; chưa có thời gian thực hiện đo thực tế. Lịch sử thông báo mỗi lần gửi không được lưu riêng.

## Kiểm chứng

- Đọc README/TEMPLATE lịch sử, bản ghi ban đầu, AGENTS.md, Class1.cs, ApplicationDbContext.cs, migration InitialCreate và trạng thái Git.
- Rà lại Mermaid và các phần quy tắc với nhau; sửa trường hợp trung bình làm tròn về 0 nhưng cha đang InProgress, làm rõ giới hạn 1–99 và CompletedAt; bổ sung kiểm tra cây con khi thay đổi hạn.
- Kiểm tra cấu trúc tài liệu bằng PowerShell: thực tế đếm được 11 khối bảng và 13 đường quan hệ; đã sửa số lượng trong tài liệu theo kết quả. Kiểm tra whitespace bằng git diff --no-index --check trên ba file tài liệu: không có lỗi whitespace, chỉ có cảnh báo chuyển LF sang CRLF theo cấu hình Git.
- Chưa render Mermaid bằng trình render; chưa build, chạy test hoặc migration vì chỉ thay đổi tài liệu và chưa sinh Entity/SQL. Không xác nhận database thực tế đã có bảng hoặc hành vi này.

## Việc tiếp theo

- Người dùng đối chiếu phạm vi: bản này quản lý học tập và cung cấp dữ liệu cho gợi ý thứ tự, chưa tự phân bổ thời gian.
- Khi triển khai, viết Entity/Fluent API/migration và kiểm tra PostgreSQL, quyền sở hữu, cây nhiệm vụ, lịch sử khi cascade, nhắc hạn và cập nhật đồng thời.
- Quy định thời hạn lưu lịch sử; xem lại StudentCode nếu triển khai nhiều trường; bổ sung ngoại lệ lịch hoặc tùy chọn tắt nhắc nếu phạm vi cần.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code nền và nội dung thiết kế; các giới hạn kiểm chứng được nêu rõ.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo; không commit/push trong tác vụ này.
