using CoinGeckoApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Services
{
    public class ExchangeService
    {

        public ExchangeModel Exchange {  get; set; }

        public ExchangeService() { }
        public ExchangeService(ExchangeModel exchange)
        {
            Exchange = exchange;
        }


    }
}
