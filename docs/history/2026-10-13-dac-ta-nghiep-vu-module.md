# 2026-10-13 — Đặc tả nghiệp vụ theo module

## Yêu cầu và mục đích

- Người dùng hỏi có nên tạo Markdown theo module để hiểu rõ nghiệp vụ từ đầu và yêu cầu tạo nếu phù hợp.
- Tách luồng, quy tắc, ảnh hưởng và nghiệm thu theo chức năng, tránh chỉ đọc bảng/endpoint rồi triển khai thiếu nghiệp vụ.
- Liên quan: [ERD](2026-10-13-thiet-ke-erd.md), [kiến trúc](2026-10-13-ghi-chu-kien-truc-backend.md), [kế hoạch/API chi tiết](2026-10-13-ke-hoach-chi-tiet-api-swagger.md).

## Code và logic thay đổi

- Thêm [mục lục module](../modules/README.md), [quy ước/quyết định](../modules/00-quy-uoc-va-quyet-dinh.md) và tám đặc tả được dẫn từ mục lục: tài khoản, học kỳ/môn, nhiệm vụ/tiến độ, lịch sử, lịch/tổng quan, thông báo, gợi ý, danh mục/quản trị.
- Bổ sung liên kết khám phá bộ module trong ERD, kiến trúc, kế hoạch chi tiết và API; cập nhật danh sách lịch sử.
- Trước: đặc tả phân tán trong ERD, kiến trúc, kế hoạch và bảng API; chưa có luồng/ngoại lệ/nghiệm thu riêng từng module.
- Sau: có luồng đầu vào → kiểm tra → thay đổi dữ liệu/giao dịch → kết quả cho từng module, kịch bản nghiệm thu và 12 nhóm rà soát Q01–Q12 (Q01–Q11 cần hoàn thiện, Q12 giữ phạm vi gợi ý đã được ghi nhận).
- Không thay đổi code, schema hoặc hợp đồng đã có. Các phần chưa có trong hợp đồng như lỗi read thông báo chưa Delivered, hiển thị tỷ lệ khi không có nhiệm vụ và xử lý lời nhắc mất hạn được đánh dấu cần chốt.
- Các module dùng cùng quyền sở hữu, hạn hiệu lực và thời điểm xét; luồng sửa nhiệm vụ phối hợp lịch sử/lời nhắc trong giao dịch. Lịch sử/lập lịch nhắc bắt đầu cùng nhiệm vụ lá, không đợi đến cuối mới bổ sung.
- Đặc tả là dự thảo, các ca nghiệm thu chưa được chạy; không coi yêu cầu tạo tài liệu là người dùng đã duyệt mọi lựa chọn.
- [Đặc tả bản đầu](../DAC-TA-NGHIEP-VU-BAN-DAU.md) và [bản ghi tương ứng](2026-10-13-dac-ta-nghiep-vu-ban-dau.md) được bổ sung trong lúc làm. Đã đọc và đồng bộ Type mặc định đề xuất, cho phép task quá hạn/hạn ngoài học kỳ, baseline và phạm vi gợi ý. README module có bảng nối NV-01–10/NT-01–12. Không yêu cầu chọn lại phạm vi đã được tài liệu đó ghi nhận; không suy ra toàn bộ quy tắc đã được duyệt.

## Kiểm chứng

- Đã đọc README/TEMPLATE lịch sử và bản ghi kế hoạch/API, các phần ERD/API/kế hoạch liên quan; đối chiếu Program.cs, DbContext, Class1 Domain/Application và migration InitialCreate.
- Code vẫn là khung nền, chưa có Entity/DbSet/controller nghiệp vụ; tài liệu ghi đúng giới hạn này.
- Đã đọc lại nội dung mới và đối chiếu endpoint, ba trạng thái nhiệm vụ, tiến độ cha, hạn kế thừa, quy tắc xóa, vòng đời thông báo và phạm vi gợi ý với nguồn thiết kế.
- Kiểm tra bằng PowerShell (ReadAllText/regex/Test-Path) trên 16 tài liệu mới/chỉnh: liên kết Markdown tương đối đều tồn tại, hàng rào code cân bằng, không có trailing whitespace. Đếm đúng 10 file trong modules gồm README, quy ước và 8 module nghiệp vụ; đủ 12 nhóm Q01–Q12, không có mã quyết định dẫn tới mục không tồn tại.
- Lượt kiểm tra bằng Python ban đầu không chạy vì máy không có lệnh python; đã thực hiện kiểm tra tài liệu bằng PowerShell như trên. Đây là kiểm tra nguồn Markdown, không phải test nghiệp vụ hoặc HTTP.
- `git -c core.autocrlf=false diff --no-index --check -- NUL <file>` trên 16 tài liệu mới/chỉnh không có lỗi whitespace; kiểm tra cả file chưa được Git theo dõi, không lấy diff thông thường rỗng làm bằng chứng không có thay đổi.
- Chưa chạy build/test/API/migration vì chỉ bổ sung tài liệu, không sửa ứng dụng. Nghiệm thu trong module là kết quả kỳ vọng, không phải test đã đạt; không render UI/Swagger/Mermaid trong tác vụ này.

## Việc tiếp theo

- Đọc mục lục và quyết định còn mở, chốt phần ảnh hưởng mốc tài khoản/schema trước khi code; không cần chốt thuật toán nâng cao để làm CRUD nền.
- Khi triển khai từng module, dẫn tới file code/test thực tế và ghi kết quả vào bản ghi mới.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu đặc tả với code nền và tài liệu thiết kế, ghi rõ phần chưa kiểm chứng bằng chạy ứng dụng.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
