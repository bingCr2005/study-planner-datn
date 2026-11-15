# 2026-11-12 — Sửa nhiệm vụ ngay tại trang Tập trung

## Yêu cầu và mục đích

- Người dùng làm rõ rằng cửa sổ nhiệm vụ mở từ đồng hồ phải cho chỉnh sửa ngay tại trang Tập trung, như ảnh tham khảo; không yêu cầu chuyển sang trang Nhiệm vụ. Đồng thời hỏi cách lịch sử Focus Record nhóm nhiều lượt học cho cùng một nhiệm vụ.
- Bản trước triển khai cửa sổ chỉ đọc rồi dẫn sang `/tasks`; đó là điểm hiểu thiếu yêu cầu. Bản này thay phần chỉ đọc bằng chính component chi tiết/chỉnh sửa nhiệm vụ đang dùng trên trang Nhiệm vụ, tránh duy trì hai bộ quy tắc checklist/tiến độ khác nhau.
- Đã đọc [mục lục](README.md), [bản ghi cửa sổ/thống kê ban đầu](2026-11-12-xem-nhiem-vu-va-thong-ke-tap-trung.md), module 09, code task detail/form/API, focus, context và harness hiện tại. Tiếp tục nhánh `feat/bat-dau-phien-hoc-ngay`, giữ thay đổi trước.

## Code và logic thay đổi

- FE ở `C:/Users/ADMIN/Desktop/Study_Planner_FE`. Thêm `src/hoc-tap/NhiemVuContext.tsx`: context nhỏ cho các thao tác nhiệm vụ, không mang bộ lọc/phân trang riêng của màn danh sách. `HocTapPage.tsx` cung cấp cùng dữ liệu/hành vi hiện có qua context mới; các form `BieuMau`/`ChonPhamVi` đọc context dùng chung.
- `src/hoc-tap/NhiemVuView.tsx`: `ChiTietNhiemVu`, `SuaNhiemVu`, menu thao tác và các form tiến độ/phạm vi/cha dùng context mới. Thêm prop `addon` cho vùng tổng thời gian focus. Logic ghi, request preview, If-Match, xác nhận ảnh hưởng lịch và tính tiến độ phía BE giữ cùng đường code đang dùng ở trang Nhiệm vụ.
- `src/focus/FocusTaskPreview.tsx`: bỏ phần xem chỉ đọc và link Chỉnh sửa trong Nhiệm vụ. Cung cấp context cho `ChiTietNhiemVu`, quản lý các lớp dialog con để mở Chỉnh sửa/tiến độ/bước con ngay trên `/focus`, đóng lớp trên trở về lớp dưới. Tổng focus vẫn đọc đúng nhiệm vụ và toàn bộ lịch sử, hủy request khi đóng/đổi trạng thái và có thử lại khi lỗi.
- `src/focus/FocusPage.tsx`: nút Cập nhật tiến độ nhiệm vụ sau khi lưu phiên cũng mở cửa sổ này tại chỗ thay cho Link sang `/tasks`. Browser kiểm tra thêm nút này giữ đường dẫn `/focus`.
- Có thể sửa mô tả/ghi chú, thêm/tick/đổi tên/xóa mục checklist, thêm bước nhỏ và mở bước con; menu Chỉnh sửa đổi tên/nội dung/ưu tiên/hạn/ước lượng; Cập nhật tiến độ dùng cho việc không có checklist theo quy tắc hiện tại. Checklist có tiến độ suy ra, không đồng thời nhập tay phần trăm. Các thao tác vẫn cần đăng nhập và được BE kiểm tra quyền sở hữu.
- Khi lưu thành công, tăng revision của cửa sổ và phát `studyplanner:refresh`; `FocusPage.tsx` cũng nghe `studyplanner:saved` để tải lại phiên bản dùng điều khiển đồng hồ. Tránh trường hợp sửa nhiệm vụ xong thì pause/resume/finish dùng If-Match cũ. Không gọi pause chỉ vì mở/sửa cửa sổ.
- Lỗi ghi giữ thông tin đã lưu trước đó, không tick giả thành công. Xung đột phiên bản hiện lỗi và nút tải dữ liệu mới; không tự gửi lại ghi. Thay đổi ảnh hưởng phiên học vẫn đi qua preview/xác nhận; nếu BE yêu cầu kết thúc phiên trước, hiển thị yêu cầu thay vì tự kết thúc.
- `src/focus/focus-details.css`: bổ sung trình bày tổng focus/thông báo trong detail dùng chung.
- Lịch sử focus hiện hành: mỗi nhóm `PhienTapTrung` hoàn tất là một bản ghi, nhiều lượt có thể gắn cùng `MaNhiemVu`. Pause/resume tạo các đoạn thực học trong cùng nhóm, không tạo nhiều dòng lịch sử. Nhóm theo ngày kết thúc theo múi giờ tài khoản; thời lượng thực học không cộng phần pause. Kết thúc rồi bắt đầu lại tạo nhóm khác. Tiêu đề bản ghi là ảnh chụp lúc học, vì thế đổi tên nhiệm vụ không viết lại lịch sử. Bấm tên mở nhiệm vụ hiện tại, bấm giờ/biểu tượng/thời lượng mở chi tiết bản ghi. Nút + nhập lượt đã học trước đó; chưa có xóa bản ghi đã lưu/menu lọc lịch sử như đầy đủ sản phẩm tham khảo.
- Không thay BE/API/schema/migration trong bản này. Cập nhật [module 09](../modules/09-dong-ho-tap-trung.md) theo hành vi đã xác minh. Bản ghi trước giữ nội dung lịch sử về phiên bản chỉ đọc, được thay thế bởi bản này.

