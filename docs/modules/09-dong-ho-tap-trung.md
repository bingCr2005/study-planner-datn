# Đồng hồ tập trung

Trạng thái đối chiếu ngày 14/11/2026: đã triển khai BE/FE, cấu hình phiên/nghỉ dùng chung với lập kế hoạch. Xem [bản ghi đồng hồ ban đầu](../history/2026-11-12-man-dong-ho-tap-trung-rieng.md) và [cải tiến lập kế hoạch/cấu hình](../history/2026-11-14-cai-tien-lap-ke-hoach.md). Module này mô tả code hiện hành.

## 1. Màn hình và luồng dùng

- Mục **Tập trung** trong menu chính mở `/focus`, bố cục đồng hồ bên trái, tổng quan và lịch sử bên phải. Màn nhỏ chuyển thành một cột.
- Pomodoro mặc định 25 phút, nghỉ 5 phút; thời lượng học/nghỉ và giới hạn học ngày lưu trên server theo tài khoản qua `/api/v1/study-settings`, dùng chung với bộ lập kế hoạch. Phiên cấu hình 15–50 phút, nghỉ 1–30 phút, hạn mức ngày 15–720 phút (mặc định 180, không ngắn hơn một phiên). Giao diện chờ đọc cấu hình thành công trước khi bắt đầu/lưu cài đặt, lỗi có Thử lại. Âm báo và mốc giờ nghỉ phía trình duyệt vẫn lưu cục bộ. Đếm lên không tự dừng theo mục tiêu Pomodoro.
- Trong khi chạy, bảng bên phải chuyển sang dòng thời gian của các phiên gần đây trong ngày và ghi chú, tương tự ảnh tham khảo. Khối thể hiện khoảng mở–kết thúc phiên, còn số phút chỉ cộng phần tập trung, không cộng tạm dừng. Khi kết thúc, tổng quan và lịch sử xuất hiện lại. Trục giờ cắt đúng biên ngày trong múi giờ tài khoản, kể cả ngày đổi giờ DST.
- Chọn một nhiệm vụ học tập, việc cá nhân hoặc **Tập trung tự do**. Danh sách lấy các việc chưa hoàn thành và chưa chia nhỏ, có tìm kiếm, tải thêm và tab Gần đây. Không bắt buộc có học kỳ, môn, deadline hay ước lượng để bấm giờ.
- Bắt đầu → Tạm dừng → Tiếp tục → Kết thúc. Tạm dừng không cộng thời gian. Kết thúc mở xác nhận với **Lưu và kết thúc** hoặc **Bỏ phiên**; cho phép lưu cả phiên ngắn, không áp đặt ngưỡng 5 phút của ảnh tham khảo.
- Pomodoro hết giờ được chốt đúng mục tiêu bằng giờ máy chủ; đọc lại trạng thái sẽ chốt phiên kể cả trình duyệt từng đóng. Nghỉ là đồng hồ riêng phía trình duyệt, không ghi vào thời gian thực học. Không tự bắt đầu phiên công việc tiếp theo.
- Đổi cấu hình chỉ áp dụng phiên sau; nhóm đang chạy giữ mục tiêu đã lưu và lịch dự kiến đã đặt giữ nguyên. Giới hạn ngày dùng khi gợi ý/lưu lịch tương lai, không khóa stopwatch/tập trung tự do sau khi đạt hạn mức.
- Ghi chú được lưu bằng nút riêng, đồng thời được gửi cùng các thao tác pause/resume/finish. Sau phiên, mở nhiệm vụ để cập nhật phần trăm/checklist hoặc để sau; không tự chuyển số phút thành phần trăm.
- Rời trang có thanh đồng hồ thu nhỏ để quay lại; tải lại trang/đăng nhập lại vẫn đọc phiên đang mở từ backend. Phiên bắt đầu bằng giao diện cũ có thể được tiếp nhận vào màn mới.
- Lịch sử có phân trang 20 bản ghi, chi tiết và sửa ghi chú. Nút **Thêm bản ghi tập trung** nhập khoảng đã học trước đó, nhiệm vụ tùy chọn, chế độ và ghi chú. Chặn khoảng tương lai, khoảng đảo, dài trên 24 giờ hoặc trùng thời gian đã ghi/phiên đang mở.
- Bấm **tên nhiệm vụ** trong lịch sử hoặc chi tiết bản ghi để mở chính cửa sổ nhiệm vụ có thể chỉnh sửa ngay tại trang Tập trung: tổng thời gian tập trung toàn bộ lịch sử, mô tả/ghi chú, checklist và các bước nhỏ. Dùng menu **… → Chỉnh sửa** để đổi tên, nội dung, ưu tiên, hạn và thông tin nhiệm vụ; **Cập nhật tiến độ** dành cho việc không có checklist. Checklist tự tính tiến độ theo quy tắc nhiệm vụ hiện có. Thêm/tick/sửa checklist, thêm bước nhỏ và mở bước con đều dùng cùng component/API với trang Nhiệm vụ. Không tự điều hướng sang trang khác để sửa. Bấm giờ/biểu tượng/thời lượng của bản ghi vẫn mở phần ghi chú phiên. Nhiệm vụ đã xóa có thông báo phù hợp, lịch sử phiên vẫn được giữ.
- Menu **… → Thống kê** mở thống kê ngày, 7 ngày hoặc 30 ngày kết thúc tại ngày được chọn: thời gian thực học, Pomodoro hoàn tất, phiên đã lưu, trung bình/ngày, cột thời gian theo ngày và phân bổ theo nhiệm vụ. Ngày không có phiên hiện 0; phân bổ lấy 5 mục lớn nhất và gộp phần còn lại để không mất thời gian. Không có điểm thành tích hoặc so sánh người dùng.
- Xem nhiệm vụ/thống kê không tạm dừng đồng hồ. Nếu Pomodoro hết giờ khi đang xem, cửa sổ giữ mở và tải lại số liệu sau khi phiên được chốt. Lỗi tải có nút thử lại, không thay lỗi bằng số 0 hoặc giữ biểu đồ của khoảng ngày cũ.
- Sau khi sửa nhiệm vụ, đọc lại phiên bản dữ liệu cho cả cửa sổ nhiệm vụ lẫn đồng hồ để thao tác pause/resume/finish tiếp theo không dùng phiên bản cũ. Thay đổi có ảnh hưởng lịch vẫn đi qua preview/xác nhận hiện có; nếu cần kết thúc phiên đang học, hiển thị yêu cầu đó, không tự dừng đồng hồ.
- Nút **Cập nhật tiến độ nhiệm vụ** sau khi lưu phiên cũng mở cửa sổ chỉnh sửa tại chỗ, giữ người dùng trên trang Tập trung.
- Khi cần xếp lại phần còn lại, dùng **Cập nhật phần chưa học** ở trang Gợi ý lịch học. Preview công bố số phiên tương lai sẽ thay thế; chỉ sau xác nhận áp dụng mới hủy cũ/thêm mới. Nhóm đang học và lịch sử focus không bị ghi lại hoặc chuyển ngày theo kế hoạch mới.

