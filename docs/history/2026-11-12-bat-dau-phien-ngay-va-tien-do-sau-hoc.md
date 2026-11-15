# 2026-11-12 — Bắt đầu phiên ngay và cập nhật tiến độ sau khi học

## Yêu cầu và mục đích

- Người dùng đồng ý có nút **Bắt đầu ngay** cho phiên tương lai và hỏi có phải vừa học vừa cập nhật tiến độ hay không.
- Cho phép học sớm khi sẵn sàng. Đồng hồ ghi thời gian thực học; kết quả công việc được xác nhận sau phiên, không suy ra phần trăm hoàn thành từ số phút.
- Đã đọc mục lục lịch sử, [luồng tự học và bấm giờ](2026-11-10-luong-tu-hoc-va-bam-gio.md), [bố cục lập lịch](2026-11-11-lam-gon-man-lap-lich-hoc.md), service phiên và code FE đang chạy. Code trước sửa thực sự chặn `start` trước giờ dự kiến ở cả FE và BE. Luồng cập nhật tiến độ sau phiên đã có sẵn, không cần người dùng chỉnh phần trăm liên tục.
- Hai repository BE và FE đều làm trên nhánh `feat/bat-dau-phien-hoc-ngay`. Nhánh FE được tạo từ trạng thái hiện hành có các commit trước đó; không ghi đè hoặc bỏ các commit ấy.

## Code và logic thay đổi

- [PhienTuHocService.DoiTrangThaiAsync](../../backend/StudyPlanner.Application/HocTap/PhienTuHoc/PhienTuHocService.cs): trước sửa yêu cầu `BatDauDuKien <= now < KetThucDuKien`. Sau sửa phiên `Planned` có thể bắt đầu trước giờ dự kiến, miễn chưa tới giờ kết thúc. Vẫn kiểm tra quyền sở hữu, phiên bản dữ liệu, nhiệm vụ chưa hoàn thành, tối đa một phiên đang học và không có đồng hồ khác chưa đóng.
- `C:/Users/ADMIN/Desktop/Study_Planner_FE/src/hoc-tap/LichView.tsx`, component `PhienChiTiet`: phiên tương lai hiện **Bắt đầu ngay** cạnh Hủy phiên và Dời phiên. Phiên đã tới giờ vẫn có **Bắt đầu học**. Hướng dẫn khi chạy đồng hồ nói rõ tập trung làm bài và cập nhật tiến độ sau; khi kết thúc hiển thị tiến độ đang có và cho cập nhật hoặc để sau.
- Luồng: mở phiên → bấm bắt đầu → BE mở `DoanThucHoc` bằng giờ máy chủ → FE đếm từ mốc đó → đóng/mở lại hoặc tải trang vẫn đọc được mốc → kết thúc đóng đoạn thực học → xem thời gian đã ghi và tùy chọn cập nhật tiến độ. Giờ dự kiến giữ nguyên để đối chiếu với thực tế.
- Thời gian đồng hồ và tiến độ là hai dữ liệu riêng. `finish` hoàn thành **phiên**, không tự hoàn thành **nhiệm vụ**. Sau phiên, nút cập nhật dùng luồng có sẵn: nhiệm vụ có checklist mở chi tiết để tick, nhiệm vụ thường mở biểu mẫu phần trăm. Có thể đóng và cập nhật sau. Thời lượng còn lại của nhiệm vụ vẫn được tính từ ước lượng và tiến độ, không tự trừ số phút đồng hồ.
- Phiên đã hết giờ mà chưa bắt đầu vẫn chỉ xác nhận đã học xong hoặc bỏ lỡ. Bắt đầu sớm chuyển ngay sang `InProgress`, nên không còn được dời/hủy như một phiên chưa bắt đầu. Không có migration hay thay đổi hợp đồng API.
- [PhienTuHocChecks](../../backend/tests/StudyPlanner.CoreChecks/PhienTuHocChecks.cs) và [Program](../../backend/tests/StudyPlanner.CoreChecks/Program.cs): bổ sung kiểm tra service với store bộ nhớ, bao gồm bắt đầu sớm, biên thời gian, khôi phục đồng hồ, gọi lại không tạo đoạn trùng, kết thúc trước giờ dự kiến, giữ nguyên tiến độ, khóa phiên bản và chặn phiên/timer khác. Store bộ nhớ không mô phỏng khóa hay rollback PostgreSQL.
- [IntegrationChecks](../../backend/tests/StudyPlanner.CoreChecks/IntegrationChecks.cs): đổi tình huống HTTP có sẵn sang bắt đầu trước lịch 10 phút, xác minh mốc thực tế và khoảng dự kiến giữ nguyên. Bộ này cần database kiểm thử riêng.
- `C:/Users/ADMIN/Desktop/Study_Planner_FE/tests/browser-hoc-tap.mjs`: bổ sung dữ liệu đồng hồ và thông báo rỗng cho fixture, kiểm tra bắt đầu sớm → đồng hồ tăng → tải trang/mở lại → kết thúc → đóng và cập nhật sau. Thêm `CORE_SESSION_ONLY=1` để chạy riêng cùng kiểm tra phiên, không phụ thuộc các kiểm tra menu cũ.
- Cập nhật [kịch bản demo](../KICH-BAN-DEMO-NHIEM-VU-VA-THOI-KHOA-BIEU.md) vốn còn nói chỉ được bắt đầu đúng khoảng giờ. Bản ghi cũ giữ lại để đối chiếu lịch sử.

