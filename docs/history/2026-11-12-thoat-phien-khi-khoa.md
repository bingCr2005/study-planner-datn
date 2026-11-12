# 2026-11-12 — Thoát phiên khi khóa tài khoản

## Yêu cầu và mục đích

- Sinh viên đang mở ứng dụng phải được chuyển về Đăng nhập kèm thông báo bị khóa ngay khi admin vô hiệu hóa.
- Trước: backend kiểm tra `DangHoatDong` trong `OnTokenValidated`, nhưng frontend chỉ phát hiện khi gọi API khác và hiển thị thông báo hết phiên chung.
- Đã đọc `AGENTS.md`, README lịch sử, bản ghi Tổng quan admin, code quản trị tài khoản, xác thực và kiểm tra Auth/Overview hiện tại. Tiếp tục nhóm chức năng admin trên nhánh đang có thay đổi; không tự commit/push.

## Code và logic thay đổi

- `Api/Auth/KenhPhienDangNhap.cs`: singleton quản lý channel có giới hạn cho từng kết nối/mã tài khoản. Phát sự kiện đến mọi tab cùng tài khoản, dispose bỏ đăng ký, không lưu token.
- `Api/Controllers/PhienDangNhapController.cs`: GET có xác thực `/api/v1/users/me/session-events`; lấy ID từ JWT, không từ request. Subscribe trước khi đọc trạng thái để không bỏ lỡ thay đổi lúc kết nối. SSE tắt buffering/cache; heartbeat mỗi 15 giây xác minh DB, password stamp và hạn token; đóng sau sự kiện khóa/hết phiên. Kết nối bị hủy giải phóng subscription.
- `Api/Controllers/Administration/NguoiDungController.cs`: sau khi service commit vô hiệu hóa/sửa trạng thái khóa thì phát `account-disabled`; xóa tài khoản phát `session-invalid`. Vẫn dùng service hiện tại, giữ bảo vệ admin cuối cùng; không phát nếu service thất bại.
- `Api/Auth/XacThucRegistration.cs` và `Api/Administration/QuanTriRegistration.cs`: dùng `TryAddSingleton` để cả cấu hình quản trị riêng và cấu hình xác thực đều lấy cùng một kênh, không đăng ký trùng. Tài khoản bị khóa trả `401/ACCOUNT_DISABLED` để FE phân biệt với hết hạn. Quyền, xác thực chữ ký và kiểm tra password stamp giữ nguyên.
- FE repo `study-planner-frontend`: đọc SSE bằng Bearer header, hủy khi đổi phiên/logout; sự kiện khóa xóa phiên và chuyển về Đăng nhập với thông báo rõ ràng. Khi lỗi mạng, kiểm tra hồ sơ/thử lại sau 5 giây, kiểm tra khi quay lại tab; watchdog 35 giây cho kết nối treo. Không kết luận khóa tài khoản chỉ vì lỗi mạng. Mock giữ lý do khóa dù phiên đã bị thu hồi.
- Không migration, không sửa module nhiệm vụ/lịch. Kích hoạt lại không tự phục hồi phiên FE đã xóa. Chưa thêm security stamp thu hồi JWT vĩnh viễn: token cũ còn hạn có thể hoạt động sau khi mở khóa nếu khách vẫn giữ token. SSE cùng tiến trình phát ngay; thay đổi từ tiến trình khác được phát hiện qua heartbeat, tối đa khoảng 15 giây khi online. Trình duyệt bị tạm dừng/offline chờ kết nối lại.

## Kiểm chứng

- BE build thành công, 0 warnings/errors, dùng artifacts path riêng để không ghi đè DLL API đang chạy.
- `StudyPlanner.SessionChecks`: 17 kiểm tra đạt trên DB localhost riêng và HTTP thật, bao gồm hai kết nối đang đứng yên nhận khóa trong dưới 2 giây, tài khoản khác không bị ảnh hưởng, JWT/quyền Admin, bảo vệ Admin cuối cùng, trạng thái DB đã lưu trước sự kiện, reconnect và API hồ sơ trả lý do khóa, heartbeat bắt thay đổi ngoài tiến trình.
- Hồi quy BE: `StudyPlanner.AdministrationChecks` 164 kiểm tra và `StudyPlanner.AuthChecks` 372 kiểm tra đều đạt.
- FE: build và lint đạt; toàn bộ 135 kiểm thử đạt. Sau khi thêm timeout cho kiểm tra hồ sơ, chạy lại riêng 6 kiểm tra session-events đều đạt.
- Chrome `--session-lock`: SSE HTTP streaming thật qua Vite với nguồn sự kiện fixture; sinh viên ở Hôm nay không thao tác vẫn tự thoát trong dưới 2 giây, thông báo khóa xuất hiện, credential bị xóa, khôi phục token cũ vẫn nhận đúng lý do, ngắt kênh không tự đăng xuất và tự nối lại. Không dùng phiên/dữ liệu thật.
- Chrome hồi quy auth đầy đủ đạt cả API fixture và adapter mock: đăng ký/đăng nhập, điều hướng theo vai trò, cập nhật hồ sơ, đổi mật khẩu, hết hạn JWT, danh sách/chi tiết/kích hoạt/vô hiệu hóa. Sửa bước chờ trong helper sidebar để chờ `aria-current` sau điều hướng thay vì chỉ chờ URL, vì React cập nhật giao diện sau URL; không sửa sidebar sản phẩm.
- Đã đọc lại diff FE/BE; `git diff --check` đạt; không có thay đổi trong FE `src/hoc-tap` hoặc BE module HocTap.
- Chưa thao tác khóa tài khoản hay dữ liệu ứng dụng thật; kiểm tra backend dùng DB localhost tên ngẫu nhiên tự tạo/dọn.

## Việc tiếp theo

- Cần khởi động lại BE đang chạy để nạp endpoint và cơ chế phát mới. Không tự dừng tiến trình API của người dùng.
- Khi triển khai qua reverse proxy, bật streaming SSE và tránh buffering.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu code/diff và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR: chưa tạo.
