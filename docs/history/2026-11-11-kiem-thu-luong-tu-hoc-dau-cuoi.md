# Kiểm thử luồng tự học từ tạo việc đến hoàn thành

## Chuẩn bị dữ liệu

Dùng tài khoản sinh viên test. Chọn một học kỳ đang hoạt động bao trùm ngày học. Có thể dùng môn **Nhập môn cơ sở dữ liệu** hoặc một môn có sẵn trong học kỳ. Chọn ngày và giờ theo múi giờ tài khoản. Để đối chiếu đúng số liệu dưới đây, dùng một ngày chỉ có khung giờ rảnh, lịch lớp và nhiệm vụ của kịch bản, không có phiên tự học đã đặt trong khung giờ đó. Kết thúc phiên đang bấm giờ của tài khoản trước khi chạy kịch bản.

Ví dụ dùng **thứ Năm, 12/11/2026**, giờ Việt Nam. Nhập và áp dụng gợi ý trước **18:30**. Nếu ngày này đã qua, đổi đồng thời ngày, thứ, giờ học lớp, giờ rảnh, hạn và ngày bắt đầu gợi ý. Để thử liền trong ngày, có thể chọn giờ bắt đầu tự học cách thời điểm chuẩn bị 10–15 phút, đặt lịch lớp kết thúc đúng giờ đó và dành đủ 25 phút rảnh sau buổi lớp.

| Mục | Dữ liệu mẫu |
| --- | --- |
| Tên nhiệm vụ | Luyện SQL — lọc và sắp xếp điểm |
| Học kỳ và môn | Học kỳ đang hoạt động · Nhập môn cơ sở dữ liệu |
| Loại và mức ưu tiên | Bài tập · Trung bình |
| Hạn hoàn thành | 12/11/2026 lúc 20:00 |
| Ước lượng | 25 phút |
| Lịch lên lớp | Thứ Năm 18:00–18:30 · phòng A101 · áp dụng từ 12/11 đến 12/11 |
| Giờ rảnh | Thứ Năm 18:00–20:00 · áp dụng từ 12/11 đến 12/11 · bật Sử dụng khung giờ này |
| Khoảng gợi ý | Ngày bắt đầu 12/11/2026 · 1 ngày · chọn đúng nhiệm vụ trên |

Tạo một nhiệm vụ đơn lẻ để dùng được biểu mẫu nhập phần trăm ở cuối bài kiểm tra. Để trống phần giờ diễn ra sự kiện. Dán nội dung sau vào nhiệm vụ.

> Dùng bảng SinhVien_Test. Lọc sinh viên có điểm từ 8.0 trở lên. Sắp xếp toàn bộ danh sách theo điểm giảm dần. Đếm số sinh viên có điểm từ 5.0 trở lên. Đối chiếu cả ba kết quả trước khi đánh dấu bài đã xong.

Nếu muốn thử nút **Mở tài liệu**, dán một liên kết HTTP(S) của tài liệu môn học vào một dòng riêng trong nội dung. Dùng bảng dưới đây trong công cụ SQL hoặc môi trường bài tập riêng.

```sql
CREATE TABLE SinhVien_Test (
    MaSV INT PRIMARY KEY,
    HoTen VARCHAR(60),
    Diem DECIMAL(3,1)
);

INSERT INTO SinhVien_Test (MaSV, HoTen, Diem)
VALUES (1, 'An', 8.5), (2, 'Binh', 6.0), (3, 'Chi', 9.0), (4, 'Dung', 4.5);
```

Kết quả đúng lần lượt là tập **An và Chi**, thứ tự **Chi, An, Binh, Dung**, và số lượng **3**. Sinh viên luyện `WHERE`, `ORDER BY` và `COUNT(*)` trong công cụ SQL. Website giúp biết việc cần làm, chọn giờ, bấm giờ và ghi tiến độ sau khi đối chiếu bài làm.

## Các bước kiểm thử theo thứ tự

