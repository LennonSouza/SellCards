namespace SteamAuth
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Net;

    public class SessionData
    {
        [JsonIgnore]
        public Dictionary<string, string> Cookies
        {
            get
            {
                return new Dictionary<string, string>()
                           {
                               { "sessionid", this.SessionID },
                               { "steamLogin", this.SteamLogin },
                               { "steamLoginSecure", this.SteamLoginSecure },
                               { "Steam_Language", "english" },
                           };
            }
        }

        public string OAuthToken { get; set; }

        public string SessionID { get; set; }

        public ulong SteamID { get; set; }

        public string SteamLogin { get; set; }

        public string SteamLoginSecure { get; set; }

        public string WebCookie { get; set; }

        public void AddCookies(CookieContainer cookies)
        {
            cookies.Add(new Cookie("mobileClientVersion", "0 (2.1.3)", "/", ".steamcommunity.com"));
            cookies.Add(new Cookie("mobileClient", "android", "/", ".steamcommunity.com"));

            cookies.Add(new Cookie("steamid", this.SteamID.ToString(), "/", ".steamcommunity.com"));
            cookies.Add(new Cookie("steamLogin", this.SteamLogin, "/", ".steamcommunity.com") { HttpOnly = true });

            cookies.Add(
                new Cookie("steamLoginSecure", this.SteamLoginSecure, "/", ".steamcommunity.com")
                    {
                        HttpOnly = true, Secure = true
                    });
            cookies.Add(new Cookie("Steam_Language", "english", "/", ".steamcommunity.com"));
            cookies.Add(new Cookie("dob", "", "/", ".steamcommunity.com"));
            cookies.Add(new Cookie("sessionid", this.SessionID, "/", ".steamcommunity.com"));
        }
    }
}