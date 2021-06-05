using Newtonsoft.Json;
using SellCards.Functions;
using SellCards.Models;
using SellCards.SteamBot.Models;
using SteamBot.SteamWebBot.Account;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SellCards {
    class Program {

        public static string BasePath = AppDomain.CurrentDomain.BaseDirectory;
        public static Config config = new Config();

        public static string login = "";
        public static string password = "";

        public static int anuncioFail = 0;
        public static int countAnuncio = 0;
        public static int countConfirmation = 0;
        public static decimal totalCards = 0;

        public static Dictionary<string, MafileProcessingModel> allMafiles = new Dictionary<string, MafileProcessingModel>();
        public static string[] allAccounts = null;
        static void Main(string[] args) 
        {

            CheckDiretoryOnStartup();
            allMafiles =  MafilesProcessing.GetAllMafiles();
            allAccounts = File.ReadAllLines(@"Config\Accs.txt");
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(@"Config\Config.json"));

            Console.Title = $"SellCards -- AccountsLoad: {allAccounts.Length} -- MaFilesLoad: {allMafiles.Count}";

            var counter = 0;
            foreach (var acc in allAccounts) {
                try {
                    countAnuncio = 1;
                    countConfirmation = 10;
                    var accSpl = acc.Split(':');
                    login = accSpl[0].ToLower();
                    password = accSpl[1];

                    Logger.info($"Processing {login}. {++counter}/{allAccounts.Length}");
                    Console.Title = $"[{counter}/{allAccounts.Length}] -- SellCards -- AccountsLoad: {allAccounts.Length} -- MaFilesLoad: {allMafiles.Count}";
                    if (!allMafiles.ContainsKey(login)) {
                        Logger.error(login + " mafile not found");
                        continue;
                    }

                    var account = new SteamWebBotAccount(login, password, allMafiles[login]);

                    GetNumberItems.Get(account);

                    Thread.Sleep(TimeSpan.FromSeconds(10));
                    _2faConfirmation.Get(account, true);

                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    _2faConfirmation.Get(account, true);

                    if (totalCards > 0) {
                        Logger.info($"Total value: ARS$ {((totalCards / 100 * 87) + Convert.ToDecimal(0.01)).ToString("F2")}", ConsoleColor.DarkBlue);
                    } else {
                        Logger.info($"Total value: ARS$ {totalCards}", ConsoleColor.DarkBlue);
                    }
                    totalCards = 0;
                } catch (Exception e) {
                    Logger.error($"Error: {e.ToString()}");
                }
            }
            Logger.info("All Done");
            Console.ReadKey();
        }

        public static void CheckDiretoryOnStartup()
        {
            string configfolder = Path.Combine(BasePath, "config");

            if (!Directory.Exists(configfolder))
            {
                Directory.CreateDirectory(configfolder);
            }

            string mafilefolder = Path.Combine(configfolder, "mafiles");

            if (!Directory.Exists(mafilefolder))
            {
                Directory.CreateDirectory(mafilefolder);
            }

            string accsFilePath = Path.Combine(configfolder, "accs.txt");

            if (!File.Exists(accsFilePath))
            {
                File.WriteAllText(accsFilePath, "login:pass");
            }

            string configfilepath = Path.Combine(configfolder,"Config.json");

            if (!File.Exists(configfilepath))
            {
                Config config = new Config();
                System.IO.File.WriteAllText(configfilepath, JsonConvert.SerializeObject(config, Formatting.Indented));
            }
        }
    }
}
