# 2026-11-17 — Đối chiếu test case ưu tiên nhiệm vụ

## Yêu cầu và mục đích

- Người dùng muốn rà lại các test case từ [đoạn chat chia sẻ](https://chatgpt.com/share/6afc18f6-39ac-83ec-8897-1caa0259f6b9) và áp dụng phần phù hợp cho đồ án. Đọc lại [bản ghi sáu nhóm](2026-11-17-xep-goi-y-theo-nhom-do-gap.md), [bản ghi ghim](2026-11-17-ghim-nhiem-vu.md), mã xếp hạng Hôm nay, bộ lọc nhiệm vụ và bộ xếp phiên.
- Phương án sáu nhóm trong lượt đầu khớp quy tắc đã chốt. Lượt cuối đề xuất bảy nhóm, hạn chính xác trước trong 24 giờ và giờ tạo để phân xử. Ba điểm này có kết quả khác quy tắc hiện hành.

## Code và logic thay đổi

- Thêm [kịch bản kiểm thử](../KICH-BAN-KIEM-THU-UU-TIEN-NHIEM-VU.md) và dẫn từ [module 07](../modules/07-goi-y-cong-viec.md). Kịch bản có tiền điều kiện về đồng hồ, phạm vi nhiệm vụ, cấu hình Admin, giờ rảnh và phiên đã đặt. Ghi kết quả theo từng màn và dùng khoảng ngày cho các nhóm sau 24 giờ.
- Trong `Study_Planner_FE/tests/today.test.mjs`, đưa các ca quan trọng vào một test có mã khớp tài liệu. Bao gồm việc gấp hơn dù ưu tiên Thấp, việc Cao thắng trong cùng nhóm K2/K4/K1 và nhiệm vụ đã ghim ở K4 vẫn đứng sau K1 trên Hôm nay. Giữ ma trận tự động 36 cặp nhóm và kiểm tra các mốc biên.
- Quy tắc ứng dụng trong `todayModel.ts` đã khớp phương án được chọn. Lượt này bổ sung tài liệu và test trên nhánh `feat/xep-goi-y-theo-nhom-do-gap`.

## Kiểm chứng

- Đã đối chiếu cách tính nhóm, thứ tự cấu hình, ghim trước phân trang, kế thừa hạn và cách trừ số phút đã đặt.
- `node --test tests/today.test.mjs tests/priority-rules.test.mjs` đạt 17/17. Đã kiểm tra 21 mã ca duy nhất và kết quả đủ 36 ô ma trận trong tài liệu. `git diff --check` đạt ở hai repository.
- Chưa chạy thủ công toàn bộ 21 ca trên môi trường Development. Lượt này dùng test model và cấu hình admin, vì mã nghiệp vụ đã được kiểm tra trình duyệt và HTTP ở bản ghi triển khai sáu nhóm.

## Việc tiếp theo

- Khi dùng tài liệu để nghiệm thu thủ công, xác nhận cấu hình trên môi trường thử và dùng mốc thời gian tương đối. Dùng test tự động cho các ca đúng mốc.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
