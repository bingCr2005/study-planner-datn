# Kịch bản kiểm thử ưu tiên nhiệm vụ

Dùng bộ ca này để kiểm thử **Gợi ý nên làm trước** trên trang Hôm nay và đối chiếu với **Nhiệm vụ** và **Lập kế hoạch**. Dữ liệu và kết quả mong đợi được chỉnh từ [đoạn chat tham khảo](https://chatgpt.com/share/6afc18f6-39ac-83ec-8897-1caa0259f6b9) theo [quy tắc của dự án](modules/07-goi-y-cong-viec.md#quy-tắc-ưu-tiên-trên-trang-hôm-nay).

## Chuẩn bị dữ liệu

- Dùng các nhiệm vụ thử trong cùng học kỳ đang chọn, hoặc tạo tất cả dưới dạng việc cá nhân. Trừ khi ghi khác, A và B là nhiệm vụ lá, chưa bắt đầu và chưa ghim. Mỗi ca dùng dữ liệu riêng để dễ nhìn thứ tự.
- Với test tự động, cố định thời điểm `T`. Khi thử thủ công, `T` là thời điểm chạy ca theo múi giờ tài khoản. Đặt hạn tương đối từ `T`; để kiểm tra **đúng** 24 giờ, 3 ngày hoặc 7 ngày, dùng test có đồng hồ cố định.
- Các ca Hôm nay mặc định dùng **Nhóm độ gấp → Mức quan trọng → Hạn hoàn thành → Đang thực hiện**. Vào **Quy tắc ưu tiên** để xác nhận quy tắc đang áp dụng. Nếu môi trường thử đã chọn quy tắc riêng, dùng cấu hình mặc định cho các ca dưới đây. Ca `HN-09` dùng cấu hình riêng.
- Với các ca Lập kế hoạch, chọn đúng các nhiệm vụ thử và phạm vi ngày chứa toàn bộ khung giờ nêu trong ca. Dùng cấu hình 25 phút học, 5 phút nghỉ, tối đa 180 phút/ngày. Trừ khi ghi khác, các việc có hạn tương lai, 0% tiến độ và chưa có phiên đã đặt.

## 1. Xếp thứ tự trên Hôm nay

Quan sát **Hôm nay → Gợi ý nên làm trước**. Dùng **Xem tất cả** nếu bảng đang chỉ hiện năm việc đầu. Thứ tự nhóm là **QH → K1 → K2 → K3 → K4 → K5**. Trong cùng nhóm, cấu hình mặc định xét **Cao → Trung bình → Thấp**, rồi hạn chính xác sớm hơn, việc đang thực hiện và mã nhiệm vụ tăng dần.

| Mã | Dữ liệu hoặc thao tác | Kết quả mong đợi |
| --- | --- | --- |
| HN-01 | A Cao còn 15 ngày. B Thấp còn đúng 1 ngày. | B đứng trước A vì K1 đứng trước K4. A vẫn là Cao và B vẫn là Thấp. |
| HN-02 | A Thấp còn 2 ngày. B Cao còn đúng 3 ngày. | Cả hai thuộc K2. B đứng trước A vì mức quan trọng cao hơn. |
| HN-03 | A Cao còn 10 ngày. B Thấp còn 9 ngày. | Cả hai thuộc K4. A đứng trước B. |
| HN-04 | A Cao còn 5 giờ. B Thấp còn 1 giờ. | Cả hai thuộc K1. A đứng trước B theo quy tắc mặc định của nhóm. |
| HN-05 | A và B đều Cao, A còn 3 giờ, B còn 2 giờ. | Cả hai thuộc K1. B đứng trước A. |
| HN-06 | A và B có cùng hạn và cùng mức Cao. A ở 50%, B ở 0%. | A đứng trước. Khi đưa cả hai về 0%, nhiệm vụ có mã nhỏ hơn đứng trước. |
| HN-07 | A đã quá hạn 1 phút và ưu tiên Thấp. B còn 1 giờ và ưu tiên Cao. | A ở nhóm **Đã quá hạn** và đứng trước B. Hoàn thành A thì danh sách gợi ý chỉ còn B. |
| HN-08 | Nhiệm vụ cha hạn sau đúng 3 ngày. Xóa hạn riêng của bước con và lưu. | Bước con lấy hạn hiệu lực của cha và vào K2. Nhiệm vụ cha có bước con được loại khỏi gợi ý. |
| HN-09 | Dùng tài khoản thử. Admin chọn **Hạn hoàn thành → Mức quan trọng**. Mở lại Hôm nay và dùng dữ liệu HN-02, sau đó HN-01. | HN-02 đổi thành A trước B vì cả hai thuộc K2. HN-01 vẫn là B trước A vì thứ tự nhóm có hiệu lực trước cấu hình admin. |
| HN-10 | A trống hạn, ưu tiên Cao. B còn 8 ngày, ưu tiên Thấp. | B ở K4, đứng trước A ở K5. Nếu bỏ hạn của B, cả hai ở K5 và A đứng trước. |

### Mốc biên và ma trận nhóm

| Nhóm | Khoảng thời gian | Dữ liệu biên và kết quả mong đợi |
| --- | --- | --- |
| QH | Hạn đã qua | `T − 1 phút` thuộc QH. |
| K1 | 0 đến đúng 24 giờ | `T`, `T + 6 giờ` và `T + 24 giờ` đều thuộc K1. |
| K2 | Trên 1 ngày đến đúng 3 ngày | `T + 1 ngày + 1 phút` và `T + 3 ngày` đều thuộc K2. |
| K3 | Trên 3 ngày đến đúng 7 ngày | `T + 3 ngày + 1 phút` và `T + 7 ngày` đều thuộc K3. |
| K4 | Trên 7 ngày | `T + 7 ngày + 1 phút` thuộc K4. |
| K5 | Chưa có hạn | Nhiệm vụ trống hạn và cây không có hạn để kế thừa thuộc K5. |

Đặt A ở mức Cao và B ở mức Thấp. Mỗi ô ghi việc đứng trước theo cấu hình mặc định. Có thể lần lượt dùng hạn đại diện là quá hạn 1 phút, còn 24 giờ, còn 2 ngày, còn 5 ngày, còn 10 ngày và trống hạn.

| Nhóm của A ↓ / Nhóm của B → | QH | K1 | K2 | K3 | K4 | K5 |
| --- | --- | --- | --- | --- | --- | --- |
| QH | A | A | A | A | A | A |
| K1 | B | A | A | A | A | A |
| K2 | B | B | A | A | A | A |
| K3 | B | B | B | A | A | A |
| K4 | B | B | B | B | A | A |
| K5 | B | B | B | B | B | A |

## 2. Ghim, bộ lọc và hạn nhiệm vụ

| Mã | Dữ liệu hoặc thao tác | Kết quả mong đợi |
| --- | --- | --- |
| GH-01 | A Cao còn 15 ngày, đã ghim. B Thấp còn 5 giờ, chưa ghim. Xem cả hai trên danh sách Nhiệm vụ và Hôm nay. | Trên Nhiệm vụ, A nằm nhóm **Đã ghim** nếu cả hai khớp bộ lọc. Trên Hôm nay, B đứng trước A vì khác nhóm. |
| GH-02 | Ghim A ở mức Thấp, rồi lọc chỉ mức Cao. Bỏ bộ lọc, tải lại và bỏ ghim A. | Bộ lọc mức Cao ẩn A. Khi bỏ lọc và tải lại, A vẫn ghim. Bỏ ghim thì A trở về vị trí theo cách sắp xếp đã chọn. |
| GH-03 | A và B đều Cao, cùng trạng thái ghim, hạn khác nhau. Chọn **Ưu tiên cao trước**. | Trong nhóm ghim tương ứng và cùng mức ưu tiên, mã nhỏ hơn đứng trước. Đổi sang **Hạn gần nhất** thì hạn sớm hơn đứng trước trong cùng nhóm ghim. |
| NV-01 | Tạo việc không đổi mức ưu tiên. Lưu với hạn để trống, sau đó thêm một ngày giờ cụ thể. | Mức ban đầu là Trung bình. Lưu hạn trống được. Thêm hạn làm đổi nhãn độ gấp khi qua mốc phân nhóm, còn mức ưu tiên vẫn là Trung bình. |
| NV-02 | Cha có hạn 20:00. Sửa hạn riêng của con lần lượt thành trống, 20:00 và 20:01 cùng ngày. | Lần đầu kế thừa 20:00. Lần hai lưu được. Lần ba báo hạn con muộn hơn hạn cha. |

## 3. Gợi ý phiên tự học

| Mã | Dữ liệu hoặc thao tác | Kết quả mong đợi |
| --- | --- | --- |
| LP-01 | A Cao còn 3 ngày và B Thấp còn 2 ngày, mỗi việc cần 25 phút. Chỉ có một khe khả dụng 25 phút trước hạn của cả hai. | Phiên bản 4 dùng Admin mặc định: A được xếp 25 phút, B thiếu 25 phút. Thử hạn trước không được làm A mất phút đã xếp. Hôm nay cũng xếp A trước B trong K2. |
| LP-02 | A mã nhỏ hơn B; cùng hạn, cùng mức Cao và trạng thái. A cần 50 phút, B cần 25 phút. Chỉ có một khe khả dụng 25 phút trước hạn. | A được xếp một phiên 25 phút; A thiếu 25 phút, B thiếu 25 phút. Mã nhiệm vụ là phân xử cuối, không đưa việc ngắn lên trước. |
| LP-03 | A chưa nhập ước lượng. B cần 25 phút và có đủ giờ rảnh. C cần 25 phút nhưng đã quá hạn. | A có cảnh báo thiếu ước lượng. B có phiên đề xuất. C có cảnh báo hạn đã qua và không có phiên mới. |
| LP-04 | A ước lượng 100 phút, tiến độ 50%. A đã có một phiên 25 phút còn hiệu lực. Tạo thêm lịch với một khe mới 25 phút. | Tổng phần còn lại theo tiến độ là 50 phút. Đã đặt 25 phút, cần xếp thêm 25 phút và đề xuất một phiên mới. |
| LP-05 | A cần 60 phút, trước hạn có đúng một khe 25 phút, không có phiên đã đặt. Tạo gợi ý. | A có một phiên 25 phút. **Chưa xếp** là 35 phút và cảnh báo hiển thị số phút còn thiếu của A. |
| LP-06 | A cần 50 phút, có một khung giờ rảnh liền 55 phút trước hạn. Dùng cấu hình học 25 phút, nghỉ 5 phút. | Đề xuất hai phiên 25 phút với 5 phút nghỉ ở giữa. |
| LP-07 | Như LP-01, thêm khe thứ hai sau hạn B nhưng trước hạn A. | B vào khe đầu, A vào khe sau; cả hai đủ 25 phút trước hạn. Lý do giải thích việc điều chỉnh thứ tự trong nhóm. Apply qua validator hiện hành thành công. |
| LP-08 | Admin đổi sang Hạn → Quan trọng. Chạy lại LP-01. | B được khe duy nhất. Nhóm độ gấp vẫn được so trước. |
| LP-09 | Tạo preview, sau đó Admin kích hoạt quy tắc khác trước khi áp dụng. | HTTP 409 PRIORITY_RULE_CHANGED; lịch cũ còn nguyên. FE cho tạo lại gợi ý. Retry yêu cầu đã thành công từ trước vẫn trả lô cũ. |
| LP-10 | Gợi ý 20 phút + 5 phút cuối; bỏ chọn phiên 20 phút. | Đang chọn 5 phút, còn chưa xếp tăng 20; nút áp dụng bị khóa và có Chọn cùng. Chọn đủ thì xác nhận hiển thị 25 phút. |
| LP-11 | Nhập ngày/số ngày/phạm vi nhiệm vụ, tạo gợi ý; quay lại sửa, mở cài đặt hoặc đi từ cảnh báo sang nhiệm vụ rồi quay lại và tải lại trang. | Bước nhập và kết quả tách nhau; giữ ngày/số ngày/chế độ/phạm vi đã chọn. Màn xử lý có đường quay lại phía trên. Sửa dữ liệu rồi tạo gợi ý mới trước khi lưu. Kiểm tra cả chiều rộng 390px. |

## Ghi chú đối chiếu đoạn chat

- Lấy bảng **sáu nhóm** và ma trận 36 cặp ở phương án đầu. Bản đề xuất cuối trong chat có bảy nhóm và xét hạn chính xác trước trong 24 giờ. Ca HN-04 ghi theo phương án sáu nhóm đã chọn.
- Kết quả khi hòa dùng trạng thái và mã nhiệm vụ theo thứ tự cấu hình. Giờ tạo là một tiêu chí sắp xếp riêng trên danh sách Nhiệm vụ.
- Cảnh báo thiếu giờ cần dữ liệu về thời lượng và khung giờ khả dụng. Các bước nhỏ thuộc cây nhiệm vụ; bộ xếp phiên dùng các nhiệm vụ lá và hiện chưa có điều kiện tiên quyết giữa các bước.
