# Danh mục môn học & học kỳ

Trang quản trị `/admin/catalog` cung cấp hai danh mục dùng chung: học kỳ mẫu và môn học mẫu có số tín chỉ. Đây là mẫu do admin chuẩn bị, độc lập với `HocKy`/`MonHoc` thuộc từng sinh viên. Sinh viên có thể chọn môn mẫu trong ô Tên môn học của form thêm môn để điền tên, mã và tín chỉ; dữ liệu chỉ được lưu vào học kỳ cá nhân khi bấm Tạo môn học. Chưa có thao tác sao chép cả học kỳ mẫu.

## Chức năng

- Môn học mẫu: thêm/sửa/xóa, tìm theo tên/mã, lọc học kỳ và phân trang 20 dòng. Bộ lọc và trang nằm trong URL nên giữ khi tải lại.
- Học kỳ mẫu: thêm/sửa tên và thứ tự hiển thị, xem số môn/tổng tín chỉ, mở danh sách môn của kỳ đó. Xóa bị chặn nếu còn môn, cần chuyển hoặc xóa môn trước.
- Form giữ dữ liệu khi lưu lỗi; có xác nhận xóa, trạng thái đang tải/lỗi/thử lại và danh sách rỗng. Không âm thầm dùng mock khi API không hoạt động.
- Chỉ tài khoản Admin có quyền gọi API quản trị, không chỉ ẩn menu ở frontend. API gợi ý riêng bên dưới cho phép sinh viên đọc. Demo/mock auth không được dùng để ghi danh mục thật.

## Gợi ý trong form thêm môn

Sinh viên bấm/focus ô Tên môn học sẽ mở danh sách gợi ý từ danh mục mẫu. Gõ tên/mã để tìm kiếm, có Xem thêm khi nhiều hơn 20 kết quả. Chọn bằng chuột hoặc phím lên/xuống và Enter; Escape hoặc chuyển ra ngoài đóng danh sách. Giao diện hiển thị tên, mã, tín chỉ và học kỳ mẫu để phân biệt các mẫu có cùng tên.

Chọn mẫu chỉ điền tên/mã/tín chỉ; học kỳ cá nhân, giảng viên và màu nhận diện không đổi. Sinh viên vẫn chỉnh thông tin hoặc tự nhập môn ngoài danh mục. Không có gợi ý/không kết nối được nguồn mẫu không khóa việc tự nhập. Form sửa môn hiện có tiếp tục dùng thông tin riêng đã lưu và không tự áp mẫu.

Nguồn đọc riêng: `GET /api/v1/catalog/courses?tuKhoa=...&trang=1`, yêu cầu đăng nhập, cho phép sinh viên. Response phân trang 20 dòng gồm mã mẫu, mã/tên học kỳ mẫu, ký hiệu, tên và tín chỉ; không trả phiên bản quản trị. Endpoint này không có thao tác ghi. Các route `/admin/catalog` vẫn giữ quyền Admin cho cả đọc/ghi.

Khi lưu, FE tiếp tục gọi API thêm môn cá nhân hiện có bằng JWT và các giá trị đã điền. Không gửi ID mẫu và không tạo FK đến danh mục; admin sửa/xóa mẫu không làm thay đổi môn cá nhân đã lưu. Chi tiết: [bản ghi nối gợi ý](../history/2026-11-13-goi-y-mon-mau-khi-them-mon.md).

## Dữ liệu và quy tắc

Danh mục hiện lưu ở `backend/StudyPlanner.Api/Administration/Data/danh-muc-mau.json`, nằm trong thư mục quản trị của backend, không dùng bảng riêng trong database. Có thể đổi nơi lưu bằng cấu hình `Catalog:FilePath` (nên dùng đường dẫn tuyệt đối đến thư mục được lưu bền vững).

File chứa schema phiên bản 1, hai bộ đếm ID tiếp theo, mảng `hocKy` và mảng `monHoc`. Học kỳ có tên, thứ tự 1–100 và phiên bản; tên tối đa 200 ký tự, không trắng hoặc trùng sau trim/đổi hoa. Môn có học kỳ mẫu, ký hiệu, tên, tín chỉ và phiên bản. Ký hiệu 1–30 ký tự gồm chữ ASCII/số/chấm/gạch ngang/gạch dưới, lưu dạng chữ hoa; không trùng trong cùng học kỳ. Tín chỉ nguyên 1–30. Môn có thể chuyển kỳ khi sửa. Chỉ xóa kỳ khi không còn môn. Không có FK hoặc dữ liệu mẫu trong các bảng cá nhân của sinh viên.

Mọi lần đọc/ghi lấy khóa file `.lock` độc quyền trước khi đọc bản mới nhất. Sửa/xóa phải gửi `phienBan` vừa đọc; server kiểm tra sau khi lấy lock, trả 409 khi có thay đổi khác. Ghi vào file tạm cùng thư mục, flush rồi thay thế file đích nguyên tử; `.bak` giữ bản trước đó. Nếu file hiện có hỏng/thiếu schema, báo lỗi, không reset về danh mục mặc định. Bộ đếm lưu trong file ngăn dùng lại ID cao nhất đã xóa. Không sửa phiên bản lịch học/nhiệm vụ sinh viên.

## API

