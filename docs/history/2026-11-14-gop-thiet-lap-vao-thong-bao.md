# 2026-11-14 — Gộp thiết lập nhắc hạn vào hộp thông báo

**Cập nhật sau triển khai 14/11/2026:** các migration liên quan đã được áp dụng vào database Development theo cấu hình dự án; xem [bản ghi cập nhật database](2026-11-14-ap-dung-migration-database.md). Các mục “chưa áp dụng” bên dưới phản ánh thời điểm trước lần triển khai này.

## Yêu cầu và mục đích

- Người dùng cung cấp luồng nhắc hạn/quá hạn của thành viên khác, hỏi có trùng với nhắc kế hoạch mới không; cho phép gộp phần phù hợp, giữ riêng phần cần độc lập và chốt đưa “Thiết lập nhắc hạn” vào khu vực “Nhận cảnh báo và xem thông báo”.
- Đã đọc mục lục, [thông báo cũ](2026-11-10-thong-bao-nhac-han.md), [cải tiến kế hoạch](2026-11-14-cai-tien-lap-ke-hoach.md), worker/quy tắc/hộp thư, cấu hình kế hoạch, chuông FE và kiểm tra hiện hành. Nhánh BE/FE: `feat/thong-bao-va-cau-hinh-nhac`; giữ các thay đổi chưa commit trước đó.

## Code và logic thay đổi

### Dùng chung giao diện và cấu hình, giữ riêng nghiệp vụ phát nhắc

- Dùng một chuông, số chưa đọc, bộ lọc và khu vực **Cài đặt → Thiết lập nhắc hạn**. Form chứa số giờ nhắc mặc định và bật/tắt nhắc rà soát tuần/giờ rảnh sắp hết hiệu lực.
- Giữ `QuyTacThongBao`/worker xử lý hạn nhiệm vụ; `NhacKeHoachService` xử lý tuần và hiệu lực giờ rảnh khi tải hộp. Không bổ sung luồng nhắc hạn thứ hai hoặc worker kế hoạch mới.
- Giữ `thong_bao` và `nhac_ke_hoach` riêng: bảng cũ bắt buộc gắn nhiệm vụ và unique theo nhiệm vụ; nhắc kế hoạch gắn tài khoản/tuần/ngày hết hiệu lực, không cần nhiệm vụ. Gộp vật lý sẽ phải đổi ràng buộc/hợp đồng và chuyển dữ liệu. Không tạo nhiệm vụ giả để dùng bảng cũ.
- Dùng lại `nguoi_dung.nhac_truoc_gio` và hàng `cau_hinh_nhac_ke_hoach` hiện có. Đợt gộp này **không thêm bảng, cột hoặc migration**; migration đổi tên khoảng học thuộc [yêu cầu riêng](2026-11-14-doi-ten-khoang-thoi-gian-hoc.md).

### Backend

- Tạo [CauHinhThongBao.cs](../../backend/StudyPlanner.Application/ThongBao/CauHinhThongBao.cs); sửa [ThongBaoController.cs](../../backend/StudyPlanner.Api/Controllers/ThongBaoController.cs), [HocTapRegistration.cs](../../backend/StudyPlanner.Api/HocTap/HocTapRegistration.cs), [MoTaSwagger.cs](../../backend/StudyPlanner.Api/Administration/MoTaSwagger.cs).
- `GET /api/v1/notifications/settings` trả `{phienBan, duLieu: {nhacTruocGio, nhacKeHoach}}` và ETag. PUT đủ hai trường, bắt buộc If-Match; số giờ nguyên 0–8760. Lưu dưới cùng khóa tài khoản/giao dịch, tăng phiên bản một lần khi đổi; lưu giống cũ không tăng. Phiên bản cũ không được ghi đè tab khác.
- Mặc định hiện có là 24 giờ, 0 nhắc đúng hạn; giờ riêng trên nhiệm vụ được ưu tiên. Không sửa override của nhiệm vụ. Thay giờ mặc định có hiệu lực khi worker/API đồng bộ lại theo quy tắc cũ; FE làm mới sau lưu.
- Tùy chọn kế hoạch dùng lại đúng hàng của `/planning-reminders/settings`. Giữ endpoint cũ tương thích; FE dùng endpoint cấu hình chung. Swagger bổ sung mô tả và sửa header để thao tác đọc thông báo/nhắc kế hoạch không bị mô tả sai là bắt buộc If-Match; chúng dùng `ngayPhat`/`docDenLuc`.

