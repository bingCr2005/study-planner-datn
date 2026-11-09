# 2026-11-06 — Chuyển đầy đủ chức năng từ frontend-preview và sửa bộ lọc

## Yêu cầu và mục đích

- Người dùng báo FE mới trống nhiệm vụ trong khi `frontend-preview` có việc cá nhân và các bước nhỏ. Người dùng yêu cầu kiểm tra trực tiếp toàn bộ code của `frontend-preview`, chuyển đủ chức năng sang `C:/Users/ADMIN/Desktop/Study_Planner_FE` và tự kiểm tra các luồng.
- Bản ghi trước là [lượt tích hợp FE](2026-11-06-tich-hop-fe-nhiem-vu-va-thoi-khoa-bieu.md). Lượt đó chuyển thiếu một số phần của giao diện cũ. Bộ kiểm thử trình duyệt trước cũng chưa mô phỏng việc API loại nhiệm vụ cá nhân khi lọc học kỳ, nên bỏ sót lỗi mặc định này.
- Đã đọc lại `frontend-preview/app.js`, `index.html`, `styles.css`, các handler, biểu mẫu và client HTTP của bản cũ. Đối chiếu với toàn bộ module React học tập hiện tại và controller/DTO/service BE liên quan.

## Nguyên nhân đã xác minh

- FE mới khởi tạo bộ lọc bằng `data.selectedSemesterId` và tự gán lại khi học kỳ của thanh bên thay đổi. Vì thế `/tasks` gửi `maHocKy` ngay khi tải dữ liệu học kỳ xong. Nhiệm vụ cá nhân có `maHocKy = null` bị loại khỏi kết quả.
- Một số chức năng của bản cũ bị bỏ sót trong lượt chuyển trước, gồm thêm nhanh, lọc ưu tiên/loại/trạng thái, thêm nhiều bước trong một lần, chi tiết bên phải, liên kết trực tiếp, nội dung lịch sử trước/sau, phiên học theo nhiệm vụ và trục giờ của lịch ngày/tuần.
- CSS của module chỉ đặt độ rộng tối đa và `margin: auto`, khiến màn hình có thể bị co vào giữa thay vì dùng hết vùng nội dung của FE team.
- Kiểm thử API thật còn phát hiện tìm nguyên văn tiếng Việt có dấu có thể trượt khi PostgreSQL dùng collation C. Phép đổi hoa ở .NET và DB cho kết quả khác nhau. Đã bổ sung nhánh tìm nguyên văn, đồng thời giữ nhánh đổi hoa hiện có.

## Code và logic thay đổi

### Phạm vi và dữ liệu

- `src/hoc-tap/boLoc.ts` lấy phạm vi từ lựa chọn rõ ràng trên URL. Mặc định `/tasks` xem chung các học kỳ và việc cá nhân. Tên học kỳ đang chọn ở thanh bên phục vụ danh sách môn của team, không tự trở thành điều kiện lọc ẩn trong module.
- Học kỳ/môn vẫn chọn theo tên từ API Academic hiện có. Chọn môn ghi cả môn và học kỳ tương ứng trên URL. Xóa bộ lọc bỏ các điều kiện của màn nhiệm vụ. Danh sách việc cá nhân được xem qua các học kỳ.
- `HocTapPage.tsx` tách trạng thái đang tải, tải lỗi và kết quả trống. API lỗi hiển thị thông báo và nút thử lại, tránh ghi nhận thành danh sách rỗng. Việc vừa tạo được mở trực tiếp, đưa về phạm vi và danh sách phù hợp để người dùng thấy kết quả. Màn đang mở được lưu trên URL, nên tải lại trang vẫn giữ màn gợi ý/giờ rảnh/phiên đang xem.

### Bảng đối chiếu với code cũ

