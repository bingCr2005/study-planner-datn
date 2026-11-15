# 2026-11-14 — Dùng chung logo giữa sinh viên và admin

## Yêu cầu và mục đích

- Người dùng yêu cầu đưa logo của trang sinh viên vào trang admin.
- Đã đọc mục lục, [bản ghi chỉnh layout admin](2026-11-14-doi-ten-danh-muc-mon-hoc-hoc-ky.md), code Brand, AppLayout, AdminLayout và CSS hiện tại. Tiếp tục nhánh nhóm chức năng hiện có.

## Code và logic thay đổi

- FE `src/admin/AdminLayout.tsx`: thay khối biểu tượng sách nền xanh và chữ `studyplanner.` bằng component `Brand` mà sidebar sinh viên đang dùng; giữ dòng Trang quản trị bên dưới.
- FE `src/admin/admin.css`: bỏ các rule của logo cũ; căn component dùng chung với ảnh 44px, chữ Study Planner và khoảng cách tương ứng sidebar sinh viên. CSS được giới hạn trong khối admin vì CSS của Brand phía sinh viên có scope `.student-app`, không áp dụng cho layout admin.
- Dùng lại asset `src/assets/studyplanner-logo.png`; không tạo hoặc sửa file ảnh. Các trang admin cùng dùng AdminLayout nên logo được đổi thống nhất.
- Luồng: mở route admin → AdminLayout render Brand → dùng cùng logo và tên thương hiệu với sinh viên. Không đổi route, menu, API hoặc dữ liệu.

## Kiểm chứng

- Đã đọc lại JSX/CSS mới, xác minh dùng cùng component/asset với sidebar sinh viên và không còn rule nền xanh/khung logo cũ.
- Build TypeScript/Vite và lint AdminLayout đạt; `git diff --check` đạt. Cảnh báo chunk >500 kB vẫn có như trước.
- Không viết/chạy thêm kiểm thử nghiệp vụ cho thay đổi logo. Chưa chụp lại trang admin trên trình duyệt trong lượt này.

## Việc tiếp theo

- F5 frontend để xem logo mới. Chưa commit/push; không khởi động lại backend.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR: chưa tạo.
