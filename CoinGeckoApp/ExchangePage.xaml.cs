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

        await viewModel.ShowTickers();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
    }
}