## 2. Dữ liệu và giao dịch

- `PhienTapTrung` trong bảng `phien_tap_trung` giữ nhóm bấm giờ: chủ sở hữu, nhiệm vụ/phiên tự học tùy chọn, ảnh chụp tiêu đề, chế độ, mục tiêu, trạng thái, mili giây tích lũy, mốc đoạn đang chạy, ghi chú và nguồn nhập tay.
- `KhoangThoiGianHoc` ánh xạ bảng `khoang_thoi_gian_hoc` (tên cũ `doan_thuc_hoc`); `MaPhienTapTrung` nối các đoạn học có nhiệm vụ với nhóm. Bắt đầu/tiếp tục mở đoạn; pause/finish đóng đoạn trong cùng giao dịch. Tập trung tự do lưu thời gian ngay trong nhóm và không tạo nhiệm vụ giả.
- Dùng khóa hàng người dùng và `PhienBanLapLich` hiện có. Unique index cho phép mỗi tài khoản tối đa một nhóm `Running` hoặc `Paused`; đồng thời kiểm tra đồng hồ/phiên tự học cũ để không chạy song song. FK của đoạn với nhóm bao gồm mã chủ sở hữu.
- Nhóm còn lưu tiêu đề và thời lượng sau khi nhiệm vụ bị xóa. FK nhiệm vụ của nhóm được đặt null khi hard delete; không xóa lịch sử nhóm. Đoạn thực học vẫn tuân theo quan hệ xóa của nhiệm vụ hiện có.
- Bắt đầu từ một phiên dự kiến chuyển phiên đó sang `InProgress`; finish/đủ Pomodoro chuyển sang `Completed`. Khoảng giờ dự kiến giữ nguyên để đối chiếu. Bỏ nhóm đang mở loại các đoạn của chính nhóm khỏi thời lượng; phiên tự học liên kết trở lại `Planned` nếu chưa hết giờ, hoặc `Missed` nếu đã hết giờ.
- Tổng quan tính từ các nhóm Completed, không cộng nhóm đang chạy/nghỉ/bị bỏ. Hôm nay được xác định theo **ngày kết thúc phiên** trong múi giờ tài khoản. Số Pomodoro = số mục tiêu đầy đủ đã học; ví dụ bản nhập tay 50 phút với mục tiêu 25 phút tính 2 lượt. Thời gian học chưa đủ mục tiêu vẫn được lưu nhưng không tính một lượt đầy đủ.

