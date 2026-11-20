# 2026-11-18 — Nâng cấp Development và nghiệm thu sửa thời gian học

## Yêu cầu và mục đích

- Người dùng đồng ý hướng triển khai, yêu cầu sao lưu và áp dụng migration Development, khởi động lại backend, xác minh dữ liệu cũ và cập nhật toàn bộ kịch bản CORE_FOCUS_ONLY.
- Đối chiếu [bản ghi triển khai](2026-11-17-chon-che-do-va-sua-thoi-gian-hoc.md), module 09 và code thực tế trước khi thao tác.

## Code và logic thay đổi

- Tiếp tục nhánh `feat/chon-che-do-va-sua-thoi-gian-hoc` của hai repository.
- FE `tests/browser-focus.mjs` thay select phạm vi cũ bằng bộ lọc nổi hiện hành, đổi các bộ chọn hàng nhiệm vụ, nhãn tạm dừng/nghỉ và nút cập nhật tiến độ theo giao diện hiện tại. Dùng Escape đóng bảng chọn neo cạnh nút.
- Development thiếu hai migration `20261117083959_GhimNhiemVu` và `20261117165006_LichSuSuaThoiGianTapTrung`. Đã kiểm tra cả hai đều thêm cột, sao lưu rồi áp dụng thành công.
- Công cụ pg_dump cục bộ phiên bản 17 gặp lỗi lệch server PostgreSQL 18.7. Chuyển sang sao lưu logic JSON toàn bộ bảng public, bảng lịch sử migration và giá trị sequence trong giao dịch RepeatableRead. Đọc lại và đối chiếu nội dung JSON trước khi nâng cấp.
- Bản sao lưu tại `backend/obj/database-backups/focus-before-upgrade-20261117-171303.json`. Tên file dùng UTC, công việc hoàn tất ngày 18/11 theo múi giờ tài khoản. Dữ liệu sao lưu nằm trong thư mục bị Git bỏ qua.
- Hash từng bảng trước/sau migration khớp, loại hai cột vừa thêm khỏi phép so sánh để so đúng dữ liệu cũ. Các bản ghi và liên kết hiện có được giữ.
- Đã xác định backend Debug cũ giữ cổng 5059, dừng đúng tiến trình của repository và khởi động build Release mới với content root API và môi trường Development. Backend chạy nền ẩn trên `http://localhost:5059`.

## Kiểm chứng

- Development đọc được cả 6 bản ghi Completed, thời lượng bằng đúng giá trị lưu cũ. Lưu lại cùng mốc qua SuaThoiGianAsync thành công, giữ revision và số lần sửa. Tổng thống kê theo nhiệm vụ khớp tổng thời lượng các bản ghi.
- Đối chiếu lại hash toàn bộ dữ liệu sau các thao tác đọc và lưu cùng mốc đạt.
- Phục hồi dữ liệu từ JSON vào PostgreSQL 17 tạm, cổng localhost riêng. Dùng schema từ migration của source hiện hành, phục hồi bảng và sequence tương ứng. Các sequence dư từ lịch sử đổi tên được bỏ qua khi schema hiện hành không có.
- Trên bản sao, thực sự rút ngắn 1 mili giây của cả 6 bản ghi cũ qua SuaThoiGianAsync. Xác minh tổng thực học giảm chính xác 1 mili giây, lịch sử có mốc trước/sau, thời lượng phiên liên kết đọc lại đúng. Mốc sửa lấy từ đoạn thực học cuối, giữ đúng các khoảng nghỉ.
- So sánh toàn bộ bảng nhiệm vụ và phiên dự kiến trước/sau các lần sửa trên bản sao đạt. Tiến độ, checklist, trạng thái, mốc dự kiến và dữ liệu phiên đặt giữ đúng giá trị. PostgreSQL tạm đã dừng sau kiểm tra.
- `CORE_FOCUS_ONLY=1 node tests/browser-hoc-tap.mjs` đạt với 270 yêu cầu fixture. Chạy toàn bộ kịch bản gồm chọn nhiệm vụ học tập/cá nhân/tự do, cấu hình, Pomodoro hết giờ, pause/resume, tải lại, ghi chú, nghỉ, dock, nhập tay, bỏ phiên, thống kê, chỉnh nhiệm vụ và màn nhỏ.
- Backend mới ghi log Now listening, Application started và Hosting environment Development. Gọi `GET /api/v1/focus/state` khi chưa đăng nhập nhận HTTP 401 đúng yêu cầu xác thực.
- Đã đọc diff bài kiểm thử và tài liệu, `git diff --check` đạt ở hai repository.
- Bài trình duyệt dùng API fixture. Phần lưu, sửa dữ liệu cũ dùng service thật và PostgreSQL phục hồi. Kiểm tra HTTP/PostgreSQL 181 assertion của lượt triển khai trước tiếp tục là bằng chứng cho validation và xung đột. Lượt này bổ sung kiểm tra Development và bản sao dữ liệu cũ.

## Việc tiếp theo

- Người dùng nghiệm thu bằng tài khoản của mình trên Development. Các bản ghi cũ đã được đọc và lưu cùng mốc trên Development. Những lần thử thay đổi thời lượng thực sự thực hiện trên bản sao độc lập.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan chưa tạo.
