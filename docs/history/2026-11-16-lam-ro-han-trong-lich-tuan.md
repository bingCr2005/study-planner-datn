# 2026-11-16 — Làm rõ hạn nộp trong lịch tuần

## Yêu cầu và mục đích

- Làm lại dải Hạn nộp phía trên trục giờ vì tên trên thẻ bị cắt, vùng nhỏ phải cuộn và danh sách hạn của một ngày khó xem. Kiểm tra trường hợp nhiệm vụ `hihi` bị thay bằng bước giữ tiến độ kế thừa hạn của nó.
- Thiết kế chốt hai thẻ dễ đọc mỗi ngày và một nút mở danh sách ngay trong lịch tuần để sinh viên vẫn thấy bối cảnh của cả tuần.
- Bản ghi liên quan [Cải tiến thời khóa biểu](2026-11-11-cai-tien-thoi-khoa-bieu.md) và [Đổi tên bước giữ tiến độ cũ](2026-11-16-doi-ten-buoc-giu-tien-do.md).

## Code và logic thay đổi

- FE `src/hoc-tap/LichKhungNhin.tsx` và `src/hoc-tap/lich.css`. Trước đó, thẻ hạn ở chế độ Tuần có tên trên một dòng, vùng thẻ có thanh cuộn riêng và nút `+N hạn` chuyển sang chế độ Ngày. Dải hạn trên các chế độ có trục giờ hiện tối đa hai thẻ mỗi ngày. Tên có tối đa hai dòng và giờ nằm bên dưới. **Xem thêm N hạn** mở danh sách đầy đủ trong ngày, chú thích giờ riêng và cho tên dài xuống dòng.
- FE `src/hoc-tap/LichView.tsx` gửi `gomHanKeThua=true` khi truy vấn dữ liệu màn Thời khóa biểu. Trước đó, lịch dựa vào truy vấn mặc định lấy nhiệm vụ lá nên có thể hiển thị tên bước kế thừa thay cho nhiệm vụ đã đặt hạn.
- BE [ThoiKhoaBieuController](../../backend/StudyPlanner.Api/Controllers/ThoiKhoaBieuController.cs) chuyển tham số vào [ThoiKhoaBieuService](../../backend/StudyPlanner.Application/HocTap/ThoiKhoaBieu/ThoiKhoaBieuService.cs). Khi bật tham số, truy vấn lọc nhiệm vụ có `HanHoanThanh` đặt trực tiếp rồi dùng biểu thức hạn hiện có để lấy các mốc trong khoảng lịch. Nhiệm vụ có hạn đặt riêng luôn có mốc tương ứng, kể cả khi là nhiệm vụ con.
- Bấm thẻ hoặc mục trong danh sách gọi luồng mở chi tiết nhiệm vụ của lịch. Khung danh sách tự điều chỉnh vị trí trong màn hình và có thể đóng bằng nút **Đóng**. Ngày và chế độ xem hiện tại được giữ khi mở danh sách.
- Dữ liệu nhiệm vụ, tiến độ và nhắc hạn tiếp tục dùng quy tắc hạn kế thừa hiện có. API giữ hành vi mặc định cho bên gọi bỏ tham số mới. Thay đổi không yêu cầu migration. [Tài liệu module](../modules/05-lich-hoc-va-tong-quan.md) ghi lại khác biệt giữa hạn hiệu lực của nhiệm vụ và mốc hạn mà màn Thời khóa biểu hiển thị.

## Kiểm chứng

- Đối chiếu diff FE và BE, luồng tạo bước giữ tiến độ và truy vấn hạn lịch. `git diff --check` của hai repo và `tsc -b` của FE đều qua.
- Chạy `CORE_CALENDAR_DEADLINES_ONLY=1` với `node tests/browser-hoc-tap.mjs` trong repo FE. Bài kiểm tra qua với dữ liệu mô phỏng, gồm tham số truy vấn, hai dòng trên thẻ, mở danh sách bốn hạn ngay trong lịch tuần, tên dài, khung rộng 390 px và mở đúng nhiệm vụ từ danh sách. Đã xem ảnh chụp ở chiều rộng 390 px.
- Build `StudyPlanner.CoreChecks.csproj` Release qua với 0 lỗi và 0 cảnh báo. Chạy bộ CoreChecks đầy đủ trên PostgreSQL tạm qua, gồm 584 kiểm tra HTTP lõi. [IntegrationChecks](../../backend/tests/StudyPlanner.CoreChecks/IntegrationChecks.cs) kiểm tra hạn cha xuất hiện theo tên gốc, hạn chỉ kế thừa không tạo mốc riêng, hạn riêng của nhiệm vụ con còn đúng ngày giờ và truy vấn cũ vẫn chạy như trước.
- Chưa thử với bản ghi `hihi` trong tài khoản sinh viên thật. Bài test API dùng cơ sở dữ liệu tạm và trình duyệt dùng dữ liệu mô phỏng.

## Việc tiếp theo

- Khi xem trên local, khởi động lại BE nếu đang chạy bản cũ, tải lại giao diện rồi mở tuần có nhiệm vụ cần đối chiếu.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan.
