using StudyPlanner.Application.Academic;

namespace StudyPlanner.Api.Academic;

public static class AcademicRegistration
{
    public static IServiceCollection AddAcademic(this IServiceCollection services)
    {
        services.AddScoped<AcademicService>();
        return services;
    }
}