## Kiểm chứng

- Đã đọc lại diff context/form/shared detail và code adapter focus, listener cập nhật phiên bản, CSS, browser fixture và các assertions. `git diff --check` đạt ở hai repository.
- FE `npm run build` đạt TypeScript và production build; Vite vẫn cảnh báo chunk JS chính trên 500 kB. Không có lỗi biên dịch.
- `node --test tests/focus-statistics.test.mjs tests/focus.test.mjs tests/hoc-tap.test.mjs`: **27/27 đạt**.
- `CORE_FOCUS_ONLY=1` với `node tests/browser-hoc-tap.mjs`: **đạt, 193 fixture requests**. Giữ các kiểm tra đồng hồ/thống kê cũ; đổi selector sang detail dùng chung và thêm kiểm tra ghi nhiệm vụ trên `/focus`, sửa tên/nội dung, checkbox cập nhật phần trăm, thêm bước nhỏ đúng cha, dialog chỉnh sửa lồng trên thống kê/nhiệm vụ và mobile.
- Fixture thêm lỗi ghi 503 một lần, xung đột 409 một lần và kiểm tra If-Match ở luồng focus. Đã xác nhận lỗi không tick giả, tải lại rồi lưu đúng, đồng hồ vẫn Running với cùng ID sau các lần sửa; pause/resume tiếp theo thành công với phiên bản mới. Kiểm tra đồng hồ không tự ghi progress/complete chạy trước các thao tác sửa do người dùng mô phỏng.
- `CORE_SESSION_ONLY=1` cùng browser harness: đạt **69 fixture requests**, gồm bắt đầu sớm, đồng hồ, khôi phục, kết thúc không tự tăng tiến độ và cập nhật tiến độ tùy chọn qua form dùng chung.
- Đã xem ảnh `backend/obj/focus-edit-screenshots/focus-task-preview.png` và `focus-task-edit-mobile.png`: checkbox có thể thao tác, tổng focus, thêm bước nhỏ, bố cục điện thoại không tràn ngang. Ảnh/data là fixture riêng.
- Lượt browser đầu phát hiện selector test viết sai escape tên dialog và tên form; đã sửa selector và chạy lại đạt. Không phải lỗi ứng dụng.
- Không chạy lại BE/PostgreSQL vì không sửa BE; kết quả 121 assertions thuộc bản trước. Browser dùng HTTP fixtures, chưa thử ghi bằng tài khoản/database người dùng. Bộ browser toàn ứng dụng chưa chạy lại; giới hạn lỗi bố cục menu ghi ở bản trước vẫn được giữ, không tuyên bố toàn bộ suite đạt.

## Việc tiếp theo

- Tải lại frontend để dùng cửa sổ sửa tại trang Tập trung. Không có migration bổ sung. Người dùng kiểm tra trải nghiệm thực tế; chưa commit/push/triển khai.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
