# 2026-11-18 — Đối chiếu luồng lập kế hoạch cho sinh viên

## Yêu cầu và mục đích

- Đối chiếu sáu nhóm yêu cầu người dùng viết trực tiếp; bổ sung chỗ thiếu, tận dụng code/API/nghiệp vụ, tập trung Web Desktop trắng–xanh.
- Đã đọc AGENTS, README/TEMPLATE lịch sử, [bản ghi UI](2026-11-18-bo-tri-lap-ke-hoach-va-xac-nhan-lich.md), [bản ghi thuật toán/Database](2026-11-18-hoan-thien-lap-ke-hoach.md), module 07 và source hiện tại. Tiếp tục nhánh `feat/hoan-thien-lap-ke-hoach` hai repo; giữ thay đổi sẵn có, không commit/push/triển khai.

## Đối chiếu với source

| Yêu cầu | Đã có, giữ nguyên | Phần thiếu đã bổ sung |
| --- | --- | --- |
| Luồng liền mạch | Tổng quan, ba bước, draft theo tài khoản/phạm vi, cảnh báo có đường xử lý, phiên → Focus → tiến độ | Lối tạo/sửa nhiệm vụ ngay ở Thiết lập; tách giờ rảnh và lịch lớp tùy chọn. Giữ đường quay lại/phạm vi gốc khi tạo nhiệm vụ hoặc đổi/xóa bộ lọc. |
| Giao diện đơn giản | Sidebar tách riêng, trắng–xanh, lý do/cấu hình mở riêng, cảnh báo thu gọn | Năm số liệu một hàng Desktop; danh sách phiên cũ/mới thu gọn và có vùng cuộn riêng. Cảnh báo phiên quá giờ dùng tiếng Việt, bỏ tên trạng thái kỹ thuật. |
| Gợi ý và áp dụng | Chọn tập con, kiểm tra phiên ngắn, preview hết hạn/qua giờ, xác nhận, retry cùng khóa | Đưa phút giữ lại/đề xuất ra ngoài phần nâng cao; giải thích cách tính; liệt kê từng phiên sẽ thay và từng phiên chọn lưu; phân biệt thiếu phút do bỏ chọn. |
| Ưu tiên | QH–K5, Admin trong nhóm, thử hạn trước có điều kiện, lý do từ dữ liệu thực | Không đổi thuật toán. Chạy lại biên 24/72/168 giờ, đổi Admin, một/hai khe K2, thiếu giờ và HTTP. |
| Các màn còn lại | Lịch tuần/năm lớp dữ liệu, thu gọn hạn/menu, cuộn tới hoạt động; lọc lớp/form giờ; tuần giờ rảnh/ngoại lệ ngày; lọc trạng thái phiên và Focus | Giữ các chức năng đã đủ, không làm thêm lịch sử/đồng hồ trùng. Các bộ lọc phiên cho xem riêng phần chưa học/đang học/hoàn thành/bỏ lỡ/đã hủy. |
| Ràng buộc | Deadline, trùng, nghỉ, giới hạn ngày, phiên ngắn, lịch sử, rollback và chống lưu trùng | Chỉ thêm dữ liệu đọc vào preview, không migration, không tự đổi hạn/ưu tiên hoặc áp dụng lịch. |

## Code và logic trước/sau

- FE [HocTapPage.tsx](../../../Study_Planner_FE/src/hoc-tap/HocTapPage.tsx): trước đây tạo/chuyển nhiệm vụ hoặc xóa bộ lọc có thể tạo URL mới làm mất dấu quay lại. Nay lưu phạm vi gốc khi rời lập lịch, giữ các tham số đó khi tạo nhiệm vụ/xóa lọc; Quay lại lập lịch khôi phục học kỳ/môn rồi đọc draft hiện có. Không nhận URL ngoài để điều hướng.
- FE [LapLichView.tsx](../../../Study_Planner_FE/src/hoc-tap/LapLichView.tsx): năm số **Cần xếp thêm / Đã có lịch · Giữ lại / Được đề xuất / Đang chọn để lưu / Còn chưa xếp sau khi lưu**. Ba số nguồn lấy từ response cũ, số chọn cộng các phiên tick, thiếu = `max(0, cần xếp thêm − chọn)`. Không tự cộng một tổng nhu cầu khác có thể sai khi đã đặt dư. Phút giữ lại có thể nằm ngoài khoảng ngày; chỉ gồm các nhiệm vụ đủ điều kiện trong phạm vi. Thiếu ước lượng/quá hạn được báo riêng.
- Khi cập nhật, backend hiện có đã bỏ các phiên sẽ thay khỏi số phút giữ lại trước khi tính nhu cầu; giữ nguyên phép tính đó. Bỏ chọn không đổi số phút đề xuất. Hộp xác nhận tách danh sách phiên cũ và phiên chọn lưu, có tên/ngày/giờ/phút; báo rõ toàn bộ phiên cũ bị thay kể cả chỉ lưu một phần gợi ý.
- Backend [XemTruocLichResponse.cs](../../backend/StudyPlanner.Application/HocTap/LapLich/XemTruocLichResponse.cs), [LapLichService.cs](../../backend/StudyPlanner.Application/HocTap/LapLich/LapLichService.cs): thêm `phienThayThe` từ chính dữ liệu phiên theo chủ sở hữu đã đọc trong giao dịch, khớp các mã phiên đã ký. Không thêm query/migration, không đổi token/hash retry hoặc transaction apply. Thông báo phiên qua giờ dùng lời dễ hiểu.
- FE [types.ts](../../../Study_Planner_FE/src/hoc-tap/types.ts), `LapLichView.tsx`: đọc trường bổ sung; nếu thiếu/không khớp chi tiết phiên cũ thì chưa cho áp dụng thay lịch, yêu cầu tạo lại gợi ý. Retry yêu cầu đã xác nhận trước đó vẫn dùng nguyên khóa/nội dung. [planning-design.css](../../../Study_Planner_FE/src/hoc-tap/planning-design.css) bố trí năm cột Desktop và danh sách có giới hạn chiều cao; giữ CSS thích ứng sẵn có, không triển khai thêm thiết kế mobile.
- [PlanningUpgradeChecks.cs](../../backend/tests/StudyPlanner.CoreChecks/PlanningUpgradeChecks.cs): thêm kiểm tra phút thêm/cập nhật và danh sách cũ khớp bản ghi thực. FE [browser-planning-upgrade.mjs](../../../Study_Planner_FE/tests/browser-planning-upgrade.mjs), [upgrade-fixture.mjs](../../../Study_Planner_FE/tests/upgrade-fixture.mjs) kiểm tra tạo nhiệm vụ rồi quay về đúng phạm vi/draft, năm số, danh sách cũ/mới và chọn một phần.