Tất cả nằm dưới `/api/v1/admin/catalog`, yêu cầu JWT với policy Admin.

| Method | Đường dẫn | Dữ liệu |
| --- | --- | --- |
| GET | `/semesters` | Mảng học kỳ theo thứ tự, kèm số môn/tổng tín chỉ |
| POST | `/semesters` | `{ ten, thuTu }`, trả 201 |
| PUT | `/semesters/{id}` | `{ ten, thuTu, phienBan }` |
| DELETE | `/semesters/{id}?phienBan=...` | 204 khi kỳ rỗng |
| GET | `/courses?maHocKyMau=...&tuKhoa=...&trang=1` | `{ danhSach, trang, kichThuocTrang: 20, tongSo }` |
| POST | `/courses` | `{ maHocKyMau, kyHieu, ten, soTinChi }`, trả 201 |
| PUT | `/courses/{id}` | Các trường trên và `phienBan` |
| DELETE | `/courses/{id}?phienBan=...` | 204 |

400: dữ liệu/bộ lọc sai hoặc thiếu phiên bản. 401/403: thiếu quyền. 404: mẫu không tồn tại. 409: trùng tên/mã, stale version hoặc kỳ còn môn.

## Dữ liệu ban đầu, sao lưu và triển khai

File `Administration/Data/danh-muc-mau.seed.json` đi cùng source/publish chứa 2 học kỳ và 12 môn tham khảo. Khi file runtime chưa tồn tại, hệ thống tạo từ seed dưới khóa; từ đó admin thay đổi file runtime qua API. Seed không ghi đè chỉnh sửa admin mỗi lần chạy/publish. File runtime `Administration/Data/danh-muc-mau.json`, file khóa, file tạm và thư mục `backups` không được đưa lên Git hoặc đóng gói publish, không nằm trong wwwroot. Khi triển khai phải giữ dữ liệu runtime hoặc mount volume bền vững, cấp quyền ghi cho tiến trình và sao lưu định kỳ; không dùng nhiều bản sao file độc lập trên nhiều máy.

Hai bảng mẫu của bản cũ đã được xuất, kiểm chứng và gỡ ngày 14/11/2026. Migration tạo hai bảng trước đó chưa commit, đã được rút khỏi source và hoàn nguyên đúng phần database của nó; model/snapshot trở lại 18 bảng nghiệp vụ. Cài mới không tạo bảng mẫu. Dữ liệu cá nhân của sinh viên vẫn dùng database như trước.

Máy khác từng áp dụng bản hai bảng cần chuyển trước khi chạy code JSON. Dừng backend trong terminal của mình, rồi từ gốc repository:

```powershell
dotnet run --project backend/tools/StudyPlanner.CatalogTransfer --configuration Catalog -- --export-only
dotnet run --project backend/tools/StudyPlanner.CatalogTransfer --configuration Catalog -- --apply
```

`--export-only` sao lưu dữ liệu, ID, phiên bản và bộ đếm sequence, không sửa DB. `--apply` xuất lại, so JSON với database trong khóa/giao dịch, tạo backup độc lập và chỉ sau khi kiểm chứng mới gỡ hai bảng và dòng lịch sử migration đã rút. Nếu JSON khác DB hoặc có migration mới hơn, từ chối, không ghi đè hoặc tự xóa dữ liệu. Chạy lại khi không còn hai bảng không làm đổi file. Dùng cấu hình database của API, không ghi thông tin bí mật vào file danh mục. Chi tiết: [bản ghi chuyển JSON](../history/2026-11-14-luu-danh-muc-mau-trong-json.md).

Đường dẫn ban đầu `App_Data` đã được chuyển sang `Administration/Data` theo yêu cầu gom dữ liệu trong phần admin; máy này đã chuyển cả file hiện có và backup, kiểm chứng hash giữ nguyên. Máy khác còn dữ liệu ở đường dẫn cũ cần chuyển dữ liệu/backup trước khi chạy bản mới, hoặc cấu hình `Catalog:FilePath` về file đã có, tránh để hệ thống hiểu là lần khởi tạo mới. Xem [bản ghi di chuyển](../history/2026-11-14-chuyen-json-vao-thu-muc-admin.md).

Sau cập nhật code cần chạy lại backend bằng terminal của người dùng. Không tự mở server chạy nền; không cần migration danh mục mới.

## Kiểm chứng

- `powershell.exe -NoProfile -File scripts/test-catalog.ps1`: JSON persistence/khóa/cancel/file hỏng/ghi lỗi, HTTP/JWT và xuất-gỡ bảng cũ. PostgreSQL chỉ phục vụ tài khoản/JWT và kiểm thử chuyển dữ liệu cũ, không lưu danh mục mới. Script dùng database ngẫu nhiên trên localhost và dọn database khi xong. `-PureOnly` kiểm thử validate và file JSON, không dùng PostgreSQL. Khi chưa cấu hình cụm test, script tự tạo PostgreSQL 17 cục bộ ở output riêng trên cổng 55443 rồi dừng đúng cụm do nó tạo.
- FE `node --test tests/catalog.test.mjs`, `npm.cmd run test:auth:browser -- --catalog`, `npm.cmd run build`.
- Chi tiết kết quả và giới hạn: [bản ghi triển khai](../history/2026-11-13-quan-ly-du-lieu-nen-tang.md).
