# 2026-11-12 — Màn đồng hồ Tập trung riêng theo ảnh tham khảo

## Yêu cầu và mục đích

- Người dùng gửi ảnh đồng hồ Pomodoro/Stopwatch, chọn nhiệm vụ, tổng quan, lịch sử, ghi chú, nhập bản ghi và xác nhận bỏ phiên; yêu cầu áp dụng vào ứng dụng thay cho đồng hồ chỉ nằm trong hộp thoại phiên tự học.
- Đã đọc mục lục lịch sử, [bắt đầu ngay và tiến độ sau học](2026-11-12-bat-dau-phien-ngay-va-tien-do-sau-hoc.md), [luồng tự học/bấm giờ](2026-11-10-luong-tu-hoc-va-bam-gio.md), code BE/FE và schema hiện hành. Tiếp tục trên nhánh `feat/bat-dau-phien-hoc-ngay` vì cùng nhóm chức năng đồng hồ; giữ các thay đổi trước và các commit có sẵn của FE.
- Trước sửa, UI chỉ có đồng hồ đếm lên trong dialog. `DoanThucHoc` đã được thiết kế cho pause/resume nhưng chưa có nhóm lưu cấu hình Pomodoro, ghi chú và lịch sử độc lập. Không dùng localStorage thay cho dữ liệu thực học trên server.

## Code và logic thay đổi

### Giao diện và điều hướng

- Trong repository FE `C:/Users/ADMIN/Desktop/Study_Planner_FE`, thêm `src/focus/FocusPage.tsx`, `focus.ts`, `focus.css` và `FocusDock.tsx`. `App.tsx` nối route `/focus`; `layouts/AppLayout.tsx` thêm mục Tập trung và thanh quay lại đồng hồ khi rời trang. Bộ lọc học kỳ hiện có không giới hạn nhiệm vụ của đồng hồ.
- Bố cục desktop có đồng hồ vòng tròn bên trái, tổng quan/ghi chú/lịch sử bên phải. Pomodoro mặc định 25 phút; đếm lên dùng mốc máy chủ. Cài đặt thay thời lượng phiên sau, giờ nghỉ và âm báo. Có chế độ toàn màn hình.
- Khi chạy, bảng bên phải hiện dòng thời gian các phiên gần đây trong ngày và ghi chú; khi kết thúc trở lại tổng quan/lịch sử. Trục giờ dùng thời điểm thực và biên ngày theo múi giờ tài khoản, có kiểm tra qua đêm và giờ DST lặp. Khối thể hiện khoảng mở phiên, không suy ra thời lượng tập trung từ chiều cao khối khi có tạm dừng.
- Chọn nhiệm vụ có tìm kiếm, tải thêm, Học tập/Cá nhân và Gần đây; dùng API nhiệm vụ với `chiLa=true` để chọn được các bước nhỏ, không chỉ nhiệm vụ gốc. Có thể chọn Tập trung tự do không gắn nhiệm vụ. Không yêu cầu ước lượng để ghi thời gian thực học.
- `hoc-tap/LichView.tsx`: Bắt đầu ngay/Bắt đầu học dẫn vào màn riêng và bắt đầu đồng hồ gắn phiên; phiên đang học có Mở đồng hồ tập trung. Dialog cũ chỉ giữ thông tin phiên và tổng thời gian đã ghi sau khi hoàn thành, không còn là nơi điều khiển đồng hồ đang chạy.
- Tạm dừng/tiếp tục, lưu ghi chú, Lưu và kết thúc/Bỏ phiên đều gọi API. Kết thúc cho mở nhiệm vụ để cập nhật kết quả hoặc để sau. Phiên ngắn vẫn có thể lưu; không áp đặt ngưỡng 5 phút từ sản phẩm tham khảo.
- Có lịch sử 20 bản ghi/trang, nhóm theo ngày, sửa ghi chú và thêm bản ghi thủ công. Thời gian nghỉ không tính vào thống kê. Bốn số tổng quan dựa trên phiên Completed; Hôm nay dùng ngày kết thúc theo múi giờ tài khoản. Một bản nhập tay 50 phút với mục tiêu 25 phút tính 2 Pomodoro.
- Đồng hồ nghỉ và tùy chọn trình bày lưu trên trình duyệt theo tài khoản; phiên thực học, ghi chú đã lưu và lịch sử lưu trên BE. Một đồng hồ hiện hành được khôi phục khi tải trang. Âm báo dùng Web Audio nếu trình duyệt cho phép.
- CSS cũ ép body tối thiểu 1024px; bổ sung ngoại lệ chỉ khi có màn focus, thu gọn phần tìm kiếm/học kỳ trên điện thoại. Laptop 1280×636 có nút điều khiển nằm trong màn; điện thoại 390×844 không cuộn ngang. Các trang khác giữ quy tắc chiều rộng cũ.

### Dữ liệu, API và quy tắc