## Kiểm chứng thực tế

- Build solution Release đạt 0 lỗi/0 cảnh báo ngoài sandbox; lần sandbox ban đầu exit 1 không có chẩn đoán, không coi lần đó đạt. FE build đạt, còn cảnh báo chunk >500 kB. Lint exit 0, còn cảnh báo React sẵn có.
- `scripts/test-study-core.ps1 -NoBuild` trên PostgreSQL localhost riêng đạt: 234 assertion thuật toán, 680 core HTTP/PostgreSQL, **80 cấu hình/lập lại lịch/ưu tiên**, 39 giờ rảnh, 47 nhắc, 181 Focus, 53 đính kèm; schema/migration/Admin cũng đạt. FE unit **183/183** đạt. Log: `backend/obj/planning-requirements-all-checks.log` (ignored).
- Runner khởi tạo ban đầu bị giữ luồng xuất Windows sau khi PostgreSQL thử đã sẵn sàng. Đã chạy trực tiếp suite trên đúng cụm đó để có log kết quả ở trên, rồi dừng đúng cụm thử; không dùng log lỗi runner ban đầu làm bằng chứng đạt.
- Browser UI thật/API fixture: lập kế hoạch đạt **392 yêu cầu**, smoke tổng đạt **584 yêu cầu**. Gồm tạo nhiệm vụ mới rồi quay về giữ số ngày 9/học kỳ/môn gốc; xem các phiên cũ/mới, hủy xác nhận, chọn một phần, phiên 20+5 phút, stale preview và retry mất phản hồi. Đã xem ảnh Desktop 1440px, năm số liệu/danh sách/nút áp dụng rõ ràng.
- Development: audit `RepeatableRead` + `SET TRANSACTION READ ONLY` xác nhận 17 bảng/27 FK, migration mới nhất `20261117165006_LichSuSuaThoiGianTapTrung`, lịch sử thực học JSONB, Admin v9 Importance → Deadline → InProgress. Không ghi dữ liệu sinh viên. Đây là kiểm tra schema/cấu hình, **không phải E2E trên dữ liệu Development**. Log riêng `backend/obj/planning-requirements-development-audit.log`.
- Đã đọc lại code/diff liên quan; `git diff --check` đạt hai repo.

## Điều chỉnh chủ động và việc còn lại

- Không ép thứ tự giờ học giống Hôm nay; hai màn giải quyết hai việc khác nhau. Giữ thử tính khả thi đã được chứng minh bằng test, không đổi thuật toán để nhìn giống nhau.
- Không suy tiến độ từ số phiên hoặc thời gian Focus; hoàn thành phiên không tự hoàn thành nhiệm vụ. Giữ phần trăm/checklist/tiến độ con.
- Không thêm màn/tùy chọn trùng lịch tuần, lịch sử theo trạng thái, ngoại lệ ngày hoặc đồng hồ. Không ẩn hoàn toàn giờ trống/giờ đêm vì sinh viên có thể học muộn; dùng cơ chế cuộn/thu gọn/lớp lịch hiện có.
- Còn nghiệm thu thao tác Development và thay đổi đồng thời. Backend cần chạy source mới để trả `phienThayThe`; lượt này không triển khai/restart dịch vụ đang dùng.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu code, bản ghi, diff và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi/code. Chỉ người dùng xác nhận mục này.

Commit/PR: chưa tạo.
