using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StudyPlanner.Application.HocTap;
using StudyPlanner.Infrastructure.HocTap;
namespace StudyPlanner.Api.HocTap;
public static class HocTapRegistration
{
    public static IServiceCollection AddHocTap(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IHocTapStore, EfHocTapStore>(); services.AddScoped<HocTapDieuPhoi>();
        services.AddScoped<NhiemVuService>(); services.AddScoped<LichHocService>(); services.AddScoped<KhungGioRanhService>();
        services.AddScoped<ThoiKhoaBieuService>(); services.AddScoped<QuyTacDatPhien>(); services.AddScoped<LapLichService>(); services.AddScoped<PhienTuHocService>();
        services.AddSingleton<IPreviewLapLichToken>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>(); var env = sp.GetRequiredService<IHostEnvironment>();
            var secret = config["Planning:SigningKey"] ?? config["Auth:SigningKey"];
            if (string.IsNullOrEmpty(secret) && !env.IsDevelopment()) throw new InvalidOperationException("Cần khóa runtime cho Planning:SigningKey hoặc Auth:SigningKey.");
            if (!string.IsNullOrEmpty(secret) && Encoding.UTF8.GetByteCount(secret) < 32) throw new InvalidOperationException("Khóa preview cần ít nhất 32 byte.");
            // Domain separation: never use raw JWT signing material for the preview protocol.
            var key = string.IsNullOrEmpty(secret) ? RandomNumberGenerator.GetBytes(64) : HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret),Encoding.UTF8.GetBytes("StudyPlanner/planning-preview/v1"));
            return new HmacPreviewLapLichToken(key);
        });
        services.AddControllers(o => o.ModelBinderProviders.Insert(0,new MocThoiGianModelBinderProvider()))
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new MocThoiGianJsonConverter()));
        return services;
    }
}
