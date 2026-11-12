# 2026-11-12 — Admin mở Tổng quan sau đăng nhập

## Yêu cầu và mục đích

- Yêu cầu: chỉnh đường dẫn để đăng nhập admin xong mở trang Tổng quan trước tiên.
- Đã đọc [bản ghi Tổng quan admin](2026-11-12-tong-quan-admin.md), [quy tắc ưu tiên](2026-11-12-quan-ly-quy-tac-uu-tien.md) và code frontend hiện tại. Trang Tổng quan đã có tại `/admin/overview`, nhưng `roleHome` và route index `/admin` vẫn trỏ vào `/admin/students`.
- Frontend thực tế nằm trong repo `D:/study-planner-frontend`. Repo này đang ở `main` và có các thay đổi quản trị chưa commit; tạo nhánh `fix/trang-mac-dinh-admin` và giữ nguyên các thay đổi có sẵn. Backend tiếp tục nhánh nhóm quản trị hiện tại.

## Code và logic thay đổi

- FE `src/auth/session.ts`: `roleHome('admin')` trả `/admin/overview`; sinh viên vẫn trả `/today`.
- FE `src/admin/AdminRoutes.tsx`: route index dùng `roleHome('admin')`, thống nhất với đăng nhập, tránh hai cấu hình mặc định khác nhau.
- Luồng: đăng nhập → API xác nhận vai trò → `AuthPage` gọi `roleHome` → Tổng quan. Phiên admin mở `/`, `/login` hoặc trang sinh viên cũng dùng hàm này; mở `/admin` chuyển theo route index; route admin không tồn tại về `/admin` rồi Tổng quan. Các chuyển hướng tiếp tục dùng `replace`.
- Không đổi route danh sách, chi tiết sinh viên hoặc quy tắc; truy cập trực tiếp các trang này vẫn được kiểm tra quyền như trước. Không sửa API hoặc dữ liệu.
- FE `tests/auth.test.mjs`, `tests/browser-auth.mjs`, `tests/browser-overview.mjs`, `tests/browser-priority-rules.mjs`: cập nhật điểm đến sau đăng nhập. Các kiểm tra danh sách bấm menu Tài khoản sinh viên trước khi kiểm tra bảng; thêm kiểm tra `/admin`, `/`, `/login`, đường dẫn admin không tồn tại và tải lại Tổng quan.
- FE `README.md` trước đó mô tả Admin vào danh sách sinh viên; cập nhật thành Tổng quan và bổ sung luồng điều hướng vào `docs/tong-quan-admin.md`.

## Kiểm chứng

- Đã đọc lại code đăng nhập, phân quyền, route gốc, route quản trị và diff liên quan; `git diff --check` FE đạt.
- Lần chạy đầu trong sandbox bị `EPERM` vì Vite/TypeScript cần ghi file tạm trong repo FE ngoài workspace. Đã chạy lại với quyền được cấp, kết quả dưới đây là lần chạy lại.
- FE `npm.cmd test`: 129/129 kiểm thử đạt, gồm xác nhận vai trò server và đường dẫn mặc định của admin/sinh viên.
- FE `npm.cmd run build`: TypeScript và Vite build thành công. Oxlint sáu file code/kiểm thử thay đổi đạt, không báo lỗi.
- FE `npm.cmd run test:auth:browser -- --overview`: đạt; đăng nhập mở thẳng Tổng quan, kiểm tra thêm `/admin`, `/`, `/login`, `/admin/unknown` và tải lại `/admin/overview`; menu Tổng quan được chọn và dữ liệu hiển thị đúng. Các kiểm tra thẻ thống kê, danh sách/chi tiết và xử lý lỗi vẫn đạt.
- FE `npm.cmd run test:auth:browser`: cả hai lượt đạt, gồm API fixture và mock lưu trữ thực tế. Xác nhận admin/demo admin đến Tổng quan, sinh viên đến Hôm nay, chuyển từ trang sinh viên về Tổng quan theo quyền, và tiếp tục mở danh sách/chi tiết qua menu.
- Trình duyệt dùng fixture/mock biệt lập, không đăng nhập hoặc ghi dữ liệu vào DB ứng dụng. Không chạy lại kiểm thử BE vì không thay đổi backend.

## Việc tiếp theo

- Chưa commit/push; không khởi động lại backend.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
