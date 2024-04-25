using CoinGeckoApp.Models;
using CoinGeckoApp.ViewModels;

namespace CoinGeckoApp;

[QueryProperty("ParamCoinId", "ParamCoinId")]
public partial class CoinPage : ContentPage, IQueryAttributable
{
    private CoinViewModel viewModel = new();
    public string ParamCoinId {  get; set; }

	public CoinPage()
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await UpdateWhenTabNavigation();
    }
    protected override async void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        // When this ContentPage is the Source
        base.OnNavigatedFrom(args);
    }
    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        // When this ContentPage is the Target
        base.OnNavigatedTo(args);
    }

    private async Task UpdateWhenTabNavigation()
    {
        // Note: This is a very important method when navigating from Pages by tapping or clicking the Tab.

        // Check and Update View State - Coin's Favourite State
        if (viewModel.Coin != null)
        {
            await viewModel.Coin.IsFavouriteAsync();
            viewModel.SetIsFavouriteImage();

            // Update QuickCharts
            await RefreshQuickCharts();
        }
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        /* Implementation for IQueryAttributable interface */
        Dictionary<string, object> dct = (Dictionary<string, object>)query;  // Convert IDictionary to Dictionary
        string id = (string)dct["ParamCoinId"];  // Extract the Navigation Route Parameter
        CoinModel coin = new CoinModel(id);

        // Check and Update if this coin is a Favourite or Not (from SQLite)
        await coin.IsFavouriteAsync();

        try
        {
            // Set the selected CoinModel in CoinViewModel
            await Task.Run(() => viewModel.SetCoin(coin));

            await RefreshQuickCharts();  // Update QuickCharts UI
        }
        catch (HttpRequestException tooManyRequestError)
        {
            await DisplayAlert("Warn", "Too many requests! Please wait for few seconds!", "Ok");
            viewModel.ResetProperties();
        }
    }

    private async void imagebtnRefreshCoinData_Clicked(object sender, EventArgs e)
	{
        // Rotate the image
        await imagebtnRefreshCoinData.RotateTo(360, 1000); // Rotate to 360 degrees in 1 second
        imagebtnRefreshCoinData.Rotation = 0; // Reset rotation

        await RefreshQuickCharts();
    }

    private async Task RefreshQuickCharts()
    {
        // Refresh ImageSources for QuickCharts UI
        try
        {
            await viewModel.SetImages();  // Load QuickCharts

        }
        catch (NullReferenceException)
        {
            await DisplayAlert("Error", "There is no selected coin!", "Ok");
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                await DisplayAlert("Warn", "Too many http requests!", "Ok");
            }
        }
    }

    private async void imagebtnStar_Clicked(object sender, EventArgs e)
    {
        try
        {
            await viewModel.ChangeFavouriteState();
        }
        catch (NullReferenceException ex)
        {
            await DisplayAlert("Error", "There is no selected coin!", "Ok");
        }
        catch(InvalidCastException invalidCast)
        {
        }
    }
}
