using Microsoft.EntityFrameworkCore;
using StudyPlanner.Infrastructure.Data;
using Microsoft.OpenApi;
using StudyPlanner.Api.Administration;
using StudyPlanner.Api.Auth;
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
    options.TagActionsBy(api => new[] { VietnameseSwagger.Group(
        api.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller) ? controller ?? "" : "") });
    options.OperationFilter<VietnameseSwagger>();
    options.SchemaFilter<VietnameseSwagger>();
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
builder.Services.AddAdministration();
builder.Services.AddStudyPlannerAuth(builder.Configuration, builder.Environment);



var app = builder.Build();

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
