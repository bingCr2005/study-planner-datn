using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Application.Administration;
using StudyPlanner.Infrastructure.Administration;

namespace StudyPlanner.Api.Administration;

public static class QuanTriRegistration
{
    public static IServiceCollection AddQuanTri(this IServiceCollection services)
    {
        services.AddControllers().AddApplicationPart(typeof(QuanTriRegistration).Assembly)
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters
                .Add(new JsonStringEnumConverter(allowIntegerValues: false)));
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problem = new ValidationProblemDetails(context.ModelState)
                {
                    Status = 400, Title = "Dữ liệu không hợp lệ", Instance = context.HttpContext.Request.Path
                };
                problem.Extensions["code"] = "VALIDATION_ERROR";
                problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                var result = new BadRequestObjectResult(problem);
                result.ContentTypes.Add("application/problem+json");
                return result;
            };
        });
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IQuanLyNguoiDungStore, EfQuanLyNguoiDungStore>();
        services.AddScoped<INguoiDungPasswordHasher, IdentityNguoiDungPasswordHasher>();
        services.AddScoped<QuanLyNguoiDungService>();
        services.AddProblemDetails();
        services.AddExceptionHandler<QuanTriExceptionHandler>();
        return services;
    }
}
