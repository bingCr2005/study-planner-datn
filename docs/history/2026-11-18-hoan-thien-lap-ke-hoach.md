# 2026-11-18 — Ổn định và làm gọn luồng lập kế hoạch

## Yêu cầu và mục đích

- Người dùng yêu cầu thực hiện cả ba giai đoạn, tận dụng chức năng hiện có; mỗi màn chỉ giúp sinh viên làm một việc chính, có đường quay lại và giữ bản nhập.
- Tiếp tục trên nhánh `feat/hoan-thien-lap-ke-hoach` ở hai repository. Giữ các thay đổi nhiệm vụ, tập trung và quản trị đã có trong working tree; không tự commit/push/triển khai.
- Đã đọc và đối chiếu [luồng áp dụng](2026-11-17-ap-dung-lich-tu-hoc.md), [kiểm thử ưu tiên](2026-11-17-doi-chieu-test-uu-tien-nhiem-vu.md), [Database và thực học](2026-11-18-nghiem-thu-sua-thoi-gian-hoc.md), module 07 và code hiện hành.
- Link chat `https://chatgpt.com/share/6afc8f97-01b0-83ec-904b-9d30bbc1a118` chỉ trả trang khung/tiêu đề khi truy cập, không đọc được nội dung hội thoại. Phần triển khai dựa trên yêu cầu và phương án người dùng đã cung cấp trong cuộc trao đổi này, không coi nội dung link là đã xác minh.

## Code và logic thay đổi

### Giai đoạn 1: ổn định và Database

- Kiểm tra Development bằng giao dịch RepeatableRead READ ONLY: 17 bảng nghiệp vụ, 27 khóa ngoại; đã có migration ghim và lịch sử sửa thời gian học, cột JSONB đúng kiểu. Admin đang dùng phiên bản 9 với Importance → Deadline → InProgress. Không đổi cấu hình thật hoặc ghi lịch thử vào Development.
- Model EF khớp migration, không có migration mới cần chạy. Tận dụng bản sao lưu `backend/obj/database-backups/focus-before-upgrade-20261117-171303.json` và bằng chứng phục hồi/nâng cấp ở bản ghi trước. Không chạy lại nâng cấp đã hoàn thành; không tạo migration trùng.
- [PlanningChecks](../../backend/tests/StudyPlanner.PlanningChecks/Program.cs): các fixture 50 phút/10 phút nghỉ nay truyền cấu hình rõ ràng; thêm kiểm tra mặc định hiện hành 25/5. Tình trạng trước sửa là kỳ vọng test còn lệch cấu hình, không phải cần đổi mặc định sản phẩm về 50/10.
- [test-study-core.ps1](../../scripts/test-study-core.ps1) bỏ đường dẫn `frontend-preview/checks.cjs` không còn tồn tại, chạy bộ test FE thật với tham số `FrontendPath`. Thêm project PriorityChecks hiện có vào [solution](../../backend/StudyPlanner.sln) và danh sách suite để restore/build/test không bỏ sót.

### Giai đoạn 2: mỗi bước một việc chính

