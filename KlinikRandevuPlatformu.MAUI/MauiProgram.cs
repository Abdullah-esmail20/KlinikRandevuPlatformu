
using Microsoft.Extensions.Logging;

namespace KlinikRandevuPlatformu.MAUI
{
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

            return builder.Build();



            builder.Services.AddSingleton(sp =>
            {
                var http = new HttpClient();
                // ✅ غيّر البورت حسب Swagger عندك
                http.BaseAddress = new Uri("https://localhost:7153");
                return http;
            });

    

            // Pages (DI)
            builder.Services.AddSingleton<Pages.LoginPage>();
           
        }
    }
}
