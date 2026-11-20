# 2026-11-17 — Ghim nhiệm vụ lên đầu danh sách

## Yêu cầu và mục đích

- Người dùng muốn chọn nhanh vài việc quan trọng giữa nhiều việc cùng ưu tiên cao. Dùng ảnh TickTick làm tham khảo cho thao tác **⋯ → Ghim nhiệm vụ** và nhóm **Đã ghim** ở đầu danh sách.
- [Bản ghi menu nhiệm vụ](2026-11-10-menu-thao-tac-nhiem-vu.md) và mã trước thay đổi đều xác nhận menu chưa có ghim. Tách nhánh `feat/ghim-nhiem-vu` ở cả backend và frontend, giữ các thay đổi đang có trong hai worktree.

## Code và logic thay đổi

- [NhiemVu.cs](../../backend/StudyPlanner.Domain/Entities/NhiemVu.cs) thêm `DaGhim`. [Migration](../../backend/StudyPlanner.Infrastructure/Migrations/20261117083959_GhimNhiemVu.cs) lưu `nhiem_vu.da_ghim` với mặc định `false`, gồm các nhiệm vụ đã tồn tại.
- [NhiemVuController.cs](../../backend/StudyPlanner.Api/Controllers/NhiemVuController.cs) nhận `PATCH /api/v1/tasks/{id}/pin`, body `daGhim` bắt buộc và `If-Match` theo phiên bản dữ liệu. [NhiemVuService.cs](../../backend/StudyPlanner.Application/HocTap/NhiemVu/NhiemVuService.cs) mở giao dịch có khóa tài khoản, tìm đúng nhiệm vụ đang hoạt động thuộc chủ, lưu cờ và tăng phiên bản khi thực sự đổi. Gửi lại cùng giá trị với phiên bản hiện tại không tăng phiên bản. API trả `daGhim` trong DTO nhiệm vụ.
- Trước đây API sắp theo tiêu chí đã chọn rồi phân trang. Màn Nhiệm vụ nay truyền `ghimTruoc=true`. API áp dụng bộ lọc, xếp việc ghim trước, xếp theo tiêu chí hiện tại trong từng nhóm rồi phân trang. Khi một bước nhỏ đã ghim khớp bộ lọc của danh sách dạng cây, bước được đưa lên đầu như một dòng riêng. Quan hệ cha/con được giữ và frontend chỉ hiện một dòng cho bước đó trong cùng trang. Màn Đã hoàn thành tiếp tục nhóm theo ngày. Cờ được giữ qua hoàn thành, mở lại, chuyển vào thùng rác hoặc khôi phục.
- Trong `Study_Planner_FE`, `src/hoc-tap/NhiemVuView.tsx` dùng menu chung để ghim/bỏ ghim từ danh sách và chi tiết, gồm cả chi tiết mở tại Tập trung. Giao diện báo thành công sau phản hồi của API rồi tải dữ liệu mới. Lỗi dùng đường hiển thị sẵn có của danh sách/chi tiết. `boLoc.ts` bật cách xếp cho màn Nhiệm vụ, `hoc-tap.css` và `src/components/Icon.tsx` hiển thị biểu tượng ghim và tiêu đề nhóm.
- Ghim chỉ điều khiển cách hiển thị, nên API dành riêng cho thao tác này giữ nguyên mức ưu tiên, hạn, tiến độ, phiên học và lịch sử hoạt động. Ghi ghim dùng phiên bản chung của tài khoản, nên bản xem trước lịch đã mở có thể cần tạo lại sau một lần ghim/bỏ ghim.

## Kiểm chứng

- Đối chiếu đường đi từ menu, API có phiên bản, bộ lọc, phân trang, DTO và cột lưu.
- Backend build Release đạt, 0 cảnh báo và 0 lỗi. EF Core xác nhận mô hình khớp migration mới.
- Frontend build TypeScript/Vite đạt, Vite báo chunk JS chính lớn hơn 500 kB. `npm test` đạt 176/176. `CORE_PIN_ONLY=1` với Chromium headless xác nhận ghim bước từ menu, chỉ hiện một dòng khi cây đang mở, tải lại vẫn còn ghim và bỏ ghim từ chi tiết. Kiểm thử trình duyệt dùng API giả.
- Lượt PostgreSQL đầu phát hiện fixture nâng cấp tiến độ dùng entity hiện hành để ghi vào lược đồ cũ. Đã chuyển phần tạo dữ liệu cũ sang SQL với đúng các cột lúc đó và thêm kiểm tra mọi nhiệm vụ cũ có cờ `false` sau nâng cấp.
- Lượt chạy lại đạt 31 kiểm tra nâng cấp và 623 kiểm tra HTTP nghiệp vụ trên PostgreSQL tạm. Các ca ghim kiểm tra quyền chủ, trường `daGhim` bắt buộc, thứ tự trước phân trang, bộ lọc, ghim lặp không tăng phiên bản và gỡ bước ghim khỏi đầu danh sách. Các nhóm giờ rảnh, cấu hình/lập lại kế hoạch, nhắc kế hoạch, tập trung và đính kèm cũng đạt. Cụm thử đã dừng.
- `git diff --check` đạt cho cả hai repository. Đã đọc lại diff và mã liên quan.

## Việc tiếp theo

- Áp dụng migration vào database Development trước khi chạy backend có chức năng ghim. Lượt này kiểm thử trên dữ liệu tạm.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
