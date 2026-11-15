# 2026-11-12 — Admin quản lý quy tắc ưu tiên

## Yêu cầu và mục đích

- Người dùng yêu cầu làm trang Quy tắc ưu tiên để admin quản lý và áp dụng vào gợi ý.
- Trước: trang admin rỗng; FE Hôm nay dùng cố định hạn → mức quan trọng → trạng thái → mã. Đã đối chiếu [đặc tả gợi ý](../modules/07-goi-y-cong-viec.md), bảng/entity `QuyTacUuTien` và unique indexes hiện có.

## Code và logic thay đổi

- Application/UuTien/CauHinhUuTien.cs kiểm tra schema 1 và danh sách tiêu chí Deadline/Importance/InProgress, không trùng, bắt buộc Deadline.
- Infrastructure/UuTien/QuyTacUuTienService.cs: đọc quy tắc đang dùng, danh sách phân trang, lưu phiên bản mới bất biến, áp dụng và xóa phiên bản chưa dùng. Dùng bảng `quy_tac_uu_tien` đã có, không thêm migration.
- Controllers/Administration/QuyTacUuTienController.cs: GET/POST `/api/v1/admin/priority-rules`, POST `/{id}/activate`, DELETE `/{id}`. Bảo vệ bằng policy Admin.
- Controllers/QuyTacDangDungController.cs: GET `/api/v1/priority-rules/active` cho người dùng đã xác thực. Khi chưa có bản áp dụng, trả mặc định phiên bản 0. Config đang dùng sai schema thì báo lỗi, không âm thầm dùng mặc định.
- Program.cs đăng ký service. Chỉ thay đổi module mới/đăng ký, không sửa module nhiệm vụ, lịch hay thuật toán tự xếp giờ của người khác.
- Lưu mới chỉ tạo bản chưa dùng. Áp dụng dùng transaction + advisory lock chung cho mọi thay đổi quy tắc; kiểm tra phiên bản đang dùng mà admin vừa xem để chặn ghi đè từ tab cũ. Unique index chỉ cho một bản đang dùng. Cấp phiên bản bằng identity sequence hiện có và kiểm tra max để không tái sử dụng phiên bản đã xóa.
- FE repo `study-planner-frontend`: trang quản lý có tên, bật/tắt tiêu chí phụ, nút đổi thứ tự, xem thử bằng cùng hàm recommend của Hôm nay; lưu phiên bản mới, xác nhận áp dụng/xóa, lịch sử phân trang và xử lý lỗi. Mặc định vẫn hạn trước mức quan trọng, không đổi cho đến khi admin áp dụng bản khác.
- Hôm nay đọc config đang dùng; quá hạn luôn trước, chưa có hạn luôn cuối, giữa các việc cùng nhóm này so sánh theo tiêu chí đã cấu hình, cuối cùng theo mã tăng dần. Mốc 6/24/72/168 giờ chỉ mô tả độ gấp, không lấn thứ tự tiêu chí admin. Mặc định giữ nguyên kết quả xếp hạng trước đây.
- FE cập nhật quy tắc khi mở/quay lại/làm mới trang và mỗi phút trong tab hiện. Nếu tải lỗi, hiển thị lỗi; có thể giữ bản đã xác nhận trong bộ nhớ, không hiển thị xếp hạng theo mặc định giả khi chưa biết config server.

## Kiểm chứng

- Build API với artifacts path riêng thành công, 0 warnings/errors. Không ghi đè DLL của API người dùng đang chạy.
- `StudyPlanner.PriorityChecks` đạt kiểm tra schema/tiêu chí và PostgreSQL/HTTP thật trên database localhost tên ngẫu nhiên: JWT 401/403, quyền chỉ đọc của sinh viên, validate tên/schema, tạo bản chưa áp dụng, áp dụng, chống stale activation và hai yêu cầu đồng thời, giữ một bản active, xóa an toàn, không tái dùng phiên bản đã xóa, phân trang. Database kiểm thử được dọn sau chạy; không dùng DB ứng dụng.
- FE build và toàn bộ 124 kiểm thử đạt, gồm mock quyền/persistence. Lint các file mới không cảnh báo; một số cảnh báo React ở Dashboard/useToday đã tồn tại trước thay đổi này.
- Chrome `--priority-rules` đạt: đổi thứ tự/bật tắt tiêu chí đổi preview, lỗi ghi giữ form, lưu không tự áp dụng, hủy/xác nhận áp dụng, F5, chỉnh sửa lưu bản mới, xóa bản chưa dùng, xử lý 409, phân trang, cấu hình thực sự đổi thứ tự Hôm nay, lỗi cấu hình không âm thầm hiển thị mặc định và retry.
- Chrome `--today` và bộ auth/admin đầy đủ đạt. Đã kiểm tra đổi config khi lấy lại focus trên Hôm nay mà không tải lại các trang nhiệm vụ không cần thiết. Đã xem ảnh giao diện desktop trong `.backend-integration/screenshots/admin-priority-rules.png` của FE; dữ liệu trong ảnh là fixture kiểm thử.
- Kiểm thử trình duyệt dùng API fixture có kiểm soát, BE integration dùng PostgreSQL riêng. Chưa thử trên phiên người dùng/DB ứng dụng; cần restart BE để nạp API mới.

## Việc tiếp theo

- Phạm vi áp dụng là gợi ý Hôm nay, không thay thứ tự bộ lọc trên trang Nhiệm vụ hoặc thuật toán phân bổ giờ.
- Chưa có lịch sử người thao tác từng lần kích hoạt; schema chỉ lưu phiên bản cấu hình, tên, thời điểm tạo và trạng thái đang dùng.
- Backend đang chạy bản cũ cần khởi động lại để nạp API mới; không tự dừng tiến trình người dùng.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bảng, code hiện hành và giới hạn phạm vi.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR: chưa tạo.
