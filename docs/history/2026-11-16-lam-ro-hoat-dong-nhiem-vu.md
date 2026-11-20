# 2026-11-16 — Làm rõ hoạt động nhiệm vụ và tệp đính kèm

## Yêu cầu và mục đích

- Người dùng muốn hộp Hoạt động giúp sinh viên biết vừa làm gì, xem ví dụ TickTick. Bố cục nhóm theo ngày và giờ đã có từ [bản ghi giao diện](2026-11-11-lam-gon-hoat-dong-nhiem-vu.md).
- Tiếp tục `fix/doi-ten-buoc-giu-tien-do` vì thay đổi gắn với các bước và tệp đính kèm trong chi tiết nhiệm vụ.

## Code và logic thay đổi

- Trước đây [NhiemVuService.cs](../../backend/StudyPlanner.Application/HocTap/NhiemVu/NhiemVuService.cs) ghi tạo cho bước con và ghi chung thay đổi tiến độ ở cha nếu có. Sau thay đổi, thao tác thêm bước ghi một dòng của cha, có tên các bước thực sự được sinh viên thêm và snapshot tiến độ trước/sau. Dòng này có cả khi tiến độ không đổi. Bước tự sinh giữ tiến độ không xuất hiện trong tên thao tác. Nếu nhiệm vụ mở lại hoặc hoàn thành, giữ loại hành động cũ cùng dữ liệu thêm bước.
- [TepDinhKemService.cs](../../backend/StudyPlanner.Infrastructure/TepDinhKem/TepDinhKemService.cs) ghi hoạt động thêm/xóa với đúng tên hiển thị, trong cùng giao dịch với metadata tệp. Tải xuống và thao tác thất bại không tạo dòng. Không lưu nội dung tệp, đường dẫn hay khóa kho trong lịch sử.
- FE `src/hoc-tap/HoatDongNhiemVu.tsx` đọc thông tin hoạt động từ JSON hiện có. Giao diện giữ nhóm ngày, giờ và cách tóm tắt bản ghi cũ. Thêm biểu tượng trong `src/components/Icon.tsx` và CSS hoạt động trong `src/hoc-tap/hoc-tap.css`. Xem [đặc tả lịch sử](../modules/04-lich-su-nhiem-vu.md).
- Dùng các giá trị `hanhDong` hiện có cùng dữ liệu chi tiết `loaiHoatDong` trong `duLieuSau`. Không cần migration hay sửa dữ liệu cũ.

## Kiểm chứng

- Build Release CoreChecks đạt 0 cảnh báo, 0 lỗi. PostgreSQL tạm đạt 576 kiểm tra nghiệp vụ HTTP và 53 kiểm tra tệp, gồm thêm bước chưa đổi tiến độ, lưu đúng tên và phần trăm, không ghi khi xem trước hoặc thiếu xác nhận, tệp không tạo dòng khi tải/xử lý lỗi và giới hạn quyền xem. Các nhóm giờ rảnh, lập lịch, nhắc hạn và tập trung cũng đạt. Đã dừng cụm thử. Không dùng DB người dùng.
- Chrome headless với `CORE_PROGRESS_ONLY=1` và `CORE_ATTACHMENTS_ONLY=1` đạt. Đã kiểm tra tên bước và tiến độ trên một dòng, hai hoạt động PDF và biểu tượng tệp. Trình duyệt dùng dữ liệu giả. TypeScript `tsc -b` và `git diff --check` đạt ở các repo.

## Việc tiếp theo

- Khởi động lại Backend và tải lại trang để sinh hoạt động chi tiết cho các thao tác mới.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
