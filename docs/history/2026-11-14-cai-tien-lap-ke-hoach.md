# 2026-11-14 — Cải tiến lập kế hoạch theo năm mức ưu tiên

**Cập nhật sau triển khai 14/11/2026:** các migration liên quan đã được áp dụng vào database Development theo cấu hình dự án; xem [bản ghi cập nhật database](2026-11-14-ap-dung-migration-database.md). Các mục “chưa áp dụng” bên dưới phản ánh thời điểm trước lần triển khai này.

## Yêu cầu và mục đích

- Sau [lượt đối chiếu code](2026-11-14-doi-chieu-luong-lap-ke-hoach.md), người dùng yêu cầu thực hiện lần lượt các cải tiến và báo cáo đã tạo/sửa gì. Người dùng tiếp tục yêu cầu comment trong code bằng tiếng Việt, giải thích các logic quan trọng và lý do ở chỗ dễ sửa nhầm.
- Năm phần: nhập giờ rảnh nhiều thứ; điều chỉnh riêng một ngày; cấu hình học dùng chung và hạn mức ngày; cập nhật phần kế hoạch chưa học; nhắc rà soát tuần/giờ rảnh sắp hết hiệu lực trong ứng dụng.
- Đã đọc mục lục lịch sử, bản ghi đối chiếu, [tiến độ cây](2026-11-13-tong-hop-tien-do-nhiem-vu.md), [đồng hồ](2026-11-12-man-dong-ho-tap-trung-rieng.md), tài liệu module và code BE/FE trước khi sửa. Nhánh ở hai repository là `feat/cai-tien-lap-ke-hoach`.
- `ThoiLuongDuKienPhut` và khả năng sinh giờ rảnh lặp theo tuần đã tồn tại. Không tạo cột thời lượng trùng hoặc sao chép mẫu giờ rảnh mỗi tuần. Các thay đổi đồng hồ/tiến độ từ lượt trước được giữ và kiểm tra hồi quy.

## Code và logic thay đổi

### 1. Nhập giờ rảnh nhiều thứ trong một lần

- Trước: mỗi form tạo một thứ; nhập thứ Hai–thứ Sáu cần năm thao tác riêng.
- Sau: form có các thứ, chọn nhanh thứ Hai–thứ Sáu/cả tuần. Gửi một `POST /availability-windows/batch`, kiểm tra đủ ngày/giờ và ghi tất cả trong một giao dịch, tăng phiên bản một lần. Chỉnh mẫu có sẵn vẫn sửa từng mẫu.
- Từ chối ngày trùng, thứ ngoài 1–7, thiếu giờ bắt buộc và mẫu trùng y hệt (`AVAILABILITY_DUPLICATE`). Không ghi một phần nếu cả batch không hợp lệ. Giờ `00:00` được phân biệt với không gửi trường giờ bằng `JsonRequired`.
- File: [GioRanhMoRongRequest.cs](../../backend/StudyPlanner.Application/HocTap/ThoiKhoaBieu/GioRanhMoRongRequest.cs), [KhungGioRanhService.cs](../../backend/StudyPlanner.Application/HocTap/ThoiKhoaBieu/KhungGioRanhService.cs), [KhungGioRanhController.cs](../../backend/StudyPlanner.Api/Controllers/KhungGioRanhController.cs); FE `src/hoc-tap/LichView.tsx`, `types.ts`.

### 2. Bỏ/thay giờ riêng một ngày

