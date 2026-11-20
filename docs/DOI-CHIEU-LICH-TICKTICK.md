# Đối chiếu lịch TickTick với Study Planner — 18/11/2026

Đã đọc nội dung 9 bài chính thức dưới đây từ dữ liệu công khai trong HTML của trang trợ giúp TickTick (`__NEXT_DATA__`). Công cụ web không trích được văn bản, nhưng HTML có nội dung bài. Các đánh giá áp dụng là quyết định cho đồ án, không phải khuyến nghị nguyên văn của TickTick.

## Ca tick hoàn thành và hủy lịch

- Tick nhiệm vụ lá không có checklist là yêu cầu hoàn thành nhiệm vụ. Nếu còn phiên dự kiến liên quan, backend xem trước những phiên sẽ không còn hợp lệ và yêu cầu xác nhận. Hai phiên trong thông báo là số phiên bị ảnh hưởng, không phải số lần đã học.
- Chọn **Giữ nguyên**: chỉ đóng xác nhận; không gửi yêu cầu hoàn thành, không hủy phiên, ô tick trở lại trạng thái cũ.
- Chọn **Hoàn thành và hủy phiên**: hoàn thành nhiệm vụ và hủy các phiên dự kiến bị ảnh hưởng sau xác nhận. Ô đã tick/màu nhạt như ảnh 4 là đúng khi chọn nút này. Không xóa lịch sử thực học và không đổi số phút đã học.
- Đổi nhãn cũ **Hủy phiên và tiếp tục** để tránh nhầm với bỏ thao tác. Hộp cũng nêu tên nhiệm vụ.
- Khi đóng xác nhận, trả focus bằng `preventScroll: true`, giữ giờ đang xem thay vì kéo lịch về đầu. Đã kiểm tra vị trí cuộn trước/sau cả hủy lẫn đồng ý ở hai kiểu vùng cuộn desktop và màn hẹp.

## Những phần phù hợp

| Tài liệu chính thức | Đồ án hiện có | Quyết định áp dụng |
| --- | --- | --- |
| [Tổng quan lịch](https://help.ticktick.com/articles/7055782166172532736) | Nguồn lịch lớp, sự kiện, hạn nhiệm vụ, tự học và giờ rảnh | Dùng trang tổng quan Lập kế hoạch làm điểm vào 5 phần; giữ phân biệt từng loại dữ liệu. |
| [Xem theo tuần](https://help.ticktick.com/articles/7055782149730861056) | Ngày, 3 ngày, tuần, 2 tuần; dải hạn và lưới giờ; nút Hôm nay | Tận dụng các chế độ sẵn có. Giữ vị trí cuộn khi xử lý xác nhận. Không biến hạn nộp thành thời lượng học. |
| [Xem theo tháng](https://help.ticktick.com/articles/7055782128335716352) | Lịch tháng, chọn ngày để xem chi tiết, lọc môn/học kỳ | Không viết lại lịch tháng. Thêm ẩn/hiện mục hoàn thành và nhớ cách xem; phù hợp khi rà lịch học kỳ. |
| [Tổng quan năm](https://help.ticktick.com/articles/7399726920725692416) | Trang Tiến độ theo học kỳ/môn và thống kê thực học riêng | P2: có thể mở rộng thống kê năm/heatmap tại Tiến độ. Chưa thêm một lịch năm vào Lập kế hoạch. Cần tách số nhiệm vụ hoàn thành và số phút thực học, không gọi số ô tích là chất lượng học tập. |
| [Danh sách theo ngày](https://help.ticktick.com/articles/7055782110086299648) | Lịch danh sách 7 ngày và phiên tự học nhóm theo ngày | Dùng lại; bổ sung lọc trạng thái phiên, thời lượng và nút bắt đầu. Không tạo thêm màn danh sách trùng chức năng. |
| [Agenda](https://help.ticktick.com/articles/7213444784319365120) | Danh sách lịch theo ngày trong khoảng chọn | Tiếp tục dùng phạm vi có giới hạn, có trước/sau. Chưa làm cuộn vô hạn hoặc một danh sách mới chỉ khác tên. |
| [Tùy chọn lịch](https://help.ticktick.com/articles/7055782085826445312) | Màu theo nguồn, bật/tắt lớp lịch, thu gọn dải hạn, bộ lọc môn/học kỳ | Đã thêm Hiện mục đã hoàn thành; nhớ chế độ xem, lớp lịch, thu gọn hạn và lựa chọn này trên trình duyệt theo tài khoản. Có dấu hiệu khi đang ẩn mục hoàn thành. Không sửa dữ liệu khi đổi hiển thị. |
| [Hiển thị nhiệm vụ cùng lịch](https://help.ticktick.com/articles/7358389904469917696) | Chọn nhiệm vụ trong gợi ý và mở chi tiết từ lịch | P2: cân nhắc một chế độ xem song song tùy chọn. Không ghép hai sidebar vì người dùng yêu cầu tách Nhiệm vụ và Lập kế hoạch. |
| [FAQ lịch](https://help.ticktick.com/articles/7063851189372190720) | Mở ngày, chi tiết, thêm lịch, dời phiên bằng biểu mẫu kiểm tra | Giữ cách sửa có kiểm tra. Nếu làm kéo thả sau này, chỉ dời phiên qua validator/preview/xác nhận; không tự đổi deadline hoặc tự hủy các phiên khác. |

## Không sao chép các chi tiết dễ sai nghiệp vụ

- Không gán tiến độ cố định như **20% = 1/5 phiên**. Tiến độ hiện tính từ checklist, phần trăm thực hiện hoặc nhiệm vụ con; đồng hồ lưu thời gian thực tế riêng.
- Không tạo phiên tự học chỉ bằng một tiêu đề trong biểu mẫu sự kiện chung: phiên phải gắn nhiệm vụ hợp lệ, còn khối lượng và phù hợp giờ rảnh/hạn/nghỉ/hạn mức ngày.
- Không tạo thêm đồng hồ trong chi tiết phiên. Thẻ chọn chế độ mở trang Tập trung hiện có, tránh hai nguồn thời gian.
- Không chép các số ví dụ, ngày giờ hoặc nhãn “đã có giờ rảnh” thành dữ liệu thật. Không cộng hai mẫu giờ rảnh chồng nhau thành hai phần thời gian độc lập; mỗi mẫu giữ thời hạn, backend hợp nhất khi tính giờ khả dụng.
- Không thêm thói quen, lịch âm, nhiều múi giờ, đăng ký lịch ngoài hoặc thay đổi giao diện thành nhiều phong cách trong lượt này. Chúng không giải quyết trực tiếp luồng học tập đang được yêu cầu.

## Mức ưu tiên còn lại

- P1: nghiệm thu bằng dữ liệu Development, nhất là nhiệm vụ đang học, nhiệm vụ có con/checklist và thay đổi đồng thời. Những quy tắc backend này được giữ nguyên.
- P2: nếu sinh viên cần, mở rộng tổng kết tháng/năm trong trang Tiến độ; xem xét kéo thả dời phiên và xem nhiệm vụ cạnh lịch sau khi chốt cách xác nhận ảnh hưởng. Đây là đề xuất, chưa phải chức năng đã triển khai.

Kết quả code và kiểm thử: [bản ghi triển khai](history/2026-11-18-bo-tri-lap-ke-hoach-va-xac-nhan-lich.md).
