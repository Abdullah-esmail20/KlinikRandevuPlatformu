using KlinikRandevuPlatformu.MAUI.Pages;

namespace KlinikRandevuPlatformu.MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // صفحات مسجلة للتنقل
        Routing.RegisterRoute("patientHome", typeof(Pages.CitySelectPage));
        Routing.RegisterRoute("clinics", typeof(Pages.ClinicsPage));
        Routing.RegisterRoute("register", typeof(Pages.RegisterPage));
    }
}