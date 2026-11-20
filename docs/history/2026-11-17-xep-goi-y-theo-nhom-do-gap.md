# 2026-11-17 — Xếp gợi ý Hôm nay theo nhóm độ gấp

## Yêu cầu và mục đích

- Người dùng chốt dùng sáu nhóm trong đoạn chat tham khảo làm tiêu chí đầu tiên cho **Gợi ý nên làm trước**. Chọn quy tắc cùng nhóm của bản đề xuất đi kèm bảng sáu nhóm, xét mức quan trọng rồi đến hạn chính xác. Giữ trạng thái đang thực hiện và mã nhiệm vụ làm tiêu chí phân xử cuối ở cấu hình mặc định.
- [Bản ghi quy tắc ưu tiên](2026-11-12-quan-ly-quy-tac-uu-tien.md) mô tả hành vi trước đây. Đối chiếu `todayModel.ts`, màn Hôm nay, cấu hình quản trị và API đọc quy tắc mặc định. Nhánh `feat/xep-goi-y-theo-nhom-do-gap` cho cả hai repository.

## Code và logic thay đổi

- Trước đây `todayModel.ts` dùng 7 nhãn độ gấp, nhưng khi sắp xếp chỉ tách nhóm quá hạn, có hạn và chưa có hạn rồi dùng tiêu chí của admin. Giữa việc có hạn, mặc định so ngày giờ hết hạn trước.
- Nay `urgency()` tính một nhóm từ hạn hiệu lực của nhiệm vụ lá và thời điểm hiện tại. Thứ tự là quá hạn → 0–24 giờ → trên 24–72 giờ → trên 72–168 giờ → trên 168 giờ → chưa có hạn. `recommend()` luôn so nhóm trước, rồi mới dùng thứ tự trong cấu hình admin. Nhiệm vụ đã hoàn thành và nhiệm vụ cha vẫn được loại khỏi gợi ý.
- `Dashboard.tsx` và `today.css` thêm tiêu đề từng nhóm có dữ liệu. Giữ thứ hạng xuyên các nhóm và nút mở rộng danh sách năm gợi ý đầu. `AdminPriorityRulesPage.tsx` ghi rõ phạm vi của tiêu chí do admin sắp xếp.
- Theo yêu cầu tiếp theo, tiêu đề K2/K3/K4 dùng khoảng ngày và K1 dùng giờ. `urgency()` biểu diễn các mốc bằng số khoảng 24 giờ tương ứng. Cập nhật bảng test trong module 07 và kỳ vọng tiêu đề của Chrome.
- `src/priority/rules.ts` và [CauHinhUuTien.cs](../../backend/StudyPlanner.Application/UuTien/CauHinhUuTien.cs) đặt mặc định `Importance → Deadline → InProgress`. Cấu hình riêng đã lưu vẫn giữ thứ tự tiêu chí của nó trong từng nhóm. Vì vậy tất cả bản cấu hình đều áp dụng sáu nhóm, còn kết quả phân xử nội nhóm phụ thuộc quy tắc đang dùng. Giữ schema 1 và dữ liệu cũ. Cập nhật [module 07](../modules/07-goi-y-cong-viec.md) theo code.

## Kiểm chứng

- Test model kiểm tra các mốc 0/24/72/168 giờ, ma trận 36 tổ hợp nhóm, hai việc Cao/Thấp cùng nhóm, thời điểm chuyển nhóm và cấu hình riêng chỉ phân xử trong nhóm. Fixture trình duyệt kiểm tra tiêu đề nhóm, thứ hạng và áp dụng quy tắc admin. Có kiểm tra thứ tự mặc định trả từ API.
- `npm run build` đạt TypeScript và Vite. Vite vẫn báo chunk JS chính trên 500 kB. `npm test` đạt 177/177. Chrome headless `--today` và `--priority-rules` đều đạt với API giả.
- Backend build Release đạt, 0 cảnh báo và 0 lỗi. `StudyPlanner.PriorityChecks` đạt kiểm tra schema cấu hình và HTTP trên PostgreSQL tạm, gồm quy tắc mặc định mới, đọc dữ liệu đã lưu, quyền, áp dụng và xử lý xung đột. Cụm kiểm thử đã dừng. Chưa chạy bằng tài khoản hoặc database Development của người dùng.
- Đã đọc diff nguồn và test liên quan. `git diff --check` đạt ở cả hai repository.
- Sau khi đổi cách ghi sang ngày, chạy lại `tests/today.test.mjs` đạt 11/11 và Chrome headless `--today` đạt. Các test biên xác nhận đúng 24 giờ vẫn thuộc K1, đúng 3 ngày thuộc K2 và đúng 7 ngày thuộc K3.

## Việc tiếp theo

- Khởi động lại backend và tải frontend mới để kiểm thử trên bản local. Nếu admin đã áp dụng quy tắc riêng, chọn **Dùng mặc định** rồi lưu và áp dụng quy tắc đó để test thứ tự mặc định trong cùng nhóm.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
