# 2026-11-04 — API học kỳ/môn học và tích hợp FE

## Yêu cầu và mục đích

- Người dùng yêu cầu đồng bộ FE với BE rồi xác nhận cần triển khai API backend quản lý học kỳ/môn.
- Trước thay đổi: entity và ràng buộc DB đã có, Swagger đang chạy chưa có route học kỳ/môn; tài liệu module 02 còn là dự thảo. FE khóa ghi dữ liệu học tập.
- Liên quan: [đặc tả module](../modules/02-hoc-ky-va-mon-hoc.md), [bản ghi đặc tả trước](2026-10-13-dac-ta-nghiep-vu-module.md).

## Code và logic thay đổi

- `backend/StudyPlanner.Application/Academic/Contracts.cs`, `AcademicService.cs`: DTO tiếng Việt, kiểm tra dữ liệu, truy vấn phạm vi chủ từ JWT, phân trang/tìm kiếm, CRUD và deletion-preview.
- `backend/StudyPlanner.Api/Controllers/SemestersController.cs`, `CoursesController.cs`, `Academic/AcademicRegistration.cs`, `Program.cs`: route `/api/v1`, đăng ký dịch vụ, 201 và Location; DELETE 204 không body.
- `VietnameseSwagger.cs`: tag và mô tả DTO/nghiệp vụ. [Hợp đồng triển khai](../API-HOC-KY-MON-HOC.md) là nguồn tên trường gửi/nhận cho FE; module 02 được cập nhật trạng thái và liên kết.
- `backend/tests/StudyPlanner.AcademicChecks/`: host HTTP với JWT thật, database PostgreSQL tạm và migration thật. Đưa project vào solution và script kiểm thử.
- Xóa môn: xác minh chủ trong giao dịch Serializable → ghi lịch sử liên kết và bỏ mã môn của task → lưu → xóa lịch/môn → commit. Xóa học kỳ bỏ cả mã môn/học kỳ của task, gồm việc chung kỳ. Lỗi ở bất kỳ bước nào rollback; không dựa vào cascade để bỏ liên kết FK ghép.
- PUT môn không nhận mã học kỳ. Trường ngoài DTO bị từ chối, không thể gán chủ hoặc di chuyển môn bằng body. ID người khác trả 404 như ID không có.
- Preview chỉ cung cấp số liệu; không khóa dữ liệu đến DELETE. Xung đột trong giao dịch dùng cơ chế 409 đã có, không bổ sung ETag/schema mới.
- FE ở repo riêng dùng DTO này, Bearer JWT, đầy đủ các trang và ID BE; ánh xạ màu hex, giữ màu BE tùy chọn, khóa chuyển học kỳ. Không tự fallback mock. Nhiệm vụ/lịch chưa có API ghi nên tiếp tục khóa.

## Kiểm chứng

- Đã đọc code hiện tại, README/TEMPLATE lịch sử, bản ghi đặc tả và các ràng buộc FK ghép trong configuration. Đã đối chiếu luồng xóa với task/history/class session thật trong DB tạm.
- Build API và AcademicChecks trên bản sao nguồn trong workspace thành công với SDK 10.0.402. Có cảnh báo NU1900 vì sandbox không truy cập được dữ liệu NuGet audit; restore thư viện/build vẫn thành công từ cache.
- Kiểm tra HTTP/JWT/PostgreSQL: quyền sở hữu hai tài khoản, trường bắt buộc/tùy chọn, ngày/tín chỉ/màu, từ chối chuyển kỳ, phân trang, preview, 204, giữ cây/hạn/tiến độ, ghi lịch sử và rollback khi cố ý gây lỗi xóa. 67 kiểm tra đạt trong lượt cuối (gồm kiểm tra FE qua proxy và rollback khi xóa học kỳ).
- Kiểm tra FE qua Vite proxy `/api/v1` tới host BE tạm, dùng JWT thật và DB tạm; CRUD, màu, quyền sở hữu và xóa không body đạt. 81 test FE và production build đạt. Browser auth/profile/admin/mock/JWT qua Chrome/Edge đạt; lượt trong sandbox timeout khi mở target, chạy ngoài sandbox đã đạt.
- Không áp dụng migration hay ghi dữ liệu vào DB ứng dụng. Không khởi động lại BE đang chạy; tiến trình cũ cần restart để nạp assembly mới.

## Việc tiếp theo

- API nhiệm vụ/lịch/worker vẫn chờ triển khai; không coi giao diện trống là dữ liệu mẫu. Kiểm tra tải đồng thời quy mô lớn/worker không nằm trong lượt kiểm thử này.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu nguồn, hợp đồng và kiểm tra tích hợp trên DB tạm.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Chưa commit/push.
