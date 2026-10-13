# 2026-10-13 — Kế hoạch triển khai chi tiết và API/Swagger

## Yêu cầu và mục đích

- Người dùng yêu cầu viết lại kế hoạch đầy đủ, chi tiết nhất có thể, bao gồm thiết kế API/Swagger và có thể lưu vào Markdown.
- Mục đích: chuyển lộ trình tổng quan thành chức năng, màn hình, hợp đồng API, thứ tự làm, tiêu chí kiểm thử và mốc bàn giao có thể lần theo.
- Bản ghi liên quan: [đánh giá và kế hoạch](2026-10-13-danh-gia-va-ke-hoach.md), [ERD](2026-10-13-thiet-ke-erd.md), [kiến trúc](2026-10-13-ghi-chu-kien-truc-backend.md).

## Code và logic thay đổi

- Thêm [kế hoạch chi tiết](../KE-HOACH-TRIEN-KHAI-CHI-TIET.md), [thiết kế API/Swagger](../THIET-KE-API-SWAGGER.md), bản ghi này và liên kết ở [README](README.md).
- Bổ sung đường dẫn tới hai tài liệu trong [kế hoạch tổng quan](../KE-HOACH-PHAT-TRIEN.md), giữ lại kết quả đánh giá/build cũ và không thay bằng kiểm chứng chưa chạy.
- Trước: kế hoạch bảy mốc tổng quan, thiếu chi tiết màn hình/endpoint/DTO/Swagger và thứ tự tích hợp UI.
- Sau: kế hoạch 12 giai đoạn (0–11), phạm vi P0/P1/P2, bảng màn hình/quyền, đầu ra/kiểm tra từng giai đoạn, thuật toán baseline để so sánh, kịch bản demo, tiêu chí hoàn thành; tài liệu API riêng có route/method, dữ liệu, lỗi, ví dụ và tiêu chí Swagger.
- Luồng dự kiến: khảo sát/chốt nghiệp vụ → hợp đồng API → khung Swagger/schema → auth → học kỳ/môn → nhiệm vụ lá và lịch sử → cây → lịch/Dashboard → phát thông báo → gợi ý → quản trị → nghiệm thu. Frontend tích hợp theo từng luồng, không chờ giai đoạn cuối.
- Không thay đổi code, ERD hoặc schema. Các API, UI và quy tắc mới đều được ghi rõ là đề xuất. Không có migration/commit/push/triển khai.
- Điểm cần quyết định trước phần code phụ thuộc: tắt nhắc/nhắc cha-con, phiên bản sửa đồng thời, thu hồi token, giới hạn cây, mã sinh viên đa trường, chính sách lịch sử. Không tự coi đã được người dùng duyệt.

## Kiểm chứng

- Đã đọc README/TEMPLATE/bản ghi liên quan, kế hoạch tổng quan, kiến trúc, ERD, Program.cs, các csproj, DbContext, Class1 Domain/Application và model snapshot. Swagger đăng ký lặp, thiếu MapControllers và schema nghiệp vụ rỗng vẫn khớp code; chưa có frontend/test trong file được kiểm kê.
- Tham khảo tài liệu Microsoft về Swagger/OpenAPI cho ASP.NET Core 8 và Problem Details; đã gắn liên kết nguồn ngay trong tài liệu API.
- Kiểm tra 5 file Markdown mới/chỉnh: liên kết Markdown tương đối đều trỏ tới file tồn tại; hàng rào code cân bằng; đếm đúng 12 giai đoạn; cả 5 ví dụ JSON parse thành công. Kiểm tra 49 dòng endpoint ghi tường minh không có cặp method/route trùng (chưa tính 16 operation CRUD danh mục mô tả theo ma trận). Đây là kiểm tra nguồn tài liệu, không phải kiểm thử OpenAPI hoặc HTTP.
- git diff --no-index --check trên các file thay đổi không báo lỗi whitespace, chỉ cảnh báo LF sẽ đổi thành CRLF theo cấu hình Git. Các file docs hiện chưa được Git theo dõi nên dùng so sánh với NUL để kiểm tra file mới, không coi git diff thông thường rỗng là không có thay đổi.
- Đọc lại các phần hợp đồng với ERD: 11 bảng, ba trạng thái task, dueDate nullable/hạn kế thừa, lịch lặp theo tuần, Notification.ReadAt/DeliveryStatus, lịch sử TaskId không FK, UserId nullable. Cơ chế auth/phiên bản chưa có trong ERD được nêu là quyết định ảnh hưởng schema trước triển khai.
- Không chạy build/test/API/Swagger thực hoặc migration vì chỉ tạo tài liệu. Không khẳng định các ví dụ JSON đã được API thật trả về hoặc đã dùng thử các sản phẩm được đề xuất khảo sát.

## Việc tiếp theo

- Rà các quyết định nghiệp vụ và hợp đồng còn mở, sau đó triển khai khung API/Swagger và schema theo mốc đầu.
- Khi có deadline/số thành viên/giờ làm thực tế mới chia lịch tuần; không gán ước lượng thời gian giả định ở tác vụ này.
- Mỗi thay đổi code tiếp theo cần lịch sử và kiểm chứng phù hợp.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu kế hoạch/hợp đồng với code nền, ERD và ghi giới hạn kiểm chứng.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
