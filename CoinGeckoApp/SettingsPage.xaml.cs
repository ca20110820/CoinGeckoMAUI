using CoinGeckoApp.Settings;
using CoinGeckoApp.Models;
using CoinGeckoApp.ViewModels;
using System.Diagnostics;

namespace CoinGeckoApp;

public partial class SettingsPage : ContentPage
{
    private UserSettingModel userSetting = new();
    private const int _maxFavourites = 15;

    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        //UserSettingModel userSetting = new();
        //try
        //{
        //    settingViewModel.UserSetting = await userSetting.ReadAsync();
        //}
        //catch (KeyNotFoundException ex)
        //{
        //    await DisplayAlert("Error", ex.Message, "Ok");
        //}

        // Get the Preference Values and Set them to the Widgets
        //await RetreiveMaxFavourites();
        await RetreiveDarkMode();
        await RetreiveQuoteCurrency();
        await RetreiveSupportedCurrencies();
    }

    private async Task RetreiveDarkMode()
    {
        switchDarkMode.IsToggled = Preferences.Get("darkmode", false);
    }
    private async Task RetreiveQuoteCurrency()
    {
        // Get the App Singleton
        App theApp = (App)Application.Current;

        // Load the Supported Currencies
        pickerQuoteCurrency.ItemsSource = null;
        pickerQuoteCurrency.ItemsSource = theApp.SupportedCurrencies;

        // Set the User Preference for Supported Currencies
        pickerQuoteCurrency.SelectedItem = Preferences.Get("quotecurrency", "usd");
    }
    private async Task RetreiveSupportedCurrencies()
    {

        // Get the App Singleton
        App theApp = (App)Application.Current;

        // Load the Exchange IDs
        pickerExchangeID.ItemsSource = null;
        pickerExchangeID.ItemsSource = theApp.ExchangeIds;
        pickerExchangeID.SelectedItem = Preferences.Get("exchangeid", "binance");
    }

    private async void switchDarkMode_Toggled(object sender, ToggledEventArgs e)
    {
        bool newValue = e.Value;

        await userSetting.ReadAsync();
        await userSetting.SwitchDarkMode();

        //Trace.WriteLine($"Persistent DarkMode Value: {userSetting.DarkMode}");
        //Trace.WriteLine($"Current DarkMode Value: {newValue}");

        Trace.Assert(userSetting.DarkMode == newValue);
    }

    private async void pickerQuoteCurrency_SelectedIndexChanged(object sender, EventArgs e)
    {
        Picker picker = (Picker)sender;
        if (picker == null) return;
        string selectedCurrency = (string)picker.SelectedItem;

        await userSetting.ReadAsync();
        await userSetting.ChangeQuoteCurrencyTo(selectedCurrency);
    }

    private async void imagebtnRefreshQuoteCurrency_Clicked(object sender, EventArgs e)
    {
        // Rotate the image
        await imagebtnRefreshQuoteCurrency.RotateTo(360, 1000); // Rotate to 360 degrees in 1 second
        imagebtnRefreshQuoteCurrency.Rotation = 0; // Reset rotation

        List<string>? supportedCurrencies;
        try
        {
            supportedCurrencies = await Task.Run(() => SettingBase.FetchSupportedCurrenciesAsync());
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert("Warn", ex.Message, "Ok");
            return;
        }

        if (supportedCurrencies == null) return;
        
        await SettingBase.WriteUpdateSettingAsync("supported_currencies", supportedCurrencies);

        // Get the App Singleton
        App theApp = (App)Application.Current;

        theApp.SupportedCurrencies = supportedCurrencies;
    }

    private async void pickerExchangeID_SelectedIndexChanged(object sender, EventArgs e)
    {
        Picker picker = (Picker)sender;
        if (picker == null) return;
        string selectedExchangeId = (string)picker.SelectedItem;

        await userSetting.ReadAsync();
        await userSetting.ChangeExchangeIdTo(selectedExchangeId);
    }

    private async void imagebtnRefreshExchangeIds_Clicked(object sender, EventArgs e)
    {
        // Rotate the image
        await imagebtnRefreshExchangeIds.RotateTo(360, 1000); // Rotate to 360 degrees in 1 second
        imagebtnRefreshExchangeIds.Rotation = 0; // Reset rotation

        List<string>? latestExchangeIds;
        try
        {
            latestExchangeIds = await Task.Run(() => ExchangeModel.GetExchangeIds());
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert("Warn", ex.Message, "Ok");
            return;
        }

        if (latestExchangeIds == null) return;

        await SettingBase.WriteUpdateSettingAsync("exchangeids", latestExchangeIds);

        // Get the App Singleton
        App theApp = (App)Application.Current;

        theApp.ExchangeIds = latestExchangeIds;
    }

    private async void btnResetUserSettings_Clicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;

        // Add Button Effects by Scale
        await button.ScaleTo(0.9, 100); // Scale to 0.9 times in 100 milliseconds
        await button.ScaleTo(1, 100); // Scale back to normal size in 100 milliseconds

        // Reset User Settings back to Default
        await ResetUserSettings();
    }

    private async Task ResetUserSettings()
    {
        // Try and Read the Settings from config.json, if exist and available
        UserSettingModel userSetting = new();

        userSetting = await userSetting.ReadAsync();
        await userSetting.ResetSettingAsync();

        // Reset Widget Values
        switchDarkMode.IsToggled = userSetting.DarkMode;
        pickerQuoteCurrency.SelectedItem = userSetting.QuoteCurrency;
        pickerExchangeID.SelectedItem = userSetting.ExchangeId;
    }
}