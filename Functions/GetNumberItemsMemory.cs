using Newtonsoft.Json;
using SteamBot.SteamWebBot.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SellCards.Functions.GetNumberItems;

namespace SellCards.Functions {
    class GetNumberItemsMemory {
        public static void Get(SteamWebBotAccount account, Description nome, string assetid) {

            string espaco = nome.market_hash_name.Replace(" ", "%20").Replace("?", "%3F");
            string URLtest = $"https://steamcommunity.com/market/listings/753/{espaco}";
            var responseItemtest = new RequestBuilder(URLtest).GET()
                .AddCookies(account.SteamGuard)
                .Execute();

            if (!responseItemtest.Content.Contains("ItemActivityTicker.Start(")) {
                Logger.error("Error finding the codeID");
                return;
            }

            int inicio = responseItemtest.Content.IndexOf("ItemActivityTicker.Start(");
            int fim = responseItemtest.Content.IndexOf(");", inicio);
            string postid = responseItemtest.Content.Substring(inicio, fim - inicio);
            string item_nameid = String.Join("", Regex.Split(postid, @"[^\d]"));

            string URLtests = $"https://steamcommunity.com/market/itemordershistogram?country=AR&language=english&currency=34&item_nameid={item_nameid}";
            var responseItemtests = new RequestBuilder(URLtests).GET()
                .AddCookies(account.SteamGuard)
                .Execute();

            Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(responseItemtests.Content);

            List<object> prices = myDeserializedClass.sell_order_graph[0];

            string price = prices[0].ToString();
            decimal menorValor = Convert.ToDecimal(price);

            //Valor a receber - 13%
            string _13 = (menorValor / 100 * 87).ToString("F2");
            string valueFinish = String.Join("", Regex.Split(_13, @"[^\d]"));

            if (menorValor > 0 && !string.IsNullOrWhiteSpace(menorValor.ToString())) {
                GetSellItem.Get(account, nome, valueFinish, assetid);
            }
        }

        public class Root {
            public int success { get; set; }
            public string sell_order_table { get; set; }
            public string sell_order_summary { get; set; }
            public string buy_order_table { get; set; }
            public string buy_order_summary { get; set; }
            public string highest_buy_order { get; set; }
            public string lowest_sell_order { get; set; }
            public List<List<object>> buy_order_graph { get; set; }
            public List<List<object>> sell_order_graph { get; set; }
            public int graph_max_y { get; set; }
            public double graph_min_x { get; set; }
            public double graph_max_x { get; set; }
            public string price_prefix { get; set; }
            public string price_suffix { get; set; }
        }
    }
}
