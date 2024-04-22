using CoinGeckoApp.Models;
using CoinGeckoApp.ViewModels;

namespace CoinGeckoApp;

public partial class FavouritesPage : ContentPage
{
	private FavouritesViewModel viewModel;

	public FavouritesPage()
	{
		InitializeComponent();
		viewModel = new(this);
		BindingContext = viewModel;
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await viewModel.GetFavouriteCoins();
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