using CoinGeckoApp.Models;
using CoinGeckoApp.Responses.Exchanges;
using CoinGeckoApp.Services;
using CoinGeckoApp.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace CoinGeckoApp;

public partial class ExchangePage : ContentPage
{
    private ExchangeService exchangeService = new();
    private ExchangeViewModel viewModel;

    public ExchangePage()
	{
		InitializeComponent();
        viewModel = new(this);  // Pass the ExchangePage instance to the ViewModel class
        BindingContext = viewModel;   // Bind the ViewModel to this page's BindingContext
    }

    protected override async void OnAppearing()
    {
        activityindivatorLoading.IsRunning = true;
        activityindivatorLoading.IsVisible = true;
        base.OnAppearing();

        RetreiveExchangeIds();

        try
        {
            // Always reset back to the default user preferences for the exchange id (when not arguments given to ShowTickers())
            await Task.Run(() => viewModel.ShowTickers());
        }
        catch (HttpRequestException ex)
        {
            // No Internet Connection error
            await DisplayAlert("Warn", "No Internet Connection!", "Ok");
            await Shell.Current.GoToAsync("//MainPage");  // Route to Home Page
        }
        finally
        {
            activityindivatorLoading.IsRunning = false;
            activityindivatorLoading.IsVisible = false;
        }

    }

    private void RetreiveExchangeIds()
    {
        App theApp = (App)Application.Current;
        pickerExchangeId.ItemsSource = null;
        pickerExchangeId.ItemsSource = theApp.ExchangeIds;
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
    }

    private async void OnCoinTapped(object sender, TappedEventArgs e)
    {
        Ticker selectedTicker;
        if (sender is Image)
        {
            Image widget = (Image)sender;
            selectedTicker = (Ticker)widget.BindingContext;
        }
        else
        {
            Label widget = (Label)sender;
            selectedTicker = (Ticker)widget.BindingContext;
        }

        if (selectedTicker == null) return;

        await Shell.Current.GoToAsync($"//CoinPage",
            new Dictionary<string, object>
            {
                ["ParamCoinId"] = selectedTicker.CoinId,
            }
            );
    }
}