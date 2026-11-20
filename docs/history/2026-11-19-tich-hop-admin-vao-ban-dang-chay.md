# 2026-11-19 — Tích hợp admin vào bản đang chạy

## Yêu cầu và mục đích

- Người dùng thấy giao diện admin cũ và yêu cầu đưa các chức năng đã mô tả vào code sử dụng được. Giữ quyền riêng tư sinh viên, comment tiếng Việt và nhánh theo nhóm công việc.
- Bản trước mới có code trong worktree; bàn giao thiếu bước tích hợp vào checkout gốc. Nhánh tích hợp backend/frontend: `feat/tich-hop-quan-tri-sinh-vien`. Hai nhánh `feat/admin-theo-doi-hoc-tap-sinh-vien` và `feat/admin-quan-ly-tai-khoan-sinh-vien` vẫn giữ riêng, chưa commit.
- Đọc [bản theo dõi](2026-11-18-admin-theo-doi-hoc-tap-sinh-vien.md), [bản tài khoản](2026-11-18-admin-quan-ly-tai-khoan-sinh-vien.md), README/TEMPLATE và đối chiếu code thực tế. Checkout gốc có nhiều thay đổi sinh viên chưa commit; không reset hoặc ghi đè các thay đổi đó.

## Code và logic thay đổi

- Backend: thêm [controller](../../backend/StudyPlanner.Api/Controllers/Administration/TheoDoiHocTapController.cs), [service/DTO theo dõi](../../backend/StudyPlanner.Infrastructure/Administration/TheoDoiHocTapService.cs), đăng ký DI và nhóm Swagger. Chỉ GET, policy Admin; đọc nhiệm vụ Student gắn kỳ/môn và khác Personal. DTO giới hạn trường để không lộ mô tả, checklist, ghi chú, tệp, giờ rảnh hoặc dữ liệu xác thực.
- [NguoiDungRequest](../../backend/StudyPlanner.Application/Administration/NguoiDungRequest.cs), [QuanLyNguoiDungService](../../backend/StudyPlanner.Application/Administration/QuanLyNguoiDungService.cs): nhận mốc cập nhật mong đợi, so sánh trong transaction; trả 409 khi hồ sơ đã đổi. Tránh bản form cũ ghi đè trạng thái vừa khóa. Client cũ không gửi trường này vẫn tương thích.
- Frontend `C:/Users/ADMIN/Desktop/Study_Planner_FE`: thêm `AdminLearningPage.tsx`, `learning.css`, `StudentAccountEditor.tsx`, `account-editor.css`; cập nhật AdminLayout/AdminRoutes, StudentAccountsPage, StudentDetails, api/admin và xử lý Escape của Modal trong PlannerUI. Chi tiết sinh viên có cả liên kết Xem học tập và nút Sửa thông tin. Màn theo dõi chỉ đọc; tạo/sửa tài khoản dùng API có sẵn, không mở đổi vai trò hoặc đổi mật khẩu tài khoản hiện có.
- Luồng theo dõi: phiên admin → menu Theo dõi học tập → bộ lọc và bảng/thống kê → chọn sinh viên → kỳ/môn, kế hoạch và thời gian theo ngày. Luồng tài khoản: nhập form → kiểm tra → API POST/PUT → thành công tải lại danh sách/chi tiết; lỗi giữ form. Không ghi form/mật khẩu vào log hoặc localStorage.
- Chuyển file có đối chiếu HEAD và sao lưu tại `backend/obj/admin-integration-backup` trước khi ghi. Không sao chép Program.cs/appsettings hoặc file nghiệp vụ sinh viên từ worktree.
- Hai bộ kiểm thử trình duyệt được hợp nhất trong `tests/browser-auth.mjs`: fixture tài khoản dùng cùng managedUsers với API đọc; selector nút sửa dùng `button.admin-detail-button` để không bấm nhầm liên kết Xem học tập.
- Cập nhật module 08/README vì mô tả chưa triển khai và danh mục tổ chức cũ không còn khớp code. Module 10 bổ sung trạng thái tích hợp; bản ghi cũ giữ nguyên lịch sử.

## Kiểm chứng

- `scripts/test-admin-learning.ps1`: build 0 lỗi/cảnh báo; 116 assertions HTTP/PostgreSQL đạt, gồm quyền, phạm vi riêng tư, hạn kế thừa, lọc/phân trang, tổng hợp và chỉ đọc.
- `scripts/test-admin-accounts.ps1`: build 0 lỗi/cảnh báo; 169 kiểm tra HTTP/PostgreSQL đạt, gồm mốc cập nhật hồ sơ. Hai bài dùng cụm PostgreSQL tạm localhost, đã dừng sau kiểm thử; không dùng DB ứng dụng.
- Frontend `npm run build`: đạt; Vite cảnh báo chunk JS lớn hơn 500 kB. `npm test`: 190/190 đạt. Có thông báo EPERM dọn cache Vite và trùng cổng WebSocket từ các test chạy đồng thời, không gây test thất bại.
- Chrome `node tests/browser-auth.mjs --admin-learning`: đạt lọc/phân trang, phạm vi sinh viên, lỗi/thử lại/rỗng, thống kê và mobile. Chrome `--admin-accounts`: lượt đầu phát hiện thiếu callback fixture dùng chung; đã sửa cùng selector nút sửa, chạy lại đạt tạo/sửa, lỗi giữ form, email trùng, payload và mobile. Đây là dữ liệu giả lập.
- Lượt build trong sandbox thất bại không có thông báo lỗi compiler; chạy lại có quyền ngoài sandbox đạt như trên.
- Kiểm tra ban đầu không có server tại 5059/5173. Đã khởi động API bằng `dotnet run --project backend/StudyPlanner.Api --launch-profile http` và frontend bằng `npm run dev -- --host localhost --port 5173 --strictPort`; cả hai báo lắng nghe đúng cổng. Không chạy migration hoặc ghi tài khoản thật.
- Kiểm tra HTTP bản đang chạy: `/admin/learning`, module AdminLearningPage và StudentAccountEditor trả 200; `/api/v1/admin/learning/summary` trả 401 khi chưa đăng nhập ở cả cổng API 5059 và proxy frontend 5173. Đường dẫn đã hoạt động và vẫn yêu cầu xác thực.
- `git diff --check` backend/frontend đạt. Đã xem hai ảnh chụp do bài kiểm thử trình duyệt tạo: menu/bảng theo dõi và hộp sửa tài khoản hiển thị đúng, không phải ảnh dữ liệu thật.
- Đã đối chiếu diff admin, controller/service, form và các điểm ghép menu/route/chi tiết. Không commit, push hoặc triển khai.

## Việc tiếp theo

- Người dùng đăng nhập admin trên `http://localhost:5173/admin/learning` để nghiệm thu dữ liệu thật. Chưa xác nhận bằng phiên đăng nhập thật và chưa tạo/sửa tài khoản thật.
- Hai HTTP thống kê và bảng không có chung snapshot xuyên request nếu sinh viên cập nhật đồng thời.
- Sau khi đóng server phát triển cần chạy lại backend/frontend từ checkout gốc chứa code tích hợp.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR: chưa có; không tự commit/push.
