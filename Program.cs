using Newtonsoft.Json;
using SellCards.Functions;
using SellCards.Models;
using SellCards.SteamBot.Models;
using SteamBot.SteamWebBot.Account;
using System;
using System.Collections.Generic;
using System.IO;

namespace SellCards {
    class Program {

        public static string api_key = "";
        public static string login = "";
        public static string password = "";

        public static int notMarketable = 0;
        public static int anuncioFail = 0;
        public static int countAnuncio = 1;

        public static bool normalCard = false;
        public static bool foilCard = false;
        public static bool network = false;
        public static bool foilPrice = false;

        public static List<string> appIDBlackList = new List<string>();

        public static Dictionary<string, MafileProcessingModel> allMafiles = MafilesProcessing.GetAllMafiles();
        public static string[] allAccounts = File.ReadAllLines(@"Config\Accs.txt");
        static void Main(string[] args) {
            Console.Title = $"SellCards -- AccountsLoad: {allAccounts.Length} -- MaFilesLoad: {allMafiles.Count}";

            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(@"Config\Config.json"));
            api_key = config.Api_Key;
            notMarketable = config.Not_Marketable;
            normalCard = config.NormalCard;
            foilCard = config.FoilCard;
            network = config.Network;
            appIDBlackList = config.AppIDBlackList;
            foilPrice = config.FoilPrice;

            var counter = 0;
            foreach (var acc in allAccounts) {
                try {
                    countAnuncio = 1;
                    var accSpl = acc.Split(':');
                    login = accSpl[0].ToLower();
                    password = accSpl[1];

                    Logger.info("Processing {0}. {1}/{2}", login, ++counter, allAccounts.Length);
                    if (!allMafiles.ContainsKey(login)) {
                        Logger.error(login + " mafile not found");
                        continue;
                    }

                    var account = new SteamWebBotAccount(login, password, allMafiles[login]);

                    GetNumberItems.Get(account);

                } catch (Exception e) {
                    Logger.error($"Error: {e.ToString()}");
                }
            }
            Logger.info("All Done");
            Console.ReadKey();
        }
    }
}
