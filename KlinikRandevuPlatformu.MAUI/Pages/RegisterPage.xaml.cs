using KlinikRandevuPlatformu.MAUI.Services;
using KlinikRandevuPlatformu.Shared.DTOs.Auth;
using KlinikRandevuPlatformu.Shared.Enums;

namespace KlinikRandevuPlatformu.MAUI.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly ApiClient _api;

    public RegisterPage(ApiClient api)
    {
        InitializeComponent();
        _api = api;

        RolePicker.ItemsSource = new List<string> { "Owner", "Patient" };
        RolePicker.SelectedIndex = 1;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
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

            var roleStr = RolePicker.SelectedItem?.ToString() ?? "Patient";
            var role = roleStr == "Owner" ? UserRole.Owner : UserRole.Patient;

            var req = new RegisterRequest
            {
                Username = u,
                Password = p,
                Role = role
            };

            await _api.PostAsync<object>("/api/auth/register", req);

            await DisplayAlert("OK", "Registered successfully", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Register Failed", ex.Message, "OK");
        }
    }
}