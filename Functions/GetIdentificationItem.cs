using Newtonsoft.Json;
using SteamBot.SteamWebBot.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SellCards.Functions.GetNumberItems;

namespace SellCards.Functions {
    class GetIdentificationItem {


        public static void Get(SteamWebBotAccount account, Description nome, string assetid) {

            string espaco = nome.market_hash_name.Replace(" ", "%20").Replace("?", "%3F");
            string URL = $"http://steamcommunity.com/market/priceoverview/?currency=34&appid=753&market_hash_name={espaco}";

            var responseItem = new RequestBuilder(URL).GET()
                .AddPOSTParam("appid", "753")
                .AddPOSTParam("currency", "34")
                .AddPOSTParam("market_hash_name", nome.market_hash_name)
                .AddCookies(account.SteamGuard)
                .Execute();

            Root marketDeseralize = JsonConvert.DeserializeObject<Root>(responseItem.Content);

            string filter = String.Join("", Regex.Split(marketDeseralize.lowest_price, @"[^\d,.]"));
            decimal menorValor = Convert.ToDecimal(filter);

            //Valor a receber - 13%
            string _13 = (menorValor / 100 * 87).ToString("F2");
            string valueFinish = String.Join("", Regex.Split(_13, @"[^\d]"));

            if (menorValor > 0 && !string.IsNullOrWhiteSpace(menorValor.ToString())) {
                GetSellItem.Get(account, nome, valueFinish, assetid);
            } 
        }

        public class Root {
            public bool success { get; set; }
            public string lowest_price { get; set; }
        }
    }
}
