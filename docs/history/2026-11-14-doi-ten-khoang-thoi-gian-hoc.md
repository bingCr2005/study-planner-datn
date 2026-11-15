# 2026-11-14 — Đổi tên khoảng thời gian học

**Cập nhật sau triển khai 14/11/2026:** các migration liên quan đã được áp dụng vào database Development theo cấu hình dự án; xem [bản ghi cập nhật database](2026-11-14-ap-dung-migration-database.md). Các mục “chưa áp dụng” bên dưới phản ánh thời điểm trước lần triển khai này.

## Yêu cầu và mục đích

- Người dùng thấy “đoạn thực học” khó hiểu, chốt tên “Khoảng thời gian học” và yêu cầu đổi toàn bộ tên liên quan/ràng buộc.
- Đã đọc mục lục, [lịch sử đồng hồ](2026-11-12-man-dong-ho-tap-trung-rieng.md), [cải tiến lập kế hoạch](2026-11-14-cai-tien-lap-ke-hoach.md), entity, mapping, dịch vụ và kiểm tra hiện hành trước khi sửa. Nhánh thực hiện: `refactor/doi-ten-khoang-thoi-gian-hoc`; giữ các thay đổi chưa commit đang có.

## Code và logic thay đổi

- Đổi `DoanThucHoc.cs`/`DoanThucHoc` thành [KhoangThoiGianHoc.cs](../../backend/StudyPlanner.Domain/Entities/KhoangThoiGianHoc.cs)/`KhoangThoiGianHoc`; DbSet thành `DanhSachKhoangThoiGianHoc`. Một bản ghi vẫn là khoảng từ bắt đầu/tiếp tục đến tạm dừng/kết thúc, không gồm thời gian nghỉ.
- Cập nhật [ApplicationDbContext](../../backend/StudyPlanner.Infrastructure/Data/ApplicationDbContext.cs), [LapKeHoachConfiguration](../../backend/StudyPlanner.Infrastructure/Data/Configurations/LapKeHoachConfiguration.cs), truy vấn trong `NhiemVuService`, `PhienTuHocService`, `PhienTuHocResponse`, `TapTrungService`, `QuyTacAnhHuongLich`, comment enum trạng thái và kiểm tra Core/Auth/Schema. Hàm đọc/mở/đóng khoảng học đổi tên tương ứng; giữ API và cách tính thời gian.
- Tạo [20261114114703_DoiTenKhoangThoiGianHoc](../../backend/StudyPlanner.Infrastructure/Migrations/20261114114703_DoiTenKhoangThoiGianHoc.cs), Designer và cập nhật snapshot. Thay bản sinh tự động drop/create bằng đổi tên tại chỗ: bảng, PK, bốn FK, CHECK, bốn index thường và sequence identity. PostgreSQL tự đổi index của PK theo tên constraint. Tổng bảng/FK vẫn 22/34.
- Up: `doan_thuc_hoc` → `khoang_thoi_gian_hoc`; Down đổi ngược toàn bộ tên, không xóa dữ liệu. Giữ nguyên mã, các mốc thời gian, liên kết, giá trị sequence, bộ lọc unique một khoảng đang chạy và hành vi xóa liên quan.
- Không sửa tên trong các migration cũ vì chúng mô tả schema tại thời điểm cũ. Tên cũ còn trong migration chuyển đổi/kiểm thử nâng cấp là có chủ đích. Liên kết entity trong bản ghi đồng hồ cũ được trỏ tới file mới, giữ nội dung lịch sử.
- Cập nhật [module đồng hồ](../modules/09-dong-ho-tap-trung.md), [ERD](../erd.md), bản ghi này và mục lục lịch sử. Không thay FE vì không có trường API hoặc bảng FE nào dùng tên entity này.

## Kiểm chứng

- Đọc lại các chỗ tham chiếu, model snapshot, Up/Down và tên ràng buộc PostgreSQL. Build CoreChecks Release đạt 0 cảnh báo/0 lỗi.
- SchemaChecks đạt; bổ sung kiểm tra cả Up/Down chỉ gồm đổi tên bảng/index và SQL ALTER RENAME, không drop/create bảng.
- PostgreSQL kiểm thử riêng: tạo dữ liệu ở schema trước đổi tên, giữ khoảng đã đóng và đang mở, kiểm tra đủ 6 constraint/5 index dùng tên mới, sequence đúng tên, Down/Up giữ mã bản ghi, thêm mới tiếp nối identity. Nhóm nâng cấp schema cũ tăng từ 23 lên **30 kiểm tra đạt**.
- Toàn CoreChecks đạt lại: quy tắc thời gian/cây/phiên/đồng hồ/cấu hình, 561 HTTP nghiệp vụ, 39 giờ rảnh, 38 cấu hình/lập lại lịch, 31 nhắc kế hoạch và 121 đồng hồ. Client nhiệm vụ FE thật → HTTP API → PostgreSQL đạt. Dữ liệu thử nằm trong DB tạm được bộ kiểm tra tạo/xóa.
- Chưa áp dụng migration vào database người dùng, chưa thử trực tiếp tài khoản người dùng. Không commit/push/triển khai.

## Việc tiếp theo

- Áp dụng migration theo thứ tự cùng bản BE mới khi nâng cấp môi trường ứng dụng. Giữ các migration cũ để nâng cấp được database hiện hữu.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
