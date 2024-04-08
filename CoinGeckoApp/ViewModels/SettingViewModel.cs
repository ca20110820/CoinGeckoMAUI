using CoinGeckoApp.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.ViewModels
{
    public class SettingViewModel : INotifyPropertyChanged
    {
        private const string applicationName = "CoinGeckoMAUIApp";

        private UserSettingModel _userSetting = new();
        public UserSettingModel UserSetting
        {
            get => _userSetting;
            set
            {
                _userSetting = value;
                OnPropertyChanged(nameof(UserSetting));
            }
        }

        private List<string> _supportedCurrencies = new List<string>();
        public List<string> SupportedCurrencies
        {
            get => _supportedCurrencies;
            set
            {
                _supportedCurrencies = value;
                OnPropertyChanged(nameof(SupportedCurrencies));
            }
        }

        private List<string> _exchangeIds = new List<string>();
        public List<string> ExchangeIds
        {
            get => _exchangeIds;
            set
            {
                _exchangeIds = value;
                OnPropertyChanged(nameof(ExchangeIds));
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
