using Microsoft.EntityFrameworkCore;
using StudyPlanner.Infrastructure.Data;
using Microsoft.OpenApi;
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
        Version = "v1",
        Description = "API quản lý kế hoạch và tiến độ học tập."
    });
});

// Lấy chuỗi kết nối từ appsettings.Development.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Đăng ký ApplicationDbContext kết nối đến PostgreSQL trên Neon
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

//đăng ký controller (dky các dịch vụ hỗ trợ controller)
builder.Services.AddControllers();



var app = builder.Build();

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

//ánh xạ các route được khai báo trong controller
app.MapControllers(); //(đưa các route trong controller vào pipeline xử lý request)

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/", () => Results.Ok(new
{
    message = "StudyPlanner API is running",
    status = "Healthy"
}));

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
