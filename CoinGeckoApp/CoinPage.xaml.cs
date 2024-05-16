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

        refreshviewCoinPage.IsRefreshing = true;
        await UpdateWhenTabNavigation();
        refreshviewCoinPage.IsRefreshing = false;
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
            viewModel.SetIsFavouriteImage();  // Update Star Image for Favourite State
            await RefreshQuickCharts();  // Update QuickCharts UI
        }
        catch (HttpRequestException err)
        {
            if (err.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                await DisplayAlert("Warn", "Too many requests! Please wait for few seconds!", "Ok");
                viewModel.ResetProperties();
            }
        }
    }

    private async void refreshviewCoinPage_Refreshing(object sender, EventArgs e)
    {
        refreshviewCoinPage.IsRefreshing = true;

        try
        {
            await viewModel.SetCoin(viewModel.Coin);  // Re-set the current Coin to fetch the latest data
            await UpdateWhenTabNavigation();
        }
        catch(NullReferenceException)
        {
            await DisplayAlert("Error", "There is no selected coin!", "Ok");
        }
        catch (HttpRequestException err)
        {
            if (err.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                await DisplayAlert("Warn", "Too many requests! Please wait for few seconds!", "Ok");
            }
        }

        refreshviewCoinPage.IsRefreshing = false;
    }

    private async void imagebtnRefreshCoinData_Clicked(object sender, EventArgs e)
	{
        // Rotate the image
        await imagebtnRefreshCoinData.RotateTo(360, 1000); // Rotate to 360 degrees in 1 second
        imagebtnRefreshCoinData.Rotation = 0; // Reset rotation

        await RefreshQuickCharts();  // Update QuickCharts UI
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

    private async void searchbarCoin_Pressed(object sender, EventArgs e)
    {
        searchResults.ItemsSource = null;  // Clear Search Results
        string coinId = searchbarCoin.Text.Trim();  // Get SearchBar Coin Id

        // Create a Coin
        CoinModel searchCoin = new(coinId);
        await searchCoin.IsFavouriteAsync();
        
        try
        {
            await viewModel.SetCoin(searchCoin);
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                await DisplayAlert("Warn", "The Coin Id you are searching does not exist.", "Ok");
            }
            if (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                await DisplayAlert("Warn", "Too many requests. Please wait for few seconds.", "Ok");
            }
        }
        finally
        {
            searchbarCoin.Text = string.Empty;
        }
    }

    private async void searchbarCoin_TextChanged(object sender, EventArgs e)
    {
        string currentTextState = searchbarCoin.Text;

        //searchResults.ItemsSource = null;
        //searchResults.ItemsSource = new string[] { "A", "B", "C" };
    }
}
