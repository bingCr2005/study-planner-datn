# 2026-11-18 — Admin theo dõi học tập sinh viên

## Yêu cầu và mục đích

- Người dùng yêu cầu hoàn thiện admin dựa trên các chức năng website đã có; sau đó giới hạn quyền vì dữ liệu sinh viên riêng tư. Theo dõi phần phù hợp, không thêm sửa/xóa mọi dữ liệu. Yêu cầu comment tiếng Việt, chia nhánh theo từng nhóm chức năng.
- Nhánh backend/frontend `feat/admin-theo-doi-hoc-tap-sinh-vien`. Giữ nguyên thay đổi chưa commit có sẵn; không gộp phần quản lý tài khoản vào nhánh này. Phần tài khoản triển khai trong worktree/nhánh khác từ HEAD đã có.
- Thư mục `.worktrees/` được bỏ qua ở Git gốc để giữ các checkout admin riêng, tránh lẫn với output kiểm thử hoặc bị mất khi dọn `backend/obj`.
- Đã đọc README/TEMPLATE lịch sử, Tổng quan admin 12/11, thay đổi đồng hồ 17/11, code controller/service/entity/frontend hiện hành. Lịch sử cũ nói quản trị chưa triển khai hoặc chưa có API xóa tài khoản không khớp code; cập nhật phần hiện hành của module 08 và mục lục module, giữ đặc tả cũ có nhãn lịch sử.

## Code và logic thay đổi

- Trước: admin có tổng quan tài khoản, danh sách/chi tiết và khóa/mở khóa, danh mục mẫu và quy tắc; chưa có màn theo dõi dữ liệu học tập sinh viên.
- Sau: [TheoDoiHocTapController](../../backend/StudyPlanner.Api/Controllers/Administration/TheoDoiHocTapController.cs) có 5 GET chỉ Admin; đăng ký service ở QuanTriRegistration và nhóm Swagger tại MoTaSwagger.
- [TheoDoiHocTapService](../../backend/StudyPlanner.Infrastructure/Administration/TheoDoiHocTapService.cs) đọc qua DTO cho phép riêng: nhiệm vụ Student có môn/học kỳ và khác Personal; thống kê gốc mặc định, lọc/phân trang, hạn kế thừa, phạm vi sinh viên/môn/kỳ, kế hoạch và thời gian học tổng hợp. Không thay bất kỳ quyền/kiểm tra chủ sở hữu của API sinh viên.
- Frontend repo `Study_Planner_FE`: AdminLearningPage/learning.css, menu/route AdminLayout/AdminRoutes và liên kết Xem học tập tại StudentDetails. Bộ lọc bảng nằm trong URL; hồ sơ/phạm vi tải riêng để bộ lọc không biến mất khi chờ. Khoảng ngày kế hoạch dùng múi giờ sinh viên, đến hết ngày cuối, gồm xử lý DST. Lỗi API không trở thành số 0 giả.
- Phạm vi: không trả mô tả, checklist, tệp, ghi chú, giờ rảnh, phiên tự do, dữ liệu Personal kể cả gắn học kỳ hoặc thông tin xác thực. Admin không được sửa/xóa nhiệm vụ qua API mới; thao tác ghi vào đường dẫn chỉ đọc trả 405.
- Luồng: JWT Admin → xác minh bộ lọc và Student → truy vấn chỉ đọc RepeatableRead → DTO cho phép → giao diện thống kê/bảng. Chọn sinh viên → tải môn/kỳ → lọc nhiệm vụ hoặc đọc khối kế hoạch/thời gian theo khoảng riêng. Không SaveChanges, không tăng phiên bản học tập.
- Không thêm model/bảng/migration. Tổng hợp thời gian dùng các trường đã có trên HEAD, không phụ thuộc thay đổi sửa giờ đang làm ở nhánh khác. Không đọc lịch sử sửa giờ hoặc ghi chú tập trung.

## Kiểm chứng

