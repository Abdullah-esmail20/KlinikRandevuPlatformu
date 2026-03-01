//using KlinikRandevuPlatformu.MAUI.Services;
//using KlinikRandevuPlatformu.MAUI.Models;

//namespace KlinikRandevuPlatformu.MAUI.Pages;

//public partial class LoginPage : ContentPage
//{
//    private readonly ApiClient _api;
//    private readonly TokenStore _token;

//    public LoginPage(ApiClient api, TokenStore token)
//    {
//        InitializeComponent();
//        _api = api;
//        _token = token;
//    }

//    private async void OnHealthClicked(object sender, EventArgs e)
//    {
//        try
//        {
//            var res = await _api.GetAsync<HealthResponse>("/api/health");
//            await DisplayAlert("API OK", res?.status ?? "null", "OK");
//        }
//        catch (Exception ex)
//        {
//            await DisplayAlert("API FAIL", ex.Message, "OK");
//        }
//    }

//    private async void OnLoginClicked(object sender, EventArgs e)
//    {
//        try
//        {
//            var u = UsernameEntry.Text?.Trim() ?? "";
//            var p = PasswordEntry.Text ?? "";

//            if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(p))
//            {
//                await DisplayAlert("Error", "Username & Password required", "OK");
//                return;
//            }

//            var res = await _api.PostAsync<LoginResponse>("/api/auth/login", new LoginRequest(u, p));
//            if (res is null) throw new Exception("Empty response");

//            _token.Save(res.token, res.user.role);
//            await DisplayAlert("OK", $"Logged in as {res.user.role}", "OK");
//        }
//        catch (Exception ex)
//        {
//            await DisplayAlert("Login Failed", ex.Message, "OK");
//        }
//    }

//    private async void OnRegisterClicked(object sender, EventArgs e)
//    {
//        await Shell.Current.GoToAsync("register");
//    }
//}

using KlinikRandevuPlatformu.MAUI.Services;

namespace KlinikRandevuPlatformu.MAUI.Pages
{

    public partial class LoginPage : ContentPage
    {
        private readonly IAuthService _authService;

        public LoginPage()
        {
            InitializeComponent();
            _authService = new AuthService(); // في المراحل المتقدمة سنستخدم Dependency Injection
        }

        private async void OnTestApiClicked(object sender, EventArgs e)
        {
            bool isConnected = await _authService.TestHealthAsync();

            if (isConnected)
            {
                await DisplayAlert("نجاح! 🎉", "تطبيق MAUI متصل بالسيرفر (API) بنجاح!", "ممتاز");
            }
            else
            {
                await DisplayAlert("خطأ ❌", "لا يمكن الاتصال بالسيرفر. تأكد أن الـ API يعمل (Run).", "حسناً");
            }
        }
    }
}