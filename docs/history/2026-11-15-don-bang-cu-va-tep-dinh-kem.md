# 2026-11-15 — Dọn bảng cũ và triển khai tệp đính kèm

## Yêu cầu và mục đích

- Người dùng yêu cầu rà bảng chưa dùng, xác nhận giữ tệp đính kèm để làm tiếp, gỡ bảng trường/khoa và xử lý chức năng cần thiết còn thiếu. Đã cho phép cập nhật database trong phiên.
- Đã đọc AGENTS, mục lục/TEMPLATE, bản ghi nhắc hạn, danh mục JSON, áp dụng DB, code entity/config/service/controller và chi tiết nhiệm vụ FE. Đọc database bằng giao dịch chỉ đọc trước khi sửa; không suy luận bảng thừa chỉ vì rỗng.
- Tách công việc khỏi nhánh thông báo đã push: BE/FE cùng nhánh `feat/don-bang-cu-va-tep-dinh-kem`. Không commit/push trong lượt này.

## Code và logic thay đổi

### Dọn schema và code dư

- Trước: model 22 bảng/34 FK; bốn bảng tổ chức và hàng đợi `lich_nhac` đều rỗng. Hồ sơ đã tự khai tên; danh mục mẫu admin là JSON. Task service vẫn cập nhật hàng đợi cũ nhưng không tạo hàng mới; chuông/worker thực tế dùng `thong_bao`.
- [Migration mới](../../backend/StudyPlanner.Infrastructure/Migrations/20261115080829_DonBangToChucVaLichNhacCu.cs): khóa bảng, từ chối bỏ Pending/Failed hoặc Delivered không có hộp thư tương ứng, backfill tên hồ sơ thiếu, rồi gỡ năm bảng và hai cột liên kết. Giữ toàn bộ bảng nghiệp vụ còn lại, không DROP CASCADE tùy tiện, không sửa migration cũ. Down từ chối phục hồi các bảng rỗng gây hiểu nhầm đã phục hồi dữ liệu.
- Gỡ entity/config của `Truong`, `Khoa`, `KhoaHoc`, `LopSinhVien`, `LichNhac` và enum trạng thái cũ. Cập nhật [DbContext](../../backend/StudyPlanner.Infrastructure/Data/ApplicationDbContext.cs), cấu hình người dùng/thông báo/lập kế hoạch, snapshot và `QuyTacThongBao`. [NhiemVuService](../../backend/StudyPlanner.Application/HocTap/NhiemVu/NhiemVuService.cs) bỏ truy vấn hàng đợi cũ, giữ giao dịch, lịch sử, hủy phiên và thông báo. Trường phản hồi `soLichNhac` giữ bằng 0 để không phá hợp đồng FE đã có.
- `AuthChecks` giữ kiểm thử nâng cấp bảng/cột lịch sử, dùng SQL độc lập entity đã gỡ, kiểm chứng migration từ chối hàng đợi chưa xử lý và giữ tên hồ sơ/thông báo đã đọc. `MigrationProgressChecks` dùng SQL cho lịch nhắc ở schema cũ và kiểm tra rename/downgrade trước khi dọn schema. `AdministrationChecks` bỏ fixture cần tổ chức cũ, vẫn kiểm tra hồ sơ tự khai; `SchemaChecks` xác minh 17 bảng/27 FK.

### Hoàn thiện tệp đính kèm

- Trước: chỉ có entity, FK, bộ lọc thùng rác và đếm ảnh hưởng xóa; chưa có tải tệp hoặc giao diện.
- [KhoTepNhiemVu](../../backend/StudyPlanner.Infrastructure/TepDinhKem/KhoTepNhiemVu.cs): kho riêng, khóa ngẫu nhiên 32 ký tự hex, kiểm tra độ dài thực của luồng, không dùng tên client làm đường dẫn. [TepDinhKemService](../../backend/StudyPlanner.Infrastructure/TepDinhKem/TepDinhKemService.cs) kiểm tra chủ, phiên bản và giới hạn; khóa tài khoản cùng cơ chế nhiệm vụ để tránh cạnh tranh xóa/sửa.
- [Controller](../../backend/StudyPlanner.Api/Controllers/TepDinhKemController.cs): bốn route danh sách/tải lên/tải xuống/xóa; API tải xuống trả attachment + octet-stream + nosniff + no-store. `Program.cs` đăng ký service/kho/worker, cấu hình đường dẫn ngoài wwwroot.
- [Worker dọn kho](../../backend/StudyPlanner.Api/HocTap/DonKhoTepWorker.cs): giữ cả tệp trong thùng rác; mỗi giờ xóa nội dung không có thông tin trong DB và đã cũ 24 giờ. Chụp danh sách tệp cũ trước khi đọc DB; mất kết nối DB thì không xóa. Thao tác xóa tệp trực tiếp ưu tiên xóa DB, lỗi dọn vật lý sẽ thử lại.
- FE repo `Study_Planner_FE`: `src/hoc-tap/TepDinhKemNhiemVu.tsx`, `tep-dinh-kem.css`, gắn trong `ChiTietNhiemVu` của `NhiemVuView.tsx` nên dùng cả ở Tập trung. Có danh sách, tải lên, tải xuống bằng JWT, xác nhận xóa, lỗi/thử lại, giới hạn, khóa thao tác lúc đang xử lý và tải mới khi revision đổi. `api.ts` hỗ trợ FormData không ép Content-Type JSON; giữ JWT/If-Match/xử lý lỗi cũ. Thêm unit và `tests/browser-tep-dinh-kem.mjs`, mở rộng fixture của `browser-hoc-tap.mjs`.
- Tệp không thay đổi tiến độ/checklist. Không có dữ liệu tệp thử được đưa vào DB ứng dụng.

