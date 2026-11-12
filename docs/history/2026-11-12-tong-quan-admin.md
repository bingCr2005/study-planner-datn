# 2026-11-12 — Tổng quan admin

## Yêu cầu và mục đích

- Người dùng chọn ba phần: thống kê sinh viên, quy tắc đang áp dụng và năm sinh viên mới đăng ký; không làm biểu đồ.
- Trước: `AdminOverviewPage` trả null. Đã đọc lịch sử, code quản trị tài khoản và [quy tắc ưu tiên](2026-11-12-quan-ly-quy-tac-uu-tien.md).

## Code và logic thay đổi

- `Infrastructure/Administration/TongQuanAdminService.cs` và controller `TongQuanAdminController`: GET `/api/v1/admin/overview`, chỉ Admin. Đếm Student toàn bộ database: tổng, đang mở, đã khóa và mới trong khoảng `[now-7 ngày, now]`. Cùng một mốc TimeProvider và transaction RepeatableRead cho thống kê và năm bản ghi mới nhất; không đếm từ một trang danh sách.
- Chỉ trả mã, họ tên, email, trạng thái và ngày tạo của năm sinh viên; không trả mật khẩu/hash hay cấu hình xác thực. Sắp ngày tạo giảm dần, cùng ngày/giờ thì mã giảm dần. Trường hợp rỗng trả số 0 và mảng rỗng.
- `DanhSachNguoiDungRequest`/`QuanLyNguoiDungService`: thêm lọc tùy chọn `VaiTro`, `TaoTu`, `TaoDen`; giữ các bộ lọc cũ. Các link từ thẻ thống kê dùng Student và chính mốc thời gian API trả về, nên tổng truy vấn danh sách khớp tiêu chí của thẻ. Khoảng ngày đảo ngược/vai trò sai trả 400.
- FE repo `study-planner-frontend`: trang gồm bốn thẻ thống kê, bảng năm sinh viên mới và thẻ quy tắc. API thống kê độc lập với API quy tắc đang dùng; một nguồn lỗi không làm nguồn kia mất nội dung. Có tải lại/thử lại, hiển thị trạng thái đang tải/lỗi và 0 chỉ khi BE xác nhận dữ liệu rỗng.
- Danh sách tài khoản giữ bộ lọc vai trò/ngày trong URL và khi mở/quay lại chi tiết, có chip bỏ lọc. Có hỗ trợ adapter mock khi chủ động bật.
- Không thêm biểu đồ, bảng/migration hay sửa module nhiệm vụ/lịch. Tiếp tục nhánh nhóm chức năng admin đang có thay đổi chưa commit, giữ toàn bộ phần quy tắc đã làm; không tự commit/push hoặc dừng API người dùng đang chạy.

## Kiểm chứng

- Build BE thành công với artifacts path riêng, 0 warnings/errors; không ghi đè DLL của API người dùng đang chạy.
- `StudyPlanner.OverviewChecks` đạt trên PostgreSQL localhost riêng, tự tạo/dọn database tên ngẫu nhiên: JWT 401/403, rỗng trả 0, 34 Student + 2 Admin cho tổng 34 (22 mở/12 khóa), mới 7 ngày có đúng 8 gồm mốc biên, top 5 đúng thứ tự, lọc danh sách theo role/trạng thái/ngày khớp thống kê ngay cả khi phân trang 5. Đã kiểm tra validate bộ lọc sai, DTO không chứa mật khẩu/hash và tải lại phản ánh trạng thái thay đổi.
- FE build, lint các file sửa và toàn bộ 129 kiểm thử đạt; có kiểm tra mock về quyền, đăng ký mới, khóa tài khoản và các bộ lọc tương ứng.
- Chrome `--overview` đạt: số liệu toàn bộ, năm sinh viên mới, không biểu đồ, tất cả thẻ mở đúng bộ lọc, chi tiết, cập nhật quy tắc, lỗi hai nguồn độc lập, retry và empty state thật. Đã xem ảnh giao diện desktop `.backend-integration/screenshots/admin-overview.png` ở FE; dữ liệu ảnh là fixture.
- Kiểm thử trình duyệt dùng API fixture, BE integration dùng DB riêng; chưa chạy trên phiên người dùng/DB ứng dụng. Không đọc cấu hình kết nối ứng dụng hoặc tự khởi động lại API người dùng.

## Việc tiếp theo

- Khởi động lại BE để nạp API và bộ lọc mới. Đây là thống kê tài khoản được phép đăng nhập, không phải theo dõi người đang online.
- Bảy ngày là khoảng 168 giờ tính đến thời điểm lấy dữ liệu; không phải bảy ngày lịch theo múi giờ.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu code và ghi phạm vi/giới hạn.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR: chưa tạo.
