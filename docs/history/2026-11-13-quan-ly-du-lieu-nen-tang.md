# 2026-11-13 — Admin quản lý dữ liệu nền tảng

## Yêu cầu và mục đích

- Người dùng yêu cầu thêm trang admin quản lý học kỳ và môn học mẫu, có danh mục môn phổ biến và tín chỉ nhập sẵn.
- Đã đọc README/TEMPLATE lịch sử, [API học kỳ/môn](2026-11-04-api-hoc-ky-mon-hoc.md), [quản trị quy tắc](2026-11-12-quan-ly-quy-tac-uu-tien.md), module 02/08, entity/configuration/API và các trang admin hiện tại.
- Trước: `HocKy` thuộc sinh viên, `MonHoc` thuộc học kỳ cá nhân; không có danh mục học kỳ/môn dùng chung. Không dùng tài khoản admin làm chủ dữ liệu mẫu hoặc trộn mẫu vào môn của sinh viên.
- Tạo nhánh `feat/quan-ly-du-lieu-nen-tang` ở hai repo. Giữ các chỉnh sửa Tiến độ có sẵn chưa commit từ [tác vụ trước](2026-11-13-sua-tien-do-hoc-ky-co-nhiem-vu-con.md); không tự commit/push.

## Code và logic thay đổi

- Domain `HocKyMau`, `MonHocMau` và Infrastructure `DuLieuNenTangConfiguration`: hai bảng độc lập `hoc_ky_mau`/`mon_hoc_mau`, FK Restrict, unique tên kỳ chuẩn hóa và mã môn trong cùng kỳ, kiểm tra thứ tự 1–100 và tín chỉ nguyên 1–30. Không FK đến người dùng, nhiệm vụ hoặc dữ liệu học tập cá nhân.
- Application `NenTang/DanhMucMau.cs`: DTO và validate tên/mã/tín chỉ; ký hiệu được trim/đổi hoa, chỉ chữ ASCII/số/chấm/gạch ngang/gạch dưới, dài tối đa 30. Tên tối đa 200, không trắng.
- Infrastructure `NenTang/DuLieuNenTangService.cs`: đọc học kỳ kèm số môn/tổng tín chỉ; môn có tìm kiếm/lọc/phân trang 20 dòng, tổng và trang cùng snapshot RepeatableRead. Ghi dùng transaction + advisory lock chung, xác minh phiên bản sau khi nhận lock. Xóa học kỳ còn môn trả 409. Cập nhật môn có thể chuyển học kỳ mẫu, vẫn kiểm tra trùng mã ở kỳ đích.
- API `Administration/DuLieuNenTangController.cs`, `Program.cs`: CRUD dưới `/api/v1/admin/catalog`, bảo vệ bằng policy Admin; không nhận quyền/owner từ body. Sai dữ liệu 400, thiếu quyền 401/403, mẫu không tồn tại 404, trùng/stale/kỳ còn môn 409.
- Migration `20261113102702_ThemDanhMucHocTapMau` và snapshot: chỉ thêm hai bảng, nạp 2 học kỳ và 12 môn tham khảo với tín chỉ có thể sửa, cập nhật identity sequence qua ID seed. Đây không phải chương trình chuẩn của một trường. Chạy migration lần đầu mới nạp dữ liệu, không reset sửa đổi của admin khi khởi động ứng dụng. Down chỉ xóa hai bảng mẫu theo đúng thứ tự FK.
- `.config/dotnet-tools.json`: cố định công cụ dotnet-ef 10.0.12 để người khác có thể restore và áp dụng migration. Không tự chạy migration trong Program.
- FE repo `D:/study-planner-frontend`: `src/admin/AdminCatalogPage.tsx`, `catalog.css`, `src/api/catalog.ts`, `AdminRoutes.tsx`, `AdminLayout.tsx`. Trang `/admin/catalog` thêm vào menu quản trị, breadcrumb đúng tiêu đề; hai tab học kỳ/môn, tìm kiếm/lọc/phân trang giữ URL, form thêm/sửa, xác nhận xóa, lỗi giữ nội dung form, loading/retry/empty state. Khi kỳ lọc không còn tồn tại, bỏ lọc đã lỗi thời. Demo/mock không được ghi dữ liệu thật; không dựng dữ liệu giả để che lỗi API.
- FE `tests/catalog.test.mjs`, `tests/browser-catalog.mjs` và harness `browser-auth.mjs`: kiểm thử hợp đồng CRUD, dữ liệu không hợp lệ, versions, quyền, giao diện, tìm kiếm/filter/F5, lưu lỗi, xóa an toàn. Cập nhật kỳ vọng menu có trang mới. Khi chạy toàn bộ suite, phát hiện test SSR Tiến độ cũ mong “Chưa có dữ liệu thống kê” mặc dù fixture không có học kỳ; đối chiếu code cả HEAD xác nhận empty-state đúng là “Chưa có học kỳ”, chỉ sửa kỳ vọng trong `tests/planner.test.mjs`, không đổi trang Tiến độ ở tác vụ này.
- `StudyPlanner.CatalogChecks`, `scripts/test-catalog.ps1`, solution: kiểm thử HTTP/PostgreSQL riêng. SchemaChecks cập nhật 20 bảng/28 FK và xác minh mẫu độc lập; sửa kỳ vọng cột cũ `ChecklistJson` để chấp nhận chính xác tên `checklist` đã có từ main (không đổi schema checklist).
- Tài liệu [module 09](../modules/09-du-lieu-nen-tang.md), mục lục module, module 08 và README frontend bổ sung chức năng, ràng buộc, route và hướng dẫn migration. Trạng thái dự thảo cũ của các danh mục trường/khoa/lớp không được hiểu là đã triển khai bởi trang này.

