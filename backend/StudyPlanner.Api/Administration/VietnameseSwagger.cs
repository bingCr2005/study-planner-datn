using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace StudyPlanner.Api.Administration;

// Chỉ bổ sung tài liệu OpenAPI; không đổi route, tên trường JSON hay nghiệp vụ.
public sealed class VietnameseSwagger : IOperationFilter, ISchemaFilter
{
    public static string Group(string controller) => controller switch
    {
        "Users" => "Quản lý người dùng",
        "Health" => "Kiểm tra hoạt động API",
        "Auth" => "Đăng ký và đăng nhập",
        "Profile" => "Hồ sơ cá nhân",
        _ => "API mẫu và hệ thống"
    };

    private static readonly Dictionary<string, string> Fields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ma"] = "Mã bản ghi.",
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
            context.Type != typeof(Controllers.LivenessResponse)) return;
        if (schema.Properties is null) return;
        foreach (var (name, value) in schema.Properties)
            if (value is OpenApiSchema property && Fields.TryGetValue(name, out var description))
                property.Description = description;
        // Cùng tên field nhưng chỉ RegisterRequest bắt buộc đủ hồ sơ; CRUD/response legacy vẫn nullable.
        if (context.Type == typeof(StudyPlanner.Application.Auth.RegisterRequest))
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
        if (context.Type == typeof(StudyPlanner.Application.Auth.LoginRequest))
            ((OpenApiSchema)schema).Example = System.Text.Json.Nodes.JsonNode.Parse("""
                {"taiKhoan":"0306000001","matKhau":"DemoPass2026!"}
                """);
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var routes = context.ApiDescription.ActionDescriptor.RouteValues;
        var controller = routes.TryGetValue("controller", out var controllerName) ? controllerName ?? "" : "";
        var action = routes.TryGetValue("action", out var actionName) ? actionName ?? "" : "";
        if (controller == "Auth")
        {
            operation.Summary = action == "Register" ? "Đăng ký tài khoản sinh viên" : "Đăng nhập";
            operation.Description = action == "Register"
                ? "Nhập đủ họ tên, email, mật khẩu, mã sinh viên, trường, lớp và ngành."
                : "Nhập email hoặc mã sinh viên vào taiKhoan và nhập matKhau.";
            operation.Security = [];
        }
        if (controller == "Profile")
        {
            operation.Summary = action switch { "Me" => "Xem hồ sơ của tôi", "Update" => "Sửa hồ sơ của tôi", _ => "Đổi mật khẩu" };
            if (action == "Update")
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
            "Users" => "người dùng", _ => ""
        };
        if (resource.Length > 0)
        {
            operation.Summary = action switch
            {
                "List" => $"Xem danh sách {resource}", "Get" => $"Xem chi tiết {resource}",
                "Create" => $"Thêm {resource}", "Update" => $"Sửa {resource}",
                "Delete" => $"Xóa {resource}", "Activate" => "Kích hoạt tài khoản", "Deactivate" => "Vô hiệu hóa tài khoản", _ => null
            };
            operation.Description = action switch
            {
                "Update" => "Gửi đầy đủ trường muốn giữ, gồm dangHoatDong; bỏ trường tùy chọn sẽ xóa giá trị.",
                "Delete" => "Xóa hẳn tài khoản và dữ liệu liên quan. Không xóa Admin hoạt động cuối cùng.",
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