- FE [LapLichView](../../../Study_Planner_FE/src/hoc-tap/LapLichView.tsx) tách **Chọn thông tin** và **Chọn lịch muốn lưu**. Form vẫn được giữ khi quay lại từ kết quả. Cấu hình dùng lại `StudySettingsForm` trong hộp riêng, có **Quay lại lập lịch**.
- Màn kết quả giữ hai số chính: tổng phút của các phiên đang chọn và `max(0, phút cần xếp − phút đang chọn)`. Đây là phần việc đủ điều kiện cần xếp thêm, chưa gồm nhiệm vụ bị loại vì thiếu ước lượng/quá hạn. Phút đã dành chỗ và cấu hình tính lịch chuyển vào **Thông tin tính lịch**. Xác nhận lưu báo cả phút chọn, phần thiếu và số phiên cũ sẽ thay thế.
- Hiện `lyDo` dưới phiên đề xuất, dùng lại `priorityReasons` trên [Hôm nay](../../../Study_Planner_FE/src/pages/Dashboard/Dashboard.tsx). Cảnh báo mở riêng bằng phần thu gọn, tự mở nếu không có phiên nào.
- [LapLichService](../../backend/StudyPlanner.Application/HocTap/LapLich/LapLichService.cs) phân biệt không còn giờ tương lai, giờ chỉ nằm sau hạn, khe đều dưới 15 phút, hạn mức ngày thiếu và trường hợp phối hợp nhiều ràng buộc. Thông báo là chẩn đoán của dữ liệu/gợi ý hiện tại, không khẳng định đã chứng minh mọi phương án đều bất khả thi. Cảnh báo có nút tới giờ rảnh, nhiệm vụ, phiên hoặc cài đặt thích hợp.
- FE [HocTapPage](../../../Study_Planner_FE/src/hoc-tap/HocTapPage.tsx) thêm đường quay lại phía trên nội dung xử lý cảnh báo. Lưu bản nhập trong sessionStorage theo tài khoản + học kỳ/môn: ngày, số ngày, chế độ cập nhật, mã nhiệm vụ đã chọn và nội dung tìm. Sau khi sang màn khác sửa dữ liệu phải tạo lại gợi ý. Bản nhập không phải lịch đã lưu; đóng phiên trình duyệt có thể mất bản nhập. Nếu storage bị chặn, có thông báo giữ trang mở.
- Dùng lại nút Ẩn/Hiện menu của lịch cho các màn lập kế hoạch. Ở chiều rộng ≤800px thu gọn menu để nội dung chính xuất hiện ngay; vẫn mở lại được menu. Đã phát hiện và sửa điểm menu đẩy toàn bộ form xuống dưới màn hình khi xem ảnh 390px.
- [LichView](../../../Study_Planner_FE/src/hoc-tap/LichView.tsx) hiển thị phút dự kiến, số phút Pomodoro thực sự lấy từ cấu hình và cách Đếm lên ghi thực tế. Báo rõ nếu một lượt Pomodoro dài hơn phiên dự kiến; không tự đổi cấu hình/mục tiêu. Tận dụng bộ chọn chế độ đã có.

### Giai đoạn 3: cùng quy tắc Admin, giữ các ràng buộc

- [QuyTacUuTienNhiemVu](../../backend/StudyPlanner.Application/UuTien/QuyTacUuTienNhiemVu.cs) dùng đúng QH, K1 [0,24h], K2 (24,72h], K3 (72,168h], K4 >168h, K5 trống hạn như FE. Trong nhóm dùng các tiêu chí Admin đang bật, cuối cùng mã nhiệm vụ. Bổ sung trạng thái vào `CongViecLapLich`; không còn phân xử bằng việc ngắn hơn.
- [QuyTacCauHinhUuTien.Doc](../../backend/StudyPlanner.Application/UuTien/CauHinhUuTien.cs) dùng chung đọc/kiểm tra JSON cho Admin và lập lịch. Cấu hình hỏng trả lỗi rõ ràng thay vì âm thầm dùng quy tắc khác.
- [QuyTacLapLich](../../backend/StudyPlanner.Application/HocTap/LapLich/QuyTacLapLich.cs), phiên bản 4: phân bổ theo thứ tự nhóm/Admin bằng bộ xếp sẵn có. Nếu còn việc có hạn chưa xếp đủ, thử thêm đúng một lượt theo hạn chính xác trong từng nhóm. Chỉ chọn lượt thử khi không nhiệm vụ nào mất phút và có ít nhất một nhiệm vụ được xếp đủ thêm.
- Ca A Cao còn 3 ngày, B Thấp còn 2 ngày: một khe chung thì A thắng theo Admin mặc định. Hai khe mà chỉ khe đầu giúp B kịp hạn thì B trước, A sau, cả hai đủ. Trước đây bộ xếp luôn xét hạn trước nên B thắng cả trường hợp một khe; cải tiến giải quyết mâu thuẫn đó mà vẫn tận dụng việc xếp hạn sớm khi có lợi cho cả hai.
- Hôm nay là danh sách nên làm trước; lịch kết quả sắp theo giờ thực tế. Thứ tự giờ có thể khác thứ tự tham chiếu trong cùng nhóm khi có cải thiện khả thi được giải thích. Giữ nguyên bộ trừ lịch bận, giờ rảnh, hạn hiệu lực kế thừa, nghỉ, phiên ngắn cuối, hạn mức địa phương và validator apply.
- Token/response thêm `phienBanUuTien`. Apply mới khóa chia sẻ cùng khóa kích hoạt Admin (`193701,11`), đọc lại phiên bản, chặn gợi ý cũ bằng `PRIORITY_RULE_CHANGED`; khóa giữ tới cuối giao dịch. Tra yêu cầu đã thành công vẫn thực hiện trước bước này, giữ nguyên hash nội dung cũ để retry sau mất phản hồi trả đúng lô cũ. Không cần sửa schema.
- Đã có và tiếp tục dùng: chọn lá/nhánh, kế thừa hạn, trừ phút đã đặt, cập nhật phần chưa học nguyên tử, chữ ký preview, revision, idempotency, chọn Pomodoro/Đếm lên, sửa thực học và lịch sử chỉnh sửa. Không viết lại các phần này; không tự đổi hạn/ưu tiên hoặc tự áp dụng lịch.

