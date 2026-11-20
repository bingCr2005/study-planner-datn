# 2026-11-17 — Sửa áp dụng lịch tự học cho sinh viên

## Yêu cầu và mục đích

- Sinh viên gặp lỗi phiên dưới 15 phút hoặc phiên đã qua giờ ngay sau khi chọn áp dụng gợi ý. Người dùng muốn sửa logic và có hướng dẫn ngắn, đặt cạnh thao tác. Đối chiếu [đoạn chat tham khảo](https://chatgpt.com/share/6afadc6a-9edc-83ec-a90c-811ba36b81ad), [bản ghi triển khai](2026-11-14-cai-tien-lap-ke-hoach.md) và code hiện tại.
- Tạo nhánh `fix/ap-dung-lich-tu-hoc` tại HEAD đang làm việc của hai repository, giữ các thay đổi đã có trong working tree. Nội dung dành cho màn Lập kế hoạch của sinh viên.

## Code và logic thay đổi

- [QuyTacLapLich.cs](../../backend/StudyPlanner.Application/HocTap/LapLich/QuyTacLapLich.cs) trước đây có thể đặt 25 phút ở khe sau rồi quay lại nhét 5 phút vào khe trước của cùng nhiệm vụ. Bộ đặt phiên lại xét theo thứ tự thời gian. Nay phiên dưới 15 phút phải xếp hết phần còn lại và nằm sau những phiên khác của việc đó. Nếu chỉ còn khe sớm hơn, giữ 5 phút trong phần chưa xếp. Tăng phiên bản quy tắc để yêu cầu chưa lưu dùng gợi ý mới.
- [QuyTacDatPhien.cs](../../backend/StudyPlanner.Application/HocTap/PhienTuHoc/QuyTacDatPhien.cs) làm tròn phần phút có thể lập lịch giống bước tạo gợi ý. Nếu phần chọn có phiên ngắn mà tổng phút không phủ đủ hoặc phiên ngắn không ở cuối, trả `SHORT_SESSION_REQUIRES_REMAINDER` kèm mã việc.
- [LapLichService.cs](../../backend/StudyPlanner.Application/HocTap/LapLich/LapLichService.cs) trả `PREVIEW_SESSION_STARTED` kèm mốc giờ nếu một phiên được chọn đã qua giờ dù token còn hạn. Kiểm tra trước khi đánh dấu hủy lịch thay thế. Yêu cầu đã lưu vẫn được tra theo khóa trước các kiểm tra thời gian và phiên bản.
- FE `src/hoc-tap/LapLichView.tsx` thêm một câu hướng dẫn đầu màn và kiểm tra lựa chọn trong phần kết quả. Nếu thiếu các phiên trước, **Chọn cùng** bổ sung chúng. Nếu phiên đã qua giờ hoặc gợi ý hết hiệu lực, **Tạo lại gợi ý** dùng lại dữ liệu trong form. Kiểm tra thời gian một lần nữa sau hộp xác nhận. Khi sang ngày mới, nút tạo lại lấy ngày hiện tại nếu ngày cũ đã qua.
- Các lỗi xác định cần tính lại kết thúc yêu cầu chờ. Nếu chưa rõ đã lưu hay chưa, giữ yêu cầu trong sessionStorage để thử lại đúng khóa và nội dung. Phạm vi cũ cũng được lưu để khôi phục form khi tải lại tab. Sau khi lưu, dùng trang Phiên tự học của lần áp dụng vừa rồi.
- Cảnh báo thiếu ước lượng, thiếu giờ rảnh và phiên đang học cần xử lý có nút dẫn đến đúng mục. `HocTapPage.tsx` mở trực tiếp chi tiết nhiệm vụ từ mã cảnh báo. `types.ts` dùng các trường `maNhiemVu` và `maLyDo` mà backend đã trả. `planning-upgrade.css` giữ thông báo và nút trong một hàng có thể xuống dòng trên màn hẹp. Hướng dẫn nằm cạnh thao tác thay cho một màn checklist riêng.

## Kiểm chứng

- Đọc lại diff hai repository và đối chiếu [module 07](../modules/07-goi-y-cong-viec.md). Thêm ca hồi quy ở [PlanningUpgradeChecks.cs](../../backend/tests/StudyPlanner.CoreChecks/PlanningUpgradeChecks.cs), FE `tests/browser-planning-upgrade.mjs` và fixture tương ứng.
- Build Release `StudyPlanner.CoreChecks` đạt 0 cảnh báo và 0 lỗi. 15 kiểm tra quy tắc cấu hình/xếp lịch đạt, gồm khe ngắn ở trước/sau phiên thường và nhiệm vụ chỉ cần 5 phút. 51 kiểm tra HTTP lập kế hoạch đạt trên PostgreSQL tạm, gồm phiên qua giờ trước khi token hết hạn, giữ lịch cũ, từ chối chỉ lưu đuôi ngắn và lưu cả hai phiên.
- Frontend build TypeScript/Vite đạt, với cảnh báo bundle trên 500 kB. `npm test` đạt 178/178. Chrome tập trung vào Lập kế hoạch đạt trên desktop và ở 390 px. Lượt 390 px cuối chạy 266 yêu cầu API giả, gồm chọn phiên ngắn, gợi ý cũ, qua giờ khi hộp xác nhận đang mở, nút xử lý cảnh báo và thử lại cùng khóa sau khi tải lại trang.
- CoreChecks chạy đầy đủ, gồm 680 kiểm tra HTTP lõi và các nhóm giờ rảnh, nhắc kế hoạch, đồng hồ và đính kèm. Dừng PostgreSQL tạm sau kiểm thử. `git diff --check` đạt trên các tệp đã sửa ở hai repository.
- Lượt chạy toàn bộ kịch bản trình duyệt dừng ở kiểm tra vị trí thanh điều hướng, trước nhóm thao tác gợi ý lịch. Chưa kiểm thử bằng tài khoản sinh viên trong môi trường Development.

## Việc tiếp theo

- Nạp bản backend/frontend mới rồi tạo lại gợi ý trên tài khoản sinh viên. Thử nhiệm vụ 30 phút với phiên tối đa 25 phút, nghỉ 5 phút và hai khe ngày mai `19:00–19:10`, `19:30–19:55`. Mong đợi lưu một phiên 25 phút, còn 5 phút chưa xếp.
- Khi thử trường hợp gợi ý đang mở qua giờ bắt đầu, xác nhận có nút **Tạo lại gợi ý** và vẫn giữ bộ lọc. Khi kiểm tra chế độ cập nhật, so phiên cũ trước và sau lần API từ chối.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
