using CoinGeckoApp.Models;
using CoinGeckoApp.ViewModels;

namespace CoinGeckoApp;

public partial class FavouritesPage : ContentPage
{
	private FavouritesViewModel viewModel;
    
    bool isInitNavigated = false;

    public FavouritesPage()
	{
		InitializeComponent();
		viewModel = new(this);
		BindingContext = viewModel;
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!isInitNavigated)
        {
            refreshviewFavouritesPage.IsRefreshing = true;

            await RefreshUI();
            isInitNavigated = true;

            refreshviewFavouritesPage.IsRefreshing = false;
        }
    }

    private async void refreshviewFavouritesPage_Refreshing(object sender, EventArgs e)
    {
        await RefreshUI();
    }

    private async Task RefreshUI()
    {
        refreshviewFavouritesPage.IsRefreshing = true;

        try
        {
            await viewModel.GetFavouriteCoins();
        }
        catch (HttpRequestException err)
        {
            if (err.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                // Too many HTTP Requests
                await DisplayAlert("Warn", "Too many requests! Please wait for a moment.", "Ok");
            }
            else
            {
                // No Internet Connection error
                await DisplayAlert("Warn", "No Internet Connection!", "Ok");
                await Shell.Current.GoToAsync("//MainPage");  // Route to Home Page
            }
        }

        refreshviewFavouritesPage.IsRefreshing = false;
    }

    private async void imagebtnStar_Clicked(object sender, EventArgs e)
    {
        ImageButton imgBtn = (ImageButton)sender;
        FavouriteCoinModel selectedItem = (FavouriteCoinModel)imgBtn.BindingContext;
        string coinId = selectedItem?.Coin?.Id ?? string.Empty;
        await viewModel.RemoveCoinFromFavourites(coinId);
    }

    private async void imagebuttonCoinLogo_Clicked(object sender, EventArgs e)
    {
        ImageButton imgBtn = (ImageButton)sender;
        FavouriteCoinModel selectedItem = (FavouriteCoinModel)imgBtn.BindingContext;
        string coinId = selectedItem?.Coin?.Id ?? string.Empty;

        await Shell.Current.GoToAsync($"//CoinPage",
            new Dictionary<string, object>
            {
                ["ParamCoinId"] = coinId,
            }
            );
    }
}