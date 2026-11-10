# Kịch bản demo nhiệm vụ, chia nhỏ và thời khóa biểu

Ngày đối chiếu 06/11/2026. Thời lượng gợi ý 12–15 phút cho luồng chính, thêm 3–5 phút nếu trình bày các tình huống mở rộng.

## 1. Phạm vi đã đối chiếu

Đã kiểm tra [commit gộp PR #8](https://github.com/bingCr2005/study-planner-datn/commit/fd670fd6c506f1cf797319e9575c629e2766955f), các service/DTO BE và mã React trong `C:/Users/ADMIN/Desktop/Study_Planner_FE` hiện có.

Commit trên chứa nền BE và giao diện `frontend-preview`. FE React, menu dọc, chọn tên học kỳ/môn và thanh kéo là các thay đổi local được tích hợp sau đó trong repository FE riêng. Kịch bản dưới đây bám tên nút và hành vi của FE React hiện tại. Khi thuyết trình, phân công của bạn là nhiệm vụ, chia nhỏ nhiệm vụ, lịch lớp theo môn, giờ rảnh và lập lịch tự học.

| Nhóm chức năng | Nội dung đã có |
| --- | --- |
| Nhiệm vụ | Thêm nhanh, tạo/sửa/xóa, việc cá nhân hoặc gắn học kỳ/môn, hạn, loại, ưu tiên, ước lượng, thời gian sự kiện |
| Chia nhỏ | Thêm một/nhiều bước cùng lần, chia tiếp nhiều cấp, chuyển vào nhiệm vụ khác, tách riêng, đổi phạm vi từ nhiệm vụ lớn nhất |
| Tiến độ | Cập nhật từng việc chưa chia nhỏ, hoàn thành/mở lại, tổng hợp tiến độ lên nhiệm vụ lớn |
| Tra cứu | Tìm tên/nội dung, lọc học kỳ/môn/loại/ưu tiên/trạng thái, sắp xếp, phân trang, các danh sách theo hạn |
| Lịch sử | Hành động, thời điểm, nguồn thay đổi và dữ liệu trước/sau |
| Lịch lớp | Thêm/sửa/xóa mẫu buổi học theo môn, lặp theo thứ trong phạm vi ngày, xác nhận trùng lớp |
| Giờ rảnh | Thêm/sửa/xóa, bật/tắt mẫu giờ rảnh và xem ảnh hưởng tới các phiên đã đặt |
| Thời khóa biểu | Ngày, tuần, tháng, danh sách, bật/tắt năm lớp dữ liệu, mở mục lịch để xem/sửa |
| Gợi ý lịch | Xem trước theo hạn/ưu tiên/khối lượng/giờ rảnh, lý do và phần chưa xếp được, chọn toàn bộ hoặc một phần để áp dụng |
| Phiên tự học | Đặt tay, dời, hủy, bắt đầu, kết thúc, xác nhận đã học xong hoặc bỏ lỡ theo điều kiện thời gian |
| Tương tác FE | Chi tiết bên phải, thanh kéo đổi độ rộng, sao chép liên kết nhiệm vụ và giữ lựa chọn màn trên URL |

## 2. Câu chuyện xuyên suốt và dữ liệu chuẩn bị

Dùng cùng tài khoản và môn mà bạn trong team vừa tạo. Các ảnh người dùng gửi có **Học kỳ 5 Năm 3** và môn **Nhập môn lập trình**, nên kịch bản sử dụng hai dữ liệu này. Tình huống xuyên suốt là sinh viên nhận bài thực hành lập trình, chia thành các bước, theo dõi tiến độ và sắp thời gian tự học quanh lịch lớp. Không cần tạo lại phần học kỳ/môn của thành viên khác.

Gọi ngày demo là **D**, ngày kế tiếp là **D+1**. Chọn các ngày nằm trong học kỳ của môn. Với ngày bất kỳ sau 06/11/2026, thay ngày theo thời điểm demo để gợi ý không dùng hạn đã qua.

| Dữ liệu | Giá trị dùng khi demo |
| --- | --- |
| Nhiệm vụ lớn | Hoàn thành bài thực hành lập trình |
| Phạm vi | Học kỳ và môn Nhập môn lập trình do bạn trong team vừa tạo |
| Loại và ưu tiên | Bài tập, ưu tiên Cao |
| Hạn nhiệm vụ lớn | D+3 lúc 23 giờ 59 |
| Ba bước nhỏ | Đọc yêu cầu, Thiết kế thuật toán, Viết và kiểm thử chương trình |
| Ước lượng | Mỗi bước 50 phút, nhập ở từng bước |
| Tiến độ dùng minh họa | Đọc yêu cầu 100%, Thiết kế thuật toán 50%, Viết và kiểm thử chương trình 0% |
| Lịch lớp | Thứ tương ứng D+1, 19 giờ–20 giờ, phòng A101 |
| Giờ rảnh | Cùng thứ đó, 19 giờ–22 giờ, bật sử dụng |
| Phạm vi lập lịch | Bắt đầu D+1, xét 7 ngày, chọn nhiệm vụ lớn vừa tạo |
| Việc phụ | Chuẩn bị tài liệu học, để trống học kỳ/môn và hạn |

Với dữ liệu này, tiến độ nhiệm vụ lớn là 50%, tổng phần việc còn lại là 75 phút. Nếu khoảng 20 giờ–22 giờ của D+1 chưa có lịch bận khác, đủ chỗ để xếp một phiên 25 phút và một phiên 50 phút, cách nhau ít nhất 10 phút. Lịch thực tế có thể đổi theo dữ liệu đã tồn tại trong tài khoản.

Trước buổi demo, chạy BE và FE React, dùng tài khoản demo có dữ liệu rõ ràng. Đối chiếu ngày học kỳ, lịch bận và múi giờ tài khoản. Những việc phụ dùng thử lỗi/xóa nên có tên riêng để phân biệt với bài lập trình chính.

## 3. Thứ tự thao tác và lời trình bày

### Bước 1 — Nhận phần trình bày từ bạn trong team, khoảng 30 giây

Mở mục Nhiệm vụ với tài khoản đang đăng nhập. Dùng dữ liệu học kỳ/môn đã tạo ở phần trước. Từ đây thao tác ở menu dọc bên trong Nhiệm vụ, gồm Hôm nay, 7 ngày tới, Tất cả nhiệm vụ và nhóm Lập kế hoạch. Mục Hôm nay/Thời khóa biểu ở thanh bên ngoài thuộc điều hướng chung của ứng dụng; không chuyển sang đó thay cho màn module đang demo nếu chưa đối chiếu tích hợp.

> Tiếp theo, em trình bày phần quản lý nhiệm vụ, chia nhỏ nhiệm vụ và thời khóa biểu. Từ tài khoản, học kỳ và môn học vừa được tạo, sinh viên có thể ghi lại việc cần làm, chia thành các bước nhỏ, theo dõi tiến độ và sắp lịch tự học phù hợp với thời gian của mình.

### Bước 2 — Tạo một nhiệm vụ theo môn, khoảng 1 phút

1. Chọn Thêm nhiệm vụ.
2. Nhập Hoàn thành bài thực hành lập trình, mô tả ngắn yêu cầu bài.
3. Chọn tên học kỳ và môn Nhập môn lập trình.
4. Chọn Bài tập, ưu tiên Cao, hạn D+3 lúc 23 giờ 59, rồi lưu.
5. Mở chi tiết bên phải. Kéo thanh chia một lần để cho thấy có thể điều chỉnh vùng đọc.

> Ở đây em tạo một bài tập thuộc môn Nhập môn lập trình. Sinh viên chọn học kỳ và môn bằng tên từ dữ liệu có sẵn. Em đặt hạn để biết thời điểm cần hoàn thành và đặt mức ưu tiên để hỗ trợ sắp xếp công việc. Thông tin này cũng được dùng khi gợi ý lịch tự học ở phần sau.

Giải thích khi cần thêm rằng nhiệm vụ có thể thuộc một môn, là việc chung của học kỳ hoặc là việc cá nhân. Chọn môn sẽ xác định học kỳ tương ứng.

### Bước 3 — Chia việc lớn thành ba bước, khoảng 1–2 phút

1. Trong chi tiết, chọn Thêm bước nhỏ.
2. Nhập Đọc yêu cầu, ước lượng 50 phút, để trống hạn riêng.
3. Trong ô thêm các bước khác cùng lúc, nhập Thiết kế thuật toán và Viết và kiểm thử chương trình, mỗi tên một dòng.
4. Lưu, mở rộng nhiệm vụ và cho thấy ba bước mới.

Các bước trong cùng lần thêm dùng chung thông tin loại, ưu tiên, hạn và ước lượng đã nhập. Có thể sửa riêng từng bước sau khi tạo.

> Một bài tập lớn thường khó theo dõi nếu chỉ có một dòng. Vì vậy em chia bài lập trình thành ba bước cụ thể. Các bước cùng thuộc học kỳ và môn của nhiệm vụ lớn. Khi không đặt hạn riêng, bước nhỏ sử dụng hạn của nhiệm vụ lớn. Sinh viên có thể chia tiếp nếu một bước vẫn còn quá rộng.

Thử nhập hạn riêng D+4 cho một bước rồi lưu để minh họa bị chặn. Sửa lại hoặc đóng biểu mẫu sau khi giải thích.

> Hạn của bước nhỏ phải cùng lúc hoặc sớm hơn hạn của nhiệm vụ lớn. Quy tắc này giúp kế hoạch chi tiết phục vụ được hạn chung của bài tập.

### Bước 4 — Cập nhật tiến độ và giải thích cách tính, khoảng 1 phút

1. Đánh dấu Đọc yêu cầu đã hoàn thành.
2. Mở Thiết kế thuật toán, chọn Cập nhật tiến độ, nhập 50%.
3. Giữ bước cuối ở 0%, quay về nhiệm vụ lớn.

> Tiến độ được nhập ở từng việc chưa chia nhỏ. Khi ba bước lần lượt là 100%, 50% và 0%, nhiệm vụ lớn được tính thành 50%. Hiện hệ thống lấy trung bình tiến độ của các bước trực tiếp. Thời lượng còn lại lại được tính theo ước lượng của từng việc, nên hai thông tin này có ý nghĩa riêng.

Nếu chỉ hoàn thành bước đầu, nhiệm vụ lớn hiển thị 33% do phần trung bình được lấy về số nguyên. Khi cả ba bước đạt 100%, nhiệm vụ lớn tự hoàn thành. Mở lại một bước sẽ khiến tiến độ lớn được tính lại.

### Bước 5 — Tra cứu và lịch sử, khoảng 1 phút

1. Tìm theo tên thực hành, chọn Mở bộ lọc để lọc theo môn hoặc ưu tiên rồi xóa bộ lọc.
2. Mở 7 ngày tới và Không có hạn để minh họa danh sách theo hạn.
3. Thêm nhanh một việc cá nhân Chuẩn bị tài liệu học, mở Chưa phân loại.
4. Mở lịch sử của bước Thiết kế thuật toán, xem trước/sau thay đổi.

> Các danh sách này dùng cùng dữ liệu nhiệm vụ nhưng chọn theo điều kiện khác nhau. Chưa phân loại hiện gồm các việc chưa gắn học kỳ và môn. Không có hạn dựa trên hạn thực sự đang được sử dụng, kể cả hạn kế thừa. Lịch sử cho biết thao tác đã thay đổi thông tin nào và lúc nào, giúp sinh viên kiểm tra lại quá trình thực hiện.

Lưu ý Tất cả nhiệm vụ mặc định hiển thị các nhiệm vụ lớn đang chưa hoàn thành. Danh sách theo hạn và Đã hoàn thành ưu tiên trả các việc chưa chia tiếp để hạn chế đếm lặp. Bảng trạng thái hiển thị kết quả theo bộ lọc và trang đang xem.

### Bước 6 — Khai báo lịch lớp, khoảng 1 phút

1. Chọn Lịch học theo môn, chọn Mở bộ lọc rồi chọn tên học kỳ và tên môn. Dòng “Chọn tên môn học ở bộ lọc phía trên để quản lý các buổi học của môn” trong ảnh là hướng dẫn chọn phạm vi, chưa phải lỗi tải dữ liệu.
2. Thêm buổi học vào thứ của D+1, từ 19 giờ đến 20 giờ, phòng A101.
3. Giữ lựa chọn Áp dụng suốt học kỳ, hoặc bỏ chọn để nhập ngày bắt đầu và kết thúc nằm trong học kỳ.
4. Mở Thời khóa biểu, xem ngày/tuần chứa D+1.

> Lịch học theo môn mô tả các buổi học cố định của sinh viên. Hệ thống lưu thứ, giờ và phạm vi áp dụng, rồi tạo các lần xuất hiện trên lịch theo tuần. Phần này sử dụng môn đã có và cung cấp các khoảng bận để hệ thống tránh khi xếp tự học.

Trùng lịch lớp được cảnh báo và có thể lưu sau khi người dùng xác nhận. Một mẫu chỉ sửa theo cả quy tắc lặp hiện có, chưa có thao tác sửa riêng một lần xuất hiện.

### Bước 7 — Khai báo giờ rảnh, khoảng 45 giây

Chọn Giờ rảnh của tôi, thêm cùng thứ của D+1, 19 giờ–22 giờ, chọn khoảng ngày chứa D+1 và bật Sử dụng.

> Giờ rảnh là thời gian sinh viên chủ động cho phép hệ thống xếp tự học. Em khai báo 19 giờ đến 22 giờ, nhưng trong đó đã có lịch lớp từ 19 giờ đến 20 giờ. Khi tính lịch, hệ thống lấy giờ rảnh rồi loại các khoảng bận để tìm thời gian có thể sử dụng.

Khoảng không có lịch cũng cần được khai báo là giờ rảnh nếu muốn hệ thống tự xếp học vào đó. Các khung giờ rảnh chồng nhau được hợp nhất khi tính toán.

### Bước 8 — Tạo gợi ý tự học, khoảng 1–2 phút

1. Chọn Gợi ý lịch học ở menu dọc. Tiêu đề trên ảnh hiện ghi Gợi ý lịch tự học; đây là cùng màn lập lịch.
2. Chọn ngày bắt đầu D+1 và 7 ngày.
3. Chọn nhiệm vụ lớn Hoàn thành bài thực hành lập trình, rồi Tạo gợi ý lịch học.
4. Chỉ vào giờ đề xuất, lý do, số phút và cảnh báo nếu có.

> Hệ thống xét những bước chưa hoàn thành, có ước lượng và còn thời gian để thực hiện trước hạn. Việc có hạn gần được xét trước, tiếp theo là mức ưu tiên rồi thời lượng cần xếp. Các phiên được đặt trong giờ rảnh sau khi trừ lịch lớp, sự kiện và các phiên đã có. Phần xem trước này cho sinh viên kiểm tra phương án trước khi lưu.

> Trong ví dụ, một bước đã xong, một bước còn 25 phút và một bước còn 50 phút, tổng cộng cần xếp thêm 75 phút. Phiên thường dài 25 đến 50 phút, giữa các phiên có khoảng nghỉ ít nhất 10 phút. Nếu không đủ chỗ trước hạn, hệ thống thông báo số phút còn thiếu để sinh viên điều chỉnh kế hoạch.

Chọn nhiệm vụ lớn khi xem gợi ý sẽ xét các bước của nhiệm vụ đó. Nếu bỏ trống lựa chọn, hệ thống xét theo phạm vi học kỳ/môn đang chọn.

### Bước 9 — Áp dụng và xem lịch, khoảng 1 phút

1. Giữ các phiên muốn sử dụng, chọn Áp dụng và xác nhận.
2. Mở Phiên tự học hoặc Thời khóa biểu để thấy kết quả.
3. Chuyển ngày/tuần/tháng/danh sách và bật/tắt một lớp lịch.

> Sau khi kiểm tra, sinh viên chọn những phiên phù hợp và bấm áp dụng. Trước khi lưu, hệ thống kiểm tra lại dữ liệu để phát hiện thay đổi về hạn, giờ rảnh hoặc lịch bận. Các phiên đã được lưu sẽ xuất hiện cùng lịch lớp và hạn nhiệm vụ trên thời khóa biểu.

Nếu bị hỏi về mất mạng khi áp dụng.

> Mỗi lần áp dụng có một khóa nhận diện. Nếu máy chủ đã lưu nhưng FE chưa nhận được phản hồi, FE có thể thử lại cùng khóa và nội dung để lấy kết quả của lần trước, tránh tạo thêm phiên trùng.

### Bước 10 — Thao tác phiên và ràng buộc lịch, khoảng 1 phút

1. Mở một phiên còn ở tương lai, chọn Dời phiên.
2. Thử đặt sang 19 giờ 10–19 giờ 35 trên D+1, nơi đã có lịch lớp, để minh họa từ chối.
3. Đóng biểu mẫu hoặc dời sang giờ hợp lệ. Nếu dùng việc phụ, có thể hủy rồi đặt tay lại để minh họa thêm/xóa chỗ đã dành.

> Khi sinh viên đặt tay hoặc dời phiên, hệ thống vẫn kiểm tra cùng các điều kiện như lịch tự động. Phiên phải nằm trong giờ rảnh, tránh lịch lớp và sự kiện, phù hợp hạn nhiệm vụ, đủ khoảng nghỉ và không vượt phần việc còn cần sắp xếp.

Chỉ dời hoặc hủy phiên dự kiến chưa đến giờ bắt đầu. Khi vào đúng khoảng giờ, nút Bắt đầu học xuất hiện. Phiên đang học có thể kết thúc. Phiên dự kiến đã hết giờ có thể xác nhận đã học xong hoặc bỏ lỡ.

> Trạng thái phiên cho biết sinh viên xử lý buổi học như thế nào. Sau buổi học, sinh viên cập nhật phần trăm nhiệm vụ theo kết quả thực tế. Ví dụ học xong một phiên vẫn có thể chưa giải xong toàn bộ bài lập trình.

Nếu cần demo Bắt đầu/Kết thúc, chuẩn bị một nhiệm vụ riêng có ước lượng 25 phút và một phiên 25 phút bắt đầu gần thời điểm trình bày bước này, nằm trong giờ rảnh hợp lệ. Không cần chờ phiên 50 phút của D+1. Nêu rõ đây là tài khoản/dữ liệu demo và việc bấm kết thúc là xác nhận của người dùng, chưa phải số giờ được bộ đếm thực học đo lại.

### Bước 11 — Thay đổi có ảnh hưởng và kết thúc, khoảng 1 phút

1. Dùng một phiên tương lai vẫn đang được đặt.
2. Sửa khung giờ rảnh để không còn bao phủ phiên đó, hoặc rút hạn của nhiệm vụ làm phiên vượt hạn.
3. Cho thấy cảnh báo ảnh hưởng. Có thể chọn quay lại để giữ dữ liệu demo.
4. Dùng một nhiệm vụ phụ để minh họa xóa có kiểm tra ảnh hưởng và lịch sử.

> Nhiệm vụ và lịch tự học có liên hệ với nhau. Khi sửa hạn hoặc giờ rảnh làm lịch cũ không còn hợp lệ, hệ thống cho biết những phiên tương lai bị ảnh hưởng và yêu cầu xác nhận. Nếu dữ liệu đã đổi từ lúc mở biểu mẫu, người dùng cần tải lại để xem dữ liệu mới trước khi lưu.

> Phần em phụ trách kết nối từ việc cần làm, các bước thực hiện, tiến độ đến thời gian học cụ thể. Sinh viên có thể chủ động điều chỉnh, còn hệ thống kiểm tra các ràng buộc để kế hoạch giữa nhiệm vụ và thời khóa biểu được nhất quán.

## 4. Ràng buộc cần nắm khi được hỏi

| Nội dung | Hành vi hiện hành |
| --- | --- |
| Quyền sở hữu | Đọc và ghi theo tài khoản đăng nhập. Môn/học kỳ được chọn phải thuộc tài khoản đó |
| Tên nhiệm vụ | Bắt buộc, không chỉ khoảng trắng, tối đa 200 ký tự |
| Ước lượng | Tùy chọn. Khi nhập là số phút nguyên dương, tối đa 100.000. Việc thiếu ước lượng vẫn được quản lý nhưng chưa đủ dữ liệu tự xếp lịch |
| Phân cấp | Cùng học kỳ/môn, không chuyển vào chính mình hoặc bước thuộc mình, tối đa 10 cấp và 1.000 nhiệm vụ trong một nhóm |
| Thêm theo nhóm | Tối đa 100 bước mỗi request. Một bước không hợp lệ thì cả lần thêm không được lưu |
| Hạn | Có thể để trống. Ưu tiên hạn riêng, nếu thiếu dùng hạn gần nhất ở phía nhiệm vụ lớn. Bước nhỏ không có hạn muộn hơn hạn đang áp dụng cho nhiệm vụ lớn |
| Tiến độ | 0% chưa bắt đầu, 1–99% đang thực hiện, 100% hoàn thành. Tiến độ nhiệm vụ lớn lấy trung bình các bước trực tiếp và tính tiếp lên các cấp |
| Thời lượng còn lại | Mỗi việc chưa chia nhỏ tính ước lượng × phần trăm còn lại, làm tròn lên phút. Nhiệm vụ lớn tổng hợp các việc cuối nhánh. Thiếu một ước lượng thì tổng chưa đầy đủ |
| Chuyển/tách | Chuyển sang việc cùng học kỳ/môn, hạn được kiểm tra lại. Đổi học kỳ/môn cho toàn nhóm thực hiện từ nhiệm vụ lớn nhất |
| Lịch lớp | Giờ kết thúc sau bắt đầu trong cùng ngày. Phạm vi nằm trong học kỳ và có ít nhất một ngày đúng thứ. Hai ngày áp dụng nhập cùng nhau hoặc để trống cả hai |
| Trùng lớp | Cảnh báo và cho phép lưu khi xác nhận. Phiên tự học trùng lịch bận bị từ chối |
| Giờ rảnh | Giờ kết thúc sau bắt đầu, ngày kết thúc từ ngày bắt đầu trở đi, có ít nhất một ngày đúng thứ. Chỉ khung đang bật được dùng khi lập lịch |
| Gợi ý | Bắt đầu từ hôm nay trở đi, xét 1–31 ngày, mặc định 14. Chỉ xét việc chưa chia tiếp, chưa hoàn thành, có ước lượng, còn hạn hoặc không hạn, thuộc phạm vi được chọn và học kỳ có thể sử dụng |
| Thứ tự xét | Hạn gần → mức ưu tiên cao → số phút cần xếp ít hơn → mã bản ghi để kết quả ổn định |
| Phiên | Tối đa 50 phút, thông thường tối thiểu 25 phút, nghỉ giữa phiên ít nhất 10 phút. Phần cuối dưới 25 phút chỉ được dùng để xếp hết lượng còn cần đặt của việc đó |
| Giới hạn khối lượng | Phần cần xếp dựa trên ước lượng còn lại và trừ thời gian đã dành trong những phiên còn hiệu lực, tránh đặt vượt lượng việc |
| Hiệu lực gợi ý | 10 phút. Áp dụng lần mới kiểm tra lại phiên bản và các điều kiện. Dữ liệu thay đổi thì cần tạo gợi ý mới |
| Bắt đầu phiên | Phiên đang dự kiến và hiện tại đã tới giờ bắt đầu nhưng chưa hết giờ, nhiệm vụ chưa xong, mỗi tài khoản tối đa một phiên đang học |
| Kết thúc/bỏ lỡ | Phiên đang học được kết thúc. Phiên dự kiến đã hết giờ có thể xác nhận hoàn thành hoặc bỏ lỡ |
| Chỉnh lịch có ảnh hưởng | Có preview và xác nhận hủy các phiên tương lai không còn hợp lệ. Phiên đang học hoặc đã tới giờ cần xử lý trước khi sửa lịch liên quan |
| Đồng thời | Dữ liệu cũ trả xung đột, yêu cầu tải lại. Các bước ghi liên quan được thực hiện trong giao dịch |
| Xóa nhiệm vụ | Xóa nhiệm vụ và các bước thuộc nó, có số liệu ảnh hưởng và dữ liệu liên quan. Lịch sử được giữ. Chưa có thùng rác/khôi phục |

## 5. Những câu hỏi dễ gặp và câu trả lời ngắn

**Tại sao cần chia nhỏ khi đã có nhiệm vụ lớn?**

> Các bước nhỏ giúp biết cụ thể cần làm gì, cập nhật tiến độ rõ hơn và ước lượng thời gian cho từng phần. Từ đó hệ thống có đầu vào chi tiết để xếp phiên tự học.

**Tiến độ lớn có tính theo số phút không?**

> Phiên bản hiện tại lấy trung bình phần trăm các bước trực tiếp. Thời lượng còn lại được tính riêng theo ước lượng của các việc chưa chia tiếp. Với nhóm nhiều cấp, mỗi nhiệm vụ lớn tổng hợp các bước trực tiếp của nó.

**Sao hoàn thành một phiên chưa đánh dấu xong nhiệm vụ?**

> Thời gian dành để học và kết quả bài làm có thể khác nhau. Sinh viên xác nhận buổi học rồi cập nhật tiến độ theo phần việc thực sự hoàn thành.

**Giờ rảnh khác khoảng trống trên lịch thế nào?**

> Giờ rảnh là khoảng sinh viên cho phép xếp tự học. Trong khoảng đó hệ thống vẫn phải trừ lịch lớp, sự kiện và các phiên đã có để lấy phần dùng được.

**Lịch tự học có dùng AI không? Có bảo đảm tối ưu nhất không?**

> Hiện em dùng thuật toán theo quy tắc có thứ tự ưu tiên. Nó lấy hạn, mức ưu tiên, lượng việc và các khoảng thời gian khả dụng để xếp. Mỗi đề xuất có lý do, và sinh viên quyết định áp dụng. Mức bảo đảm của phiên bản này là tôn trọng các ràng buộc đã kiểm tra, chưa chứng minh là phương án tối ưu toàn cục.

**Nếu không đủ giờ rảnh thì làm sao?**

> Hệ thống cho biết phần chưa xếp được và lý do. Sinh viên có thể bổ sung giờ rảnh, điều chỉnh ước lượng hoặc xem lại kế hoạch và hạn sau khi cân nhắc yêu cầu môn học.

**Tại sao có cảnh báo khi sửa một nhiệm vụ?**

> Vì hạn, tiến độ và ước lượng ảnh hưởng đến các phiên đã đặt. Hệ thống kiểm tra xem phiên nào không còn phù hợp, rồi yêu cầu xác nhận để xử lý cùng thao tác.

**Vì sao ở Tất cả nhiệm vụ không thấy việc đã xong?**

> Danh sách mặc định tập trung vào nhiệm vụ lớn còn phải làm. Em xem các việc đã hoàn thành ở mục Đã hoàn thành hoặc lọc trạng thái tương ứng.

**Sao Chưa phân loại và Không có hạn khác nhau?**

> Chưa phân loại đang dựa trên việc chưa gắn học kỳ/môn. Không có hạn dựa trên thời điểm hoàn thành. Một việc cá nhân vẫn có thể đặt hạn, một việc thuộc môn vẫn có thể chưa đặt hạn.

## 6. Đối chiếu trực tiếp 10 ảnh giao diện người dùng gửi

### Những dữ liệu cần chỉnh trước buổi trình bày

1. Chi tiết `ky01` có các bước `bbbb`, `aaa`, phần ước lượng ghi “Tổng hợp từ các bước nhỏ”, còn lại dự kiến “Chưa đủ dữ liệu”. Cần mở từng bước chưa chia tiếp, chọn Chỉnh sửa, nhập **Thời gian ước lượng (phút)** rồi lưu. Không nhập tổng thời lượng ở nhiệm vụ lớn để thay cho các bước.
2. Ảnh kết quả gợi ý nêu rõ `bbbb` và `aaa` chưa nhập thời lượng dự kiến. Code bỏ qua việc thiếu ước lượng trước khi cộng các tổng phút. Vì vậy cả bốn ô có thể bằng 0 dù vẫn còn việc chưa xong; không diễn giải kết quả đó là đã hoàn thành hết hoặc đã có đủ giờ học.
3. Các khung giờ rảnh nhìn thấy đều là thứ Hai, áp dụng 06/11–06/12/2026. Nhiệm vụ `ky01` hiện có hạn 16 giờ 50 ngày 07/11/2026. Lần thứ Hai đầu tiên trong khoảng này là 09/11, sau hạn đó. Nếu muốn xếp các bước kế thừa hạn `ky01`, cần thêm giờ rảnh trước hạn hoặc chọn nhiệm vụ demo mới có hạn hợp lý. Bổ sung ước lượng là cần thiết nhưng chưa đủ để bảo đảm có đề xuất.
4. Ảnh có nhiều khung thứ Hai 19 giờ–22 giờ trùng nhau. Khi tính lịch, phần giao nhau được hợp nhất, không nhân số giờ lên theo số bản ghi. Có thể giữ để minh họa quy tắc hoặc tự dọn dữ liệu demo; tài liệu này không thực hiện xóa.
5. Màn Lịch học theo môn đang để Tất cả môn học. Chọn một môn cụ thể trước khi trình bày danh sách buổi học và thêm buổi học cho môn đó.
6. Phiên tự học đang trống nghĩa là trong khoảng ngày và bộ lọc đang xem chưa có phiên được trả về. Sau khi có đề xuất hợp lệ, phải chọn phiên và **Áp dụng … phiên đã chọn** thì mới tạo dữ liệu phiên. Chỉ bấm Tạo gợi ý lịch học chưa lưu phiên.

Các tên thử như `aaa`, `bbbb`, `ky01` có thể dùng để kiểm tra kỹ thuật, nhưng khi thuyết trình nên tạo bài có tên và nội dung cụ thể. Dùng biểu mẫu trong ảnh với tên học kỳ/môn, loại công việc, ưu tiên, hạn và ước lượng; không yêu cầu sinh viên nhập mã bản ghi.

### Ý nghĩa bốn ô trong kết quả gợi ý

| Ô hiển thị | Cách giải thích |
| --- | --- |
| Cần xếp thêm | Tổng phút còn cần đặt của những việc đủ điều kiện, sau khi trừ phần đã dành trong các phiên còn hiệu lực |
| Đã dành chỗ | Phần thời gian đã được các phiên Planned/InProgress còn hiệu lực giữ chỗ cho những việc đủ điều kiện đang xét |
| Mới đề xuất | Tổng thời lượng trong các phiên của phương án xem trước, chưa lưu vào lịch |
| Còn thiếu | Phần cần xếp của các việc đủ điều kiện nhưng thuật toán chưa tìm đủ chỗ; phải đọc thêm cảnh báo vì việc thiếu ước lượng không thể quy đổi thành số phút thiếu |

Ảnh thể hiện giao diện và dữ liệu tại thời điểm chụp. Việc đối chiếu ảnh cùng code không thay thế buổi chạy thử trên tài khoản demo sau khi đã chuẩn bị đủ dữ liệu.

## 7. Cách xử lý các tình huống khi demo

- Không có gợi ý thì đọc cảnh báo trên màn hình, kiểm ước lượng từng bước cuối, ngày/hạn còn hiệu lực, khung giờ rảnh đang bật và lịch bận. Nếu đã đủ thời lượng trong các phiên còn hiệu lực, hệ thống có thể không cần đề xuất thêm.
- Báo dữ liệu đã đổi thì đóng biểu mẫu, tải lại, mở lại từ kết quả mới nhất. Tạo lại gợi ý sau các thay đổi thay vì cố áp dụng phương án cũ.
- Không thấy việc cá nhân khi lọc môn thì xóa bộ lọc hoặc mở Chưa phân loại. Mặc định xem chung học kỳ không áp bộ lọc học kỳ từ thanh bên.
- Thiếu nút Bắt đầu hoặc Bỏ lỡ thì đối chiếu giờ và trạng thái phiên. Không chờ hết một phiên tương lai trong luồng demo chính, sử dụng dữ liệu phiên được chuẩn bị đúng thời điểm nếu cần minh họa.
- Phần bảng trạng thái, thanh kéo và các chế độ xem chỉ cần giới thiệu ngắn. Dành trọng tâm cho chia nhỏ, cách tính tiến độ, ràng buộc lịch và xem trước/áp dụng.

## 8. Phạm vi nên trình bày đúng mức đã có

Những chức năng đã có ở FE/BE được liệt kê trong phần 1. Worker phát nhắc hạn tự động, bộ đếm đo giờ thực học, đính kèm, thùng rác, nhiệm vụ lặp, kéo thả thẻ giữa cột và Dashboard thống kê tích hợp đầy đủ thuộc các phần chưa được triển khai hoặc nghiệm thu trong lượt core này. Chỉ trao đổi các giới hạn này khi hội đồng hỏi về phạm vi hoặc hướng phát triển.

Phần giao diện và nghiệp vụ đã có kiểm thử ở những lượt triển khai trước. Tài liệu này là kết quả đối chiếu mã nguồn và soạn kịch bản, chưa phải xác nhận buổi demo trên dữ liệu cụ thể của người dùng đã chạy thành công.

## 9. Lần theo code khi cần giải thích sâu

| Vấn đề | Nguồn BE | Nguồn FE |
| --- | --- | --- |
| CRUD và lịch sử | `backend/StudyPlanner.Application/HocTap/NhiemVu/NhiemVuService.cs` | `src/hoc-tap/NhiemVuView.tsx` |
| Tiến độ, hạn kế thừa, ràng buộc chia nhỏ | `QuyTacCay.cs`, entity `NhiemVu.cs` | Chi tiết, form tiến độ, form thêm bước |
| Lọc theo hạn/phạm vi | `NhiemVuService.DanhSachAsync` | `HocTapPage.tsx`, `boLoc.ts` |
| Lịch lớp và giờ rảnh | `LichHocService.cs`, `KhungGioRanhService.cs` | `LichView.tsx` |
| Lịch tổng hợp | `ThoiKhoaBieuService.cs`, `QuyTacThoiGian.cs` | `LichView.tsx`, `lich.ts` |
| Thứ tự xếp và phiên/nghỉ | `QuyTacLapLich.cs` | Màn gợi ý |
| Tạo gợi ý và áp dụng | `LapLichService.cs`, `QuyTacDatPhien.cs` | `LapLichView.tsx` |
| Chuyển trạng thái phiên | `PhienTuHocService.cs` | Chi tiết phiên trong `LichView.tsx` |
| Phiên bản và giao dịch | `HocTapDieuPhoi.cs`, `EfHocTapStore.cs` | `api.ts`, preview trước lệnh ghi |

Các tên file BE rút gọn trong bảng thuộc `backend/StudyPlanner.Application/HocTap`, trừ entity và store đã ghi rõ. Đường dẫn FE thuộc repository `C:/Users/ADMIN/Desktop/Study_Planner_FE`.

## 10. Giới thiệu nội dung từng màn hình trước khi thao tác

Không chỉ đọc tên nút hoặc báo thao tác thành công. Khi mở mỗi màn, nói mục đích của màn đó, chỉ vào các vùng thông tin, giải thích dữ liệu đang hiển thị rồi mới thao tác. Phần dưới bám các ảnh người dùng gửi và code hiện tại.

### 10.1. Bố cục và phạm vi dữ liệu

- Thanh bên ngoài là điều hướng chung của ứng dụng, có dữ liệu học kỳ/môn do thành viên khác phụ trách.
- Menu dọc bên trong Nhiệm vụ là các công cụ của module đang trình bày, gồm danh sách nhiệm vụ và nhóm Lập kế hoạch.
- Bộ lọc Học kỳ/Môn học trong vùng nội dung xác định phạm vi màn hiện tại. Học kỳ đang hiển thị ở thanh đầu trang không có nghĩa màn nhiệm vụ đang bị giới hạn vào học kỳ đó; xem giá trị bộ lọc trong màn.
- Chọn theo tên từ dữ liệu của tài khoản, không nhập mã bản ghi trên giao diện.
- Bộ lọc mặc định thu gọn để dành chỗ cho dữ liệu. Mở bộ lọc hiện các lựa chọn học kỳ/môn và các tiêu chí nhiệm vụ; Thu gọn bộ lọc ẩn vùng chọn, vẫn giữ nguyên điều kiện. Các nhãn bên dưới tóm tắt những lựa chọn đang áp dụng. Ô tìm nhiệm vụ, Xóa bộ lọc và Tải lại luôn hiển thị để thao tác nhanh. Thu/mở không tự tải lại dữ liệu; F5 trở về trạng thái thu gọn, các điều kiện trên URL vẫn được áp dụng.

> Đây là khu vực làm việc của sinh viên. Phần nhiệm vụ cho biết mình cần làm gì, còn nhóm lập kế hoạch giúp sắp thời gian thực hiện. Hai ô học kỳ và môn học xác định phạm vi dữ liệu em muốn xem.

### 10.2. Tất cả nhiệm vụ và các danh sách theo hạn

- Dòng nhiệm vụ có ô hoàn thành, tên, phạm vi môn/việc cá nhân, tiến độ, hạn và dấu ưu tiên. Mũi tên xuất hiện khi có các bước nhỏ.
- Ô Tìm nhiệm vụ tìm theo tên/nội dung. Hiển thị chọn nhóm danh sách, Ưu tiên chọn mức quan trọng, Loại công việc chọn bài tập/thi/ôn tập/cá nhân, Trạng thái chọn mức thực hiện, Sắp xếp đổi thứ tự. Đây là các tiêu chí riêng, có thể kết hợp.
- Ghi nhanh tạo việc ít thông tin; Thêm nhiệm vụ mở biểu mẫu đầy đủ; Xóa bộ lọc gỡ điều kiện, không xóa dữ liệu; Tải lại đọc dữ liệu mới từ BE. Nút Trang trước/sau chuyển trang kết quả.
- Tất cả nhiệm vụ mặc định tập trung vào các nhiệm vụ lớn chưa hoàn thành; không trình bày như danh sách tất cả trạng thái. Hôm nay/7 ngày tới/Quá hạn/Không có hạn/Đã hoàn thành dùng các việc chưa chia tiếp để tránh đếm lặp. 7 ngày tới có tính hôm nay.
- Chưa phân loại là chưa gắn học kỳ/môn. Không có hạn là không có hạn hiệu lực, kể cả hạn được kế thừa. Quá hạn là tình trạng tính từ hạn và tiến độ, không phải trạng thái nhập tay thứ tư.

> Màn này giúp sinh viên biết những việc còn phải xử lý. Em có thể xem chung hoặc lọc theo môn, hạn, ưu tiên và trạng thái. Một việc có thể chưa gắn môn nhưng vẫn có hạn, nên Chưa phân loại và Không có hạn phục vụ hai nhu cầu khác nhau.

### 10.3. Biểu mẫu và chi tiết nhiệm vụ

- Biểu mẫu gồm tên, nội dung, học kỳ/môn, loại công việc, ưu tiên, hạn hoàn thành, ước lượng phút và phần mở rộng giờ diễn ra sự kiện. Tên là bắt buộc; ước lượng có thể để trống nhưng khi đó chưa đủ dữ liệu xếp lịch tự học.
- Hạn là mốc cần hoàn thành. Giờ sự kiện là khoảng diễn ra ca thi/hoạt động, dùng làm lịch bận. Ước lượng là khối lượng cần học, chưa phải một buổi tự học đã được đặt.
- Chi tiết bên phải hiển thị trạng thái, mô tả, hạn hiệu lực, tiến độ, ưu tiên, loại, ước lượng và còn lại dự kiến. Thanh chia cho phép chỉnh độ rộng khi màn hình đủ lớn.
- Chỉnh sửa đổi thông tin; Thêm bước nhỏ chia việc; Cập nhật tiến độ chỉ dùng với việc chưa chia tiếp; Chuyển vào nhiệm vụ lớn/tách riêng đổi cách tổ chức; Đổi học kỳ hoặc môn đổi phạm vi cả nhóm từ nhiệm vụ lớn nhất; Sao chép liên kết để mở lại đúng việc; Xem phiên học nối sang lịch của việc đó.
- Các bước nhỏ thể hiện cấu trúc công việc, lịch sử thể hiện việc đã thay đổi. Tiến độ lớn tổng hợp trung bình các bước trực tiếp; ước lượng và còn lại tổng hợp từ những bước cuối cùng.

> Khi chọn một nhiệm vụ, em xem được nội dung cần làm, thời hạn và mức hoàn thành. Em chia việc lớn thành các bước có thể thực hiện và cập nhật kết quả ở từng bước. Hệ thống tổng hợp lại để sinh viên theo dõi toàn bộ bài tập.

### 10.4. Bảng trạng thái

- Gom kết quả theo các cột Chưa bắt đầu, Đang thực hiện và Đã hoàn thành; phản ánh cùng tiến độ và trạng thái của nhiệm vụ.
- Vẫn chịu bộ lọc và trang đang xem. Muốn xem việc hoàn thành phải chọn danh sách/trạng thái tương ứng vì mặc định tập trung việc chưa xong.
- Không giới thiệu kéo thả chuyển cột vì module này chưa có thao tác đó.

> Cùng dữ liệu nhiệm vụ có thể xem theo bảng trạng thái để thấy việc nào chưa làm, đang làm và đã hoàn thành. Trạng thái được gắn với tiến độ, nên khi em cập nhật phần trăm thì bảng và danh sách dùng cùng kết quả.

### 10.5. Thời khóa biểu

- Có ngày đang xem, nút về trước/về sau, Hôm nay và chế độ ngày/tuần/tháng/danh sách. Đây là cách quan sát cùng nguồn dữ liệu.
- Năm ô chọn bật/tắt Học trên lớp, Sự kiện, Hạn hoàn thành, Tự học và Giờ rảnh. Bỏ chọn một lớp chỉ ẩn nó, không xóa hay tắt nguồn dữ liệu.
- Học trên lớp đến từ mẫu lịch môn; Sự kiện đến từ giờ diễn ra sự kiện trong nhiệm vụ; Hạn hoàn thành là mốc; Tự học là phiên đã lưu; Giờ rảnh là khoảng sinh viên cho phép xếp học.
- Hạn hoàn thành không tự chiếm cả khoảng thời gian học. Sự kiện và lịch lớp mới tạo khoảng bận; giờ rảnh cũng không có nghĩa mọi phút trong đó đều dùng được.

> Thời khóa biểu tổng hợp những thông tin có liên quan đến thời gian học. Em có thể bật riêng từng nhóm để dễ quan sát. Mốc hạn nhắc sinh viên phải xong việc khi nào, còn phiên tự học cho biết đã dành khoảng nào để thực hiện việc đó.

### 10.6. Lịch học theo môn và giờ rảnh

- Lịch học theo môn cần chọn một môn, có nút Thêm buổi học; mỗi dòng thể hiện thứ, giờ, phòng và thao tác chỉnh/xóa. Một mẫu lặp theo tuần trong phạm vi ngày, không phải mỗi lần xuất hiện là một bản ghi mới.
- Biểu mẫu buổi học mặc định chọn Áp dụng suốt học kỳ. Bỏ chọn để nhập khoảng ngày riêng; cả hai ngày khi đó đều bắt buộc. Giao diện không còn hướng dẫn người dùng bỏ trống ngày để suy ra phạm vi.
- Giờ rảnh có thứ, khoảng giờ, ngày bắt đầu/kết thúc, trạng thái đang sử dụng/đã tắt và thao tác chỉnh/xóa. Đây là lịch của sinh viên, không được nhân lên theo môn.
- Lịch lớp trùng có cảnh báo và xác nhận; phiên tự học trùng lịch bận bị từ chối. Các khung rảnh trùng nhau được hợp nhất khi tính thời gian, không cộng lặp.

> Lịch học theo môn mô tả thời gian cố định đã bận. Giờ rảnh mô tả thời gian em sẵn sàng tự học. Hệ thống kết hợp hai nguồn, trừ thêm sự kiện và phiên đã có để tìm khoảng có thể xếp.

### 10.7. Gợi ý lịch học

- Ngày bắt đầu và số ngày muốn lập lịch xác định khoảng cần xét. Ô tìm nhiệm vụ giúp tìm tên trong lựa chọn; có thể chọn nhiều nhiệm vụ, chọn nhiệm vụ lớn bao gồm các bước. Bỏ trống lựa chọn xét theo bộ lọc học kỳ/môn.
- Tạo gợi ý chỉ tính phương án, chưa lưu phiên. Thứ tự xét là hạn gần, ưu tiên cao, lượng phút cần đặt ít hơn, rồi mã để ổn định kết quả. Thuật toán dùng quy tắc, không phải AI.
- Cần xếp thêm là phần còn cần đặt; Đã dành chỗ là thời lượng trong phiên còn hiệu lực; Mới đề xuất là số phút trong phương án; Còn thiếu là phần có thể tính được nhưng chưa tìm đủ chỗ. Đọc cảnh báo thiếu ước lượng riêng, không hiểu số 0 là mọi việc đều đã xong.
- Mỗi đề xuất có việc cần học, bắt đầu/kết thúc và lý do. Chọn tất cả/Bỏ chọn/Áp dụng cho phép sinh viên quyết định phương án. Gợi ý có hạn dùng 10 phút; áp dụng kiểm tra lại dữ liệu và điều kiện.

> Màn gợi ý chuyển lượng công việc còn lại thành phương án học cụ thể trong giờ rảnh. Sinh viên xem các phiên và lý do trước khi chọn áp dụng. Nếu thiếu dữ liệu hoặc thiếu thời gian, em đọc cảnh báo để biết cần bổ sung gì.

### 10.8. Phiên tự học

- Có nút Đặt phiên tự học, khoảng ngày đang xem, chế độ xem và danh sách các phiên đã lưu. Mỗi dòng gồm tên nhiệm vụ, giờ bắt đầu/kết thúc, trạng thái và Xem và cập nhật.
- Phiên có thể do đặt tay hoặc áp dụng gợi ý. Cả hai cùng chịu kiểm tra giờ rảnh, lịch bận, hạn, khoảng nghỉ và lượng việc chưa được đặt.
- Planned là đã lên lịch; InProgress là đang học; Completed là đã xác nhận hoàn thành buổi; Missed là bỏ lỡ; Cancelled là đã hủy. Nút thao tác phụ thuộc trạng thái và thời gian hiện tại.
- Hoàn thành buổi học không tự tăng phần trăm nhiệm vụ và chưa phải số phút thực học do bộ đếm đo.

> Phiên tự học là buổi đã được lưu vào kế hoạch, gắn với một nhiệm vụ cụ thể. Sinh viên có thể dời hoặc hủy phiên tương lai, bắt đầu khi đến giờ và xác nhận kết quả buổi học. Kết quả bài làm vẫn được theo dõi qua tiến độ nhiệm vụ.

### 10.9. Xác nhận xóa và thông báo kết quả

- Hộp trong giao diện hiển thị tên nhiệm vụ, số nhiệm vụ/bước bị xóa và những dữ liệu liên quan có ảnh hưởng; không dùng hộp mặc định mang tên localhost.
- Nút Hủy và dấu đóng/Esc hủy thao tác trước khi gửi xóa. Nút Xóa nhiệm vụ màu đỏ thể hiện hành động; khi gửi thì hiển thị Đang xóa và khóa bấm lặp.
- Thành công mới đóng hộp và hiện thông báo Đã xóa nhiệm vụ. Lỗi giữ hộp cùng nội dung lỗi để người dùng xử lý. Nhiệm vụ và các bước bị xóa, lịch sử vẫn được giữ, không có khôi phục/thùng rác trong phạm vi này.

> Trước khi xóa, hệ thống cho sinh viên biết phạm vi ảnh hưởng để kiểm tra lại. Khi xác nhận, máy chủ kiểm tra và thực hiện xóa; giao diện chỉ báo thành công sau khi nhận được kết quả. Lịch sử được giữ để truy vết những gì đã xảy ra.
