# Các bảng database đang sử dụng

Đối chiếu code BE/FE và database Development ngày **15/11/2026**. Sau migration `20261115080829_DonBangToChucVaLichNhacCu`, còn **17 bảng nghiệp vụ, 27 khóa ngoại**, cộng bảng hạ tầng `__EFMigrationsHistory`. Không còn migration chờ áp dụng. Số bản ghi dưới đây là ảnh chụp tại thời điểm kiểm tra, tính cả dữ liệu nằm trong thùng rác khi có bộ lọc xóa mềm.

## Những bảng giữ lại

| Bảng | Số bản ghi | Chức năng đã có |
| --- | ---: | --- |
| `nguoi_dung` | 10 | Đăng nhập, hồ sơ, quản trị tài khoản, múi giờ và cấu hình học/nhắc hạn |
| `hoc_ky` | 12 | Học kỳ riêng của sinh viên |
| `mon_hoc` | 27 | Môn riêng trong học kỳ; khác danh mục mẫu JSON của admin |
| `lich_hoc` | 5 | Các buổi học trên lớp lặp theo thứ/khung giờ, chặn trùng khi gợi ý tự học |
| `nhiem_vu` | 57 | Việc học/cá nhân, cây nhiệm vụ, checklist, tiến độ, ước lượng, thùng rác |
| `lich_su_nhiem_vu` | 366 | Lịch sử thay đổi nhiệm vụ; giữ được lịch sử sau xóa |
| `khung_gio_ranh` | 3 | Mẫu giờ rảnh lặp theo tuần trong khoảng ngày hiệu lực |
| `ngoai_le_gio_ranh` | 0 | Bỏ hoặc đổi giờ của riêng một ngày; API và giao diện đã có |
| `phien_tu_hoc` | 6 | Phiên dự kiến, bắt đầu ngay, dời/hủy và kết thúc |
| `lan_ap_dung_lich` | 3 | Ghi lần áp dụng gợi ý và khóa chống tạo phiên trùng khi gửi lại yêu cầu |
| `phien_tap_trung` | 4 | Một phiên Pomodoro/bấm giờ, trạng thái và ghi chú |
| `khoang_thoi_gian_hoc` | 5 | Các khoảng học thực tế giữa bắt đầu/tiếp tục và tạm dừng/kết thúc; loại thời gian nghỉ |
| `quy_tac_uu_tien` | 2 | Admin cấu hình, trang Hôm nay dùng để sắp thứ tự việc nên làm |
| `thong_bao` | 21 | Chuông nhắc hạn/quá hạn, trạng thái đã đọc, chống phát trùng; worker và API đang dùng |
| `cau_hinh_nhac_ke_hoach` | 0 | Bật/tắt nhắc kế hoạch trong cài đặt chuông; chưa có dòng thì mặc định tắt |
| `nhac_ke_hoach` | 0 | Nhắc xem kế hoạch tuần/giờ rảnh sắp hết hiệu lực khi đã bật và tải hộp thông báo |
| `tep_dinh_kem_nhiem_vu` | 0 | Đã bổ sung tải lên, danh sách, tải xuống, xóa và giao diện trong chi tiết nhiệm vụ |

Các bảng rỗng vẫn có chức năng sử dụng thực tế. Không tạo dữ liệu giả vào tài khoản người dùng chỉ để làm các bảng này có bản ghi. `phien_tu_hoc`, `phien_tap_trung`, `khoang_thoi_gian_hoc` có vai trò khác nhau: lịch dự kiến → phiên tập trung → các khoảng thực học; gộp chúng sẽ làm mất cách biểu diễn tạm dừng, nhiều lần học và lịch sử.

## Những bảng đã gỡ