## Kiểm chứng

- Kiểm thử hiện trạng/tính năng đã có: schema, di trú tiến độ 31 assertion, HTTP core 680, giờ rảnh/ngoại lệ 39, nhắc kế hoạch 47, tập trung 181, đính kèm 53; toàn bộ đạt trên database ngẫu nhiên trong cụm PostgreSQL localhost riêng. Có kiểm tra client FE thật → HTTP → PostgreSQL.
- Kiểm thử mới/sau cải tiến: PlanningChecks 234 assertion gồm mặc định 25/5, Admin, mốc biên, trạng thái/mã, hai ca K2, không lấy phút của việc khác, 60 fixture sinh tự động và fixture 300 nhiệm vụ. Planning HTTP 75 assertion gồm apply được ca B trước/A sau, đổi Admin làm preview hết hiệu lực, retry lô thành công sau đổi quy tắc, giữ nguyên lịch/hạn/ưu tiên, cảnh báo hạn mức ngày, giờ rảnh đã qua và giờ rảnh chỉ sau hạn. Đây là kết quả sau sửa, không phải khẳng định thuật toán cũ đã đạt các ca mới.
- Bộ PriorityChecks HTTP/PostgreSQL đạt: quyền, schema cấu hình, bản nháp bất biến, kích hoạt, khóa đồng thời, phiên bản, xóa an toàn và phân trang.
- FE unit đạt 178/178. Browser fixture đạt: lập lịch, Hôm nay có giải thích ưu tiên, chọn chế độ/phiên 151 yêu cầu, tập trung 279 yêu cầu. Luồng lập lịch có kiểm tra hai bước, phút chọn 20+5, quay lại, cấu hình riêng, stale preview, xác nhận qua giờ bắt đầu, mất phản hồi/reload/retry cùng khóa, desktop và 390px. Các bài trình duyệt dùng API fixture; tính nhất quán lưu thật được kiểm tra riêng bằng HTTP/PostgreSQL ở trên.
- `dotnet ef migrations has-pending-model-changes ... --no-build`: không có thay đổi model chưa có migration. Build backend/FE thành công. Lint FE exit 0, còn cảnh báo React hiện hành và bundle lớn; không coi đó là lint sạch cảnh báo.
- Một lượt gọi nhầm runner auth không chọn đúng chế độ Hôm nay bị timeout khi trang chưa dựng root; lượt `node tests/browser-auth.mjs --today` đúng phạm vi sau đó đạt. Không tính lượt auth đó là nghiệm thu toàn bộ auth.
- Đã xem ảnh desktop/mobile, đọc diff/code liên quan, kiểm tra khoảng trắng ở cả hai repo. Cụm PostgreSQL thử đã dừng, không còn PID. Không thay lịch/nhiệm vụ Development để chạy các ca thử; không khởi động lại dịch vụ Development đang dùng.

## Việc tiếp theo và giới hạn

- Thuật toán hai lượt có giới hạn, không chứng minh tối ưu toàn cục, không đảo mọi hoán vị hoặc thêm điều kiện tiên quyết cha–con. Khi thiếu giờ vẫn yêu cầu sinh viên quyết định thêm/đổi giờ hoặc sửa nhiệm vụ; không tự nới hạn mức.
- Khởi động lại backend bằng source mới trước khi nghiệm thu thủ công trên Development; FE dev server nạp lại thay đổi. Chưa triển khai ra môi trường dùng chung.
- Xem [module 07](../modules/07-goi-y-cong-viec.md) và [kịch bản LP-01…LP-11](../KICH-BAN-KIEM-THU-UU-TIEN-NHIEM-VU.md) cho dữ liệu test cụ thể.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
