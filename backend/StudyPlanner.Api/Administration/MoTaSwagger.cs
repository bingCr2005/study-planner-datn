using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace StudyPlanner.Api.Administration;

// Chỉ bổ sung tài liệu OpenAPI; không đổi route, tên trường JSON hay nghiệp vụ.
public sealed class MoTaSwagger : IOperationFilter, ISchemaFilter
{
    public static string Group(string controller) => controller switch
    {
        "NguoiDung" => "Quản lý người dùng",
        "Health" => "Kiểm tra hoạt động API",
        "Semesters" => "Quản lý học kỳ",
        "Courses" => "Quản lý môn học",
        "XacThuc" => "Đăng ký và đăng nhập",
        "HoSo" => "Hồ sơ cá nhân",
        "NhiemVu" => "Quản lý nhiệm vụ và chia nhỏ",
        "LichSuNhiemVu" => "Lịch sử nhiệm vụ",
        "LichHoc" => "Quản lý lịch học theo môn",
        "KhungGioRanh" => "Quản lý giờ rảnh",
        "ThoiKhoaBieu" => "Thời khóa biểu",
        "LapLich" => "Gợi ý và áp dụng lịch tự học",
        "PhienTuHoc" => "Quản lý phiên tự học",
        _ => "API mẫu và hệ thống"
    };

