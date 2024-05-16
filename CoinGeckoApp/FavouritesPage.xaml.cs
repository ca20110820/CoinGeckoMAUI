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
            await RefreshUI();
            isInitNavigated = true;
        }
    }

    private async void refreshviewFavouritesPage_Refreshing(object sender, EventArgs e)
    {
        await RefreshUI();
    }

    private async Task RefreshUI()
    {
        refreshviewFavouritesPage.IsRefreshing = true;
        await viewModel.GetFavouriteCoins();
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