- Thêm [NgoaiLeGioRanh.cs](../../backend/StudyPlanner.Domain/Entities/NgoaiLeGioRanh.cs), [mapping](../../backend/StudyPlanner.Infrastructure/Data/Configurations/NgoaiLeGioRanhConfiguration.cs) và [QuyTacGioRanh.cs](../../backend/StudyPlanner.Application/HocTap/ThoiKhoaBieu/QuyTacGioRanh.cs).
- Mỗi mẫu/ngày chỉ có một ngoại lệ: bỏ ngày hoặc thay bằng một cặp giờ. Xóa ngoại lệ trả về lịch lặp gốc. Ngày phải đúng thứ, trong hiệu lực mẫu và chưa qua ngày địa phương. FK kép gồm mã mẫu và chủ bảo vệ liên kết cùng tài khoản.
- Luồng: đọc các mẫu/ngoại lệ → sinh từng lần lặp → loại ngày bị ghi đè → thêm khoảng giờ thay thế → hợp các mẫu. Giờ thay thế ngoài giờ mẫu gốc vẫn được tính khi truy vấn lịch; mẫu khác còn phủ cùng giờ sẽ không bị coi là mất giờ rảnh.
- Preview chỉ tính trên dữ liệu đề xuất, không sửa entity đang được theo dõi. Lưu/xóa có `If-Match`; nếu làm mất tính hợp lệ của phiên tương lai thì yêu cầu xác nhận hủy. Phiên đang học/đã đến giờ bị ảnh hưởng sẽ chặn thao tác. Thay ngoại lệ và hủy phiên liên quan trong cùng giao dịch.
- Sửa mẫu tuần không được làm các ngoại lệ tương lai rơi ra ngoài phạm vi/ngày thứ của mẫu; cần xử lý chúng trước. Ngoại lệ quá khứ không khóa việc sửa mẫu vĩnh viễn.
- Calendar và bộ lập lịch dùng chung nguồn đã áp dụng ngoại lệ qua [ThoiKhoaBieuService.cs](../../backend/StudyPlanner.Application/HocTap/ThoiKhoaBieu/ThoiKhoaBieuService.cs). FE tạo `src/hoc-tap/NgoaiLeGioRanh.tsx`, nút **Điều chỉnh một ngày**, danh sách điều chỉnh và **Dùng lại lịch lặp**.

### 3. Cấu hình học chung và giới hạn mỗi ngày

- Thêm [CauHinhHoc.cs](../../backend/StudyPlanner.Application/HocTap/LapLich/CauHinhHoc.cs), [CauHinhHocController.cs](../../backend/StudyPlanner.Api/Controllers/CauHinhHocController.cs), ba thuộc tính trong [NguoiDung.cs](../../backend/StudyPlanner.Domain/Entities/NguoiDung.cs) và mapping/default/CHECK trong [NguoiDungConfiguration.cs](../../backend/StudyPlanner.Infrastructure/Data/Configurations/NguoiDungConfiguration.cs).
- `GET/PUT /study-settings` lưu cấu hình tài khoản: mặc định **25 phút học / 5 phút nghỉ / 180 phút mỗi ngày**. Phiên 15–50 phút; nghỉ 1–30; hạn mức ngày 15–720 và không ngắn hơn một phiên. PUT đủ ba trường và `If-Match`; thay đổi làm preview cũ hết hiệu lực.
- [QuyTacLapLich.cs](../../backend/StudyPlanner.Application/HocTap/LapLich/QuyTacLapLich.cs) nhận cấu hình thay hằng số 25–50 phút/nghỉ 10 phút trước đây. Phiên thường tối thiểu 15 phút; phần cuối có thể ngắn hơn nếu xếp hết phần còn lại. Tách khe theo nửa đêm địa phương, trừ ngân sách của Planned/InProgress tương lai cùng các phiên mới, không tính giờ nghỉ vào hạn mức.
- [QuyTacDatPhien.cs](../../backend/StudyPlanner.Application/HocTap/PhienTuHoc/QuyTacDatPhien.cs) kiểm tra cùng cấu hình khi lưu/dời/apply. Cấu hình mới không tự sửa phiên đã đặt hoặc đồng hồ đang chạy; đồng hồ tự do không bị khóa khi học vượt ngân sách lịch.
- FE tạo `src/hoc-tap/StudySettings.tsx`, `planning-upgrade.css`; sửa `LapLichView.tsx` và `src/focus/FocusPage.tsx`. Cả hai màn đọc cùng cấu hình server. Chờ tải thành công trước khi bắt đầu phiên mới; âm báo vẫn lưu theo trình duyệt. Bản đang bấm giờ giữ mục tiêu ban đầu.

### 4. Cập nhật phần kế hoạch chưa học

