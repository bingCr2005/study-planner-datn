# Module 07 — Gợi ý và áp dụng lịch tự học

Trạng thái đối chiếu code ngày 2026-11-14: BE/FE đã có xếp lịch, giờ rảnh nhiều thứ, ngoại lệ theo ngày, cấu hình học dùng chung, cập nhật phần chưa học và nhắc kế hoạch tùy chọn. Xem [bản ghi triển khai/kiểm chứng](../history/2026-11-14-cai-tien-lap-ke-hoach.md), [đối chiếu trước cải tiến](../history/2026-11-14-doi-chieu-luong-lap-ke-hoach.md), [nhiệm vụ](03-nhiem-vu-va-tien-do.md) và [đồng hồ](09-dong-ho-tap-trung.md).

## 1. Luồng hiện hành

1. Thêm nhiệm vụ học tập/cá nhân, nhập thời lượng dự kiến cho lá; hạn/ưu tiên khi có. Thiếu ước lượng vẫn quản lý được việc nhưng chưa đủ dữ liệu để tự xếp lịch.
2. Nhập lịch lớp và mẫu giờ rảnh lặp theo tuần trong khoảng ngày hiệu lực. Lịch lớp/sự kiện/phiên đã đặt được trừ khi tính giờ khả dụng; không cần cấm mẫu giờ rảnh trùng lịch lớp ngay khi nhập.
3. Chọn phạm vi nhiệm vụ, ngày bắt đầu, số ngày và cách lập: **Tạo thêm lịch** hoặc **Cập nhật phần chưa học**. Chọn cha đưa các lá phù hợp trong nhánh vào phạm vi; không cộng cha như việc độc lập. Có thể chỉnh cấu hình học/nghỉ dùng chung trước khi tạo gợi ý.
4. Xem phiên đề xuất, lý do/cảnh báo/phần chưa xếp; chọn các phiên rồi áp dụng. Preview chưa ghi phiên, apply kiểm tra lại dữ liệu và ràng buộc trước khi lưu chung giao dịch.
5. Mở phiên → Bắt đầu ngay/Bắt đầu học → đồng hồ thực học. Phiên từ kế hoạch dùng đếm lên, không coi giờ dự kiến là giờ đã học. Kết thúc rồi cập nhật tiến độ/checklist nhiệm vụ hoặc để sau.

## 2. Thời lượng dự kiến và tiến độ

- `NhiemVu.ThoiLuongDuKienPhut` đã là cột nullable lưu phút: 2 giờ = 120, 3 giờ = 180. Không cần thêm cột giờ trùng chức năng.
- Phút còn lại = `ceil(ThoiLuongDuKienPhut × (100 − TienDo) / 100)` ở lá có ước lượng. Đây là ước lượng theo tiến độ, không phải thời gian còn lại chính xác suy từ đồng hồ.
- Khi xếp lịch mới, trừ thêm phần đã dành chỗ trong các phiên Planned/InProgress còn hiệu lực để tránh đặt trùng khối lượng. Giờ đã dành chỗ không phải giờ thực học đã hoàn thành.
- Lá có checklist tính tiến độ từ checklist; lá không checklist dùng phần trăm cập nhật. Cha tổng hợp tiến độ con trực tiếp và thời lượng từ các lá, không cộng ước lượng cha lẫn con.
- Ghi thực học không tự đổi thành phần trăm; không trừ số phút đã học thêm lần nữa khỏi công thức đã dựa trên tiến độ. Người dùng có thể chỉnh ước lượng khi việc khó/dễ hơn dự kiến rồi tạo gợi ý mới.

## 3. Giờ rảnh lặp hằng tuần

