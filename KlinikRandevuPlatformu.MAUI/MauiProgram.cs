
using KlinikRandevuPlatformu.MAUI.Services;
using Microsoft.Extensions.Logging;

namespace KlinikRandevuPlatformu.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

   

        builder.Services.AddSingleton(sp =>
        {
            var http = new HttpClient();
            http.BaseAddress = new Uri("https://localhost:7153"); // غيّر البورت إذا مختلف
            return http;
        });

        builder.Services.AddSingleton<ApiClient>();
        // ✅ سجل الخدمات قبل Build
        builder.Services.AddSingleton<TokenStore>();
        builder.Services.AddSingleton<Pages.CitySelectPage>();
        builder.Services.AddTransient<Pages.ClinicsPage>();

        // ✅ سجل الصفحات (إذا تستخدم DI داخل الصفحات)
        builder.Services.AddSingleton<Pages.LoginPage>();
        builder.Services.AddTransient<Pages.RegisterPage>();

        // ✅ هذا لازم يكون آخر سطر
        return builder.Build();
    }
}