| Chức năng trong frontend-preview | Nguồn cũ | Phần FE mới tương ứng |
| --- | --- | --- |
| Các danh sách hôm nay, 7 ngày, tất cả, cá nhân, quá hạn, chưa có hạn, đã hoàn thành | `NAV`, `loadTasks` | Bộ Hiển thị trong `HocTapPage`, truy vấn trong `boLoc` |
| Tìm tên/nội dung, lọc học kỳ, môn, ưu tiên, loại, trạng thái, sắp xếp và phân trang | `taskControls`, `task-filters` | Bộ lọc đầy đủ và `NhiemVuView` |
| Thêm nhanh một việc | `quick-add` | `ThemNhanh`, gửi đúng phạm vi đang xem rồi mở việc vừa tạo |
| Xem nhiệm vụ con nhiều cấp | `taskRow`, `childrenRows` | Mở/thu gọn các bước nhỏ bằng tên |
| Chi tiết bên phải và dữ liệu ước lượng, hạn, tiến độ, loại, sự kiện | `renderDetail` | `ChiTietNhiemVu` dạng panel bên phải, có cuộn riêng |
| Tạo/sửa nhiệm vụ, thời gian sự kiện, ước lượng | `taskFields`, `collectTask`, `editTask` | `SuaNhiemVu` và giữ chính xác thời điểm cũ khi chỉ sửa nội dung |
| Thêm nhiều nhiệm vụ con nguyên khối | `nhieuTieuDe`, `editTask` | Ô mỗi dòng một bước nhỏ, gửi một request `/children` |
| Hoàn thành/mở lại, tiến độ, tổng hợp theo các bước nhỏ | `toggleTask`, `progress` | Checkbox, biểu mẫu tiến độ và kết quả tính từ BE |
| Chuyển vào một nhiệm vụ/bước nhỏ hoặc tách riêng | `change-parent` | Chọn nhóm theo tên rồi duyệt đủ các bước trong nhóm, loại chính nhiệm vụ đang chuyển và các bước thuộc nó |
| Đổi học kỳ/môn cho nhóm nhiệm vụ | `change-scope` | `DoiPhamVi`, chọn tên, mở lại việc theo phạm vi vừa lưu |
| Xóa có số liệu ảnh hưởng | `deletion-preview`, `delete-task` | Xác nhận số nhiệm vụ, lịch nhắc, thông báo, tệp và phiên học liên quan |
| Lịch sử trước/sau | `renderHistory`, `history` | Nội dung trước/sau trong các mục mở rộng và phân trang thêm lịch sử |
| Sao chép liên kết, mở thẳng một nhiệm vụ | `copy-link`, `#task/` | `/tasks?task=...`, có kiểm quyền từ BE và ô sao chép thủ công dự phòng |
| Xem/đặt phiên học của một nhiệm vụ | `task-sessions`, `add-session-for-task` | Bộ phiên theo nhiệm vụ và biểu mẫu đặt phiên điền sẵn tên việc |
| Lịch ngày/tuần theo giờ, chia cột khi trùng giờ | `renderCalendar`, `renderDayBlocks` | `LichView`, thuật toán chia cột chuyển sang `lich.ts` |
| Lịch tháng/danh sách, điều hướng, bật/tắt năm lớp lịch | `calendarControls`, `visibleEvents` | Các chế độ và lớp lịch trong `LichView` |
| Thêm vào lịch bằng bốn loại dữ liệu | `calendar-add` | Chọn lịch lớp, phiên tự học, nhiệm vụ/sự kiện hoặc giờ rảnh |
| CRUD giờ rảnh và mẫu lịch lớp | `availabilityForm`, `classForm`, `calendarMutation` | Biểu mẫu theo tên môn, xem ảnh hưởng, xác nhận trùng lớp/hủy phiên |
| Phiên từ 7 ngày trước đến 31 ngày tới và các trạng thái | `renderSessions`, `sessionForm`, `session-*` | Khoảng phiên tương ứng, đặt/dời/bắt đầu/kết thúc/bỏ lỡ/hủy theo thời điểm hợp lệ |
| Gợi ý, lý do, số phút, chọn tất cả/bỏ chọn và áp dụng | `planningForm`, `renderPlanning`, `applyPlan` | `LapLichView`, xác nhận trước khi áp dụng và giữ nguyên khóa khi thử lại |
| JWT, phiên bản, xem ảnh hưởng, tải lại, lỗi và khôi phục yêu cầu chờ | `api`, `performTask`, `savePendingApply` | Client học tập và đăng nhập hiện có của team, `If-Match`/preview/idempotency được giữ |

### File được sửa

- FE gồm `src/hoc-tap/HocTapPage.tsx`, `NhiemVuView.tsx`, `LichView.tsx`, `LapLichView.tsx`, `hoc-tap.css`, thêm `boLoc.ts` và `lich.ts`. Bố cục chi tiết được đặt bên phải cả khu vực điều khiển, lịch ngày/tuần khôi phục trục 24 giờ và xử lý các mục trùng giờ.
- Kiểm thử FE gồm `tests/hoc-tap.test.mjs`, `tests/browser-hoc-tap.mjs` và các kỳ vọng dành cho module nhiệm vụ trong `tests/planner.test.mjs`. Phản hồi API giả lập đã thực sự áp các điều kiện FK và bộ lọc, có cả dữ liệu cá nhân lẫn dữ liệu theo môn.
- BE gồm [NhiemVuService.cs](../../backend/StudyPlanner.Application/HocTap/NhiemVuService.cs) và [IntegrationChecks.cs](../../backend/tests/StudyPlanner.CoreChecks/IntegrationChecks.cs). Bổ sung tìm nguyên văn tiếng Việt và các điều kiện hồi quy về việc cá nhân/tìm tên.
- Đã lưu dấu nội dung của các file FE trước khi sửa. Các file của Academic, đăng nhập, quản trị, hồ sơ, Dashboard, API chung và CSS chung trùng bản đầu vào. `package-lock.json` giữ đúng nội dung thay đổi đã có từ trước.

### Luồng đọc/ghi