- `KhungGioRanh` lưu thứ 1–7, giờ địa phương, ngày hiệu lực và trạng thái sử dụng. `SinhGioRanh` sinh các lần xuất hiện cách 7 ngày khi xem lịch/lập kế hoạch. Tuần sau không cần nhập lại nếu mẫu còn hiệu lực; không cần job sao chép bản ghi mỗi tuần.
- Form tạo giờ rảnh cho chọn nhiều thứ rồi lưu một lần; ví dụ thứ Hai–thứ Sáu 20:00–23:00 tạo năm mẫu trong cùng giao dịch và tăng revision một lần. Nếu có mẫu trùng y hệt, trả `AVAILABILITY_DUPLICATE` để bỏ chọn thứ trùng; không nhân bản hoặc lưu một phần các thứ còn lại. Ngày kết thúc mặc định sau 90 ngày, có thể sửa.
- Sửa/xóa mẫu có preview ảnh hưởng các phiên đã đặt; yêu cầu xác nhận hủy phiên dự kiến mất tính hợp lệ. Nếu ảnh hưởng phiên đang học/đã đến giờ thì bị chặn theo quy tắc lịch. Không tự dời toàn bộ lịch đã áp dụng.
- Ngoại lệ theo mẫu/ngày cho **bỏ riêng ngày này**, thay giờ của ngày đó hoặc xóa ngoại lệ để trở lại mẫu gốc. Ngày phải đúng thứ và nằm trong hiệu lực mẫu; người khác không thể đọc/ghi ngoại lệ. Calendar, preview và apply dùng cùng nguồn giờ rảnh đã áp dụng ngoại lệ. Sửa ngoại lệ có preview ảnh hưởng/If-Match và xác nhận hủy các phiên mất tính hợp lệ; chặn khi ảnh hưởng phiên đang học/đã đến giờ. Các tuần khác không đổi.
- Sự kiện cá nhân có giờ vẫn tạo khoảng bận được bộ xếp tránh. Không có thao tác thay cả chuỗi “từ ngày này trở đi”; sửa mẫu hiện hành có hiệu lực theo phạm vi ngày đang lưu.
- Nhắc kế hoạch là tùy chọn mặc định tắt. Khi bật, lần mở/tải hộp nhắc trong ứng dụng tạo nhắc rà soát tuần và nhắc các mẫu giờ rảnh hết hiệu lực trong 7 ngày tới; khóa theo tài khoản/tuần hoặc ngày hết hiệu lực chống trùng. Có đọc một/đọc tất cả, phân trang và kiểm tra snapshot. Không có worker tự phát khi đóng ứng dụng, email hay push bên ngoài. Nhắc không phải điều kiện để lịch lặp hoạt động và không tự áp dụng lịch học.

## 4. Thuật toán đã có

`QuyTacLapLich.Xep` là thuật toán tham lam xác định, nhận việc/khe rảnh/thời điểm xét, không tự đọc DB hoặc đồng hồ bên trong:

1. Service sinh/gộp giờ rảnh theo múi giờ và trừ lịch lớp, sự kiện có giờ, phiên học còn hiệu lực cùng khoảng nghỉ. Loại phần quá khứ.
2. Sắp việc theo hạn hiệu lực gần nhất, không hạn sau cùng; cùng hạn xét ưu tiên cao trước, phần thời lượng cần xếp ít hơn trước, cuối cùng mã nhiệm vụ để ổn định.
3. Tìm khe sớm nhất trước hạn cho từng việc, chia nhiều phiên khi cần. Độ dài mục tiêu và khoảng nghỉ lấy từ cấu hình tài khoản; phiên thông thường ít nhất 15 phút, phần cuối có thể ngắn hơn khi xếp hết phần còn lại. Tính giới hạn học mỗi ngày theo múi giờ tài khoản, gồm cả phút tương lai đã dành trong các phiên Planned/InProgress và phút mới đề xuất. Khe qua nửa đêm được tách theo ngày địa phương; không cộng thời gian nghỉ vào hạn mức học.
4. Trừ phiên vừa chọn/khoảng nghỉ rồi tiếp tục. Trả lịch theo thời gian cùng lý do và phần chưa xếp.

