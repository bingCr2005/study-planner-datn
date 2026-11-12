# 2026-11-10 — Hộp thông báo nhắc hạn

## Yêu cầu và mục đích

- Người dùng yêu cầu triển khai đề xuất thông báo: nhắc hạn/quá hạn trước, chuông đếm chưa đọc, Tất cả/Chưa đọc, mở đúng nhiệm vụ, đọc tất cả và giữ trạng thái sau F5.
- Nguồn trước: [đặc tả thông báo](../modules/06-thong-bao-nhac-han.md), [module học tập đã merge](2026-11-06-giai-quyet-conflict-hoc-ky-mon-hoc.md).

## Code và logic thay đổi

- `backend/StudyPlanner.Application/ThongBao/QuyTacThongBao.cs`: quy tắc tạo/cập nhật một thông báo trên mỗi nhiệm vụ theo unique index hiện có; chỉ nhắc nhiệm vụ lá, dùng `QuyTacCay.HanHieuLuc` và giờ nhắc task hoặc mặc định tài khoản. Giá trị 0 nhắc đúng hạn; quá hạn khi `han < now`.
- `backend/StudyPlanner.Infrastructure/ThongBao/ThongBaoService.cs`: đồng bộ, danh sách phân trang, lọc chưa đọc và ghi thời điểm đọc. Dùng transaction và cùng khóa hàng tài khoản của task writes; không tăng `PhienBanLapLich`, không sửa nhiệm vụ/học kỳ/lịch.
- `backend/StudyPlanner.Api/Controllers/ThongBaoController.cs`: GET `/api/v1/notifications?chuaDoc=false&trang=1&kichThuocTrang=20`, PATCH `/{id}/read` với `ngayPhat` của bản vừa xem, POST `/read-all` với `docDenLuc` từ GET. Đều lấy chủ từ JWT, không nhận owner từ client. Chỉ trả Delivered. Không dùng If-Match học tập vì trạng thái đọc độc lập dữ liệu lịch.
- `backend/StudyPlanner.Api/ThongBao/ThongBaoRegistration.cs` và `Program.cs`: đăng ký service/worker. Worker chạy ngay khi khởi động rồi mỗi phút, đọc tài khoản theo lô 100, scope/giao dịch riêng từng tài khoản; lỗi thử ở lượt sau, không phát ngoài transaction.
- `backend/tests/StudyPlanner.NotificationChecks/`: kiểm thử quy tắc và PostgreSQL tùy chọn, yêu cầu database localhost biệt lập; không dùng connection string ứng dụng.
- FE nằm ở repo `study-planner-frontend`: chuông dùng API mới, số chưa đọc từ server, bộ lọc/phân trang, trạng thái lỗi/thử lại, đọc tất cả, mở đúng query task/semester/subject; dữ liệu trong bộ nhớ, trạng thái đọc ở DB.

Trước: FE đọc mảng nhiệm vụ cũ rỗng, BE chỉ có entity/schema và lịch nhắc chưa có worker/hộp thư API. Sau: hộp thư hoạt động cho toàn bộ tài khoản, không phụ thuộc học kỳ được chọn.

Chính sách cụ thể so với đặc tả dự thảo: nhắc nhiệm vụ lá để không lặp cha/con. Khi qua hạn, cùng bản ghi chuyển sang giai đoạn quá hạn và chưa đọc một lần; không lưu lịch sử từng lần phát. Khởi động sau hạn phát thẳng quá hạn. Đổi hạn ra ngoài cửa sổ/bỏ hạn/biến thành cha hủy thông báo cũ; hoàn thành giữ lịch sử Delivered nhưng không phát thêm. Mở lại cùng hạn/giai đoạn không phát trùng. Đổi hạn vào cửa sổ dùng lại bản ghi và đặt lại chưa đọc khi mốc/giai đoạn thay đổi.

Worker hiện suy ra từ nhiệm vụ hiện hành, không tiêu thụ/xóa các hàng `LichNhac` do task service lập. Không sửa task service của module khác; `LichNhac` vẫn là lịch dự kiến hiện có. Chỉ `ThongBao` Delivered quyết định hộp thư. Cần hợp nhất cơ chế nếu sau này triển khai nhiều mốc/email/push. Không thêm migration hay chạy migration tự động.

Read idempotent giữ lần đọc đầu; phiên bản `ngayPhat` tránh thao tác cũ đọc nhầm giai đoạn mới. Read-all chỉ đánh dấu những hàng phát không muộn hơn mốc GET, không đọc nhầm thông báo mới xuất hiện trong lúc người dùng thao tác.

## Kiểm chứng

- Đã đối chiếu entity, unique/check constraints, `EfHocTapStore`, hạn kế thừa và JWT.
- Build API với `--artifacts-path` riêng thành công, 0 warnings/errors; không ghi đè DLL của BE người dùng đang chạy.
- `StudyPlanner.NotificationChecks`: 13 kiểm tra quy tắc đạt. Đã chạy PostgreSQL 17 riêng tại localhost; tạo DB ngẫu nhiên, chạy migration trong DB đó và dọn sau test. Kiểm tra đồng thời hai service, cách ly chủ sở hữu, hạn kế thừa, giữ trạng thái qua context mới, chuyển giai đoạn và cutoff đọc tất cả đều đạt.
- HTTP host kiểm thử dùng JWT thật: anonymous 401, tài khoản khác 404, DTO tiếng Việt, phân trang sai 400, thiếu snapshot 400, snapshot cũ 409, đọc/đọc tất cả 204 và cutoff tương lai 400 đều đạt.
- FE build và 118 unit tests đạt; lint các file thông báo không cảnh báo. Chrome kiểm tra badge, lọc, phân trang, F5, phạm vi tài khoản, lỗi/thử lại, link chi tiết và tên dài mobile đạt. Chrome hồi quy Hôm nay (ưu tiên theo hạn, học kỳ, chi tiết tên dài) đạt. Browser tests dùng API fixture; BE integration dùng DB riêng, không coi là thử trên dữ liệu thật của người dùng.
- Khi kiểm tra đã phát hiện và sửa annotation Required trên record DTO, tránh lỗi server khi request thiếu ngày phát. Không sửa source nhiệm vụ.
- Chưa chạy kiểm tra trực tiếp trên phiên người dùng/DB ứng dụng; BE hiện chạy bản cũ cần khởi động lại để nạp API/worker mới. Không tự dừng tiến trình BE người dùng đang chạy.

## Việc tiếp theo

- Chưa có nhắc lớp học/phiên tự học, email, browser push hoặc nhiều mốc. Backend phải đang chạy để worker phát; trình duyệt đọc lại mỗi phút/khi focus/lưu dữ liệu. Đây không phải SignalR/push thời gian thực.
- Với dữ liệu lớn, tối ưu chọn tài khoản/nhắc đến hạn; hiện đọc cây nhiệm vụ của một tài khoản để tính hạn kế thừa chính xác.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code và ghi các giới hạn.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR: chưa tạo.
