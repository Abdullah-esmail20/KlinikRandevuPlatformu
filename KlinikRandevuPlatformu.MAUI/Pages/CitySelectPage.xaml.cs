namespace KlinikRandevuPlatformu.MAUI.Pages;

public partial class CitySelectPage : ContentPage
{
    public CitySelectPage()
    {
        InitializeComponent();

        CityPicker.ItemsSource = new List<string>
        {
            "Istanbul", "Ankara", "Bartin", "Isparta"
        };

        CityPicker.SelectedIndex = 0;
    }

    private async void OnShowClinics(object sender, EventArgs e)
    {
        var city = CityPicker.SelectedItem?.ToString() ?? "";
        await Shell.Current.GoToAsync($"clinics?city={Uri.EscapeDataString(city)}");
    }
}