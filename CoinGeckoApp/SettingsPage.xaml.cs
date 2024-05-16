using CoinGeckoApp.Settings;
using CoinGeckoApp.Models;
using CoinGeckoApp.ViewModels;
using System.Diagnostics;
using CoinGeckoApp.Responses.Exchanges;
using CoinGeckoApp.Services;
using CoinGeckoApp.Helpers;

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

        refreshviewSettingsPage.IsRefreshing = true;
        RetreiveAllSettings();
        refreshviewSettingsPage.IsRefreshing = false;
    }

    public void refreshviewSettingsPage_Refreshing(object sender, EventArgs e)
    {
        refreshviewSettingsPage.IsRefreshing = true;
        RetreiveAllSettings();
        refreshviewSettingsPage.IsRefreshing = false;
    }

    private void RetreiveAllSettings()
    {
        // Get the Preference Values and Set them to the Widgets
        RetreiveDarkMode();
        RetreiveQuoteCurrency();
        RetreiveSupportedCurrencies();
    }
    private void RetreiveDarkMode()
    {
        switchDarkMode.IsToggled = Preferences.Get("darkmode", false);
    }
    private void RetreiveQuoteCurrency()
    {
        // Get the App Singleton
        App theApp = (App)Application.Current;

        // Load the Supported Currencies
        pickerQuoteCurrency.ItemsSource = null;
        pickerQuoteCurrency.ItemsSource = theApp?.SupportedCurrencies ?? null;

        // Set the User Preference for Supported Currencies
        pickerQuoteCurrency.SelectedItem = Preferences.Get("quotecurrency", "usd");
    }
    private void RetreiveSupportedCurrencies()
    {

        // Get the App Singleton
        App theApp = (App)Application.Current;

        // Load the Exchange IDs
        pickerExchangeID.ItemsSource = null;
        pickerExchangeID.ItemsSource = theApp?.ExchangeIds ?? null;
        pickerExchangeID.SelectedItem = Preferences.Get("exchangeid", "binance");
    }

    private async void switchDarkMode_Toggled(object sender, ToggledEventArgs e)
    {
        App theApp = (App)Application.Current;

        try
        {
            await userSetting.ReadAsync();
            await userSetting.SwitchDarkMode();

            if (userSetting.DarkMode)
            {
                theApp.UserAppTheme = AppTheme.Dark;
            }
            else
            {
                theApp.UserAppTheme = AppTheme.Light;
            }

        }
        catch (KeyNotFoundException)
        {
            // Set User Preference to default due to un-initialized user preferences when the app is installed
            await userSetting.ResetSettingAsync();
            await userSetting.ReadAsync();
            await userSetting.SwitchDarkMode();
        }
    }

    private async void pickerQuoteCurrency_SelectedIndexChanged(object sender, EventArgs e)
    {
        Picker picker = (Picker)sender;
        if (picker == null) return;
        string selectedCurrency = (string)picker.SelectedItem;

        try
        {
            await userSetting.ReadAsync();
            await userSetting.ChangeQuoteCurrencyTo(selectedCurrency);
        }
        catch(KeyNotFoundException ex)
        {
            Trace.WriteLine(ex);
        }
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

        try
        {
            await userSetting.ReadAsync();
            await userSetting.ChangeExchangeIdTo(selectedExchangeId);
        }
        catch(KeyNotFoundException ex)
        {
            Trace.WriteLine(ex);
        }
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
        try
        {
            await ResetUserSettings();
        }
        catch (Newtonsoft.Json.JsonReaderException err)
        {
            await Task.Delay(2000);
            await DisplayAlert("Error", err.Message, "Ok");
        }
    }

    private async Task ResetUserSettings()
    {
        UserSettingModel userSetting = new();

        // Try and Read the Settings from config.json, if exist and available
        try
        {
            userSetting = await userSetting.ReadAsync();
        }
        catch (KeyNotFoundException ex)
        {
            Trace.WriteLine($"We failed to initialized or set the settings due to connection failure!\n{ex}");
            return;
        }

        await userSetting.ResetSettingAsync();

        // Reset Widget Values
        switchDarkMode.IsToggled = userSetting.DarkMode;
        pickerQuoteCurrency.SelectedItem = userSetting.QuoteCurrency;
        pickerExchangeID.SelectedItem = userSetting.ExchangeId;
    }

    private async void btnCleanData_Clicked(object sender, EventArgs e)
    {
        bool result = await DisplayAlert("Confirmation", "Are you sure you want to proceed?", "Yes", "No");

        if (!result) return;

        FileSystemHelper fsHelper = new();
        try
        {
            Directory.Delete(fsHelper.AppDataDir, true);
            Directory.Delete(fsHelper.CacheDir, true);
        }
        catch (DirectoryNotFoundException) {}
    }
}