Cảnh báo gồm thiếu ước lượng, quá hạn, học kỳ lưu trữ, không có/không đủ giờ hoặc hạn mức ngày, đã đủ chỗ hoặc phiên cũ cần rà soát. Việc quá hạn chưa được tự ép vào lịch tương lai. Phạm vi 1–31 ngày, tối đa 1.000 lá/2.000 phiên. Chưa chứng minh tối ưu toàn cục, chưa cá nhân hóa theo lịch sử và không phải AI.

### Cấu hình dùng chung và cập nhật kế hoạch

- Cấu hình server mặc định **25 phút học / 5 phút nghỉ / 180 phút học mỗi ngày**. Cho phép phiên 15–50 phút, nghỉ 1–30 phút, ngày 15–720 phút và không ít hơn một phiên. Cấu hình áp dụng cho đồng hồ Pomodoro phiên sau, bộ gợi ý và validator lưu/dời phiên; âm báo vẫn là tùy chọn trình duyệt.
- Thay cấu hình tăng revision và làm preview cũ hết hiệu lực; không tự sửa các phiên đã đặt hoặc mục tiêu đồng hồ đang chạy. Hạn mức ngày dùng để lập/kiểm tra lịch tương lai, không phải khóa đồng hồ tự do khi học vượt mức.
- Chế độ cập nhật chỉ xét các phiên **Planned chưa đến giờ, nằm trọn trong khoảng ngày và thuộc các lá hợp lệ đã chọn**. Các phiên đó tạm được bỏ khỏi phần dành chỗ/khoảng bận khi tính preview mới; các phiên ngoài phạm vi, đang học, đã học hoặc đã bỏ lỡ được giữ.
- Preview trả `maPhienThayThe` và `cauHinhHoc`; token ký cả danh sách thay thế và cấu hình cùng revision/quy tắc. Người dùng xác nhận rồi apply mới hủy đúng các phiên cũ và thêm các phiên mới đã chọn trong một giao dịch. Hỏng kiểm tra/lưu phải rollback cả hủy cũ và thêm mới. Bỏ chọn một phần phiên mới có thể làm kế hoạch mới chưa phủ đủ công việc; số lượng phiên cũ bị thay thế vẫn được thông báo trước khi xác nhận.

## 5. API và tính nhất quán

| Route | Hành vi |
| --- | --- |
| `POST /api/v1/planning/preview` | Nhận ngày bắt đầu/số ngày, học kỳ/môn tùy chọn, danh sách mã nhiệm vụ; trả phiên/cảnh báo/tổng phút/phiên bản/giờ tính và token preview |
| `POST /api/v1/planning/apply` | Nhận token và tập con nguyên vẹn các phiên đề xuất, yêu cầu `Idempotency-Key`; kiểm tra lại rồi ghi |
| `GET/PUT /api/v1/study-settings` | Cấu hình học dùng chung; PUT đủ `phienHocPhut`, `nghiGiuaPhienPhut`, `gioiHanHocMoiNgayPhut` và If-Match |
| `POST /api/v1/availability-windows/batch` | Tạo nhiều thứ cùng khung giờ/ngày hiệu lực trong một giao dịch |
| `GET /api/v1/availability-windows/{id}/exceptions` | Các ngoại lệ của mẫu thuộc tài khoản |
| `POST /api/v1/availability-windows/{id}/exceptions/change-preview` | Xem ảnh hưởng thêm/sửa/xóa ngoại lệ, chưa ghi |
| `PUT/DELETE /api/v1/availability-windows/{id}/exceptions/{date}` | Lưu/bỏ ngoại lệ một ngày, If-Match và xác nhận ảnh hưởng khi cần |
| `GET/PUT /api/v1/planning-reminders/settings` | Bật/tắt nhắc kế hoạch; PUT `{dangBat}` và If-Match |
| `GET /api/v1/planning-reminders` | Tạo nhắc đến hạn khi mở hộp và trả danh sách/số chưa đọc |

