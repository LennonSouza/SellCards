﻿using Newtonsoft.Json;
using SteamBot.SteamWebBot.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace SellCards.Functions {
    class GetNumberItems {
        public static int itemsMarketable = 0;

        public static void Get(SteamWebBotAccount account) {

            string Referer = $"https://steamcommunity.com/inventory/{account.SteamGuard.Session.SteamID}/753/6?l=brazilian";

            var response = new RequestBuilder(Referer)
                .GET()
                .AddCookies(account.SteamGuard)
                .Execute();

            if (response.IsSuccessful) {
                Root invDeseralize = JsonConvert.DeserializeObject<Root>(response.Content);

                //Se não houver items comercializavel
                if (invDeseralize.assets == null || invDeseralize.assets.Count == Program.config.Not_Marketable) {
                    Logger.info("Empty inventory or non-marketable items!");
                    return;
                }

                itemsMarketable = invDeseralize.total_inventory_count - Program.config.Not_Marketable;
                Logger.info($"Available items: {itemsMarketable}");

                foreach (var item in invDeseralize.assets) {

                    var nome = invDeseralize.descriptions.Where(c => c.classid == item.classid).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(nome.ToString())) {
                        if (nome.marketable == 1) {

                            if (Program.config.AppIDBlackList.Contains(nome.market_fee_app.ToString())) continue;

                            //Foil Card
                            if (nome.tags[2].internal_name == "cardborder_1" && Program.config.FoilCard) {

                                if (Program.config.Network) {
                                    GetIdentificationItem.Get(account, nome, item.assetid);
                                } else {
                                    GetNumberItemsMemory.Get(account, nome, item.assetid);
                                }
                            }
                            //Normal Card
                            if (nome.tags[2].internal_name == "cardborder_0" && Program.config.NormalCard) {

                                if (Program.config.Network) {
                                    GetIdentificationItem.Get(account, nome, item.assetid);
                                } else {
                                    GetNumberItemsMemory.Get(account, nome, item.assetid);
                                }                                
                            }
                        }
                    }
                }
            }
        }

        public class Asset {
            public int appid { get; set; }
            public string contextid { get; set; }
            public string assetid { get; set; }
            public string classid { get; set; }
            public string instanceid { get; set; }
            public string amount { get; set; }
        }

        public class Description {
            public string value { get; set; }
            public string color { get; set; }
            public int appid { get; set; }
            public string classid { get; set; }
            public string instanceid { get; set; }
            public int currency { get; set; }
            public string background_color { get; set; }
            public string icon_url { get; set; }
            public string icon_url_large { get; set; }
            public List<Description> descriptions { get; set; }
            public int tradable { get; set; }
            public List<OwnerAction> owner_actions { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public string market_name { get; set; }
            public string market_hash_name { get; set; }
            public int market_fee_app { get; set; }
            public int commodity { get; set; }
            public int market_tradable_restriction { get; set; }
            public int market_marketable_restriction { get; set; }
            public int marketable { get; set; }
            public List<Tag> tags { get; set; }
            public string name_color { get; set; }
            public DateTime? item_expiration { get; set; }
        }

        public class OwnerAction {
            public string link { get; set; }
            public string name { get; set; }
        }

        public class Tag {
            public string category { get; set; }
            public string internal_name { get; set; }
            public string localized_category_name { get; set; }
            public string localized_tag_name { get; set; }
        }

        public class Root {
            public List<Asset> assets { get; set; }
            public List<Description> descriptions { get; set; }
            public int more_items { get; set; }
            public string last_assetid { get; set; }
            public int total_inventory_count { get; set; }
            public int success { get; set; }
            public int rwgrsn { get; set; }
        }
    }
}