    private static readonly Dictionary<string, string> Fields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["phienBan"] = "Phiên bản nguồn theo tài khoản; gửi If-Match dạng \"ht-N\" khi sửa dữ liệu.",
        ["tieuDe"] = "Tiêu đề bắt buộc, tối đa 200 ký tự, không chỉ khoảng trắng.",
        ["maHocKy"] = "Học kỳ của chính người dùng; môn nếu có phải thuộc học kỳ này.",
        ["maMonHoc"] = "Môn của chính người dùng; có thể bỏ trống với nhiệm vụ cá nhân.",
        ["maNhiemVuCha"] = "Cha cùng phạm vi. null để tách thành nhiệm vụ gốc.",
        ["hanHoanThanh"] = "Hạn riêng, có offset. null có thể kế thừa hạn tổ tiên.",
        ["hanHieuLuc"] = "Hạn dùng chung khi lọc, hiển thị lịch và xếp phiên; được server tính.",
        ["thoiLuongDuKienPhut"] = "Ước lượng nguyên dương cho lá, tối đa 100.000 phút. Không nhập tay ở cha.",
        ["tienDo"] = "0–100; trạng thái do server xác định. Cha lấy trung bình con trực tiếp.",
        ["huyPhienLienQuan"] = "Chỉ true sau khi đã xem ảnh hưởng và xác nhận hủy các phiên Planned bị tác động.",
        ["thuTrongTuan"] = "1 = thứ Hai, 7 = Chủ nhật.",
        ["gioBatDau"] = "Giờ địa phương theo múi giờ tài khoản, ví dụ 19:00:00.",
        ["gioKetThuc"] = "Phải sau giờ bắt đầu trong cùng ngày.",
        ["tokenPreview"] = "Phiếu gợi ý do server ký, không được sửa nội dung, có hạn 10 phút.",
        ["ma"] = "Mã bản ghi.",
        ["ten"] = "Tên bắt buộc, tối đa 200 ký tự.",
        ["ngayBatDau"] = "Ngày bắt đầu học kỳ, dạng yyyy-MM-dd.",
        ["ngayKetThuc"] = "Ngày kết thúc phải từ ngày bắt đầu trở đi, dạng yyyy-MM-dd.",
        ["kyHieu"] = "Mã môn tùy chọn, tối đa 50 ký tự.",
        ["soTinChi"] = "Số tín chỉ tùy chọn; nếu có phải lớn hơn 0.",
        ["giangVien"] = "Giảng viên tùy chọn, tối đa 200 ký tự.",
        ["mauSac"] = "Màu tùy chọn, đúng định dạng #RRGGBB; phản hồi viết hoa.",
        ["soMonHoc"] = "Số môn học sẽ bị xóa.",
        ["soBuoiHoc"] = "Số mẫu lịch học sẽ bị xóa, không phải số lần xuất hiện theo tuần.",
        ["soNhiemVuBoLienKet"] = "Số nhiệm vụ giữ lại và bỏ liên kết tương ứng.",
        ["id"] = "Mã bản ghi trên đường dẫn.",
        ["tenTruong"] = "Tên trường tự nhập; có thể null, tối đa 200 ký tự.",
        ["tenLop"] = "Tên lớp tự nhập; có thể null, tối đa 200 ký tự.",
        ["email"] = "Địa chỉ email hợp lệ; không trùng tài khoản khác sau khi chuẩn hóa.",
        ["taiKhoan"] = "Email hoặc mã sinh viên của tài khoản. Giữ số 0 đầu mã sinh viên.",
        ["hoTen"] = "Họ và tên người dùng; bắt buộc, tối đa 200 ký tự.",
        ["matKhau"] = "Mật khẩu tạo tài khoản, dài 8–128 ký tự.",
        ["maSoSinhVien"] = "Mã sinh viên; có thể null. Khi có giá trị phải duy nhất trong đơn vị triển khai.",
        ["nganhHoc"] = "Ngành học; có thể null, tối đa 200 ký tự.",
        ["dangHoatDong"] = "true = hoạt động; false = vô hiệu hóa.",
        ["vaiTro"] = "Student = sinh viên; Admin = quản trị.",
        ["muiGio"] = "Múi giờ tài khoản.",
        ["nhacTruocGio"] = "Số giờ nhắc trước hạn mặc định.",
        ["ngayTao"] = "Thời điểm tạo do hệ thống ghi, theo UTC.",
        ["ngayCapNhat"] = "Thời điểm cập nhật gần nhất do hệ thống ghi, theo UTC.",
        ["tuKhoa"] = "Từ khóa tìm kiếm, có thể bỏ trống.",
        ["trang"] = "Số trang, bắt đầu từ 1; mặc định 1.",
        ["kichThuocTrang"] = "Số bản ghi mỗi trang, từ 1 đến 100; mặc định 20.",
        ["danhSach"] = "Danh sách bản ghi của trang hiện tại.",
        ["tongSo"] = "Tổng số bản ghi phù hợp bộ lọc.",
        ["trangThai"] = "Trạng thái hoạt động của API.",
        ["kiemTraLuc"] = "Thời điểm kiểm tra theo UTC."
    };

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        // Tránh gán nghĩa trường CRUD cho các kiểu khác, ví dụ status của ProblemDetails là mã HTTP.
        if (context.Type.Namespace != "StudyPlanner.Application.Administration" &&
            context.Type.Namespace != "StudyPlanner.Application.Auth" &&
            context.Type.Namespace != "StudyPlanner.Application.HocTap" &&
            context.Type.Namespace != "StudyPlanner.Application.Academic" &&
            context.Type != typeof(Controllers.LivenessResponse)) return;
        if (schema.Properties is null) return;
        foreach (var (name, value) in schema.Properties)
            if (value is OpenApiSchema property && Fields.TryGetValue(name, out var description))
                property.Description = description;
        // Cùng tên field nhưng chỉ DangKyRequest bắt buộc đủ hồ sơ; CRUD/response legacy vẫn nullable.
        if (context.Type == typeof(StudyPlanner.Application.Auth.DangKyRequest))
        {
            foreach (var name in new[] { "maSoSinhVien", "tenTruong", "tenLop", "nganhHoc" })
                if (schema.Properties.TryGetValue(name, out var value) && value is OpenApiSchema property)
                    property.Description = name == "maSoSinhVien"
                        ? "Bắt buộc, tối đa 50 ký tự; duy nhất trong hệ thống, giữ số 0 đầu."
                        : "Bắt buộc nhập, tối đa 200 ký tự; không nhận chuỗi trắng.";
            ((OpenApiSchema)schema).Example = System.Text.Json.Nodes.JsonNode.Parse("""
                {"email":"sinhvien01@example.test","hoTen":"Sinh viên thử","matKhau":"DemoPass2026!","maSoSinhVien":"0306000001","tenTruong":"Trường mẫu","tenLop":"CNTT01","nganhHoc":"Công nghệ thông tin"}
                """);
        }
        if (context.Type == typeof(StudyPlanner.Application.Auth.DangNhapRequest))
            ((OpenApiSchema)schema).Example = System.Text.Json.Nodes.JsonNode.Parse("""
                {"taiKhoan":"0306000001","matKhau":"DemoPass2026!"}
                """);
        if (context.Type.Namespace == "StudyPlanner.Application.HocTap")
        {
            if (schema.Properties.TryGetValue("trangThai",out var state) && state is OpenApiSchema stateSchema)
                stateSchema.Description = "Trạng thái nghiệp vụ; nhiệm vụ NotStarted/InProgress/Completed, phiên Planned/InProgress/Completed/Missed/Cancelled.";
            if (context.Type == typeof(StudyPlanner.Application.HocTap.NhiemVuRequest))
                ((OpenApiSchema)schema).Example = System.Text.Json.Nodes.JsonNode.Parse("""
                    {"tieuDe":"Hoàn thiện ERD","moTa":"Đối chiếu quan hệ và ràng buộc","loai":"Assignment","mucDoUuTien":"High","hanHoanThanh":"2026-11-10T23:59:00+07:00","thoiLuongDuKienPhut":120}
                    """);
        }
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var routes = context.ApiDescription.ActionDescriptor.RouteValues;
        var controller = routes.TryGetValue("controller", out var controllerName) ? controllerName ?? "" : "";
        var action = routes.TryGetValue("action", out var actionName) ? actionName ?? "" : "";
        if (typeof(HocTap.HocTapControllerBase).IsAssignableFrom(context.MethodInfo.DeclaringType))
        {
            var method = context.ApiDescription.HttpMethod ?? "GET";
            operation.Summary = action switch
            {
                "DanhSach" => "Xem danh sách", "ChiTiet" => "Xem chi tiết", "Them" => "Tạo mới", "Sua" => "Sửa dữ liệu", "Xoa" => "Xóa có kiểm tra ảnh hưởng",
                "Cay" => "Xem nhánh nhiệm vụ", "ThemCon" => "Thêm nhóm nhiệm vụ con", "DoiCha" => "Chuyển hoặc tách nhiệm vụ cha", "DoiPhamVi" => "Đổi học kỳ/môn của cả cây",
                "TienDo" => "Cập nhật tiến độ nhiệm vụ lá", "HoanThanh" => "Hoàn thành nhiệm vụ lá", "MoLai" => "Mở lại nhiệm vụ lá", "XemThayDoi" or "XemXoa" or "XemAnhHuong" => "Xem ảnh hưởng trước khi thay đổi",
                "XemTruoc" => "Gợi ý lịch tự học, chưa ghi phiên", "ApDung" => "Áp dụng các phiên đã chọn", "BatDau" => "Bắt đầu phiên", "KetThuc" => "Kết thúc phiên, không tự hoàn thành nhiệm vụ",
                "BoLo" => "Xác nhận bỏ lỡ phiên", "Huy" => "Hủy phiên dự kiến", _ => "Xem thời khóa biểu"
            };
            operation.Description = "Chỉ thao tác dữ liệu của tài khoản đăng nhập. Response có phienBan/duLieu và ETag; lỗi dùng ProblemDetails. " +
                (method == "PUT" ? "PUT gửi đầy đủ DTO; thiếu trường tùy chọn sẽ bỏ giá trị. " : "") +
                (controller == "LapLich" ? "Preview không ghi DB. Apply kiểm tra lại hạn, giờ rảnh, lịch bận, thời lượng và phiên bản; Idempotency-Key chống ghi lặp." : "Thay đổi ảnh hưởng phiên học yêu cầu xem trước/xác nhận; lỗi rollback toàn bộ.");
            var preview = action is "XemTruoc" or "XemThayDoi" or "XemXoa" or "XemAnhHuong";
            if (method != "GET" && !preview && !(controller == "LapLich" && action == "ApDung"))
            {
                operation.Parameters ??= [];
                if (!operation.Parameters.Any(p => p.Name == "If-Match")) operation.Parameters.Add(new OpenApiParameter { Name="If-Match",In=ParameterLocation.Header,Required=true,Description="ETag đọc gần nhất, ví dụ \"ht-0\". Thiếu trả 428, cũ trả 409.",Schema=new OpenApiSchema { Type=JsonSchemaType.String } });
            }
            operation.Responses ??= [];
            foreach (var status in new[] { "400","401","404","409","428" })
                operation.Responses.TryAdd(status,new OpenApiResponse { Description=status == "428" ? "Thiếu If-Match." : "Lỗi có code/traceId trong ProblemDetails." });
        }
        if (controller == "XacThuc")
        {
            operation.Summary = action == "DangKy" ? "Đăng ký tài khoản sinh viên" : "Đăng nhập";
            operation.Description = action == "DangKy"
                ? "Nhập đủ họ tên, email, mật khẩu, mã sinh viên, trường, lớp và ngành."
                : "Nhập email hoặc mã sinh viên vào taiKhoan và nhập matKhau.";
            operation.Security = [];
        }
        if (controller == "HoSo")
        {
            operation.Summary = action switch { "XemHoSo" => "Xem hồ sơ của tôi", "SuaHoSo" => "Sửa hồ sơ của tôi", _ => "Đổi mật khẩu" };
            if (action == "SuaHoSo")
            {
                // PATCH nhận JsonElement để phân biệt thiếu field với null. Đặt mẫu riêng cho thao tác này,
                // tránh gắn mẫu hồ sơ vào mọi API khác cũng dùng JsonElement.
                if (operation.RequestBody?.Content is not null)
                    foreach (var media in operation.RequestBody.Content.Values)
                        if (media is OpenApiMediaType content)
                            content.Example = System.Text.Json.Nodes.JsonNode.Parse("""
                                {"hoTen":"Sinh viên cập nhật","tenTruong":"Trường mẫu","tenLop":"CNTT02","nganhHoc":"Công nghệ thông tin"}
                                """);
            }
        }
        var resource = controller switch
        {
            "NguoiDung" => "người dùng", _ => ""
        };
        if (controller is "Semesters" or "Courses")
        {
            var name = controller == "Semesters" ? "học kỳ" : "môn học";
            operation.Summary = action switch
            {
                "List" => "Danh sách " + name, "Get" => "Chi tiết " + name,
                "Create" => "Tạo " + name, "Update" => "Sửa " + name,
                "Delete" => "Xóa " + name, "Preview" => "Xem ảnh hưởng trước khi xóa " + name,
                "ListCourses" => "Danh sách môn trong học kỳ", "CreateCourse" => "Thêm môn vào học kỳ", _ => operation.Summary
            };
            operation.Description = action switch
            {
                "Preview" => "Chỉ dữ liệu thuộc người gọi. Preview không khóa dữ liệu; DELETE kiểm tra lại tại thời điểm xóa.",
                "Delete" => "Giao dịch giữ nhiệm vụ/cây/hạn/tiến độ và ghi lịch sử bỏ liên kết; xóa lịch và tài nguyên. Xóa môn giữ liên kết học kỳ, xóa học kỳ bỏ cả hai liên kết. Trả 204 không body.",
                "Update" when controller == "Courses" => "Không hỗ trợ chuyển học kỳ; gửi maHocKy hoặc trường ngoài DTO sẽ trả 400.",
                _ => "Bearer JWT; chỉ dữ liệu của người gọi. ID không tồn tại hoặc không thuộc người gọi đều trả 404."
            };
        }
        if (resource.Length > 0)
        {
            operation.Summary = action switch
            {
                "DanhSach" => $"Xem danh sách {resource}", "ChiTiet" => $"Xem chi tiết {resource}",
                "Them" => $"Thêm {resource}", "Sua" => $"Sửa {resource}",
                "Xoa" => $"Xóa {resource}", "KichHoat" => "Kích hoạt tài khoản", "VoHieuHoa" => "Vô hiệu hóa tài khoản", _ => null
            };
            operation.Description = action switch
            {
                "Sua" => "Gửi đầy đủ trường muốn giữ, gồm dangHoatDong; bỏ trường tùy chọn sẽ xóa giá trị.",
                "Xoa" => "Xóa hẳn tài khoản và dữ liệu liên quan. Không xóa Admin hoạt động cuối cùng.",
                _ => null
            };
        }
        else if (controller == "Health")
        {
            operation.Summary = "Kiểm tra API còn hoạt động";
            operation.Description = "Healthy nghĩa là API trả lời được; không kiểm tra kết nối PostgreSQL.";
        }

        if (operation.Parameters is not null)
            foreach (var value in operation.Parameters)
                if (value is OpenApiParameter parameter && parameter.Name is not null && Fields.TryGetValue(parameter.Name, out var description))
                    parameter.Description = parameter.Name.Equals("dangHoatDong", StringComparison.OrdinalIgnoreCase)
                        ? "true = hoạt động; false = vô hiệu hóa; bỏ trống = tất cả." : description;

        if (operation.Responses is not null)
            foreach (var (code, value) in operation.Responses)
                if (value is OpenApiResponse response)
                    response.Description = code switch
                    {
                        "200" => "Thành công.",
                        "201" => "Tạo thành công.",
                        "204" => "Thành công; không có nội dung trả về.",
                        "400" => "Dữ liệu hoặc định dạng tham số không hợp lệ.",
                        "404" => "Không tìm thấy dữ liệu.",
                        "409" => "Dữ liệu trùng, đang được tham chiếu hoặc xung đột cập nhật.",
                        _ => response.Description
                    };
    }
}
