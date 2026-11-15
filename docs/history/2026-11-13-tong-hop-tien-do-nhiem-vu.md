# 2026-11-13 — Tổng hợp tiến độ nhiệm vụ từ con lên cha

## Yêu cầu và mục đích

- Người dùng chốt khuyến nghị: nhiệm vụ cha lấy tiến độ từ con, cập nhật xuyên suốt cây; checklist chỉ là nguồn tiến độ ở nhiệm vụ lá. Yêu cầu cải tiến code, không chỉ dừng ở tư vấn.
- Đã đọc mục lục, [quy tắc checklist độc lập trước đây](2026-11-10-tach-checklist-va-tien-do-nhiem-vu-con.md), [cửa sổ sửa nhiệm vụ tại trang focus](2026-11-12-sua-nhiem-vu-tai-trang-tap-trung.md), code service/cây/DTO/giao dịch/migration, FE chi tiết và các kiểm tra hiện có.
- Trước sửa: cha/con cố ý độc lập; hoàn tất checklist cha có thể đưa cha vào Completed dù con chưa xong. Backend không khóa thêm con vào Completed. Điều này không phù hợp khi phần trăm của “Làm đồ án” phải phản ánh toàn bộ các nhánh slide/báo cáo.
- Tạo nhánh `feat/tong-hop-tien-do-nhiem-vu` ở hai repository vì đây là nhóm nghiệp vụ riêng; giữ nguyên các thay đổi focus chưa commit từ trước. Không tự commit/push/triển khai.

## Code và logic thay đổi

### Một nguồn tiến độ và cập nhật cả cây

- [QuyTacCay.cs](../../backend/StudyPlanner.Application/HocTap/NhiemVu/QuyTacCay.cs): `TongHopTienDo` duyệt từ dưới lên, cha bằng tổng phần trăm các con trực tiếp chia số con, lấy phần nguyên. Không gom tất cả cháu vào mẫu số; nhánh nhiều bước không tự có trọng số lớn hơn nhánh khác. `GanTienDo` dùng chung cho phần trăm/trạng thái/ngày hoàn thành.
- Mọi con đang thuộc cây đều tham gia, kể cả đã Completed; việc xuất hiện trong danh sách Hoàn thành không cắt quan hệ cha–con. 100% chỉ có khi mọi con trực tiếp 100%. Dùng phần nguyên ngăn 99,5% thành 100% giả. Trạng thái theo phần trăm nguyên lưu: 0 NotStarted, 1–99 InProgress, 100 Completed. Ngày hoàn thành giữ khi vẫn 100%, xóa khi giảm và ghi lại khi hoàn thành lần sau.
- Lá có checklist tiếp tục lấy tỷ lệ mục đã xong; lá không checklist dùng phần trăm nhập tay. Cha không được nhập tay progress/complete/reopen (`TASK_PROGRESS_IS_DERIVED`) hoặc gắn checklist không rỗng (`TASK_CHECKLIST_REQUIRES_LEAF`). Mô tả/metadata cha vẫn chỉnh sửa bình thường.
- [NhiemVuService.cs](../../backend/StudyPlanner.Application/HocTap/NhiemVu/NhiemVuService.cs): thêm/xóa/thùng rác/chuyển con tính lại mọi tổ tiên trước khi so sánh ảnh hưởng lịch, lưu lịch sử và commit. Chuyển cha tính cả nhánh nguồn/đích. Cha mất con cuối cùng trở lại lá 0% vì nguồn tiến độ dẫn xuất không còn, không giữ Completed giả.
- Khôi phục con tính lại cây cha còn tồn tại và nhóm khôi phục; nếu cha không còn phù hợp phạm vi/hạn/giới hạn thì giữ cơ chế khôi phục thành gốc riêng. Lịch sử tổ tiên thay đổi có nguồn System và phân biệt Completed/Reopened. Toàn bộ cây, phần việc bảo toàn, phiên dự kiến, lịch sử và revision vẫn chung giao dịch; lỗi không được giữ một phần thay đổi.

### Bảo toàn phần việc khi chia nhỏ và xác nhận

