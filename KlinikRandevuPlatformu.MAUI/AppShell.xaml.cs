using KlinikRandevuPlatformu.MAUI.Pages;

namespace KlinikRandevuPlatformu.MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // صفحات مسجلة للتنقل
        Routing.RegisterRoute("register", typeof( LoginPage));
      
    }
}