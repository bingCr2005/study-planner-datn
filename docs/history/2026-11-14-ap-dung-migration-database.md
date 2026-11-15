# 2026-11-14 — Áp dụng migration vào database Development

## Yêu cầu và mục đích

- Người dùng hỏi phần migration chưa áp dụng và cho phép: “nếu thấy cái nào hợp lý cần được làm ngay thì cứ chạy db đi”. Đã kiểm tra cấu hình Development thực tế, lịch sử migration, [cải tiến kế hoạch](2026-11-14-cai-tien-lap-ke-hoach.md), [đổi tên khoảng học](2026-11-14-doi-ten-khoang-thoi-gian-hoc.md) và [cấu hình thông báo](2026-11-14-gop-thiet-lap-vao-thong-bao.md) trước khi ghi database.
- Database cấu hình là PostgreSQL 18.7; trước lượt này đã có migration đồng hồ, còn thiếu ba migration về tiến độ/kế hoạch/đổi tên. Thực hiện trên nhánh `feat/thong-bao-va-cau-hinh-nhac`, không commit/push/triển khai ứng dụng.

## Code và logic thay đổi

- Đã áp dụng thực tế:
  1. `20261113010000_TongHopTienDoNhiemVu`: tổng hợp tiến độ từ các con, giữ checklist của cha thành nhiệm vụ con.
  2. `20261114092625_CaiTienLapKeHoach`: thêm `ngoai_le_gio_ranh`, `cau_hinh_nhac_ke_hoach`, `nhac_ke_hoach`; thêm ba cột học/nghỉ/hạn mức ngày của `nguoi_dung`.
  3. `20261114114703_DoiTenKhoangThoiGianHoc`: đổi tên bảng/PK/FK/CHECK/index/sequence tại chỗ, giữ dữ liệu.
  4. [20261114122238_DoiTenRangBuocKhongNullKhoangHoc](../../backend/StudyPlanner.Infrastructure/Migrations/20261114122238_DoiTenRangBuocKhongNullKhoangHoc.cs): migration bổ sung phát hiện khi kiểm tra PostgreSQL 18 thực tế.
- PostgreSQL 18 có bốn constraint NOT NULL tự sinh còn mang tiền tố `doan_thuc_hoc` sau đổi tên bảng. Migration bổ sung chỉ đổi tên các constraint loại `n` khi tồn tại; trên PostgreSQL 17 không có các dòng tương ứng nên không đổi gì. Down đổi ngược tên; không sửa cột hay dữ liệu. Có Designer mới; snapshot không phát sinh thay đổi model.
- Sửa [MigrationProgressChecks.cs](../../backend/tests/StudyPlanner.CoreChecks/MigrationProgressChecks.cs): kiểm tra đủ sáu PK/FK/CHECK và mọi tên constraint, chấp nhận thêm NOT NULL của PostgreSQL 18 thay vì cố định tổng constraint luôn bằng sáu.
- Dùng công cụ tạm `backend/obj/db-maintenance/` để đọc trạng thái, sao lưu, diễn tập, áp dụng và xác minh; không thêm công cụ này vào source sản phẩm. Kết nối đọc từ cấu hình dự án, không in chuỗi kết nối/mật khẩu hoặc nội dung bản ghi.
- Cập nhật trạng thái hiện hành trong [module 07](../modules/07-goi-y-cong-viec.md), [module 09](../modules/09-dong-ho-tap-trung.md), [ERD](../erd.md), thêm liên kết cập nhật vào ba bản ghi trước. Nội dung kiểm chứng trước triển khai vẫn được giữ làm lịch sử.

## Kiểm chứng

- Trước ghi DB đã tạo bản sao lưu custom bằng công cụ PostgreSQL 18 portable từ [EDB](https://productsdl.enterprisedb.com/download-postgresql-binaries). Công cụ PostgreSQL 17 có sẵn không dùng để dump server 18. Không cài công cụ vào hệ thống.
- Backup giữ tại `backend/obj/database-backups/study-planner-before-upgrade-20261114-121636.dump`, 105.626 byte, 20 mục dữ liệu bảng trong TOC (gồm bảng lịch sử migration). Đã kiểm tra thư mục này bị Git bỏ qua; không đưa dữ liệu backup vào tài liệu hoặc commit.
- Đã khôi phục backup vào PostgreSQL 18 cục bộ biệt lập, chạy đủ migration, đối chiếu bản ghi/dấu kiểm trước–sau, rồi mới áp dụng vào DB cấu hình. Các trường tiến độ/checklist/trạng thái được phép chuyển theo migration; các dữ liệu gốc còn lại được đối chiếu theo mã bản ghi. Nhắc hạn có thể bị hủy theo trạng thái nhiệm vụ mới, lịch sử chỉ được thêm.
- Sau nâng cấp: không còn migration pending; ba bảng mới có 0 bản ghi, nhắc kế hoạch vẫn mặc định tắt. Mười tài khoản có cấu hình mặc định 25 phút học / 5 phút nghỉ / 180 phút mỗi ngày.
- Nhiệm vụ từ **53 lên 57**, do bảo toàn bốn checklist cha thành bốn con “Công việc chung”; không xóa nhiệm vụ gốc. Giữ đủ **10 tài khoản, 6 phiên tự học, 4 phiên tập trung và 5 khoảng thời gian học**. Số phiên tính cả bản ghi bị lọc bởi trạng thái thùng rác để so sánh đúng với dữ liệu gốc.
- Kết nối mới xác minh đủ sáu PK/FK/CHECK, bốn NOT NULL và năm index của `khoang_thoi_gian_hoc` đều dùng tên mới, sequence đúng tên; không còn bảng cũ và không có cha sai phần trăm so với trung bình con.
- CoreChecks trên PostgreSQL 18 cục bộ: đạt 30 kiểm tra nâng cấp schema, 561 HTTP nghiệp vụ, 39 giờ rảnh, 38 cấu hình/lập lại lịch, 47 nhắc kế hoạch/cấu hình, 121 đồng hồ cùng các kiểm tra quy tắc; client nhiệm vụ FE thật → HTTP API → PostgreSQL đạt.
- PostgreSQL 17 cục bộ với `STUDYPLANNER_FOCUS_ONLY=1`: các kiểm tra thuần và 121 HTTP đồng hồ đạt, xác nhận chuỗi migration mới vẫn chạy trên 17. Không ghi dữ liệu thử vào DB đang dùng.
- Đã dừng các cụm PostgreSQL thử, xóa thư mục dữ liệu diễn tập chứa bản khôi phục và file ZIP tải công cụ; giữ backup gốc và công cụ portable trong `obj`. Không dừng hay khởi động lại tiến trình BE của người dùng.
- Chưa thử giao diện bằng phiên đăng nhập hiện có của người dùng sau nâng cấp; cần BE/FE nạp code mới. Không tự ghi người dùng đã duyệt code.

## Việc tiếp theo

- Database Development đã cập nhật xong, không cần người dùng chạy lại migration. Khởi động lại BE và tải lại FE nếu chúng vẫn chạy bản code cũ.
- Backup là dữ liệu riêng cần giữ ngoài Git. Không tự downgrade bản tiến độ vì có dữ liệu bảo toàn mới; khi cần khôi phục phải chọn phương án phù hợp từ backup và kiểm tra dữ liệu phát sinh sau thời điểm sao lưu.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