| Bảng | Trước khi gỡ | Thay thế hiện hành |
| --- | --- | --- |
| `truong` | 0 dòng | Hồ sơ lưu `nguoi_dung.ten_truong` |
| `khoa` | 0 dòng | Không có luồng quản lý khoa trong phạm vi cá nhân; ngành học lưu chuỗi trên hồ sơ |
| `khoa_hoc` | 0 dòng | Không có luồng quản lý khóa tuyển sinh |
| `lop_sinh_vien` | 0 dòng, không tài khoản nào còn `ma_lop` | Hồ sơ lưu `nguoi_dung.ten_lop` |
| `lich_nhac` | 0 dòng, không thông báo nào còn liên kết | Worker/API suy ra nhắc hạn từ nhiệm vụ và ghi `thong_bao` |

Đồng thời gỡ entity/configuration/DbSet tổ chức, entity/enum lịch nhắc cũ, `nguoi_dung.ma_lop`, `thong_bao.ma_lich_nhac` và các FK/index liên quan. Không xóa migration lịch sử: database mới vẫn nâng cấp được từ schema ban đầu. Danh mục môn/học kỳ mẫu của admin vẫn dùng JSON riêng, không bị ảnh hưởng.

Migration khóa các bảng liên quan trong giao dịch, bổ sung tên trường/lớp cho hồ sơ cũ nếu tên chưa có, không ghi đè tên tự khai. Từ chối chạy nếu còn lịch nhắc Pending/Failed hoặc lịch Delivered chưa có thông báo tương ứng. Giữ mọi thông báo và mốc đã đọc. Không tự downgrade thành các bảng rỗng: phục hồi phải đối chiếu backup và dữ liệu mới phát sinh.

## Cách dùng tệp đính kèm

Mở chi tiết nhiệm vụ ở trang Nhiệm vụ hoặc trong Tập trung → **Tệp đính kèm → Đính kèm tệp**. Bấm tên để tải xuống, bấm Xóa rồi xác nhận để gỡ tệp.

| API | Xử lý |
| --- | --- |
| `GET /api/v1/tasks/{id}/attachments` | Danh sách tệp của đúng chủ nhiệm vụ, trả phiên bản dữ liệu |
| `POST /api/v1/tasks/{id}/attachments` | Multipart, trường `tep`; cần JWT và `If-Match` |
| `GET /api/v1/tasks/{id}/attachments/{tepId}/download` | Kiểm tra quyền rồi trả nội dung dạng tải xuống, không mở HTML/SVG trực tiếp |
| `DELETE /api/v1/tasks/{id}/attachments/{tepId}` | Cần JWT/`If-Match`; xóa thông tin và nội dung tệp |

- Giới hạn 10 MB/tệp, 20 tệp/nhiệm vụ, không nhận tệp rỗng; tên tối đa 255 ký tự. Khóa kho do máy chủ sinh, không dùng đường dẫn từ người dùng. API không trả khóa hoặc đường dẫn vật lý.
- Thông tin tệp lưu database; nội dung lưu ngoài `wwwroot`, mặc định `backend/StudyPlanner.Api/App_Data/tep-nhiem-vu`. Có thể cấu hình `Attachments:StoragePath`. Thư mục không vào Git hoặc gói publish. Khi triển khai phải giữ volume bền vững và sao lưu cả DB lẫn kho tệp; nếu chạy nhiều API phải dùng chung kho.
- Đưa vào thùng rác: giữ tệp để khôi phục, nhưng API ẩn cả danh sách và tải xuống. Xóa vĩnh viễn nhiệm vụ: FK cascade xóa thông tin tệp; worker dọn nội dung mồ côi mỗi giờ sau khi tệp đủ 24 giờ tuổi. Worker đọc cả bản ghi trong thùng rác, không xóa tệp còn được tham chiếu.
- Khi kết quả COMMIT chưa xác định do mất kết nối, giữ nội dung để tránh xóa nhầm tệp đã ghi thành công; lượt dọn sau đọc lại DB rồi mới xóa tệp không còn tham chiếu.
- Phạm vi hiện tại là tải lên/tải xuống/xóa. Chưa có xem trước nội dung, quét mã độc, phiên bản tệp hoặc kho đối tượng bên ngoài. Không bắt buộc thêm các phần này chỉ để sử dụng bảng.

Xem [lịch sử triển khai và kiểm chứng](history/2026-11-15-don-bang-cu-va-tep-dinh-kem.md).
