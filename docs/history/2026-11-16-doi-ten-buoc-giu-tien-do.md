# 2026-11-16 — Đổi tên bước giữ tiến độ cũ

## Yêu cầu và mục đích

- Đổi tên bước “Phần việc đã thực hiện” và dùng từ “nhiệm vụ” cho nhất quán. Tên mới là “Nhiệm vụ đã bắt đầu” để phù hợp với phần việc đang làm dở. Xem [quy tắc bảo toàn tiến độ](2026-11-13-tong-hop-tien-do-nhiem-vu.md).
- Dùng nhánh `fix/doi-ten-buoc-giu-tien-do` cho Backend và Frontend.

## Code và logic thay đổi

- [NhiemVuService.cs](../../backend/StudyPlanner.Application/HocTap/NhiemVu/NhiemVuService.cs) đặt tên mới trong `ChuanBiChiaNho` khi tạo bước giữ tiến độ nhập tay. `Study_Planner_FE/src/hoc-tap/api.ts` dùng cùng tên trong hộp xác nhận. Luồng vẫn giữ phần trăm và ước lượng trên bước mới, sau đó tổng hợp tiến độ cha như trước.
- Cập nhật tên trong [IntegrationChecks.cs](../../backend/tests/StudyPlanner.CoreChecks/IntegrationChecks.cs), các kiểm thử và fixture tiến độ Frontend, và [đặc tả hiện hành](../modules/03-nhiem-vu-va-tien-do.md). Tên là dữ liệu đã lưu, nên các bước được tạo trước khi đổi tiếp tục hiển thị tên cũ.

## Kiểm chứng

- Đã đối chiếu diff Backend và Frontend. `git diff --check` đạt. Build Release `StudyPlanner.CoreChecks` đạt 0 cảnh báo, 0 lỗi. `node --test tests/task-progress.test.mjs` đạt 6/6.
- Không chạy kiểm thử PostgreSQL hoặc trình duyệt trong lượt chỉ đổi tên. Không cập nhật dữ liệu đã lưu.

## Việc tiếp theo

- Khởi động lại Backend và tải lại Frontend để dùng tên mới. Bước cũ có thể đổi tên trong màn Nhiệm vụ nếu cần.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