### Frontend trong `C:/Users/ADMIN/Desktop/Study_Planner_FE`

- Tạo `src/components/NotificationSettings.tsx`, `src/api/notificationSettings.ts`; sửa `NotificationBell.tsx`, `notifications.css`. Bấm Cài đặt mở form; lưu thành công quay về danh sách. Có tải/lỗi/thử lại, giữ giá trị đang sửa khi lưu lỗi; xung đột không tự ghi đè, có nút tải cấu hình mới. Form vừa khung nhìn điện thoại.
- Chuyển checkbox khỏi `PlanningReminderList.tsx` vào form chung. `usePlanningReminders.ts` chỉ tải inbox; bỏ tải thiết lập kèm mỗi lượt poll và bỏ hàm API cấu hình cũ không dùng trong `planningReminders.ts`. Form chỉ đọc cấu hình khi mở/tải lại.
- Hai nguồn inbox tiếp tục phân trang và xử lý lỗi độc lập; chưa phải một API danh sách hợp nhất theo thời gian. “Đọc tất cả” chỉ gọi nguồn có chưa đọc, dùng cutoff riêng. Không hiển thị rỗng khi một nguồn đang tải/lỗi hoặc có nhắc kế hoạch. Comment mới dùng tiếng Việt.

## Kiểm chứng

- Đọc lại diff/service/controller, rule/worker cũ, dữ liệu cấu hình, FE hook/form/API và ảnh chụp 390×844. Build BE và FE đạt.
- CoreChecks với PostgreSQL thử riêng đạt: 30 kiểm tra nâng cấp schema, 561 HTTP nghiệp vụ, 39 giờ rảnh, 38 cấu hình/lập lại lịch, **47 nhắc kế hoạch/cấu hình** (từ 31), 121 đồng hồ và các nhóm quy tắc thuần. Bổ sung anonymous, mặc định, giá trị 0, endpoint tương thích, cách ly tài khoản, no-op, stale If-Match, thiếu trường/ngoài giới hạn và không lưu một phần khi lỗi trong `PlanningReminderChecks.cs`.
- `dotnet run --project backend/tests/StudyPlanner.NotificationChecks -c Release --no-restore`: **13 rule cũ đạt**; HTTP/JWT/ownership/validation/snapshot/read/read-all và PostgreSQL concurrency/persistence/hạn kế thừa/cutoff đạt. Không chạy worker trên dữ liệu người dùng.
- FE `npm test`: **134/134 đạt**; `npm run build` đạt TypeScript/production. Còn cảnh báo bundle chính trên 500 kB như trước, chưa chia tải theo trang trong phạm vi này.
- `CORE_REMINDERS_ONLY=1 node tests/browser-hoc-tap.mjs`: đạt 24/0, bật/tắt, lỗi lưu/thử lại giữ input, đọc lại giá trị, badge/chuyển trang/đọc và mobile; không còn request `/planning-reminders/settings` khi xem hộp. Sửa `upgrade-fixture.mjs`, `browser-hoc-tap.mjs`, `browser-planning-reminders.mjs` tương ứng.
- `node tests/browser-auth.mjs --notifications`: nhắc hạn cũ đạt badge, lọc/phân trang, học kỳ độc lập, mobile, đọc tất cả, F5, lỗi/thử lại và mở nhiệm vụ. Browser dùng API giả; HTTP/PostgreSQL chạy riêng, không ghi thành kiểm thử trình duyệt nối database người dùng.
- DB thử được bộ kiểm tra tạo/xóa; đã dừng PostgreSQL thử. Chưa áp dụng migration hoặc thử trực tiếp tài khoản người dùng. Không commit/push/triển khai.

## Việc tiếp theo

- Khi thêm nhiều loại cảnh báo, có thể làm API hộp thư hợp nhất nhưng cần giữ snapshot/phân trang và chuyển dữ liệu có kiểm chứng. Hiện bảo toàn nghiệp vụ nhắc hạn đang có.
- Các migration trước đó, gồm cấu hình kế hoạch và đổi tên khoảng học, cần áp dụng theo thứ tự khi nâng cấp database ứng dụng.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
