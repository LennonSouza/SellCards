using Newtonsoft.Json.Linq;
using SteamBot.SteamWebBot.Account;
using System;
using System.Net;
using System.Threading;
using static SellCards.Functions.GetNumberItems;

namespace SellCards.Functions {
    class GetSellItem {
        public static void Get(SteamWebBotAccount account, Description nome, string valueFinish, string assetid) {
           
            var response = new RequestBuilder("https://steamcommunity.com/market/sellitem/")
                    .POST()
                    .AddHeader(HttpRequestHeader.Referer, $"https://steamcommunity.com/profiles/{account.SteamGuard.Session.SteamID}/inventory/")
                    .AddPOSTParam("sessionid", account.SteamGuard.Session.SessionID)
                    .AddPOSTParam("appid", "753")
                    .AddPOSTParam("contextid", "6")
                    .AddPOSTParam("assetid", assetid)
                    .AddPOSTParam("amount", "1")
                    .AddPOSTParam("price", valueFinish.ToString())
                    .AddCookies(account.SteamGuard)
                    .Execute();

            bool confirmation = false;
            try {
                JObject json = JObject.Parse(response.Content);
                if (json.GetValue("success").Value<JValue>().Value is bool status && status == true) {
                    confirmation = true;
                    Logger.info("Ad created successfully!");
                    Program.anuncioFail = 0;
                } else if (json.GetValue("success").Value<JValue>().Value is bool statu && statu == false) {
                    Logger.info("Ad not created!");
                    Program.anuncioFail++;
                    Thread.Sleep(TimeSpan.FromSeconds(1));

                    if (Program.anuncioFail > 50) {
                        Logger.error("Too many ads failed!");
                        Logger.error("I'll try again in 10 minutes!");
                        Thread.Sleep(TimeSpan.FromMinutes(10));
                        Program.anuncioFail = 0;
                        GetNumberItems.Get(account);
                    } 

                } else {
                    Logger.error("I don't know the error!");
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));

            } catch (Exception) {

                Logger.error("Error reading Json");
            }

            if (confirmation == true) {
                SteamAuth.Confirmation[] confirmations = account.SteamGuard.FetchConfirmations();

                var responseTrade = account.SteamGuard.AcceptMultipleConfirmations(confirmations);
                Logger.info($"Mobile Confirmation = {responseTrade.ToString().Replace("True", "Success").Replace("False", "Fail")} - {Program.countAnuncio}");

            }
        }
    }
}
