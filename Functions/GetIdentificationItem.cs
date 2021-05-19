﻿using Newtonsoft.Json;
using SteamBot.SteamWebBot.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SellCards.Functions.GetNumberItems;

namespace SellCards.Functions {
    class GetIdentificationItem {


        public static void Get(SteamWebBotAccount account, Description nome) {
            string URL = $"http://steamcommunity.com/market/priceoverview/?currency=34&appid=753&market_hash_name={nome.market_hash_name}";

            var responseItem = new RequestBuilder(URL).GET()
                .AddPOSTParam("appid", "753")
                .AddPOSTParam("currency", "34")
                .AddPOSTParam("market_hash_name", nome.market_hash_name)
                .AddCookies(account.SteamGuard)
                .Execute();

            Root marketDeseralize = JsonConvert.DeserializeObject<Root>(responseItem.Content);

            string filter = String.Join("", Regex.Split(marketDeseralize.lowest_price, @"[^\d.,]"));
            decimal menorValor = Convert.ToDecimal(filter);

            if (menorValor > 0 && !string.IsNullOrWhiteSpace(menorValor.ToString())) {

            }

        }

        public class Root {
            public bool success { get; set; }
            public string lowest_price { get; set; }
        }
    }
}
