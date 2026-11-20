# 2026-11-17 — Căn giữa nút Bắt đầu và gọn mặt đồng hồ

## Yêu cầu và mục đích

- Căn nút **Bắt đầu** với tâm vòng đồng hồ và bỏ câu trang trí dưới nút, ở góc dưới màn hình. Bản ghi liên quan [Bảng chọn nổi trên Đồng hồ](2026-11-17-bang-chon-noi-dong-ho.md).

## Code và logic thay đổi

- FE `src/focus/focus.css`. Ở màn desktop thấp, nhóm điều khiển chuyển sang flex ngang và nút hẹp hơn chiều rộng tối thiểu của nhóm. Căn giữa trục ngang của nhóm để nút nằm cùng trục với vòng tròn cả ở bố cục thường và toàn màn hình.
- FE `src/focus/FocusPage.tsx`. Bỏ dòng trang trí ở đầu trang, phần chữ khi đồng hồ chưa chạy và câu ở chân trang. Nút **Bắt đầu** dùng nhãn chữ. Nút **Toàn màn hình** vẫn nằm ở góc phải của cột đồng hồ.
- Mặt đồng hồ đặt số giữa vòng và dành vùng riêng cho nhãn nghỉ hoặc tạm dừng và thời gian tập trung của phiên đếm ngược. Chấm tiến độ chỉ xuất hiện khi tiến độ lớn hơn 0. Luồng bắt đầu và tiếp tục phiên giữ nguyên.

## Kiểm chứng

- Đọc lại diff và kiểm tra khoảng trắng. Chưa chạy build hoặc trình duyệt theo yêu cầu trước đó của người dùng.

## Việc tiếp theo

- Tải lại trang và kiểm tra nút ở bố cục thường và toàn màn hình.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng phạm vi kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan.