- Build API/bộ LearningAdminChecks vào output riêng đạt 0 lỗi, 0 cảnh báo. Không ghi đè DLL của API người dùng.
- `scripts/test-admin-learning.ps1 -NoBuild`: đạt 116 kiểm tra HTTP/PostgreSQL thật, gồm JWT 401/403, 405 khi ghi, không lộ trường riêng tư/nhiệm vụ Personal/thùng rác/phiên tự do, loại Admin, giữ Student đã khóa, tổng trên nhiều trang, hoàn thành, lọc kỳ, hạn kế thừa, thời gian bấm giờ/nhập tay và không thay nhiệm vụ/phiên bản.
- Các lượt đầu gặp lỗi bộ chạy PostgreSQL trên Windows: pipe tiến trình con và Start-Process -Wait chờ cả cây postgres. Đã sửa chạy ẩn, tách stdout/stderr và chỉ WaitForExit của pg_ctl; lượt cuối đạt. Các cụm test do lượt bị ngắt tạo đã được kiểm tra/dừng, không dừng cụm DB ứng dụng.
- Frontend build đạt, có cảnh báo Vite chunk >500 kB. Ba test ngày/múi giờ/DST/phản hồi sai đạt. Chrome `--admin-learning` đạt lọc/phân trang, chọn Student/môn/kỳ, quá hạn/bước nhỏ, lỗi/thử lại, rỗng, thời gian tổng hợp, chỉ GET và mobile không tràn.
- Chạy toàn bộ `npm test` trong repo frontend hiện hành: 186/186 đạt. `git diff --check` đạt; đã đọc lại controller, service/DTO mới, đăng ký, giao diện/bộ kiểm thử và tài liệu. Không sửa các thay đổi nghiệp vụ khác đang có trong working tree.
- Khi kiểm tra cuối phát hiện checkout gốc ở cả hai repo đã được chuyển sang nhánh `feat/hoan-thien-lap-ke-hoach` trong lúc thực hiện. Đã mở hai worktree thực sự của nhánh theo dõi tại `.worktrees/admin-theo-doi-hoc-tap-backend` và `.worktrees/admin-theo-doi-hoc-tap-frontend`, chỉ mang các thay đổi admin. Không đổi nhánh lập kế hoạch hoặc chép những thay đổi học tập chưa commit vào nhánh admin.
- Đã build/kiểm thử lại trong worktree độc lập trên HEAD: backend đạt 116 kiểm tra; frontend build đạt, toàn bộ 179/179 test và Chrome `--admin-learning` đạt. Không còn phụ thuộc trường lịch sử sửa giờ của nhánh đồng hồ; khối thời gian hiển thị số phiên hoàn thành và thời lượng bấm giờ/nhập tay.
- Sau khi tách đã sao lưu từng file ở `backend/obj/admin-learning-fe/checkout-backup` của workspace gốc, gỡ đúng các đoạn admin và chuẩn hóa LF/CRLF còn sót. Kiểm tra diff xác nhận các file admin từng sửa trong hai checkout lập kế hoạch không còn chênh lệch nội dung; các thay đổi khác được giữ nguyên. Cả bốn worktree admin qua kiểm tra diff; các cụm PostgreSQL kiểm thử đã dừng.
- Bài trình duyệt đầu lỗi cú pháp selector của bài test; sửa selector và chạy lại đạt. Đã xem ảnh `.backend-integration/screenshots/admin-learning.png`; ảnh dùng fixture, không phải dữ liệu thật.
- Chưa nghiệm thu trên phiên hoặc DB sinh viên thật. Không tự áp migration, restart server, commit, push hoặc triển khai.

## Việc tiếp theo

- Người dùng xem/đối chiếu code và thử trên môi trường của mình sau khi nạp backend mới; không cần migration mới cho phần theo dõi admin.
- Hai HTTP đọc thống kê/bảng có ảnh chụp riêng; không cam kết chung mốc dữ liệu giữa các request khi sinh viên đang sửa.
- Không dùng số giờ hoặc tiến độ tự khai để kết luận chất lượng học tập. Không triển khai nhật ký admin hay gửi thông báo chung trong nhánh chỉ đọc này.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo; không tự commit/push.