| Bước | Thao tác | Kết quả cần thấy |
| --- | --- | --- |
| 1. Tạo việc | Mở **Nhiệm vụ → Tất cả nhiệm vụ → Tạo nhiệm vụ**. Nhập dữ liệu mẫu và lưu. | Có nhiệm vụ đúng môn, hạn 20:00, ước lượng 25 phút, tiến độ 0%. |
| 2. Khai báo giờ bận | Vào **Lịch học theo môn**, chọn học kỳ và môn, bấm **Thêm buổi học**. Bỏ chọn **Áp dụng suốt học kỳ** để nhập ngày 12/11 ở cả hai đầu. | Có buổi thứ Năm 18:00–18:30, phòng A101. |
| 3. Khai báo giờ có thể học | Vào **Giờ rảnh của tôi → Thêm giờ rảnh**, nhập khung trong bảng. | Khung 18:00–20:00 hiển thị **Đang dùng**. |
| 4. Đối chiếu lịch | Vào **Thời khóa biểu**, chọn ngày 12/11 ở chế độ **Ngày**. Bật các nhóm **Học trên lớp**, **Giờ rảnh**, **Hạn nộp**. | Thấy lịch lớp 18:00–18:30 trong khung rảnh và mốc hạn 20:00 ở đầu ngày. |
| 5. Xem trước | Vào **Gợi ý lịch học**, nhập ngày và số ngày, bấm **Chọn nhiệm vụ**, đánh dấu nhiệm vụ SQL, rồi bấm **Tạo gợi ý lịch học**. | Có 1 phiên **18:30–18:55**. Cần xếp 25 phút, Đã đặt 0 phút, Đề xuất 25 phút, Chưa xếp 0 phút. Đây là bản xem trước. |
| 6. Lưu kế hoạch | Giữ phiên được chọn, bấm **Áp dụng 1 phiên đã chọn → Áp dụng lịch** trước giờ bắt đầu và trước hạn của gợi ý. | Web báo **Đã lưu 1 phiên tự học** và mở **Phiên tự học**. Phiên ở trạng thái **Đã lên lịch**. Nút **Xem trên thời khóa biểu** mở tuần có phiên. |
| 7. Bắt đầu học | Mở lại **Phiên tự học**, bấm **Mở phiên** trong khoảng 18:30 đến trước 18:55. Bấm **Bắt đầu học**, rồi làm ba truy vấn trong công cụ SQL. | Trạng thái **Đang học**, đồng hồ đếm lên. Có thể tải lại và mở lại phiên để đối chiếu đồng hồ vẫn chạy. |
| 8. Kết thúc phiên | Sau khi dừng học, bấm **Kết thúc phiên**. | Phiên sang **Đã hoàn thành** và hiện thời gian thực học đã lưu. Trong dữ liệu test, có thể dừng sau khi đồng hồ đã chạy đủ để quan sát. |
| 9. Chốt nhiệm vụ | Bấm **Cập nhật tiến độ nhiệm vụ**. Khi cả ba truy vấn cho kết quả đúng, nhập **100%** và lưu. Mở **Nhiệm vụ → Đã hoàn thành**, chọn ngày đã lưu tiến độ và tìm nhiệm vụ SQL. | Nhiệm vụ xuất hiện ở **Đã hoàn thành** với 100%. Mở lại phiên để kiểm tra thời gian thực học vẫn còn. |

Nếu đang ở Thời khóa biểu và menu học tập đang thu gọn, bấm **Công cụ học tập** để mở lại các mục lập kế hoạch. Sau khi bấm **Xem trên thời khóa biểu** ở bước 6, có thể dùng cách này để trở lại Phiên tự học.

## Điểm kết thúc và ca bổ sung

- **Đã áp dụng gợi ý** là đã dành chỗ trên lịch. Đến giờ, sinh viên mở phiên để bắt đầu học.
- **Đã kết thúc phiên** là đã lưu kết quả buổi học và số giây đồng hồ ghi được. Nếu bài còn dở, lưu phần trăm thực tế và tạo gợi ý cho phần còn lại. Nếu bài đã xong, lưu 100%.
- **Luồng đạt** khi kiểm tra lại thấy cả phiên có trạng thái hoàn thành cùng thời gian thực học và nhiệm vụ nằm trong **Đã hoàn thành**. Lưu tiến độ xong, mở danh sách hoàn thành để đối chiếu ngày và trạng thái.

| Ca bổ sung | Kết quả cần thấy |
| --- | --- |
| Sau bước 6, thử **Dời phiên** sang 18:05–18:30 khi chưa đến 18:05 | Báo trùng lịch lớp. Phiên đã lưu giữ giờ cũ. |
| Tạo gợi ý cho nhiệm vụ test khác chưa có ước lượng | Nhiệm vụ hiện trong **Việc chưa xếp được** với lý do chưa nhập thời lượng. |
| Mở phiên trước giờ bắt đầu | Có thể xem nhiệm vụ, dời hoặc hủy phiên. Nút **Bắt đầu học** xuất hiện khi đến giờ. |
| Để một phiên test khác qua giờ mà chưa bấm bắt đầu | Chọn **Xác nhận đã học xong** hoặc **Xác nhận bỏ lỡ** theo thực tế. Phiên xác nhận thủ công hiển thị trạng thái cùng thông tin thời lượng bấm giờ chưa được ghi. |

## Kiểm chứng kịch bản

- Đã đối chiếu tên nút và đường chuyển màn với `LapLichView.tsx`, `LichView.tsx` và `DoiTienDo` trong FE. Đã đối chiếu giờ đề xuất và điều kiện bắt đầu/kết thúc với `LapLichService`, `QuyTacLapLich` và `PhienTuHocService` trong BE.
- Kiểm tra mẫu SQL bằng cơ sở dữ liệu trong bộ nhớ. Cả ba kết quả, thứ của ngày mẫu và độ dài 25 phút đều đúng. Các kết quả giao diện trong bảng là tiêu chí cần kiểm khi chạy với tài khoản test.
