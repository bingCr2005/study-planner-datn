# 2026-11-18 — Admin quản lý tài khoản sinh viên

## Yêu cầu và mục đích

- Người dùng yêu cầu admin có thao tác quản lý phù hợp chức năng website, giữ quyền riêng tư của sinh viên và tách nhánh từng nhóm chức năng. Yêu cầu comment tiếng Việt để dễ đọc code.
- Nhánh backend/frontend `feat/admin-quan-ly-tai-khoan-sinh-vien` nằm trong các worktree riêng, dựa trên HEAD đã có. Không mang thay đổi chưa commit của luồng học tập hoặc nhánh theo dõi vào nhánh này.
- Vị trí local dưới workspace gốc: `.worktrees/admin-quan-ly-tai-khoan-backend` và `.worktrees/admin-quan-ly-tai-khoan-frontend`. Đã di chuyển bằng git worktree move và kiểm tra lại tên nhánh sau khi kiểm thử; nội dung code không thay đổi do di chuyển.
- Đã đọc AGENTS, README/TEMPLATE lịch sử, bản ghi khóa tài khoản/Tổng quan admin và code quản lý tài khoản hiện hành ở cả hai repo.

## Code và logic thay đổi

- Trước: backend đã có API tạo/sửa tài khoản, giao diện chỉ có danh sách, chi tiết và khóa/mở khóa. PUT chưa biết form đang dựa trên lần cập nhật nào.
- Frontend `src/admin/StudentAccountEditor.tsx`, `account-editor.css`: biểu mẫu dùng chung cho danh sách/chi tiết; tạo Student, nhập thông tin hồ sơ và mật khẩu ban đầu; sửa chỉ hồ sơ, không có trường đổi vai trò/mật khẩu/trạng thái. Khi sửa không gửi mật khẩu. Lỗi giữ dữ liệu để sửa/thử lại; chặn gửi trùng, chặn đóng khi đang gửi, không lưu form/mật khẩu ra log hoặc localStorage.
- `src/api/admin.ts`: thêm saveManagedStudent, danh sách trường cho phép rõ ràng. Trim thông tin hồ sơ, giữ nguyên mật khẩu mới. Bỏ các thuộc tính lạ như vaiTro/matKhauBam, không có API xóa trong tính năng mới. Chỉ cho editor sinh viên, không dùng editor này để sửa Admin.
- `StudentAccountsPage`/`StudentDetails`: nút Thêm sinh viên và Sửa thông tin; lưu thành công tải lại danh sách/chi tiết. Sau tạo bỏ bộ lọc cũ về danh sách Student; không giả vờ tài khoản mới bắt buộc nằm trang đầu nếu dữ liệu nhiều trang.
- `src/components/PlannerUI.tsx`: sự kiện Escape preventDefault rồi đi qua onClose để form đang lưu được quyền chặn đóng, cùng hành vi nút Hủy/backdrop.
- Backend [NguoiDungRequest](../../backend/StudyPlanner.Application/Administration/NguoiDungRequest.cs) thêm `NgayCapNhatMongDoi` tùy chọn. [QuanLyNguoiDungService](../../backend/StudyPlanner.Application/Administration/QuanLyNguoiDungService.cs) kiểm tra mốc này ngay sau đọc tài khoản trong transaction Serializable. Mốc sai trả 409/ACCOUNT_CHANGED, không ghi hồ sơ hoặc trạng thái. FE mới luôn gửi mốc của hồ sơ đang sửa; client cũ thiếu mốc vẫn được xử lý tương thích như trước.
- Luồng: JWT Admin → mở form từ dữ liệu API → validate → POST tạo hoặc PUT kèm mốc cập nhật → backend kiểm tra quyền, mốc, email/mã sinh viên, admin cuối cùng và unique → transaction lưu → response DTO không chứa thông tin xác thực → FE đóng form và tải lại.
- Chỉ bổ sung giao diện cho thao tác quản lý tài khoản đã có. Không thêm quyền xóa/sửa nhiệm vụ, đọc nội dung riêng, giao nhiệm vụ, thay mật khẩu sinh viên hoặc thay vai trò. Không thêm nút xóa tài khoản; backend DELETE có sẵn từ trước được giữ nguyên, không mở rộng ở tác vụ này.
- Không thay model/schema, không có migration mới. Bảo vệ concurrent bằng mốc cập nhật áp dụng cho form mới gửi mốc; các client cũ bỏ mốc không được xem là đã có cơ chế form cũ này.

## Kiểm chứng

- Backend `scripts/test-admin-accounts.ps1`: build riêng đạt 0 lỗi/0 cảnh báo; đạt toàn bộ 169 kiểm tra PostgreSQL/HTTP. Bao gồm ca mới: form trước khi khóa gửi dangHoatDong=true bị 409/ACCOUNT_CHANGED; tên/trạng thái mới giữ nguyên; form có mốc đúng lưu được. Bộ kiểm thử cũ về unique, DTO không có hash, vai trò Student mặc định, admin cuối cùng, rollback và quyền chưa xác thực vẫn đạt.
- Script tạo cụm PostgreSQL riêng chỉ nghe localhost và database tên ngẫu nhiên, tự dọn database/dừng cụm. Không đọc cấu hình ứng dụng, không dùng DB sinh viên thật.
- FE build TypeScript/Vite đạt; Vite có cảnh báo chunk >500 kB. Bốn test API editor mới đạt; toàn bộ suite đạt 180/180. Lượt toàn suite có log WebSocket port 24678 đã dùng do các Vite test chạy đồng thời, không có test thất bại.
- Chrome `--admin-accounts` đạt tạo Student, sửa hồ sơ, lưu lỗi giữ form, email trùng, hủy, không gửi mật khẩu khi sửa, có mốc cập nhật, không DELETE và mobile không tràn. Đã xem ảnh `.backend-integration/screenshots/admin-account-editor.png`, dữ liệu là fixture.
- Đọc lại diff/request/service/test backend và các file editor/API/giao diện/test frontend; `git diff --check` đạt ở cả hai worktree. Ảnh/log kiểm thử được bỏ qua trong Git frontend.
- Không tạo/sửa/khóa tài khoản thật. Không áp migration, restart server, commit, push hoặc triển khai.

## Việc tiếp theo

- Cần nạp cả backend và frontend của nhánh này để form mới có bảo vệ mốc cập nhật. API backend cũ sẽ không kiểm tra thuộc tính mới.
- Hai nhóm admin được giữ độc lập trong các worktree/nhánh khác nhau theo yêu cầu. Chưa merge các nhánh vào nhau; người dùng cần xem code/diff trước khi yêu cầu commit hoặc tích hợp.
- Chưa bổ sung nhật ký ai sửa tài khoản hoặc quy trình đặt lại mật khẩu, vì không thuộc nhóm giao diện tạo/sửa hồ sơ hiện có.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo; không tự commit/push.