- Trước: chỉ tạo thêm lịch sau khi trừ thời lượng đã đặt. Sau: thêm `capNhatKeHoach`, mặc định false để giữ luồng cũ.
- [LapLichService.cs](../../backend/StudyPlanner.Application/HocTap/LapLich/LapLichService.cs) chỉ chọn phiên Planned chưa đến giờ, nằm trọn khoảng ngày, thuộc các lá hợp lệ đã chọn. Phiên ngoài phạm vi, đang học, đã học/bỏ lỡ được giữ. Phiên cũ được hủy bằng trạng thái, không xóa lịch sử.
- Preview tạm bỏ phần dành chỗ của những phiên được thay thế để tính lại; trả `maPhienThayThe` và `cauHinhHoc`. [NoiDungPreviewLapLich.cs](../../backend/StudyPlanner.Application/HocTap/LapLich/NoiDungPreviewLapLich.cs) ký cả cấu hình/danh sách ID cùng phiên bản nguồn và quy tắc. [Request](../../backend/StudyPlanner.Application/HocTap/LapLich/XemTruocLichRequest.cs)/[response](../../backend/StudyPlanner.Application/HocTap/LapLich/XemTruocLichResponse.cs) được mở rộng tương ứng.
- Luồng apply: kiểm tra key retry → token/revision/cấu hình → phiên cũ vẫn chưa bắt đầu → hủy đúng danh sách trong token → kiểm tra các phiên mới → thêm chúng → commit chung giao dịch. Lỗi giữa chừng rollback cả hủy cũ và thêm mới. Cùng key/nội dung sau mất phản hồi trả kết quả đã lưu.
- FE `LapLichView.tsx` có **Tạo thêm lịch/Cập nhật phần chưa học**, hiện số phiên cũ sẽ thay và xác nhận trước apply. Hủy xác nhận không gửi apply. Nếu bỏ chọn một phần đề xuất thì vẫn thay toàn bộ danh sách phiên cũ đã báo; người dùng cần xem phần thời lượng còn thiếu.

### 5. Nhắc kế hoạch trong ứng dụng

- Tạo [CauHinhNhacKeHoach.cs](../../backend/StudyPlanner.Domain/Entities/CauHinhNhacKeHoach.cs), [NhacKeHoach.cs](../../backend/StudyPlanner.Domain/Entities/NhacKeHoach.cs), hai mapping tương ứng, [NhacKeHoachService.cs](../../backend/StudyPlanner.Application/HocTap/NhacKeHoach/NhacKeHoachService.cs) và [NhacKeHoachController.cs](../../backend/StudyPlanner.Api/Controllers/NhacKeHoachController.cs).
- Mặc định tắt. Khi bật rồi mở/quay lại ứng dụng, request tải hộp tạo một nhắc rà soát cho tuần địa phương hiện tại; mẫu giờ rảnh hết hiệu lực trong 7 ngày tới được gom theo ngày hết hiệu lực. Năm mẫu thứ Hai–thứ Sáu cùng ngày hết hạn tạo một nhắc, không năm nhắc.
- Khóa theo người dùng và unique chủ/khóa chống trùng bảo đảm tải đồng thời không sinh đôi. Đọc danh sách/đánh dấu đã đọc không làm đổi phiên bản lập lịch. Bật/tắt cấu hình dùng `If-Match`.
- Đọc một dùng `ngayPhat`; đọc tất cả dùng mốc `docDenLuc` để không đánh dấu cả nhắc mới hơn snapshot. Có lọc chưa đọc, phân trang, quyền sở hữu và đường dẫn giới hạn tới gợi ý lịch/giờ rảnh.
- FE tạo `src/api/planningReminders.ts`, `src/hooks/usePlanningReminders.ts`, `src/components/PlanningReminderList.tsx`; sửa `NotificationBell.tsx`, `notifications.css`. Hộp có bật/tắt, số chưa đọc chung, đọc một/tất cả và bấm mở đúng trang. Làm mới mỗi 60 giây khi tab hiển thị hoặc khi quay lại/lưu dữ liệu; thông báo lỗi độc lập với nhắc hạn nhiệm vụ. Hộp nằm trong khung nhìn điện thoại; không hiển thị trạng thái rỗng khi đang có nhắc kế hoạch.
- Đây là nhắc trong ứng dụng khi tải hộp, chưa có worker/push/email lúc ứng dụng đóng. Không tự tạo nhiệm vụ, phiên học hoặc bản sao giờ rảnh; tắt nhắc không ngừng quy tắc lặp và không xóa lịch sử nhắc.

### Dữ liệu, đăng ký và tài liệu