- Khi lá có checklist nhận con đầu tiên, `ChuanBiChiaNho` giữ toàn bộ nhãn/ô tick trong con **Công việc chung**, cùng chủ/phạm vi/ưu tiên và ước lượng cũ; xóa checklist trên cha rồi tổng hợp từ con.
- Lá không checklist nhưng có tiến độ >0 nhận con đầu tiên sẽ có con **Phần việc đã thực hiện** giữ phần trăm, mốc hoàn thành và ước lượng cũ. Đây là bảo toàn phần việc cũ, không đồng thời cộng phần trăm cũ trên cha. Ví dụ cũ 60%, thêm việc mới 0% → hai con 60/0, cha 30%.
- Khi chuyển một việc vào một lá khác cũng áp dụng quy tắc bảo toàn ở đích. Khôi phục vào cha đã trở thành lá và có phần việc riêng mới cũng giữ phần đó trước khi gắn lại con. Không chuyển/nhân bản bản ghi focus hoặc đoạn thực học cũ sang con bảo toàn; các bản ghi lịch sử đó vẫn thuộc đúng ID nhiệm vụ đã học.
- [ThemNhiemVuConRequest.cs](../../backend/StudyPlanner.Application/HocTap/NhiemVu/ThemNhiemVuConRequest.cs) và [ChaNhiemVuRequest.cs](../../backend/StudyPlanner.Application/HocTap/NhiemVu/ChaNhiemVuRequest.cs) thêm `XacNhanDoiCauTruc`. [AnhHuongNhiemVuResponse.cs](../../backend/StudyPlanner.Application/HocTap/NhiemVu/AnhHuongNhiemVuResponse.cs) thêm `MaNhiemVuMoLai`, `MaChecklistChuyenThanhBuoc`, `MaTienDoChuyenThanhBuoc`.
- Preview mô phỏng đầy đủ để trả các ảnh hưởng trên, không ghi/bump revision. Khi children/parent gây mở lại hoặc chuyển nguồn tiến độ mà thiếu cờ, BE trả `TASK_STRUCTURE_CONFIRMATION_REQUIRED` và dữ liệu ảnh hưởng; không chỉ dựa vào FE. Sửa tiến độ/checklist trực tiếp ở lá là thao tác rõ ràng của người dùng, không cần cờ cấu trúc; vẫn cập nhật tổ tiên.
- Các con bảo toàn được trả cùng danh sách con tạo mới. Kiểm tra độ sâu 10 cấp/1.000 node tính cả con bảo toàn. Xác nhận cấu trúc không bỏ qua xác nhận hủy phiên dự kiến hoặc chặn thay đổi ảnh hưởng phiên đang học.

### Chuyển dữ liệu hiện có

- [Migration dữ liệu](../../backend/StudyPlanner.Infrastructure/Migrations/20261113010000_TongHopTienDoNhiemVu.cs) cùng Designer dùng model hiện có, không thêm cột/bảng thường trực. Chạy sau migration focus trước đó.
- Kiểm tra trước toàn bộ cây/chủ/phạm vi/nhóm xóa mềm, JSON checklist và giới hạn kể cả các con sẽ tạo. Dữ liệu không phù hợp khiến migration dừng/rollback với thông báo, không cắt hoặc bỏ nhiệm vụ.
- Checklist cha cũ được chuyển sang con Công việc chung trong cùng nhóm đang dùng hoặc nhóm thùng rác. Giữ các node gốc và quan hệ hiện có; tiến độ độc lập cũ của cha được lưu trong lịch sử trước/sau rồi thay bằng tổng hợp. Lá nhập tay không checklist giữ giá trị; lá có checklist được chuẩn hóa theo checklist.
- Tính lại từ tầng sâu lên, ghi lịch sử System và tăng revision các tài khoản thay đổi. Hủy reminder/thông báo chờ của node mới hoàn thành, giữ thông báo đã phát. Không tự phục hồi các reminder đã Cancelled trước đó khi mở lại; không tự đặt lại phiên học cũ đã hủy.
- Down từ chối tự động để tránh xóa con bảo toàn có thể đã được người dùng sửa/học/lập lịch sau nâng cấp. Nếu cần quay lại phải dùng bản sao lưu trước nâng cấp đã kiểm tra.
- Chỉ thử trên PostgreSQL riêng; chưa tự áp dụng migration vào database ứng dụng.

### Giao diện dùng cùng quy tắc

- Repository FE `C:/Users/ADMIN/Desktop/Study_Planner_FE`: `src/hoc-tap/NhiemVuView.tsx` cho cha mở chi tiết thay vì thanh nhập phần trăm/đánh dấu hoàn thành. Ẩn menu tiến độ nhập tay và nút chuyển checklist ở cha, hiện lời giải thích nguồn tiến độ gồm cả con đã hoàn thành. Lá vẫn có các thao tác phù hợp nguồn của mình.
- `src/hoc-tap/api.ts`/`types.ts`: đọc các ảnh hưởng mới, gộp xác nhận cấu trúc và lịch học trong một dialog, chỉ gửi cờ sau đồng ý và giữ If-Match của lần đọc. Hủy không gửi ghi; lỗi/xung đột không tự nâng phiên bản rồi ghi lại. Có xử lý lỗi xác nhận trả từ BE nếu client cũ chưa gửi cờ.
- Chi tiết tại trang Tập trung dùng chính component nhiệm vụ nên tự nhận quy tắc mới. Đồng hồ vẫn ghi thực học riêng, không tự tăng tiến độ.
- Cập nhật [module 03](../modules/03-nhiem-vu-va-tien-do.md), [hợp đồng API](../THIET-KE-API-SWAGGER.md), [ERD/quy tắc](../erd.md) và Swagger theo code đã xác minh; giữ bản ghi độc lập cũ làm lịch sử.

