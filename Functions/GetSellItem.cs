using Newtonsoft.Json.Linq;
using SteamBot.SteamWebBot.Account;
using System;
using System.Net;
using System.Threading;
using static SellCards.Functions.GetNumberItems;

namespace SellCards.Functions {
    class GetSellItem {
        public static void Get(SteamWebBotAccount account, Description nome, string valueFinish, string assetid) {

            bool confirmation = false;
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

            if (response.Content.Contains("There was a problem listing your item. Refresh the page and try again")) {
                Logger.error("Steam sucks: There was a problem listing your item!");
                return;
            }

            if (response.Content.Contains("You already have a listing for this item pending confirmation. Please confirm or cancel the existing listing")) {
                Logger.error("I will confirm your item!");
                confirmation = true;
                _2faConfirmation.Get(account, confirmation);
                return;
            }

            try {
                JObject json = JObject.Parse(response.Content);
                if (json.GetValue("success").Value<JValue>().Value is bool status && status == true) {
                    confirmation = true;
                    if (nome.tags[2].internal_name == "cardborder_1") {
                        Logger.info($"Ad created successfully! - ARS$ {GetNumberItemsMemory.menorValor.ToString("F2")} - {Program.countAnuncio++}/{GetNumberItems.itemsMarketable}", ConsoleColor.DarkGreen);
                    } else {
                        Logger.info($"Ad created successfully! - ARS$ {GetNumberItemsMemory.menorValor.ToString("F2")} - {Program.countAnuncio++}/{GetNumberItems.itemsMarketable}");
                    }
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

            if (confirmation && Program.countAnuncio > Program.countConfirmation) {
                _2faConfirmation.Get(account, confirmation);
                Program.countConfirmation += 5;
            }

            if (confirmation && Program.countAnuncio == GetNumberItems.itemsMarketable) {
                _2faConfirmation.Get(account, confirmation);
            }
        }
    }
}
