using CoinGeckoApp.CoinLoreAPI.Core;

namespace CoinGeckoApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            webviewTopPrices.Source = new HtmlWebViewSource
            {
                Html = WidgetHTML.GenerateHtmlPriceTickerWidget()
            };

            webviewCoinList.Source = new HtmlWebViewSource
            {
                Html = WidgetHTML.GenerateHtmlCryptoListWidget(dataTop:100)
            };
        }
    }
}