## 3. Hợp đồng API hiện hành

Các route dưới tiền tố `/api/v1/focus`, cần đăng nhập. Response dùng `{ phienBan, duLieu }` và ETag; các lệnh POST yêu cầu `If-Match` đọc gần nhất. Thời điểm có offset và được chuẩn hóa UTC.

Cấu hình chung nằm ngoài tiền tố focus: `GET/PUT /api/v1/study-settings`, PUT đủ ba số `phienHocPhut`, `nghiGiuaPhienPhut`, `gioiHanHocMoiNgayPhut` và If-Match. Ghi cấu hình đổi revision nên preview kế hoạch cũ cần tạo lại. Hợp đồng kỹ thuật `/focus/start` vẫn hỗ trợ mục tiêu 60–10.800 giây để tương thích API, trong khi form cấu hình dùng chung giới hạn 15–50 phút.

| Method / route | Chức năng |
| --- | --- |
| `GET /state?trang=1` | Nhóm hiện tại, lịch sử, tổng quan, giờ máy chủ và múi giờ; chốt countdown đã hết giờ |
| `GET /statistics?ngay=2026-11-12&soNgay=7` | Chỉ đọc thống kê 1/7/30 ngày kết thúc tại `ngay`; mặc định hôm nay theo múi giờ tài khoản, 7 ngày |
| `GET /tasks/{id}/summary` | Chỉ đọc tổng thời gian, số phiên và Pomodoro của đúng nhiệm vụ trong toàn bộ lịch sử; kiểm tra quyền sở hữu và nhiệm vụ chưa bị xóa |
| `POST /start` | Chế độ Pomodoro/Stopwatch, mục tiêu 60–10.800 giây, nhiệm vụ hoặc phiên tự học tùy chọn |
| `POST /{id}/pause` | Đóng đoạn đang chạy, giữ nhóm để tiếp tục |
| `POST /{id}/resume` | Mở đoạn mới, không tính khoảng tạm dừng |
| `POST /{id}/finish` | Lưu thời gian thực học và hoàn thành nhóm/phiên liên kết |
| `POST /{id}/discard` | Bỏ nhóm đang mở sau xác nhận trên giao diện |
| `POST /{id}/note` | Lưu ghi chú tối đa 2.000 ký tự, kể cả nhóm đã hoàn thành |
| `POST /records` | Nhập thủ công khoảng đã học, không gắn với một phiên dự kiến |

Mất phản hồi ghi không được tự gửi lại theo vòng lặp; đọc lại trạng thái trước. Hai tab tranh cùng phiên bản chỉ một lệnh được ghi. Gọi start trùng cùng nhiệm vụ/phiên với phiên bản mới trả nhóm đang có; pause/finish lặp lại không cộng lại thời gian.

Thống kê chỉ cộng nhóm Completed theo ngày kết thúc trong múi giờ tài khoản, không tự chốt phiên khi đọc thống kê. Màn đồng hồ tiếp tục đọc state để chốt countdown đến hạn rồi yêu cầu làm mới số liệu. Phiên qua nửa đêm thuộc ngày kết thúc, không chia thời lượng cho nhiều ngày. Cùng mã nhiệm vụ được gộp dù tiêu đề từng đổi; bản ghi đã bỏ liên kết giữ tiêu đề chụp. Khoảng có trên 10.000 bản ghi trả lỗi giới hạn, không cắt rồi trả tổng thiếu. Tổng theo nhiệm vụ không cộng nhiệm vụ con và không ghi tiến độ.

