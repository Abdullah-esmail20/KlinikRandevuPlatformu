using KlinikRandevuPlatformu.MAUI.Models;
using KlinikRandevuPlatformu.MAUI.Services;

namespace KlinikRandevuPlatformu.MAUI.Pages;

public partial class LoginPage : ContentPage
{
    private readonly ApiClient _api;
    private readonly TokenStore _token;

    public LoginPage(ApiClient api, TokenStore token)
    {
        InitializeComponent();
        _api = api;
        _token = token;
    }

    private async void OnHealthClicked(object sender, EventArgs e)
    {
        try
        {
            var res = await _api.GetAsync<HealthResponse>("/api/health");
            await DisplayAlert("API OK", res?.status ?? "null", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("API FAIL", ex.Message, "OK");
        }
    }

       //  <!-- ✅ هذا الزر اللي ينقلك للتسجيل -->
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("register");
    }
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            var u = UsernameEntry.Text?.Trim() ?? "";
            var p = PasswordEntry.Text ?? "";

            if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(p))
            {
                await DisplayAlert("Error", "Username & Password required", "OK");
                return;
            }

            var res = await _api.PostAsync<LoginResponse>("/api/auth/login", new LoginRequest(u, p));
            if (res is null) throw new Exception("Empty response");

            _token.Save(res.token, res.user.role);
            await DisplayAlert("OK", $"Logged in as {res.user.role}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Login Failed", ex.Message, "OK");
        }




    }
}