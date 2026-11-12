using StudyPlanner.Api.CanhBao;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Infrastructure.Data;
using Microsoft.OpenApi;
using StudyPlanner.Api.Administration;
using StudyPlanner.Api.Auth;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Api.Academic;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//đăng ký dịch vụ sinh tài liệu OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Study Planner API",
        Version = "v1"
    });
    options.TagActionsBy(api => new[] { MoTaSwagger.Group(
        api.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller) ? controller ?? "" : "") });
    options.OperationFilter<MoTaSwagger>();
    options.SchemaFilter<MoTaSwagger>();
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT",
        Description = "Nhập tokenTruyCap từ API đăng nhập."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// Lấy chuỗi kết nối từ appsettings.Development.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Đăng ký ApplicationDbContext kết nối đến PostgreSQL trên Neon
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

//đăng ký controller (dky các dịch vụ hỗ trợ controller)
builder.Services.AddQuanTri();
builder.Services.AddHocTap();
builder.Services.AddThongBao();
builder.Services.AddAcademic();
builder.Services.AddStudyPlannerXacThuc(builder.Configuration, builder.Environment);



var app = builder.Build();
// Lệnh chủ động tại máy phát triển; không có endpoint công khai để tự cấp Admin.
var bootstrapEmail = builder.Configuration["promote-admin"];
if (!string.IsNullOrWhiteSpace(bootstrapEmail))
{
    if (!app.Environment.IsDevelopment()) throw new InvalidOperationException("Chỉ khởi tạo Admin trong Development.");
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
    if (await db.DanhSachNguoiDung.AnyAsync(x => x.VaiTro == StudyPlanner.Domain.Enums.VaiTroNguoiDung.Admin))
        throw new InvalidOperationException("Đã có Admin; không dùng lại lệnh khởi tạo.");
    var normalized = bootstrapEmail.Trim().ToUpperInvariant();
    var user = await db.DanhSachNguoiDung.SingleOrDefaultAsync(x => x.EmailChuanHoa == normalized && x.DangHoatDong)
        ?? throw new InvalidOperationException("Cần đăng ký tài khoản hoạt động trước.");
    user.VaiTro = StudyPlanner.Domain.Enums.VaiTroNguoiDung.Admin; user.NgayCapNhat = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync(); await transaction.CommitAsync();
    Console.WriteLine("Đã khởi tạo Admin. Chạy lại API bình thường và đăng nhập.");
    return;
}
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Study Planner API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

//ánh xạ các route được khai báo trong controller
app.MapControllers(); //(đưa các route trong controller vào pipeline xử lý request)

app.Run();
