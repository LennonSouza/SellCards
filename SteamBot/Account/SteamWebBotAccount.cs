using System.IO;
using System.Text.RegularExpressions;
using global::SteamBot.SteamWebBot.Utils;
using SellCards.Models;
using Newtonsoft.Json;
using SteamAuth;
using SellCards;

namespace SteamBot.SteamWebBot.Account
{

    public class SteamWebBotAccount
    {
        public SteamWebBotAccount(string username, string password, MafileProcessingModel maFile)
        {
            this.Username = username;
            this.Password = password;
            this.SteamGuard = JsonConvert.DeserializeObject<SteamGuardAccount>(maFile.MafileContent);

            if (SteamGuard.IsSessionExpired)
            {
                Logger.info($"Session for {username} is expired. Refreshing session.");
                this.SteamGuard.Login(username, password);
                File.WriteAllText(maFile.MafilePath, JsonConvert.SerializeObject(this.SteamGuard));
            }
        }

        public string ApiKey { get; set; }

        public string Nickname { get; set; }

        public SteamGuardAccount SteamGuard { get; }

        public SteamID SteamID { get; set; }

        public string Username { get; }

        private string Password { get; }

        public void RefreshApiKey()
        {
            var webResponse = new RequestBuilder("http://steamcommunity.com/dev/apikey?l=en").GET()
                .AddCookies(this.SteamGuard).Execute();
            this.ApiKey = new Regex(@"<p>Key: (\w{32})</p>").Match(webResponse.Content).Groups[1].Value;
        }

        public void RefreshNickname()
        {
            var webResponse =
                new RequestBuilder("http://" + $"steamcommunity.com/profiles/{this.SteamID.SteamID64}/?xml=1").GET()
                    .AddCookies(this.SteamGuard).Execute();
            this.Nickname = new Regex(@"\<steamID\>\<!\[CDATA\[(.+)\]\]\>\<\/steamID\>").Match(webResponse.Content).Groups[1]
                .Value;
        }

        public void RefreshSteamID()
        {
            var webResponse = new RequestBuilder(Constants.STEAM_ACCOUNT_DETAILS_URL).GET().AddCookies(this.SteamGuard)
                .Execute();
            var steamID3 = new Regex(Constants.STEAMID3_REGEX_PATTERN).Match(webResponse.Content).Groups[1].Value;
            this.SteamID = new SteamID(uint.Parse(steamID3));
        }
    }
}