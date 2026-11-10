# 2026-11-09 — Chia thư mục học tập theo chức năng

## Yêu cầu và mục đích

Giúp nhóm tìm service, request, response và quy tắc của cùng chức năng trong một thư mục. Xem [lịch sử tích hợp](2026-11-06-chuyen-day-du-frontend-preview-va-sua-bo-loc.md).

## Code và logic thay đổi

Trước đây 51 file nằm trực tiếp trong [HocTap](../../backend/StudyPlanner.Application/HocTap). Nay NhiemVu chứa 13 file, ThoiKhoaBieu 13, LapLich 13, PhienTuHoc 4 và DungChung 8. ChonPhienTuHocRequest ở LapLich vì dùng để chọn phiên khi áp dụng lịch. PhienHocTap ở DungChung vì biểu diễn ngữ cảnh giao dịch. Controller vẫn gọi cùng service, dùng cùng DTO và store. Đã cập nhật đường dẫn service trong [kịch bản demo](../KICH-BAN-DEMO-NHIEM-VU-VA-THOI-KHOA-BIEU.md).

## Kiểm chứng

Đối chiếu Git blob của từng file đích với đường dẫn cũ ở HEAD, khớp đủ 51 file. Build Debug của Application thành công. Build solution Debug gặp DLL bị khóa tại thư mục API. Build toàn solution Release thành công với 0 cảnh báo và 0 lỗi. CoreChecks Release đạt kiểm tra thời gian và 15 kiểm tra cây nhiệm vụ, hạn và tiến độ. Phần tích hợp PostgreSQL/HTTP được bỏ qua do chưa cấu hình cơ sở dữ liệu kiểm thử.

## Việc tiếp theo

Tạo nhánh refactor trước khi commit. Phiên làm việc không có quyền ghi ref vào .git.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu thay đổi và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan.
