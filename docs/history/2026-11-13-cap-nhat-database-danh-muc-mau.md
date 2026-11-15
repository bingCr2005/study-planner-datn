# 2026-11-13 — Cập nhật database để trang Dữ liệu nền tảng hoạt động

## Yêu cầu và mục đích

- Người dùng mở `/admin/catalog` và báo “Máy chủ đang gặp sự cố. Vui lòng thử lại sau.”
- Đã đọc mục lục và [bản ghi triển khai danh mục](2026-11-13-quan-ly-du-lieu-nen-tang.md). Bản triển khai trước chuẩn bị đầy đủ code/migration nhưng mới áp dụng migration trên database kiểm thử, chưa cập nhật database ứng dụng.

## Code và logic thay đổi

- Không sửa code frontend/backend trong lượt này; cập nhật database ứng dụng bằng migration đã có `20261113102702_ThemDanhMucHocTapMau`, sau khi được cấp quyền thực thi thao tác.
- Trước: kiểm tra `migrations list` trên database cấu hình của backend cho thấy duy nhất migration danh mục còn Pending. Trang có route nhưng truy vấn hai bảng chưa tồn tại nên không tải được danh mục.
- Sau: tạo `hoc_ky_mau`/`mon_hoc_mau`, constraints/index/FK Restrict và 2 học kỳ/12 môn tham khảo; cập nhật sequence và lịch sử migration. Không chạy Down, không sửa bảng dữ liệu học tập của sinh viên.
- Luồng: xác nhận migration chờ bằng kiểm tra chỉ đọc → áp dụng đúng migration → kiểm tra lịch sử migration → chạy truy vấn service danh mục bằng kết nối READ ONLY → hướng dẫn tải lại trang.
- [Module 09](../modules/09-du-lieu-nen-tang.md) cập nhật trạng thái database thực tế. Bản ghi triển khai ban đầu giữ nguyên trạng thái lịch sử tại thời điểm đó.

## Kiểm chứng

- Backend ở localhost:5059 truy cập được Swagger; cả hai route `/api/v1/admin/catalog/semesters` và `/courses` đã có trong phiên backend đang chạy. Request không JWT qua proxy FE nhận 401, không phải route thiếu; không dùng token hay đăng nhập thay người dùng.
- `dotnet-ef migrations list --configuration Catalog --no-build`: trước có migration danh mục Pending; sau khi cập nhật không còn Pending.
- `dotnet-ef database update 20261113102702_ThemDanhMucHocTapMau --project backend/StudyPlanner.Infrastructure --startup-project backend/StudyPlanner.Api --configuration Catalog --no-build`: hoàn tất, log chỉ áp dụng migration danh mục mới.
- Helper kiểm chứng tạm trong `backend/obj/catalog-live-check` build đạt, 0 cảnh báo/lỗi. Chỉ gọi `DuLieuNenTangService.HocKyAsync`, `MonHocAsync` và đọc trạng thái migration; ép `default_transaction_read_only=on`. Kết quả: 2 học kỳ, 12 môn, không còn migration chờ. Không in thông tin kết nối hoặc bí mật.
- Không cần khởi động lại backend vì route mới đã được nạp; không dừng tiến trình người dùng. Chưa kiểm tra trực tiếp thao tác trên phiên trình duyệt đăng nhập của người dùng; kiểm chứng trên database thực tế và service đọc dữ liệu đã đạt.
- Không chạy lại toàn bộ unit/integration tests vì không thay đổi code. Các kết quả kiểm thử chức năng ở bản ghi triển khai trước vẫn giữ đúng phạm vi của chúng.

## Việc tiếp theo

- Người dùng bấm Thử lại hoặc F5 để giao diện tải danh mục từ database đã cập nhật.
- Chưa commit/push.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với migration, kết quả truy vấn và trạng thái runtime.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
