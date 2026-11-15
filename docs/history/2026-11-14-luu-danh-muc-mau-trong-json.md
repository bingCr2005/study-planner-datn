# 2026-11-14 — Lưu danh mục mẫu trong JSON, không thêm bảng

## Yêu cầu và mục đích

- Người dùng chọn lưu học kỳ/môn mẫu trong JSON thay vì thêm bảng. Giữ trang admin và gợi ý trong ô tên môn.
- Đã đọc AGENTS, mục lục, TEMPLATE, [triển khai danh mục](2026-11-13-quan-ly-du-lieu-nen-tang.md), [cập nhật DB](2026-11-13-cap-nhat-database-danh-muc-mau.md), [gợi ý môn](2026-11-13-goi-y-mon-mau-khi-them-mon.md), module 09 và code hiện tại. Tiếp tục nhánh cùng chức năng `feat/quan-ly-du-lieu-nen-tang`; giữ các thay đổi chưa commit khác.

## Code và logic thay đổi

- Trước: `DuLieuNenTangService` đọc/ghi hai bảng qua EF, transaction và advisory lock; migration tạo bảng đã áp dụng trên DB ứng dụng nhưng source chưa commit. Frontend dùng API danh mục admin và gợi ý đã có.
- Sau: `Infrastructure/NenTang/DanhMucJsonStore.cs` lưu schema, bộ đếm ID, mảng học kỳ/môn trong file. `DuLieuNenTangService.cs` giữ DTO, validate, unique, phiên bản, tìm kiếm/phân trang và chặn xóa kỳ có môn; xử lý trong khóa file, không truy cập DB.
- `Program.cs` đăng ký store singleton với đường dẫn mặc định `App_Data/danh-muc-mau.json`, cấu hình thay được qua `Catalog:FilePath`; API/quyền và contract frontend không đổi.
- `Data/danh-muc-mau.seed.json` có 2 học kỳ/12 môn tham khảo. Chỉ dùng khi chưa có file runtime. `.csproj` đóng gói seed, loại App_Data khỏi output/publish; `.gitignore` loại file runtime/backup khỏi Git. File không phục vụ công khai qua wwwroot và không chứa cấu hình kết nối.
- Mọi đọc/ghi lấy OS file lock độc quyền chung đường dẫn, kể cả nhiều service instance/process cùng máy. Đọc file mới nhất dưới lock → kiểm tra version → thay đổi bản trong bộ nhớ → validate toàn bộ → ghi file tạm cùng thư mục, flush đĩa → File.Replace và giữ `.bak`. Lỗi ghi/cancel không thay thế file cũ; file hỏng/thiếu schema không bị reset về seed. Hai bộ đếm tăng đơn điệu ngăn tái sử dụng ID đã xóa.
- Rút entity/configuration/migration tạo bảng mẫu chưa commit; snapshot khôi phục đúng model trước phần danh mục. SchemaChecks trở lại 18 bảng/27 FK, giữ xác minh mapping checklist đã sửa. Các file lịch sử 13/11 giữ nguyên sự kiện tại thời điểm triển khai; module 09/mục lục module cập nhật theo code hiện tại.
- `backend/tools/StudyPlanner.CatalogTransfer`: công cụ chuyển dữ liệu đã tồn tại. Khóa file, khóa advisory của luồng DB cũ và hai bảng/lịch sử migration; yêu cầu migration danh mục cũ là mới nhất; xuất toàn bộ hàng, ID/version và high-water mark sequence. Tạo backup độc lập, ghi JSON và đọc/so lại cả hai bản trước khi gỡ bảng. Nếu JSON hiện có khác DB, bảng thiếu một phần hoặc migration mới hơn thì từ chối. Không dùng CASCADE; chỉ gỡ `mon_hoc_mau`, `hoc_ky_mau` và dòng lịch sử migration chưa commit tương ứng, cùng một DB transaction. Lỗi rollback DB, file export vẫn là bản sao có thể dùng đối chiếu. Không thay các bảng cá nhân của sinh viên.
- `StudyPlanner.CatalogChecks`: thêm JSON và transfer checks; HTTP/JWT kiểm thử với JSON thực, DB riêng chỉ chứa tài khoản. `scripts/test-catalog.ps1 -PureOnly` chạy validate/file checks không cần PostgreSQL.