- Chỉ dùng dữ liệu tài khoản đăng nhập. Preview không tạo phiên, token hết hạn sau 10 phút. Apply kiểm tra phiên bản nguồn/quy tắc/thời gian/ràng buộc; dữ liệu đã đổi cần tạo gợi ý mới, không tự sửa hạn/ưu tiên/tiến độ cho vừa lịch.
- Retry cùng key/nội dung trả kết quả đã lưu; cùng key khác nội dung bị từ chối. FE giữ yêu cầu chưa rõ kết quả để kiểm tra/thử lại, tránh tạo trùng sau mất phản hồi.
- Có thể bỏ chọn phiên; không sửa thời gian một phiên preview rồi gửi apply. Dời phiên dùng luồng riêng.
- Preview nhận `capNhatKeHoach`; bỏ qua/mặc định false vẫn là tạo thêm lịch. Apply kiểm tra lại cấu hình trong token, trạng thái/giờ của các phiên thay thế và hạn mức ngày. Retry sau mất phản hồi vẫn trả đúng batch đã hoàn tất, không hủy/thêm lần hai.

## 6. File và kiểm chứng

- [NhiemVu](../../backend/StudyPlanner.Domain/Entities/NhiemVu.cs), [KhungGioRanh](../../backend/StudyPlanner.Domain/Entities/KhungGioRanh.cs), [sinh lịch lặp](../../backend/StudyPlanner.Application/HocTap/DungChung/QuyTacThoiGian.cs), [dịch vụ giờ rảnh](../../backend/StudyPlanner.Application/HocTap/ThoiKhoaBieu/KhungGioRanhService.cs).
- [QuyTacLapLich](../../backend/StudyPlanner.Application/HocTap/LapLich/QuyTacLapLich.cs), [LapLichService](../../backend/StudyPlanner.Application/HocTap/LapLich/LapLichService.cs), [QuyTacDatPhien](../../backend/StudyPlanner.Application/HocTap/PhienTuHoc/QuyTacDatPhien.cs), [controller](../../backend/StudyPlanner.Api/Controllers/LapLichController.cs).
- [CauHinhHoc](../../backend/StudyPlanner.Application/HocTap/LapLich/CauHinhHoc.cs), [QuyTacGioRanh](../../backend/StudyPlanner.Application/HocTap/ThoiKhoaBieu/QuyTacGioRanh.cs) và [NhacKeHoachService](../../backend/StudyPlanner.Application/HocTap/NhacKeHoach/NhacKeHoachService.cs) phụ trách cấu hình, ngoại lệ và nhắc tùy chọn.
- FE ở `C:/Users/ADMIN/Desktop/Study_Planner_FE/src/`: `hoc-tap/LichView.tsx`, `hoc-tap/LapLichView.tsx`, `focus/FocusPage.tsx`.
- FE có thêm `hoc-tap/StudySettings.tsx` dùng chung form cấu hình ở lịch và đồng hồ. [Kịch bản tự học](../history/2026-11-11-kiem-thu-luong-tu-hoc-dau-cuoi.md) là ví dụ lịch lớp/giờ rảnh/phiên/tiến độ ở quy tắc thời lượng trước đây; chỉnh độ dài phiên/nghỉ theo cấu hình hiện hành khi chạy lại.
- Migration `20261114092625_CaiTienLapKeHoach` thêm dữ liệu cấu hình/ngoại lệ/nhắc. Đã áp dụng vào database Development ngày 14/11/2026 sau khi sao lưu và diễn tập khôi phục/nâng cấp; xem [bản ghi database](../history/2026-11-14-ap-dung-migration-database.md). Khởi động lại BE và tải lại FE để nạp code mới. Kết quả kiểm chứng nằm trong [bản ghi triển khai 14/11](../history/2026-11-14-cai-tien-lap-ke-hoach.md); không nhầm với lượt đối chiếu tài liệu trước đó cùng ngày.

Đánh giá chất lượng gợi ý cần cùng bộ đầu vào: số phút xếp được trước hạn, xung đột và phần chưa xếp/lý do. Thêm tên thuật toán phức tạp chưa đủ để khẳng định lịch tốt hơn.