Luồng: JWT Admin → tải danh mục chung → lọc/chọn mẫu → validate form → API kiểm tra quyền/đầu vào → transaction/lock → kiểm tra phiên bản, FK và unique → lưu DB → FE tải lại số liệu/danh sách. Không phát sinh học kỳ/môn của sinh viên hoặc tăng phiên bản lịch của họ.

## Kiểm chứng

- Build toàn solution vào `backend/obj/catalog-solution/bin/` với `--no-restore`, `CopyRetryCount=0`: đạt, 0 cảnh báo, 0 lỗi; không ghi đè DLL của backend đang chạy.
- SchemaChecks chạy từ output riêng: đạt toàn bộ, có 20 bảng/28 FK, không shadow property, FK mẫu Restrict và độc lập bảng sinh viên. Kiểm tra metadata này không mở kết nối DB.
- `scripts/test-catalog.ps1`: đạt toàn bộ kiểm tra validate và PostgreSQL/HTTP. Tự tạo cụm PostgreSQL 17 trong thư mục obj riêng trên localhost:55443, database ngẫu nhiên; chạy migration thật, xác minh seed và snapshot khớp model; JWT anonymous/student bị chặn; CRUD/persistence, trùng tên/mã, kỳ thiếu, tín chỉ 0, thiếu/stale version, hai admin cập nhật đồng thời, xóa kỳ còn môn, tìm kiếm và trang hai đều đạt. Kiểm tra không có dữ liệu cá nhân được tạo/sửa; database tạm đã dọn và PostgreSQL do script khởi động đã dừng.
- FE `npm.cmd test`: 148/148 đạt sau khi cập nhật kỳ vọng empty-state cũ. Trong lần chạy đầu có một test SSR Tiến độ lệch kỳ vọng như mô tả trên; không che kết quả thất bại đó bằng cách bỏ test.
- FE build TypeScript/Vite đạt. Có cảnh báo kích thước chunk chính trên 500 kB; không có lỗi build. Oxlint bốn file mới đạt, không cảnh báo sau khi chỉnh vòng đời effect/dependencies.
- Chrome `--catalog`: đạt CRUD, tìm kiếm, lọc, phân trang/F5, lỗi lưu giữ form, mã trùng, phiên bản cũ, xóa có hủy, chặn xóa học kỳ còn môn, retry, danh mục rỗng và viewport mobile không tràn. Những lần đầu cần sửa điều kiện chờ trong test để chờ kết quả lọc/empty-state thay vì bắt trúng DOM trước khi fetch xong; lượt cuối đạt.
- Chrome auth/admin đầy đủ: cả lượt fixture API và mock persistence đạt, menu mới không làm hỏng luồng đăng nhập/quản trị cũ. Đã xem ảnh desktop `.backend-integration/screenshots/admin-catalog.png` ở FE; ảnh dùng fixture kiểm thử, không phải dữ liệu thật.
- Đã đọc lại diff/migration/code và chạy kiểm tra khoảng trắng ở hai repo. Không áp dụng migration lên DB ứng dụng, không khởi động lại backend người dùng, không đăng nhập hay ghi dữ liệu vào phiên/database thật.

## Việc tiếp theo

- Để dùng trang với DB ứng dụng: restore công cụ, áp dụng migration rồi khởi động lại backend; lệnh trong module 09. Dữ liệu mẫu mới chỉ đã được nạp trên database kiểm thử.
- Trang này quản lý danh mục mẫu. Luồng sinh viên chọn/sao chép mẫu vào học kỳ cá nhân chưa được yêu cầu và chưa triển khai.
- Chưa commit/push hoặc triển khai.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