Luồng mới: JWT → API danh mục hiện có → khóa file → đọc/lọc hoặc kiểm tra phiên bản và ghi nguyên tử → trả DTO như trước. Chọn mẫu trong form sinh viên vẫn chỉ tự điền, API thêm môn cá nhân tiếp tục lưu database, không liên kết đến JSON.

## Kiểm chứng

- Build toàn solution vào output riêng `backend/obj/json-catalog-check/bin/`: đạt, 0 cảnh báo/lỗi. Không ghi đè DLL của API người dùng đang chạy. SchemaChecks đạt: đúng 18 bảng/27 FK, không model/bảng mẫu mới và không pending model changes trên database test.
- File checks đạt: tạo seed một lần, 20 ghi đồng thời qua store độc lập không mất dữ liệu/trùng ID, hai cập nhật cùng version chỉ một thành công, ID không tái dùng sau xóa/restart, backup, dọn file tạm, cancel, JSON hỏng/thiếu trường, và mô phỏng lỗi File.Replace giữ file cũ.
- `scripts/test-catalog.ps1`: validate, JSON thực và HTTP/JWT/PostgreSQL riêng đều đạt; quyền admin/student, CRUD/persistence, unique, chặn xóa kỳ có môn, stale version, tìm kiếm/phân trang và endpoint gợi ý không đổi. Database test được dọn, PostgreSQL do script tạo được dừng.
- TransferChecks đạt trên DB riêng với tên/tín chỉ/version đã chỉnh và sequence cao hơn ID hiện có: export không gỡ bảng, JSON khác thì từ chối không mất dữ liệu DB, export verified rồi gỡ đúng hai bảng, giữ tài khoản, hoàn nguyên đúng dòng migration, chạy lại an toàn.
- Publish API đạt; xác minh seed nằm trong output và không đóng gói App_Data runtime. `git diff --check` đạt; đã đọc lại diff/store/service/công cụ chuyển.
- Không đổi frontend. Lần thử lại Chrome `--catalog` dừng ở điều kiện chờ modal khi chuyển tab/lọc: nút thêm bị click trong lúc tải, bài test không mở được modal. Không coi bài trình duyệt này là đạt và không sửa hành vi frontend ngoài yêu cầu lưu trữ. Kết quả HTTP admin trên JSON thực đạt như trên. Chrome `--course-suggestions` riêng đạt luồng gợi ý/chọn/tự điền/lưu, sửa môn, tự nhập, lỗi/retry, request cũ và mobile; bài browser dùng fixture.
- Đã xuất DB ứng dụng bằng `--export-only`, kiểm chứng đủ 2 học kỳ/12 môn và backup. Sau khi công cụ đã qua test và được cấp quyền, `--apply` xuất/so lại thành công rồi gỡ đúng hai bảng. Kiểm tra lại `--export-only` trả không còn bảng cũ và giữ JSON nguyên vẹn. File runtime hiện có 2 học kỳ/12 môn và hai backup export tại `App_Data/backups/`.
- Không đăng nhập thay người dùng hoặc tạo/sửa dữ liệu học tập thật. Không dừng/khởi động API hiện tại hoặc mở server nền. Phiên API cũ cần người dùng chạy lại để nạp bản JSON.

## Việc tiếp theo

- Trong terminal backend: Ctrl+C phiên cũ, chạy lại `dotnet run --project StudyPlanner.Api` để chuyển runtime sang JSON. Nếu chỉ F5 frontend trong khi API cũ còn chạy, API cũ vẫn tìm hai bảng đã gỡ và sẽ báo lỗi danh mục.
- Khi triển khai giữ App_Data hoặc cấu hình đường dẫn/volume bền vững, sao lưu file định kỳ. Khóa file bảo vệ cùng file dùng chung, không tự đồng bộ các file ở nhiều máy khác nhau.
- Chưa commit/push; phần người dùng đọc/đối chiếu do người dùng xác nhận.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR: chưa tạo.
