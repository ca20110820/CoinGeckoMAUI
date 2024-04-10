using CoinGeckoApp.Models;
using CoinGeckoApp.Responses.Exchanges;
using CoinGeckoApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace CoinGeckoApp;

public partial class ExchangePage : ContentPage
{
    private ExchangeService exchangeService = new();

    public ExchangePage()
	{
		InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await RetreiveExchangeData();
    }

    private async Task RetreiveExchangeData()
    {
        App theApp = (App)Application.Current;

        // Set the Title based from Exchange ID User Preference
        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        string exchangeId = Preferences.Get("exchangeid", "binance");
        exchangeId = textInfo.ToTitleCase(exchangeId.ToLower());  // Warn: It capitalizes the first character
        labelExchangeId.Text = exchangeId;  // Set to binance if there is some error

        // Re-instantiate the Exchange Service with the current state of "exchangeid"
        exchangeService = new(new ExchangeModel(Preferences.Get("exchangeid", "binance")));  // Get the exchange service based on current state of exchangeid

        // Try to get the Tickers from Json DB if available; Otherwise Fetch from API then Save.
        List<Ticker>? tickers = new List<Ticker>();

        // Check if App's ExchangeTickers is not null
        if (theApp.ExchangeTickers != null)
        {
            Trace.WriteLine("Getting Tickers from App");
            collectionviewTickers.ItemsSource = null;
            collectionviewTickers.ItemsSource = theApp.ExchangeTickers;
            return;
        }

        // Execute the following if theApp.ExchangeTickers is null
        try
        {
            Trace.WriteLine("Trying to get Tickers from API ...");
            APIExchangeIdTickersResponse?  apiResponse = await Task.Run(() => exchangeService.FetchExchangeTickers());
            if (apiResponse == null) await DisplayPopUpAndRouteToPage("MainPage", "Warning", "API Reponse is null!");
            if (apiResponse.Tickers == null) await DisplayPopUpAndRouteToPage("MainPage", "Warning", "The Tickers from API Response is null!");

            theApp.ExchangeTickers = apiResponse.Tickers;  // Set the App's ExchangeTickers property

            // Update the CollectionView UI
            collectionviewTickers.ItemsSource = null;
            collectionviewTickers.ItemsSource = theApp.ExchangeTickers;
        }
        catch (HttpRequestException e)  // Error: Too Many Http Requests
        {
            await DisplayPopUpAndRouteToPage("MainPage", "Warning", "Too many requests! Please wait for few seconds in the Home Page.");
            return;
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
    }

    private async Task DisplayPopUpAndRouteToPage(string routeToPage, string title, string message, string userResponse = "Ok")
    {
        await DisplayAlert(title, message, userResponse);
        await Shell.Current.GoToAsync($"//{routeToPage}");
    }
}