# 2026-11-17 — Chọn chế độ và sửa thời gian thực học

## Yêu cầu và mục đích

- Người dùng yêu cầu chọn Pomodoro hoặc Stopwatch khi bắt đầu phiên từ lịch, cho phép sửa thời gian đã lưu và bổ sung test case. Kiểm tra đầu vào, chặn khoảng trùng và tương lai, lưu lịch sử chỉnh sửa.
- Luồng trước đó chuyển ngay sang Stopwatch khi mở phiên dự kiến. Lịch sử chỉ sửa ghi chú. Backend đã hỗ trợ tạo phiên có chế độ được gửi lên, nhưng frontend luôn gửi Stopwatch với phiên từ lịch.
- Đã đối chiếu [module Đồng hồ tập trung](../modules/09-dong-ho-tap-trung.md), code đồng hồ, cấu hình học, lịch dự kiến, thống kê và [bản ghi bắt đầu sớm](2026-11-12-bat-dau-phien-ngay-va-tien-do-sau-hoc.md).

## Code và logic thay đổi

- Hai repository dùng nhánh `feat/chon-che-do-va-sua-thoi-gian-hoc`. Khi bắt đầu, cả hai đã có nhiều thay đổi chưa commit từ công việc trước. Những thay đổi đó được giữ trong working tree.
- FE `src/hoc-tap/LichView.tsx`, hàm `PhienChiTiet`, mở lựa chọn sau khi bấm **Bắt đầu ngay** hoặc **Bắt đầu học**. **Quay lại** đóng lựa chọn. Chọn chế độ mới chuyển tới `/focus?session=...&mode=...`.
- FE `src/focus/FocusPage.tsx` truyền chế độ đã chọn vào `begin`. Mở lại phiên đang chạy lấy chế độ trong bản ghi hiện hành. Đường dẫn phiên cũ thiếu tham số mode tiếp tục dùng Stopwatch theo hành vi cũ.
- FE `src/focus/FocusRecordTime.tsx` thêm biểu mẫu từ **Lịch sử → Chi tiết lần tập trung → Sửa thời gian**. Mốc hiển thị theo múi giờ tài khoản, có giây và mili giây. Mốc chưa đổi giữ độ chính xác gốc. **Hủy** quay lại chi tiết. Lưu thành công làm mới lịch sử, tổng quan và khối vừa kết thúc. Xung đột phiên bản yêu cầu tải lại bản ghi trước khi gửi tiếp.
- [TapTrungController](../../backend/StudyPlanner.Api/Controllers/TapTrungController.cs) thêm `GET/PUT /api/v1/focus/{id}/time`. [Request](../../backend/StudyPlanner.Application/HocTap/TapTrung/TapTrungRequest.cs) bắt buộc có hai mốc. [Response](../../backend/StudyPlanner.Application/HocTap/TapTrung/TapTrungResponse.cs) trả bản ghi, cờ cần nhập thời lượng và danh sách sửa trước/sau.
- [TapTrungService](../../backend/StudyPlanner.Application/HocTap/TapTrung/TapTrungService.cs), các hàm `DocThoiGianAsync` và `SuaThoiGianAsync`, kiểm tra quyền sở hữu và trạng thái Completed. PUT đi qua khóa tài khoản và If-Match hiện có. Lỗi đầu vào trả 400, trùng khoảng hoặc trạng thái không hợp lệ trả 409, bản ghi ngoài tài khoản trả 404.
- Luồng sửa lấy mốc mới, kiểm tra bắt đầu trước kết thúc và kết thúc không muộn hơn giờ máy chủ, kiểm tra trùng với bản ghi khác hoặc đoạn học cũ, trừ các khoảng dừng, tính thời lượng, thay các đoạn thực học của chính bản ghi và ghi lịch sử trong cùng giao dịch. Hai khoảng được chạm mốc. Khoảng mới phải có thực học dương. Bản nhập tay giữ giới hạn 24 giờ. Phiên Stopwatch đo thực tế hỗ trợ sửa khoảng nhiều giờ.
- Mốc dừng ban đầu được lưu trong JSON để thao tác thu hẹp rồi mở rộng lại vẫn trừ đúng khoảng dừng. Chỉ cộng phần khoảng dừng nằm trong mốc mới. Các đoạn có nhiệm vụ giữ liên kết tới nhiệm vụ và phiên dự kiến, nên lịch đọc lại đúng số giây thực học.
- Phiên tự do có tạm dừng, hoặc bản ghi đã mất các đoạn khi nhiệm vụ bị xóa hẳn, chỉ còn mốc và tổng thời lượng. API yêu cầu nhập thêm thực học lớn hơn 0 và không vượt độ dài khoảng mới. Giao diện giải thích ngay dưới trường số giây. Đây là cách xử lý dữ liệu cũ được chọn trong lượt này.
- [IHocTapStore](../../backend/StudyPlanner.Application/HocTap/DungChung/IHocTapStore.cs) và [EfHocTapStore](../../backend/StudyPlanner.Infrastructure/HocTap/EfHocTapStore.cs) bổ sung đọc đoạn học kể cả nhiệm vụ trong thùng rác để kiểm tra trùng và giữ khoảng dừng.
- [Entity](../../backend/StudyPlanner.Domain/Entities/PhienTapTrung.cs), [cấu hình bảng](../../backend/StudyPlanner.Infrastructure/Data/Configurations/PhienTapTrungConfiguration.cs) và [migration](../../backend/StudyPlanner.Infrastructure/Migrations/20261117165006_LichSuSuaThoiGianTapTrung.cs) thêm JSON nullable chứa khoảng dừng gốc và các lần sửa. Lịch sử gồm giờ sửa của server, mốc trước/sau và tổng mili giây trước/sau. Lưu mốc và thời lượng giống bản hiện hành trả lại dữ liệu, giữ số lần sửa và revision.
- [ThongKeTapTrungService](../../backend/StudyPlanner.Application/HocTap/TapTrung/ThongKeTapTrungService.cs) và tổng quan trong TapTrungService tính tối đa một lượt cho mỗi Pomodoro bấm giờ đạt mục tiêu. Điều chỉnh thời lượng dài hơn mục tiêu vẫn là một lượt. Bản nhập tay tính các mục tiêu đầy đủ. Đây là quy tắc bổ sung để số lượt nhất quán khi sửa giờ.
- Cập nhật [module 07](../modules/07-goi-y-cong-viec.md) vì mô tả cũ ghi phiên từ lịch dùng đếm lên, và [module 09](../modules/09-dong-ho-tap-trung.md) để phản ánh API, giao diện, quy tắc và migration mới.