- [ApplicationDbContext.cs](../../backend/StudyPlanner.Infrastructure/Data/ApplicationDbContext.cs) thêm ba DbSet; [HocTapRegistration.cs](../../backend/StudyPlanner.Api/HocTap/HocTapRegistration.cs) đăng ký cấu hình/nhắc; [MoTaSwagger.cs](../../backend/StudyPlanner.Api/Administration/MoTaSwagger.cs) mô tả endpoint/quy tắc mới.
- Tạo migration [20261114092625_CaiTienLapKeHoach.cs](../../backend/StudyPlanner.Infrastructure/Migrations/20261114092625_CaiTienLapKeHoach.cs), Designer và cập nhật snapshot. Up thêm ba cột người dùng, bảng `ngoai_le_gio_ranh`, `cau_hinh_nhac_ke_hoach`, `nhac_ke_hoach`, khóa/CHECK/index. Model hiện có 22 bảng/34 FK.
- **Chưa áp dụng migration vào database ứng dụng của người dùng.** Đã áp dụng trên database kiểm thử tạm và nâng cấp từ schema cũ trong kiểm thử. Down bỏ dữ liệu phần mở rộng; không thực hiện trên dữ liệu người dùng.
- Cập nhật [module 07](../modules/07-goi-y-cong-viec.md), [module 09](../modules/09-dong-ho-tap-trung.md), [thiết kế API](../THIET-KE-API-SWAGGER.md), [ERD](../erd.md), bản ghi này và [mục lục lịch sử](README.md). Tài liệu ERD/API giữ phần dự thảo cũ làm lịch sử nhưng ghi rõ hợp đồng/model hiện hành ở đầu.
- Các đường dẫn FE trong bản ghi thuộc repository riêng `C:/Users/ADMIN/Desktop/Study_Planner_FE/`. Không commit, push hoặc triển khai trong lượt này.

### Comment tiếng Việt theo yêu cầu bổ sung

- Rà comment thủ công trong source/test BE và `src/`, `tests/` FE, gồm cả file cũ ngoài nhóm chức năng mới. Đã dịch/bổ sung comment trong **40 file BE/test và 36 file FE/test**; giữ nguyên nghiệp vụ, định danh, hợp đồng API và nội dung assertion. Mã Designer/ModelSnapshot và thẻ XML tự sinh không dịch thủ công.
- Comment tập trung vào lý do: batch phải lưu chung; preview không sửa entity; hợp nhiều mẫu trước khi xét hủy phiên; quota tách ngày địa phương; đuôi phiên ngắn; token/cấu hình cũ; rollback/retry apply; khóa chống trùng và gom nhắc; thời gian tạm dừng; bảo toàn tiến độ cũ. Không thêm comment chỉ lặp lại cú pháp.
- Nhóm BE cũ chỉ đổi comment: `Api/Program.cs`, `Api/HocTap/MocThoiGianModelBinder.cs`, `Api/ThongBao/ThongBaoRegistration.cs`; `Application/Academic/AcademicService.cs`, `Contracts.cs`, `Application/HocTap/DungChung/IHocTapStore.cs`, `Application/ThongBao/QuyTacThongBao.cs`; `Infrastructure/HocTap/EfHocTapStore.cs`, `Infrastructure/ThongBao/ThongBaoService.cs`; `tests/StudyPlanner.AcademicChecks/Program.cs`, `StudyPlanner.PlanningChecks/Program.cs`, `StudyPlanner.CoreChecks/ClientContractChecks.mjs`. Các đường dẫn Api/Application/Infrastructure ở đây nằm dưới `backend/StudyPlanner.` tương ứng.
- Nhóm FE cũ chỉ đổi comment: `src/api/{academic,config,mockAuth}.ts`, `src/auth/{session,demo}.ts`, `src/data/{semesterSelection,planner}.ts`, `src/contexts/PlannerContext.tsx`, `src/hooks/useNotifications.ts`, `src/pages/SubjectDetailPage.tsx`, `src/pages/Dashboard/{todayApi,useToday}.ts`, `src/{student-simple,theme}.css`, `tests/{browser-auth,backend-live,academic-live,browser-subjects,browser-today}.mjs`. Comment context cũ được sửa cho đúng hiện trạng: context đó khóa ghi nhiệm vụ/lịch, còn màn học tập dùng API riêng.

## Kiểm chứng

