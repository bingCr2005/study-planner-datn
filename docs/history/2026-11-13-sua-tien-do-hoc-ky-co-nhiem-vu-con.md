# 2026-11-13 — Sửa Tiến độ khi học kỳ có nhiệm vụ con đã hoàn thành

## Yêu cầu và mục đích

- Người dùng báo trang Tiến độ học tập của Học kỳ 5 hiện “Dữ liệu tiến độ chưa hợp lệ. Vui lòng thử lại.”
- Đã đọc mục lục lịch sử, code API Nhiệm vụ hiện tại, bộ đọc và model thống kê FE, các kiểm thử và tài liệu FE `docs/tien-do-hoc-tap.md`. Liên quan [bản ghi gộp main](2026-11-12-gop-main-vao-quy-tac-uu-tien.md).
- Hai repo đều dùng nhánh sửa riêng `fix/tien-do-hoc-ky-co-nhiem-vu-con`. Frontend ở `D:/study-planner-frontend`; backend không đổi logic hoặc dữ liệu.

## Code và logic thay đổi

- BE hiện tại `backend/StudyPlanner.Application/HocTap/NhiemVu/NhiemVuService.cs`, `DanhSachAsync`: nhóm Completed không lọc chỉ nhiệm vụ gốc; nhóm chưa hoàn thành có thể trả cả con khi cha đã hoàn thành. Vì vậy `chiLa=false&cheDo=all` không bảo đảm mọi dòng có `MaNhiemVuCha = null`.
- FE `src/statistics/statisticsApi.ts` trước đó từ chối bất kỳ dòng nào có cha, khiến cả lần tải thống kê thất bại khi API trả nhiệm vụ con hợp lệ. Đây là lỗi không khớp hợp đồng sau thay đổi bộ lọc Nhiệm vụ từ main; không phải bằng chứng dữ liệu của người dùng bị hỏng.
- Sau sửa: xác minh tất cả dòng như trước về ID, trạng thái, học kỳ, ngày giờ, phiên bản và mã trùng. Đọc đủ trang bằng số dòng thô và tổng API; chỉ sau khi có snapshot đầy đủ mới lọc nhiệm vụ chính. Trang toàn nhiệm vụ con không làm dừng phân trang sớm hoặc báo lỗi giả, nhiệm vụ con không bị cộng vào tổng.
- Luồng: học kỳ chọn → GET ba nhóm trạng thái với JWT → kiểm tra/phân trang đầy đủ → đối chiếu phiên bản giữa nhóm → giữ `maNhiemVuCha === null` → tính tổng và tỷ lệ. Dữ liệu sai định dạng vẫn báo lỗi; snapshot thay đổi vẫn chỉ thử lại một lần.
- FE `tests/statistics.test.mjs` và `tests/browser-statistics.mjs`: fixture cũ chỉ trả nhiệm vụ chính nên bỏ sót lỗi; cập nhật quy tắc theo BE. Thêm trường hợp trang đầu toàn nhiệm vụ con, nhiệm vụ chính ở trang sau, toàn con, con chưa xong dưới cha hoàn thành và con có timestamp sai. Clone DTO fixture trước khi cố tình làm dữ liệu sai để tránh các kiểm thử ảnh hưởng lẫn nhau.
- FE `docs/tien-do-hoc-tap.md`: sửa mô tả cũ rằng API luôn trả nhiệm vụ chính. Liên kết xem Completed có thể hiện cả nhiệm vụ con, nên số dòng ở danh sách đó có thể lớn hơn số nhiệm vụ chính trong Tiến độ; không đổi module Nhiệm vụ để ép số liệu bằng nhau.

## Kiểm chứng

- Đã đọc lại diff bộ đọc, fixture/test và tài liệu; `git diff --check` hai repo đạt.
- FE `node --test tests/statistics.test.mjs`: 9/9 kiểm thử đạt sau sửa. Lần chạy đầu lộ việc test dữ liệu sai làm biến đổi fixture dùng chung; đã clone DTO rồi chạy lại đạt.
- FE `npm.cmd run build`: TypeScript và Vite build thành công. Oxlint ba file code/test thay đổi đạt, không báo lỗi.
- FE `npm.cmd run test:auth:browser -- --statistics`: đạt với API fixture mô phỏng đúng điều kiện trả nhiệm vụ con của BE. Kiểm tra tổng/tỷ lệ, nhiệm vụ con không làm tăng số liệu, đổi học kỳ/F5, bộ lọc môn, đọc đủ trang, cache/lỗi/retry và liên kết sang trang Nhiệm vụ. Lần chạy đầu chỉ ra kỳ vọng cũ rằng danh sách Completed luôn chỉ chứa nhiệm vụ chính; đã sửa fixture, kỳ vọng và tài liệu theo hợp đồng BE, không thay đổi module Nhiệm vụ.
- Kiểm thử trình duyệt không báo lỗi JavaScript và chỉ gửi GET cho API nhiệm vụ; không ghi dữ liệu. Không chạy lại kiểm thử backend vì không sửa BE.
- Không đăng nhập phiên người dùng hoặc truy vấn database thật ở lượt này; không khẳng định số lượng nhiệm vụ hiện tại của Học kỳ 5 từ fixture.

## Việc tiếp theo

- Chưa commit/push. Không cần sửa database hoặc khởi động lại backend cho bản sửa FE này.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