## Kiểm chứng

- Đã đọc lại diff BE, FE, kiểm tra trạng thái phiên và cách tính `ThoiLuongConLaiPhut`; `git diff --check` hai repository đạt.
- `dotnet build backend/tests/StudyPlanner.CoreChecks/StudyPlanner.CoreChecks.csproj --configuration Release --no-restore`: đạt, 0 cảnh báo và 0 lỗi. Lượt build trong sandbox dừng mà không báo lỗi compiler; chạy lại với quyền thực thi phù hợp đạt.
- `dotnet backend/tests/StudyPlanner.CoreChecks/bin/Release/net10.0/StudyPlanner.CoreChecks.dll`: đạt các kiểm tra thời gian, 13 kiểm tra cây/tiến độ và **23 kiểm tra phiên/bắt đầu sớm/đồng hồ/tiến độ** mới.
- Tại repo FE, `node node_modules/typescript/bin/tsc --noEmit -p tsconfig.app.json` và `node --check tests/browser-hoc-tap.mjs`: đạt.
- `node --test tests/hoc-tap.test.mjs`: **17/17 đạt**.
- PowerShell tại repo FE: `$env:CORE_SESSION_ONLY='1'; node tests/browser-hoc-tap.mjs`: đạt, **59 request giả lập**, chạy route và component React thật. Đã xác minh tiến độ không tự thay đổi khi bắt đầu/kết thúc, có thể đóng sau phiên rồi mở lại và tự cập nhật từ 25% lên 50%.
- Bộ browser tổng dừng ở assertion **Learning navigation must be vertical and positioned to the left of content** trước khi tới luồng phiên. Đã xuất kịch bản nguyên bản từ `git show HEAD:tests/browser-hoc-tap.mjs` vào `obj` và chạy lại: cùng lỗi. Không tính bộ tổng là đạt.
- Chưa chạy PostgreSQL/HTTP vì môi trường không có `STUDYPLANNER_TEST_POSTGRES`. Không dùng database ứng dụng để thay thế. Kết quả browser là HTTP giả lập, không phải xác minh FE → BE → PostgreSQL đầu cuối.

## Việc tiếp theo

- Khởi động lại backend và tải lại frontend để dùng quy tắc mới. Thử một phiên tương lai bằng dữ liệu của người dùng: Bắt đầu ngay, học, Kết thúc phiên, rồi cập nhật phần trăm/checklist theo kết quả.
- Khi có cấu hình PostgreSQL kiểm thử riêng, chạy lại IntegrationChecks để xác minh thêm giao dịch và endpoint thật.
- Lỗi kiểm tra bố cục menu của bộ browser tổng cần xử lý trong phần việc liên quan tới menu; luồng phiên đã có cách chạy riêng tái lập được.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa commit/push.