## Kiểm chứng

- Database nguồn PostgreSQL 18.7. Backup mới: `backend/obj/database-backups/study-planner-before-upgrade-20261115-081729.dump`, **119.281 byte**, **23 mục TABLE DATA** gồm lịch sử migration. Bản dump nằm trong thư mục bị Git bỏ qua, không đưa vào source.
- Khôi phục vào PostgreSQL 18 cục bộ riêng trước khi áp dụng. Dump nguồn có annotation cột `pg_attver` mà PostgreSQL chuẩn không nhận; giữ nguyên dump, chỉ lọc đúng câu ALTER COLUMN này trong phần **pre-data** để diễn tập. Phần COPY dữ liệu và post-data phục hồi trực tiếp, không qua bộ lọc. Database diễn tập được xóa sau kiểm tra.
- Diễn tập và áp dụng thật đều đối chiếu hash của mọi bản ghi thuộc 17 bảng còn giữ, chỉ loại hai cột được gỡ khỏi phép so. Số bản ghi và mọi giá trị còn giữ không đổi. Không dùng hash bỏ qua tiến độ hoặc nội dung nhiệm vụ. Đã áp dụng `20261115080829_DonBangToChucVaLichNhacCu` vào DB Development; xác minh **17 bảng, 27 FK, không còn 5 bảng/2 cột cũ, không migration pending**.
- Giữ đủ 10 tài khoản, 57 nhiệm vụ, 366 lịch sử, 21 thông báo, 12 học kỳ, 27 môn, 5 buổi học, 3 giờ rảnh, 3 lần áp dụng, 6 phiên tự học, 4 phiên tập trung, 5 khoảng thực học. Bảng ngoại lệ/cấu hình nhắc/nhắc kế hoạch/tệp hiện rỗng vì chưa có dữ liệu người dùng cho chúng.
- Build solution Debug và SchemaChecks đạt, 0 cảnh báo/lỗi. Core: 30 kiểm tra nâng cấp, 561 HTTP nghiệp vụ, 39 giờ rảnh, 38 cấu hình/lập lại lịch, 47 nhắc kế hoạch và 121 tập trung đạt; client FE thật → HTTP → PostgreSQL đạt. Lượt Core đầy đủ ban đầu dừng ở cách gửi body vượt request limit bị Kestrel đóng kết nối; đã sửa bài thử riêng thành tệp 10 MB + 1 byte để xác minh giới hạn dịch vụ và chạy lại riêng.
- `AttachmentHttpChecks`: **47** kiểm tra đạt, bao gồm JWT/chủ khác, If-Match thiếu/cũ, tệp rỗng/quá cỡ, giữ từng byte, tên đường dẫn, 20 tệp, thùng rác/khôi phục, cascade, dọn mồ côi. DB test localhost riêng, luôn dọn sau test. Auth **367** và Administration **162** kiểm tra PostgreSQL/HTTP đạt.
- FE: **168/168 unit**, TypeScript/Vite build đạt; còn cảnh báo bundle hơn 500 KB. Chrome headless đạt tải lên multipart, F5, tải xuống có JWT, lỗi/thử lại, xác nhận xóa, không gửi tệp quá cỡ và kiểm tra chiều rộng 390 px. Đã xem ảnh chụp giao diện. Browser dùng fixture, không coi là thử GUI trên tài khoản thật.
- Build Release đạt 0 cảnh báo/lỗi; EF `has-pending-model-changes` xác nhận snapshot khớp model. Bộ thông báo đạt 13 kiểm tra quy tắc và HTTP/JWT/PostgreSQL về quyền, phiên bản đọc, cạnh tranh, lưu trạng thái, hạn kế thừa và cutoff đọc tất cả. Chrome hồi quy Tập trung đạt 282 yêu cầu fixture, bao gồm sửa nhiệm vụ trong trang Tập trung. Sau giới hạn worker chỉ dọn tệp có khóa hệ thống, chạy lại riêng 47 kiểm tra tệp và đạt.
- Đã đọc lại diff về DbContext/FK/task service/migration, toàn bộ code kho/service/controller/worker và diff FE. `git diff --check` đạt ở cả hai repo; cảnh báo LF/CRLF là quy đổi dòng theo cấu hình Git.

## Việc tiếp theo

- Khởi động lại BE và tải lại FE để dùng code mới; API cũ vẫn giữ entity/cột đã gỡ nên cần nạp lại. Không tự dừng phiên BE của người dùng.
- Khi triển khai giữ và sao lưu kho tệp cùng DB. Chưa triển khai xem trước/quét mã độc/phiên bản tệp; xem [báo cáo bảng và phạm vi đầy đủ](../database-tables.md).
- Không xóa thêm bảng chỉ vì chưa có dòng dữ liệu; mọi bảng còn lại đã có luồng sử dụng. Không tự commit/push hoặc tự nhận người dùng đã duyệt.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR: chưa tạo.