## Kiểm chứng

- Đọc lại code, diff của controller, service, DTO, entity, store, migration, snapshot, thống kê, biểu mẫu sửa giờ, chọn chế độ, bài kiểm thử và tài liệu liên quan.
- Build CoreChecks Release vào `backend/obj/chinh-thoi-gian-hoc` đạt 0 lỗi, 0 cảnh báo. `dotnet ef migrations has-pending-model-changes` trên build cùng đường dẫn xác nhận model khớp migration.
- Chạy [FocusHttpChecks](../../backend/tests/StudyPlanner.CoreChecks/FocusHttpChecks.cs) với PostgreSQL 17 tạm, cổng localhost tự chọn, database kiểm thử độc lập. Đạt 181 assertion HTTP/PostgreSQL. CoreChecks cũng đạt kiểm tra thời gian, 24 assertion nhiệm vụ, 23 assertion phiên tự học, quy tắc đồng hồ và 15 assertion lập kế hoạch. PostgreSQL tạm đã dừng sau kiểm thử.
- FE `npm run build` đạt. Vite cảnh báo kích thước chunk trên 500 kB. `node --test tests/focus.test.mjs tests/focus-statistics.test.mjs tests/hoc-tap.test.mjs` đạt 28/28.
- `CORE_SESSION_ONLY=1 node tests/browser-hoc-tap.mjs` đạt trên Chrome headless với 142 yêu cầu fixture. Bài trình duyệt kiểm tra kết nối giao diện và API giả lập. Kiểm tra lưu dữ liệu và validation backend dùng PostgreSQL riêng ở trên.
- `git diff --check` đạt ở hai repository.
- Các ca quan trọng đã bổ sung hoặc xác minh trong bộ kiểm thử hiện hành gồm các trường hợp sau.

| Trường hợp | Kết quả đã kiểm tra |
| --- | --- |
| Bắt đầu từ lịch, quay lại ở bước chọn | Phiên vẫn Planned, chưa có đồng hồ |
| Bắt đầu sớm với Stopwatch | Chế độ đúng, tải lại vẫn có phiên, thời gian thực học được lưu |
| Bắt đầu trong giờ với Pomodoro | Chế độ đúng, mục tiêu lấy cấu hình FE, liên kết phiên được giữ |
| Pomodoro đã hết khi đọc lại | Backend chốt đúng mục tiêu, phiên liên kết Completed |
| Đã học, tạm dừng rồi tiếp tục | Chỉ cộng các đoạn thực học |
| Sửa phiên có 20 giây học và 40 giây dừng, rút cuối 3 giây | Còn 17 giây, lịch và thống kê đọc lại đúng |
| Thu hẹp rồi mở rộng lại | Giữ khoảng dừng gốc, tổng về 20 giây, có ba lần sửa |
| Mốc đảo, bằng nhau, tương lai, chỉ nằm trong đoạn dừng | Từ chối, bản ghi giữ dữ liệu đã lưu |
| Khoảng sửa trùng bản khác, chạm mốc bản khác | Trùng bị từ chối, chạm mốc được chấp nhận |
| Mở/sửa bản ghi ngoài tài khoản, sửa bản đang chạy, dùng phiên bản cũ | Trả đúng lỗi quyền, trạng thái hoặc xung đột |
| Gửi lại cùng mốc và thời lượng | Giữ revision và số lần sửa |
| Bản tự do có dừng, thiếu tổng thực học hoặc vượt khoảng | Từ chối, nhận tổng hợp lệ và lưu chính xác |
| Sửa Pomodoro dài hơn một mục tiêu | Thống kê vẫn tính một lượt của bản đo đó |
| Sửa bản nhập tay dài hơn 24 giờ | Từ chối theo giới hạn bản nhập tay |
| Biểu mẫu có hai mốc bằng nhau, sửa hợp lệ và mở lịch sử | Hiện lỗi, lưu được mốc hợp lệ, xem được trước/sau |
| Kết thúc phiên và cập nhật tiến độ | Tiến độ được cập nhật qua thao tác riêng trên giao diện |

- Chưa chạy toàn bộ kịch bản `CORE_FOCUS_ONLY` trong lượt này. Kịch bản đó có bộ chọn cũ lệch giao diện chọn nhiệm vụ hiện hành. Lượt này chạy luồng riêng từ lịch và bộ unit tương ứng. Chưa nghiệm thu với database Development đang dùng của sinh viên.

## Việc tiếp theo

- Môi trường ứng dụng cần áp dụng migration mới và khởi động lại backend để sử dụng API sửa giờ. Migration đã được kiểm tra trên database độc lập, chưa áp dụng vào database Development trong lượt này.
- Người dùng nghiệm thu chọn chế độ từ lịch và sửa bản ghi thực tế theo các ca ở trên.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan chưa tạo.