- [PhienTapTrung](../../backend/StudyPlanner.Domain/Entities/PhienTapTrung.cs), [cấu hình bảng](../../backend/StudyPlanner.Infrastructure/Data/Configurations/PhienTapTrungConfiguration.cs), [ApplicationDbContext](../../backend/StudyPlanner.Infrastructure/Data/ApplicationDbContext.cs): nhóm đồng hồ có chủ, task/phiên dự kiến tùy chọn, tiêu đề chụp lúc bắt đầu, chế độ, trạng thái, mục tiêu, mili giây tích lũy, mốc đoạn đang chạy, ghi chú và nguồn nhập tay.
- [DoanThucHoc](../../backend/StudyPlanner.Domain/Entities/KhoangThoiGianHoc.cs) và [LapKeHoachConfiguration](../../backend/StudyPlanner.Infrastructure/Data/Configurations/LapKeHoachConfiguration.cs): thêm FK nhóm đồng hồ gồm cả chủ sở hữu. Nhóm có nhiệm vụ tạo các đoạn thực học; tập trung tự do lưu thời gian ở nhóm, không tạo nhiệm vụ giả.
- [TapTrungController](../../backend/StudyPlanner.Api/Controllers/TapTrungController.cs), [đăng ký dịch vụ](../../backend/StudyPlanner.Api/HocTap/HocTapRegistration.cs), [Swagger](../../backend/StudyPlanner.Api/Administration/MoTaSwagger.cs) cung cấp `/api/v1/focus/state`, `/start`, `/records` và `/{id}/{operation}` với pause/resume/finish/discard/note. Ghi cần If-Match; đọc và ghi chỉ xét tài khoản đăng nhập.
- [TapTrungService](../../backend/StudyPlanner.Application/HocTap/TapTrung/TapTrungService.cs) và [QuyTacTapTrung](../../backend/StudyPlanner.Application/HocTap/TapTrung/QuyTacTapTrung.cs): thời lượng = phần đã tích lũy + thời gian từ mốc đoạn đang chạy. Pause đóng đoạn, resume mở mốc mới. Pomodoro bị chặn tại đúng mục tiêu, tránh tính quá giờ khi tab bị treo/đóng. GET state chốt countdown đã hết trong giao dịch có khóa và trả phiên bản mới.
- Mỗi tài khoản tối đa một nhóm Running/Paused, được bảo vệ bằng khóa người dùng và unique index. Vẫn chặn đồng hồ/phiên đang chạy từ API cũ. Hai tab cùng gửi một phiên bản chỉ một lệnh ghi thành công. Start cùng task/phiên trả nhóm hiện có; pause/finish lặp không cộng hai lần.
- [PhienTuHocService](../../backend/StudyPlanner.Application/HocTap/PhienTuHoc/PhienTuHocService.cs) kiểm tra nhóm focus khi bắt đầu qua API cũ và đồng bộ nhóm khi kết thúc qua API cũ. Có thể tiếp nhận phiên đã chạy trước khi có màn mới, giữ các đoạn và thời gian đã học. State trả mã phiên cũ để UI cho tiếp tục.
- Finish nhóm gắn lịch tự học sẽ hoàn thành phiên dự kiến nhưng giữ nguyên khoảng dự kiến, không tăng tiến độ nhiệm vụ. Discard xóa các đoạn của chính nhóm, loại thời lượng khỏi lịch sử; phiên liên kết trở lại Planned nếu chưa hết giờ hoặc Missed nếu đã hết giờ.
- Nhập tay chỉ nhận khoảng quá khứ, đúng chiều, tối đa 24 giờ, không trùng khoảng phiên đã lưu/đang mở; ghi chú tối đa 2.000 ký tự. Nhóm giữ ảnh chụp tiêu đề, thời lượng và ghi chú khi task bị hard delete, FK task chuyển null. Tiến độ còn lại vẫn tính từ ước lượng và phần trăm của nhiệm vụ.
- Migration [20261112100228_ThemDongHoTapTrung](../../backend/StudyPlanner.Infrastructure/Migrations/20261112100228_ThemDongHoTapTrung.cs) cùng Designer/snapshot tạo bảng mới và FK nullable. Không áp dụng migration vào database ứng dụng trong tác vụ này.
- [SchemaChecks](../../backend/tests/StudyPlanner.SchemaChecks/Program.cs) cập nhật 19 bảng/31 FK; ghi nhận mapping `ChecklistJson → checklist` đã có trong code trước, thay cho kỳ vọng snake_case tuyệt đối của kiểm tra cũ.
- Bổ sung [module 09](../modules/09-dong-ho-tap-trung.md), cập nhật [mục lục module](../modules/README.md), [kịch bản demo](../KICH-BAN-DEMO-NHIEM-VU-VA-THOI-KHOA-BIEU.md) và ghi chú trạng thái ở [ERD](../erd.md)/[thiết kế API](../THIET-KE-API-SWAGGER.md). Phần giới thiệu cũ nói chưa có API/lập lịch không còn khớp code, được đánh dấu là lịch sử thay vì dùng làm trạng thái hiện tại.