## 4. File để lần theo

- [TapTrungController](../../backend/StudyPlanner.Api/Controllers/TapTrungController.cs) → [TapTrungService](../../backend/StudyPlanner.Application/HocTap/TapTrung/TapTrungService.cs) → [QuyTacTapTrung](../../backend/StudyPlanner.Application/HocTap/TapTrung/QuyTacTapTrung.cs).
- Hai API thống kê đi qua [ThongKeTapTrungService](../../backend/StudyPlanner.Application/HocTap/TapTrung/ThongKeTapTrungService.cs) và [DTO thống kê](../../backend/StudyPlanner.Application/HocTap/TapTrung/ThongKeTapTrungResponse.cs).
- [CauHinhHocService/DTO](../../backend/StudyPlanner.Application/HocTap/LapLich/CauHinhHoc.cs) và [CauHinhHocController](../../backend/StudyPlanner.Api/Controllers/CauHinhHocController.cs) lưu cấu hình chung; FE `hoc-tap/StudySettings.tsx` dùng chung ở focus và lập kế hoạch.
- [PhienTapTrung](../../backend/StudyPlanner.Domain/Entities/PhienTapTrung.cs), [cấu hình bảng](../../backend/StudyPlanner.Infrastructure/Data/Configurations/PhienTapTrungConfiguration.cs), [PhienTuHocService](../../backend/StudyPlanner.Application/HocTap/PhienTuHoc/PhienTuHocService.cs) giữ tương thích thao tác kết thúc từ API cũ.
- FE tại `C:/Users/ADMIN/Desktop/Study_Planner_FE/src/focus/`: `FocusPage.tsx`, `FocusDock.tsx`, `focus.ts`, `focus.css`. `App.tsx` khai báo route; `AppLayout.tsx` nối menu; `hoc-tap/LichView.tsx` chuyển nút bắt đầu sang màn mới.
- `FocusTaskPreview.tsx` cung cấp context và các lớp dialog tại trang focus, dùng `ChiTietNhiemVu`/`SuaNhiemVu` trong `hoc-tap/NhiemVuView.tsx` và `NhiemVuContext.tsx` dùng chung với trang nhiệm vụ. `FocusStatistics.tsx`, `FocusDialog.tsx`, `focus-statistics.ts` và `focus-details.css` phụ trách thống kê/bản ghi. Xem [thống kê ban đầu](../history/2026-11-12-xem-nhiem-vu-va-thong-ke-tap-trung.md) và [sửa nhiệm vụ tại trang focus](../history/2026-11-12-sua-nhiem-vu-tai-trang-tap-trung.md).

## 5. Chạy và kiểm tra

Database Development đã có migration đồng hồ và đã được nâng cấp đến `20261114122238_DoiTenRangBuocKhongNullKhoangHoc` ngày 14/11/2026. Xem [bản ghi database](../history/2026-11-14-ap-dung-migration-database.md). Với môi trường khác chưa nâng cấp, dùng:

```powershell
dotnet ef database update --project backend/StudyPlanner.Infrastructure --startup-project backend/StudyPlanner.Api --configuration Release
```

Khởi động lại backend, tải lại frontend và vào Tập trung. Nếu database chưa có migration thì các endpoint mới chưa hoạt động.

Phần xem nhanh/thống kê dùng bảng focus hiện có. Cấu hình chung mới cần migration `20261114092625_CaiTienLapKeHoach` (cũng thêm ngoại lệ giờ rảnh và nhắc kế hoạch); đã áp dụng vào database Development trong lượt nâng cấp 14/11/2026. Không chỉ tải lại FE nếu BE/database chưa có endpoint/cột mới.

Kiểm thử backend: build CoreChecks rồi chạy với `STUDYPLANNER_FOCUS_ONLY=1` và biến kết nối PostgreSQL kiểm thử riêng theo quy tắc của CoreChecks. Không dùng database ứng dụng để kiểm thử. FE: `node --test tests/focus-statistics.test.mjs tests/focus.test.mjs tests/hoc-tap.test.mjs`; đặt `CORE_FOCUS_ONLY=1` để chạy kịch bản màn riêng, hoặc `CORE_SESSION_ONLY=1` để chạy luồng từ phiên tự học bằng `node tests/browser-hoc-tap.mjs`.