- Đã đối chiếu DTO/controller, dịch vụ, quy tắc sinh lịch/hạn mức, chữ ký preview, giao dịch apply, mapping/migration, form FE và diff. Các comment thủ công được rà để dùng tiếng Việt và giải thích lý do ở nhánh bảo vệ quan trọng; không đổi tên API/định danh thư viện.
- `dotnet build backend/tests/StudyPlanner.CoreChecks/StudyPlanner.CoreChecks.csproj -c Release --no-restore`: đạt, 0 cảnh báo/0 lỗi.
- `dotnet run --project backend/tests/StudyPlanner.SchemaChecks -c Release`: đạt, 22 bảng/34 FK, SQL PostgreSQL/constraint hợp lệ; kiểm tra này không mở kết nối DB.
- `dotnet run --project backend/tests/StudyPlanner.CoreChecks -c Release --no-build` với cấu hình PostgreSQL kiểm thử cục bộ: đạt các nhóm quy tắc thời gian, 22 kiểm tra cây/tiến độ, 23 phiên tự học, đồng hồ và 12 quy tắc cấu hình/ngân sách ngày; 23 kiểm tra nâng cấp schema cũ; **561 HTTP nghiệp vụ cũ + 39 giờ rảnh + 38 cấu hình/cập nhật lịch + 31 nhắc kế hoạch + 121 đồng hồ** trên PostgreSQL tạm.
- Có kiểm tra client nhiệm vụ FE thật → HTTP API → PostgreSQL cho tiến độ cây. Các database thử do bộ kiểm tra tạo/xóa; đã dừng PostgreSQL kiểm thử sau khi đạt. Không dùng database ứng dụng làm dữ liệu thử.
- Tạo `AvailabilityUpgradeChecks.cs`, `PlanningUpgradeChecks.cs`, `PlanningReminderChecks.cs`; sửa runner, SchemaChecks, dữ liệu cấu hình của kiểm tra cũ và cách tạo dữ liệu schema cũ trong MigrationProgressChecks. Kiểm tra mới gồm batch nguyên tử/thiếu trường/ownership, preview không ghi, đổi giờ ngoài mẫu, hợp mẫu, hủy có xác nhận/chặn đang học, quota qua nửa đêm, stale token, giữ lịch sử, rollback, retry và nhắc đồng thời/chưa đọc.
- FE `npm test`: **134/134 đạt**. `npm run build`: TypeScript và bản production đạt; Vite còn cảnh báo chunk JS chính lớn hơn 500 kB, chưa tách tải theo trang trong phạm vi này.
- Sau lượt chỉ sửa comment: build CoreChecks tiếp tục đạt 0 cảnh báo/0 lỗi; FE `npx tsc -b`, `node --check` toàn bộ `tests/*.mjs` đạt. Không chạy lại database chỉ để kiểm tra thay chữ trong comment. `git diff --check` cả hai repo đạt; các liên kết file tương đối trong năm tài liệu triển khai đã kiểm tra tồn tại.
- Trình duyệt Chromium headless với React thật/API giả độc lập: `CORE_PLANNING_ONLY=1`, `CORE_REMINDERS_ONLY=1`, `CORE_FOCUS_ONLY=1`, `CORE_SESSION_ONLY=1`, `CORE_PROGRESS_ONLY=1` chạy `node tests/browser-hoc-tap.mjs`: đều đạt. Luồng mới kiểm tra nhập nhiều thứ một request, bỏ/đổi/khôi phục ngày, cấu hình đồng bộ với đồng hồ, hủy và xác nhận thay lịch; bật/tắt nhắc, badge, đọc/chuyển trang, khung nhìn điện thoại. Hồi quy đồng hồ, bắt đầu sớm và tiến độ cây đạt.
- `node tests/browser-auth.mjs --notifications`: đạt nhắc hạn cũ, lỗi/thử lại, phân trang, chuyển học kỳ, đọc tất cả, tải lại và liên kết nhiệm vụ. Đã sửa fixture nhắc kế hoạch để trả đúng envelope phiên bản; lỗi fixture trước khi sửa không phải lượt kiểm chứng đạt.
- Tạo/cập nhật các fixture/kịch bản FE `upgrade-fixture.mjs`, `browser-planning-upgrade.mjs`, `browser-planning-reminders.mjs`, `browser-hoc-tap.mjs`, `browser-focus.mjs`, `browser-notifications.mjs`. Chụp và đọc lại giao diện giờ rảnh/ngoại lệ/preview thay lịch/hộp nhắc bằng dữ liệu giả.
- Các lượt trình duyệt trên kiểm tra giao diện với API giả; không ghi thành kiểm thử toàn bộ trình duyệt kết nối DB thật. Chưa thử thủ công bằng tài khoản người dùng sau nâng cấp DB; chưa chạy toàn bộ kịch bản các module không liên quan.

## Việc tiếp theo

- Khi đưa lên môi trường ứng dụng: sao lưu, kiểm tra cấu hình môi trường, áp dụng các migration còn thiếu theo thứ tự, khởi động lại BE và tải FE mới. Các endpoint mới cần schema đã nâng cấp.
- Có thể thêm worker/push sau nếu cần nhắc khi đóng ứng dụng; hiện chưa triển khai. Chất lượng thuật toán tham lam cần đánh giá bằng dữ liệu cụ thể, chưa tuyên bố tối ưu toàn cục.
- Tách bundle FE là việc tối ưu độc lập còn lại từ cảnh báo build.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