1. Màn hình lấy bộ lọc người dùng chọn, đọc API theo JWT và nhận phiên bản dữ liệu.
2. Người dùng thêm/sửa nhiệm vụ, chia nhỏ hoặc đổi phạm vi. Các thao tác có ảnh hưởng đọc preview rồi kiểm lại phiên bản đã mở.
3. Xác nhận ảnh hưởng cần thiết rồi ghi. BE kiểm quyền, ràng buộc và giao dịch, trả dữ liệu đã lưu. FE tải lại danh sách/chi tiết, giữ liên kết nhiệm vụ đang xem.
4. Giờ rảnh/lịch lớp/phiên dùng cùng hợp đồng API hiện hành. Lịch ngày/tuần dựng khối theo múi giờ tài khoản, tách cột các lịch bận trùng giờ và giữ giờ rảnh làm nền.
5. Gợi ý chưa ghi phiên. Khi áp dụng, client lưu khóa/nội dung chờ trước khi gửi. Mất phản hồi dùng lại đúng khóa và nội dung, tránh tạo trùng.

## Kiểm chứng

- FE build production thành công với TypeScript và Vite. `npm test` đạt 100/100 bài ở lượt cuối.
- Chuỗi trình duyệt đạt với dữ liệu giống trường hợp người dùng báo, gồm `aaa`, bước `aa`, `vbbbb` là việc cá nhân và một việc theo môn. Học kỳ của thanh bên có giá trị nhưng danh sách mặc định vẫn hiển thị cả hai phạm vi.
- Đã kiểm trực tiếp bộ lọc, hoàn thành/mở lại, bảng trạng thái, thêm nhanh, tạo/sửa/xóa, hủy xác nhận xóa, lỗi phiên bản cũ, lịch sử trước/sau, sao chép/mở liên kết, thêm ba bước cùng lúc, đổi tiến độ, đổi phạm vi, chuyển vào một bước bên trong nhiệm vụ khác và tách lại.
- Đã kiểm xem phiên theo nhiệm vụ, đặt/dời/hủy, bắt đầu/kết thúc, xác nhận đã học xong khi hết giờ và xác nhận bỏ lỡ. Đã kiểm tạo/sửa/xóa lịch lớp, xác nhận trùng lớp, tạo/sửa/xóa giờ rảnh, gợi ý/áp dụng rồi tải lại trang và thử lại cùng khóa khi cố ý mất phản hồi, menu thêm bốn loại dữ liệu vào lịch và lịch tháng/trục giờ tuần.
- Chuỗi UI cuối có 241 request giả lập. Đã xác nhận các route ghi chỉ thuộc module học tập. Các lần thất bại khi bổ sung kịch bản đã được sửa và chạy lại, kết quả cuối được ghi ở đây.
- Đã mở ảnh desktop của danh sách, chi tiết bên phải, lịch tuần theo giờ và lịch tháng để rà bố cục. Ảnh và các bản sao chuẩn bị nằm dưới `backend/obj/fe-core-fix-20261106`, được Git bỏ qua.
- Kiểm thử BE trên PostgreSQL cục bộ riêng đạt 260 điều kiện HTTP/PostgreSQL, 15 điều kiện nhiệm vụ và kiểm tra thời gian thuần. Client prototype cũ đạt 11 điều kiện HTTP. Database tên ngẫu nhiên được bộ test tạo và xóa.
- Client FE mới đã gọi API/PostgreSQL thật để xác minh dữ liệu tên/ID học kỳ/môn, tạo việc cá nhân có dấu, tìm lại khi có học kỳ/môn, loại đúng sau khi người dùng lọc môn rõ ràng, tạo nhiệm vụ theo môn, thêm bước, xem ảnh hưởng/phiên bản, tổng hợp tiến độ, lịch sử và xóa 204.
- Đã đọc diff BE, các file React/helpers và các phần liên quan trong nguồn `frontend-preview`. `git diff --check` đạt. Đã kiểm tra database tạm được dọn và dừng đúng cụm PostgreSQL kiểm thử.

## Việc tiếp theo và giới hạn

- Người dùng tải lại FE ở `Study_Planner_FE`, mở mục Nhiệm vụ để bắt đầu từ danh sách chung. Phần BE có sửa tìm kiếm nên cần khởi động lại tiến trình API để nạp assembly mới.
- Chuỗi trình duyệt dùng phản hồi giả lập theo hợp đồng API. API/PostgreSQL dùng host thật và database kiểm thử riêng. Toàn bộ chuỗi trình duyệt với tài khoản và dữ liệu thật của người dùng vẫn cần người dùng xác nhận.
- Các lỗi đo sidebar/chờ phần tử trong bộ browser cũ của team được ghi ở bản ghi trước. Lượt này kiểm tra phạm vi file được bảo vệ và bộ test FE chung, không coi các bài browser cũ đó đã đạt.
- Bản ghi này thay thế mô tả phạm vi chuyển đổi còn thiếu ở lượt trước bằng trạng thái đã đối chiếu với code nguồn cũ. Code hiện là thay đổi local trên các nhánh công việc hiện có, chờ người dùng đọc/test và quyết định commit/push.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu nguồn cũ, code mới, diff và kết quả kiểm chứng cuối.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code. Chỉ người dùng xác nhận mục này.

Commit/PR chờ người dùng thực hiện hoặc yêu cầu.
