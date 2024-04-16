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
        base.OnAppearing();

        try
        {
            await viewModel.ShowTickers();
        }
        catch (HttpRequestException ex)
        {
            // No Internet Connection error
            await DisplayAlert("Warn", "No Internet Connection!", "Ok");
            await Shell.Current.GoToAsync("//MainPage");  // Route to Home Page
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
    }
}