## Kiểm chứng

- Đã đọc lại code/diff thuật toán, service/giao dịch/restore, DTO/xác nhận, migration SQL/model, kiểm tra API, FE controls/helper/fixture và tài liệu.
- `npm test`: **134/134 đạt**, gồm 6 kiểm tra xác nhận cấu trúc mới. Ba assertion SSR cũ không còn khớp menu focus/lịch thu gọn được cập nhật theo markup thực tế, vẫn giữ kiểm tra xác thực và chặn ghi trước khi tải API; không sửa ứng dụng để chiều assertion cũ.
- Browser HTTP fixtures: cây nhiệm vụ **79 requests**, focus **196 requests**, phiên tự học **72 requests**, cả ba đạt. Kiểm tra cha không thể tự complete, con đã xong vẫn thuộc cây, hủy xác nhận không POST, xác nhận mở lại, bảo toàn checklist/tiến độ/ước lượng và hồi quy pause/resume/finish/cập nhật tiến độ sau học.
- TypeScript + production FE build đạt; còn cảnh báo chunk JS >500 kB đã có. `git diff --check` đạt ở hai repository. Đã xem ảnh `backend/obj/task-progress-screenshots/task-progress-parent.png`, dữ liệu fixture, không phải nhiệm vụ người dùng.
- SchemaChecks đạt: 19 bảng, 31 FK, các CHECK/delete behavior và SQL sinh từ model, không kết nối database.
- Kiểm tra BE thuần: 22 assertions cây/hạn/tiến độ, 23 assertions phiên tự học và các nhóm thời gian/focus đạt.
- Migration PostgreSQL: **21 assertions đạt** với dữ liệu legacy đang dùng/thùng rác, bảo toàn checklist, tiến độ/ngày hoàn thành/revision/reminder, lặp migration, từ chối Down và rollback khi vượt giới hạn. Lượt đầu phát hiện PostgreSQL tính aggregate LATERAL trên hàng checklist rỗng trước khi lọc; đã sửa chia bằng NULLIF và chạy lại đạt.
- Core HTTP/PostgreSQL: **561 assertions đạt**, gồm cây ba cấp, 100% thật, ngày hoàn thành, preview không ghi, từ chối thiếu xác nhận, mở lại tổ tiên, thùng rác/restore, chuyển cha cả hai nhánh, xóa con cuối, bảo toàn checklist/tiến độ/ước lượng, sai chủ/phiên bản và lỗi ghi sau khi đã lưu một phần phải rollback. Có ca khôi phục con vào cha đã trở lại lá và được nhập tiến độ mới.
- Hồi quy focus HTTP/PostgreSQL: **121 assertions đạt**, gồm stopwatch/countdown/pause/resume, lịch sử, ghi chú, bản nhập tay, phiên tự học cũ, quyền sở hữu, thống kê và không tự tăng tiến độ theo thời gian học.
- Bộ kiểm tra client/API cũ còn tìm `frontend-preview/checks-http.cjs` đã bỏ. Thay bằng [ClientContractChecks.mjs](../../backend/tests/StudyPlanner.CoreChecks/ClientContractChecks.mjs), nạp chính `src/hoc-tap/api.ts` của FE qua Vite rồi gọi API thật của database tạm: hủy/xác nhận, checklist giữ nguyên, cha 25→75→100→66, từ chối sửa tay cha và stale If-Match đều đạt. Truyền tài khoản fixture qua stdin, không ghi token ra log. Mặc định tìm repository FE cùng thư mục cha; có thể chỉ định `STUDYPLANNER_TEST_FRONTEND` khi bố trí khác. Tiến trình có timeout 60 giây; không âm thầm bỏ kiểm tra khi thiếu FE.
- Build Release CoreChecks/API đạt 0 warning, 0 error. Các lần build đầu trong sandbox chỉ trả Build FAILED không có diagnostic; đã chạy lại với quyền thực thi cần thiết và xác minh kết quả.
- Mọi database kiểm thử tạm đã được xóa trong finally; cụm PostgreSQL kiểm thử riêng được dừng sau khi chạy xong. Không dùng database ứng dụng.
- Chưa chạy bộ browser toàn ứng dụng; các kịch bản liên quan được chạy riêng. Chưa thử ghi bằng tài khoản/database người dùng và chưa triển khai.

## Việc tiếp theo

- Áp dụng migration dữ liệu khi chuẩn bị database ứng dụng, sau đó khởi động lại BE và tải lại FE. Không dùng bộ CoreChecks lên database ứng dụng.
- Người dùng đọc diff/bản ghi và kiểm tra trải nghiệm với dữ liệu thật sau nâng cấp. Chưa commit/push/triển khai.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
