using KlinikRandevuPlatformu.MAUI.Models;
using KlinikRandevuPlatformu.MAUI.Services;

namespace KlinikRandevuPlatformu.MAUI.Pages;

[QueryProperty(nameof(City), "city")]
public partial class ClinicsPage : ContentPage
{
    private readonly ApiClient _api;
    public string City { get; set; } = "";

    public ClinicsPage(ApiClient api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var list = await _api.GetAsync<List<ClinicListItem>>($"/api/clinics?city={Uri.EscapeDataString(City)}");
            ClinicsView.ItemsSource = list ?? new();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnSelected(object sender, SelectionChangedEventArgs e)
    {
        ClinicsView.SelectedItem = null;
        var item = e.CurrentSelection.FirstOrDefault() as ClinicListItem;
        if (item is null) return;

        await DisplayAlert("Selected", $"{item.clinicName}", "OK");
        // لاحقًا نفتح ClinicDetails + Services + Booking
    }
}