## Kiểm chứng

- Đã đọc lại code/diff các route, điểm nối FE/BE, xử lý trạng thái, migration, FK/index và logic tổng hợp; `git diff --check` hai repo đạt.
- Build Release CoreChecks và API đạt, 0 lỗi/0 cảnh báo. `dotnet ef migrations has-pending-model-changes ... --configuration Release --no-build` báo không có thay đổi model sau migration cuối.
- Các kiểm tra thời gian hiện có, 13 kiểm tra cây/tiến độ, 23 kiểm tra phiên và quy tắc focus thuần đạt. [FocusChecks](../../backend/tests/StudyPlanner.CoreChecks/FocusChecks.cs) kiểm tra cap countdown, giờ nghỉ, resume, đếm lên và validation cấu hình.
- [FocusHttpChecks](../../backend/tests/StudyPlanner.CoreChecks/FocusHttpChecks.cs), chạy bằng `STUDYPLANNER_FOCUS_ONLY=1`: **93 điều kiện HTTP/PostgreSQL đạt**. Tạo cluster kiểm thử localhost riêng trong obj, tạo/xóa database ngẫu nhiên cho mỗi lượt; không dùng cấu hình hoặc dữ liệu DB ứng dụng. Bao gồm migration thật, JWT/quyền sở hữu, ETag, pause/resume, hết giờ sau đóng trình duyệt, ghi chú, nhập tay, trùng giờ, bỏ phiên, API cũ, hai tab tranh phiên bản và lịch sử sau xóa nhiệm vụ.
- SchemaChecks đạt: 19 bảng, 31 FK, không cột shadow, mapping/constraint và sinh SQL đúng.
- TypeScript `--noEmit`, `node --check` các script FE và **24/24** kiểm tra `tests/focus.test.mjs` + `tests/hoc-tap.test.mjs` đạt, bao gồm dòng thời gian qua đêm và giờ DST lặp.
- `npm run build` FE đạt với TypeScript và Vite production. Vite có cảnh báo chunk JS sau nén lớn hơn 500 kB; không có lỗi build, chưa tách lại các route của toàn ứng dụng trong phần việc này.
- `CORE_FOCUS_ONLY=1 node tests/browser-hoc-tap.mjs`: đạt, lượt cuối **82 request giả lập**, dùng component/route React thật. Có Pomodoro, đếm lên, chọn việc học/cá nhân/tự do/gần đây, settings, pause/resume, reload, ghi chú, nghỉ, mini dock, nhập tay, discard, tổng quan, dòng thời gian và lịch sử. Số request thay đổi theo các lượt polling khi chạy.
- `CORE_SESSION_ONLY=1 node tests/browser-hoc-tap.mjs`: đạt, lượt cuối **67 request giả lập**, kiểm tra từ phiên tương lai vào màn đồng hồ, khôi phục, kết thúc, rồi cập nhật tiến độ riêng.
- Kịch bản standalone đóng Chromium qua CDP, dọn request chờ và giới hạn chờ Vite đóng trước khi kết thúc tiến trình CLI. Chỉ thoát thành công sau khi tất cả assertion của chế độ đã chạy đạt; lỗi assertion vẫn trả lỗi. Cách này tránh các lượt kiểm thử đã báo PASS nhưng còn giữ watcher Vite sau đó.
- Đã mở ảnh desktop, laptop 1280×636 và mobile 390×844 trong `backend/obj/focus-screenshots/` để kiểm tra bố cục. Kịch bản kiểm tra nút laptop trong viewport và mobile không tràn ngang.
- Chạy lại bộ browser tổng: dừng ở **Learning navigation must be vertical and positioned to the left of content**, như lỗi đã tái hiện bằng kịch bản nguyên bản ở bản ghi trước. Không ghi bộ tổng là đạt.
- Browser dùng HTTP giả lập, kiểm tra API/SQL thật chạy riêng. Chưa chạy toàn bộ trình duyệt nối API/PostgreSQL thật với tài khoản của người dùng. Chưa xác minh âm thanh và toàn màn hình bằng thao tác trên trình duyệt của người dùng.
- Database tạm của mỗi lượt đã được xóa trong finally; cụm PostgreSQL kiểm thử riêng đã được dừng sau khi kiểm tra hoàn tất.

## Việc tiếp theo

- Cần áp dụng migration vào database ứng dụng rồi khởi động lại BE trước khi dùng API mới. Lệnh và cách chạy được ghi trong [module 09](../modules/09-dong-ho-tap-trung.md).
- Người dùng có thể vào Tập trung hoặc bấm bắt đầu từ Phiên tự học để kiểm tra bằng dữ liệu của mình; phần người dùng đọc và đối chiếu code chưa được xác nhận.
- Lỗi kiểm tra menu của bộ browser tổng thuộc phần bố cục cũ, chưa sửa trong nhóm đồng hồ.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa commit/push/triển khai.
