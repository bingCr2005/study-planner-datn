# 2026-10-13 — Đặc tả nghiệp vụ bản đầu

## Yêu cầu và mục đích

- Người dùng hỏi cách thực hiện bước chốt nghiệp vụ và thiết kế; đã chọn bản đầu gợi ý việc nên làm trước có giải thích, chưa tự phân bổ giờ học, qua câu trả lời trong hội thoại.
- Mục đích: đưa ra bản hành vi cụ thể và tình huống nghiệm thu trước khi triển khai, thay vì tiếp tục chỉ nói về kế hoạch.
- Bản ghi liên quan: [ERD](2026-10-13-thiet-ke-erd.md), [kế hoạch/API](2026-10-13-ke-hoach-chi-tiet-api-swagger.md).

## Code và logic thay đổi

- Thêm [đặc tả](../DAC-TA-NGHIEP-VU-BAN-DAU.md); liên kết từ [kế hoạch](../KE-HOACH-TRIEN-KHAI-CHI-TIET.md), [API](../THIET-KE-API-SWAGGER.md) và mục lục lịch sử.
- Trước: có ERD/kế hoạch/API nhưng nhiều quyết định nghiệp vụ rải rác, chưa có tình huống nghiệm thu gắn mã và phân biệt hạn với ngày dự định làm.
- Sau: có phạm vi, 10 nhóm nghiệp vụ NV-01–10, luồng sử dụng, từ ngữ, bảng nối màn hình/API/dữ liệu, 12 tình huống NT-01–12 và trạng thái từng quyết định.
- Đồng bộ API về Type mặc định đề xuất: Assignment khi có môn, Personal khi không môn; giữ khả năng chọn giá trị khác hợp lệ. Chưa có code hoặc migration thực thi mặc định này.
- Luồng dự kiến: người dùng tạo việc → kiểm tra sở hữu/dữ liệu → tính hạn/tiến độ → ghi nhiệm vụ/lịch sử/lịch nhắc → hiển thị lịch và gợi ý. Gợi ý chỉ truy vấn, không tự ghi buổi học.
- Giữ các lựa chọn ERD hiện tại như cây nhiều cấp, tiến độ cha theo con, nhắc cha/con và giới hạn chưa tắt nhắc; ghi rõ là đề xuất chưa tự nhận người dùng đã duyệt.
- Không thay đổi code ứng dụng/ERD/database; không commit/push/triển khai.

## Kiểm chứng

- Đọc README/TEMPLATE, lịch sử liên quan, ERD, phần quyết định của kế hoạch/API, Domain Class1, DbContext và Program.cs; code vẫn chưa có nghiệp vụ.
- Xem nguồn chính thức MyStudyLife về tính năng và Todoist về deadline; dẫn nguồn ngay ở phần khảo sát. Đây là nghiên cứu tài liệu, chưa dùng thử trực tiếp hoặc phỏng vấn sinh viên. Không gán quy tắc của dự án cho sản phẩm tham khảo.
- Kiểm tra 5 file Markdown mới/chỉnh: các liên kết Markdown tương đối đều tồn tại; đếm đủ 10 nhóm NV và 12 tình huống NT. git diff --no-index --check không báo lỗi whitespace, chỉ cảnh báo LF sẽ chuyển CRLF theo cấu hình Git. Đã đọc lại nội dung đặc tả và các đoạn đồng bộ trong kế hoạch/API; không có thay đổi code để build.
- Không chạy build/test/API/migration vì chỉ thay đổi tài liệu. Các tình huống NT là tiêu chí cần kiểm thử khi code có, không phải test đã chạy thành công.

## Việc tiếp theo

- Người dùng đọc các quy tắc và ví dụ; nếu thay hành vi thì cập nhật tài liệu liên quan trước phần code phụ thuộc. Chỉ xác nhận phạm vi gợi ý đã được người dùng đưa ra, không xác nhận đã đọc/duyệt tài liệu.
- Có thể bắt đầu khung API/Swagger; chốt các lựa chọn ảnh hưởng schema trước migration tương ứng. Hoàn thiện thiết kế kỹ thuật Auth/sửa đồng thời và module thông báo đúng thời điểm triển khai.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu đặc tả với code nền và tài liệu liên quan, phân biệt nguồn khảo sát với đề xuất.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
