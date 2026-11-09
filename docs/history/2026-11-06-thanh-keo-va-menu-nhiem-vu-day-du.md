# 2026-11-06 — Thanh kéo chia chi tiết và menu nhiệm vụ đầy đủ

## Yêu cầu và mục đích

- Người dùng yêu cầu thanh kéo giữa danh sách và chi tiết để chỉnh độ rộng. Trong cùng lượt, người dùng chỉ ra các mục của frontend-preview còn thiếu trên menu dọc, gồm các danh sách nhiệm vụ và nhóm Lập kế hoạch.
- Đã đọc lịch sử, [bản ghi CSS](2026-11-06-dong-bo-css-voi-frontend-preview.md), HocTapPage, CSS, kiểm thử UI hiện có và NAV của `frontend-preview/app.js`.

## Code và logic thay đổi

- Trong repository `C:/Users/ADMIN/Desktop/Study_Planner_FE`, thêm `src/hoc-tap/KhungChiaChiTiet.tsx`. Component dựng cột danh sách, thanh chia 8px và panel chi tiết. Pointer capture giữ thao tác kéo khi con trỏ đi khỏi thanh. Kết thúc, hủy hoặc mất pointer capture đều dọn trạng thái kéo.
- Kéo trái tăng độ rộng chi tiết, kéo phải giảm. Chi tiết tối thiểu 260px, tối đa 640px, đồng thời dành ít nhất 360px cho danh sách. ResizeObserver cập nhật giới hạn khi vùng làm việc đổi kích thước. Độ rộng mong muốn lưu trong localStorage, lỗi storage dùng lựa chọn trong phiên.
- Thanh có role separator, tên và giá trị độ rộng cho công cụ hỗ trợ tiếp cận. Tab đến thanh rồi dùng trái/phải để chỉnh từng 16px, Shift tăng bước 40px, Home/End đến giới hạn. Nhấp đúp trở về 310px. Khi layout chuyển về xếp dọc ở màn hình nhỏ, thanh kéo được ẩn theo cùng breakpoint 1050px.
- `HocTapPage.tsx` dùng component mới cho vùng danh sách/chi tiết, bổ sung từng mục Hôm nay, 7 ngày tới, Tất cả nhiệm vụ, Chưa phân loại, Quá hạn, Không có hạn và Đã hoàn thành trên menu. Mỗi mục cập nhật view trên URL, trả về trang đầu và mở danh sách tương ứng. Bỏ lựa chọn trạng thái cũ khi đổi danh sách để tránh điều kiện mâu thuẫn.
- Nhóm Lập kế hoạch có Thời khóa biểu, Lịch học theo môn, Giờ rảnh của tôi, Gợi ý lịch học và Phiên tự học. Bảng trạng thái vẫn có mục riêng. Các mục mới gọi bộ lọc/API hiện có, cùng trạng thái với ô Hiển thị.
- `hoc-tap.css` thêm kiểu thanh kéo, trạng thái hover/focus/đang kéo, ba cột và nhãn nhóm menu. `tests/browser-hoc-tap.mjs` kiểm thao tác chuột/bàn phím và các mục menu. Kiểm tra render nhãn lịch được cập nhật trong `tests/planner.test.mjs`.
- Các file ngoài phạm vi module học tập và kiểm thử được kiểm tra dấu nội dung với bản đầu vào, kết quả trùng khớp.

## Kiểm chứng

- Build TypeScript/Vite thành công. Bộ kiểm thử FE đạt 100/100 bài.
- Chuỗi browser cuối đạt ở 1280×720, có 253 request giả lập. Thử kéo bằng mouse events thật của trình duyệt cho thấy chi tiết tăng 100px, sau F5 giữ độ rộng. Phím trái/phải, Home/End, giới hạn danh sách, chuyển màn nhỏ và nhấp đúp về mặc định đều đạt.
- Đã bấm từng mục danh sách nhiệm vụ, xác nhận view URL và dấu mục đang chọn, rồi chạy tiếp các thao tác nhiệm vụ/lịch/phiên/gợi ý hiện có. Lỗi chờ dữ liệu giả lập ở một lượt được chạy lại và đối chiếu, kết quả đạt ở lượt cuối.
- Đã mở ảnh desktop để kiểm vị trí thanh kéo và các mục menu. Ảnh nằm trong `backend/obj/fe-detail-splitter`, được Git bỏ qua.
- Đã đọc component, JSX, CSS và kiểm tra git diff --check của FE.

## Việc tiếp theo

- Người dùng tải lại FE. Thanh chia xuất hiện khi đang mở chi tiết nhiệm vụ trên bố cục hai cột. Độ rộng lưu là tùy chọn giao diện trong trình duyệt.
- Mã đang là thay đổi local trên nhánh tích hợp FE hiện có, chờ người dùng test và quyết định commit/push.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu yêu cầu, code và kiểm thử thực tế.
- [ ] Người dùng đã đọc bản ghi và code liên quan. Chỉ người dùng xác nhận mục này.
