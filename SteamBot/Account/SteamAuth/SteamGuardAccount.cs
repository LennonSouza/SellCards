namespace SteamAuth
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Newtonsoft.Json;
    using SellCards;
    using SteamBot.SteamWebBot.Utils;

    public class SteamGuardAccount
    {
        private static byte[] steamGuardCodeTranslations = new byte[]
                                                               {
                                                                   50, 51, 52, 53, 54, 55, 56, 57, 66, 67, 68, 70, 71,
                                                                   72, 74, 75, 77, 78, 80, 81, 82, 84, 86, 87, 88, 89
                                                               };

        private string password;

        private string username;

        [JsonProperty("account_name")]
        public string AccountName { get; set; }

        [JsonIgnore]
        public Dictionary<string, string> Cookies
        {
            get
            {
                return this.Session.Cookies;
            }
        }

        [JsonProperty("device_id")]
        public string DeviceID { get; set; }

        [JsonProperty("fully_enrolled")]
        public bool FullyEnrolled { get; set; }

        [JsonProperty("identity_secret")]
        public string IdentitySecret { get; set; }

        [JsonIgnore]
        public bool IsSessionExpired
        {
            get
            {
                var webResponse = new RequestBuilder(Constants.STEAM_ACCOUNT_DETAILS_URL).GET().AddCookies(this)
                    .Execute();

                if (webResponse.Content.Contains("youraccount_steamid"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [JsonProperty("revocation_code")]
        public string RevocationCode { get; set; }

        [JsonProperty("secret_1")]
        public string Secret1 { get; set; }

        [JsonProperty("serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty("server_time")]
        public long ServerTime { get; set; }

        [JsonProperty("Session")]
        public SessionData Session { get; set; }

        [JsonProperty("shared_secret")]
        public string SharedSecret { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("token_gid")]
        public string TokenGID { get; set; }

        [JsonProperty("uri")]
        public string URI { get; set; }

        [JsonIgnore]
        public UserLogin UserLogin { get; set; }

        public bool AcceptConfirmation(Confirmation conf)
        {
            return this._sendConfirmationAjax(conf, "allow");
        }

        public bool AcceptMultipleConfirmations(Confirmation[] confs)
        {
            return this._sendMultiConfirmationAjax(confs, "allow");
        }

        public bool DeactivateAuthenticator(int scheme = 2)
        {
            var postData = new NameValueCollection();
            postData.Add("steamid", this.Session.SteamID.ToString());
            postData.Add("steamguard_scheme", scheme.ToString());
            postData.Add("revocation_code", this.RevocationCode);
            postData.Add("access_token", this.Session.OAuthToken);

            try
            {
                var response = SteamWeb.MobileLoginRequest(
                    APIEndpoints.STEAMAPI_BASE + "/ITwoFactorService/RemoveAuthenticator/v0001",
                    "POST",
                    postData);
                var removeResponse = JsonConvert.DeserializeObject<RemoveAuthenticatorResponse>(response);

                if (removeResponse == null || removeResponse.Response == null || !removeResponse.Response.Success)
                    return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DenyConfirmation(Confirmation conf)
        {
            return this._sendConfirmationAjax(conf, "cancel");
        }

        public bool DenyMultipleConfirmations(Confirmation[] confs)
        {
            return this._sendMultiConfirmationAjax(confs, "cancel");
        }

        public Confirmation[] FetchConfirmations()
        {
            var url = this.GenerateConfirmationURL();

            var cookies = new CookieContainer();
            this.Session.AddCookies(cookies);

            var response = SteamWeb.Request(url, "GET", "", cookies);

            /*So you're going to see this abomination and you're going to be upset.
              It's understandable. But the thing is, regex for HTML -- while awful -- makes this way faster than parsing a DOM, plus we don't need another library.
              And because the data is always in the same place and same format... It's not as if we're trying to naturally understand HTML here. Just extract strings.
              I'm sorry. */

            var confRegex = new Regex(
                "<div class=\"mobileconf_list_entry\" id=\"conf[0-9]+\" data-confid=\"(\\d+)\" data-key=\"(\\d+)\" data-type=\"(\\d+)\" data-creator=\"(\\d+)\"");

            if (response == null || !confRegex.IsMatch(response))
            {
                if (response == null || !response.Contains("<div>Nothing to confirm</div>"))
                {
                    //throw new WGTokenInvalidException();
                }

                return new Confirmation[0];
            }

            var confirmations = confRegex.Matches(response);

            var ret = new List<Confirmation>();
            foreach (Match confirmation in confirmations)
            {
                if (confirmation.Groups.Count != 5) continue;
                ulong confID;
                ulong confKey;
                int confType;
                ulong confCreator;

                if (!ulong.TryParse(confirmation.Groups[1].Value, out confID)
                    || !ulong.TryParse(confirmation.Groups[2].Value, out confKey)
                    || !int.TryParse(confirmation.Groups[3].Value, out confType) || !ulong.TryParse(
                        confirmation.Groups[4].Value,
                        out confCreator))
                {
                    continue;
                }

                ret.Add(new Confirmation(confID, confKey, confType, confCreator));
            }

            return ret.ToArray();
        }

        public async Task<Confirmation[]> FetchConfirmationsAsync()
        {
            var url = this.GenerateConfirmationURL();

            var cookies = new CookieContainer();
            this.Session.AddCookies(cookies);

            var response = await SteamWeb.RequestAsync(url, "GET", null, cookies);

            /*So you're going to see this abomination and you're going to be upset.
                          It's understandable. But the thing is, regex for HTML -- while awful -- makes this way faster than parsing a DOM, plus we don't need another library.
                          And because the data is always in the same place and same format... It's not as if we're trying to naturally understand HTML here. Just extract strings.
                          I'm sorry. */

            var confRegex = new Regex(
                "<div class=\"mobileconf_list_entry\" id=\"conf[0-9]+\" data-confid=\"(\\d+)\" data-key=\"(\\d+)\" data-type=\"(\\d+)\" data-creator=\"(\\d+)\"");

            if (response == null || !confRegex.IsMatch(response))
            {
                if (response == null || !response.Contains("<div>Nothing to confirm</div>"))
                {
                    throw new WGTokenInvalidException();
                }

                return new Confirmation[0];
            }

            var confirmations = confRegex.Matches(response);

            var ret = new List<Confirmation>();
            foreach (Match confirmation in confirmations)
            {
                if (confirmation.Groups.Count != 5) continue;
                ulong confID;
                ulong confKey;
                int confType;
                ulong confCreator;

                if (!ulong.TryParse(confirmation.Groups[1].Value, out confID)
                    || !ulong.TryParse(confirmation.Groups[2].Value, out confKey)
                    || !int.TryParse(confirmation.Groups[3].Value, out confType) || !ulong.TryParse(
                        confirmation.Groups[4].Value,
                        out confCreator))
                {
                    continue;
                }

                ret.Add(new Confirmation(confID, confKey, confType, confCreator));
            }

            return ret.ToArray();
        }

        public string GenerateConfirmationQueryParams(string tag)
        {
            if (String.IsNullOrEmpty(this.DeviceID))
                throw new ArgumentException("Device ID is not present");

            var queryParams = this.GenerateConfirmationQueryParamsAsNVC(tag);

            return "p=" + queryParams["p"] + "&a=" + queryParams["a"] + "&k=" + queryParams["k"] + "&t="
                   + queryParams["t"] + "&m=android&tag=" + queryParams["tag"];
        }

        public NameValueCollection GenerateConfirmationQueryParamsAsNVC(string tag)
        {
            if (String.IsNullOrEmpty(this.DeviceID))
                throw new ArgumentException("Device ID is not present");

            var time = TimeAligner.GetSteamTime();

            var ret = new NameValueCollection();
            ret.Add("p", this.DeviceID);
            ret.Add("a", this.Session.SteamID.ToString());
            ret.Add("k", this._generateConfirmationHashForTime(time, tag));
            ret.Add("t", time.ToString());
            ret.Add("m", "android");
            ret.Add("tag", tag);

            return ret;
        }

        public string GenerateConfirmationURL(string tag = "conf")
        {
            var endpoint = APIEndpoints.COMMUNITY_BASE + "/mobileconf/conf?";
            var queryString = this.GenerateConfirmationQueryParams(tag);
            return endpoint + queryString;
        }

        public string GenerateSteamGuardCode()
        {
            return this.GenerateSteamGuardCodeForTime(TimeAligner.GetSteamTime());
        }

        public string GenerateSteamGuardCodeForTime(long time)
        {
            if (this.SharedSecret == null || this.SharedSecret.Length == 0)
            {
                return "";
            }

            var sharedSecretUnescaped = Regex.Unescape(this.SharedSecret);
            var sharedSecretArray = Convert.FromBase64String(sharedSecretUnescaped);
            var timeArray = new byte[8];

            time /= 30L;

            for (var i = 8; i > 0; i--)
            {
                timeArray[i - 1] = (byte)time;
                time >>= 8;
            }

            var hmacGenerator = new HMACSHA1();
            hmacGenerator.Key = sharedSecretArray;
            var hashedData = hmacGenerator.ComputeHash(timeArray);
            var codeArray = new byte[5];
            try
            {
                var b = (byte)(hashedData[19] & 0xF);
                var codePoint = (hashedData[b] & 0x7F) << 24 | (hashedData[b + 1] & 0xFF) << 16
                                                             | (hashedData[b + 2] & 0xFF) << 8
                                                             | (hashedData[b + 3] & 0xFF);

                for (var i = 0; i < 5; ++i)
                {
                    codeArray[i] = steamGuardCodeTranslations[codePoint % steamGuardCodeTranslations.Length];
                    codePoint /= steamGuardCodeTranslations.Length;
                }
            }
            catch (Exception)
            {
                return null; //Change later, catch-alls are bad!
            }

            return Encoding.UTF8.GetString(codeArray);
        }

        /// <summary>
        /// Deprecated. Simply returns conf.Creator.
        /// </summary>
        /// <param name="conf"></param>
        /// <returns>The Creator field of conf</returns>
        public long GetConfirmationTradeOfferID(Confirmation conf)
        {
            if (conf.ConfType != Confirmation.ConfirmationType.Trade)
                throw new ArgumentException("conf must be a trade confirmation.");

            return (long)conf.Creator;
        }

        public LoginResult Login(string username, string password)
        {
            this.username = username;
            this.password = password;

            this.UserLogin = new UserLogin(username, password);
            this.UserLogin.TwoFactorCode = this.GenerateSteamGuardCode();
            var loginResult = this.UserLogin.DoLogin();
            if (!this.UserLogin.LoggedIn)
            {
                Waiter.waitForSec(30);
                this.UserLogin.TwoFactorCode = this.GenerateSteamGuardCode();
                loginResult = this.UserLogin.DoLogin();
            }

            if (loginResult != LoginResult.LoginOkay)
            {
                throw new ArgumentException(loginResult.ToString());
            }

            this.Session = this.UserLogin.Session;

            return loginResult;
        }

        /// <summary>
        /// Refreshes the Steam session. Necessary to perform confirmations if your session has expired or changed.
        /// </summary>
        /// <returns></returns>
        public bool RefreshConfirmationsSession()
        {
            var url = APIEndpoints.MOBILEAUTH_GETWGTOKEN;
            var postData = new NameValueCollection();
            postData.Add("access_token", this.Session.OAuthToken);

            string response = null;
            try
            {
                response = SteamWeb.Request(url, "POST", postData);
            }
            catch (WebException)
            {
                return false;
            }

            if (response == null) return false;

            try
            {
                var refreshResponse = JsonConvert.DeserializeObject<RefreshSessionDataResponse>(response);
                if (refreshResponse == null || refreshResponse.Response == null
                                            || String.IsNullOrEmpty(refreshResponse.Response.Token))
                    return false;

                var token = this.Session.SteamID + "%7C%7C" + refreshResponse.Response.Token;
                var tokenSecure = this.Session.SteamID + "%7C%7C" + refreshResponse.Response.TokenSecure;

                this.Session.SteamLogin = token;
                this.Session.SteamLoginSecure = tokenSecure;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Refreshes the Steam session. Necessary to perform confirmations if your session has expired or changed.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RefreshConfirmationsSessionAsync()
        {
            var url = APIEndpoints.MOBILEAUTH_GETWGTOKEN;
            var postData = new NameValueCollection();
            postData.Add("access_token", this.Session.OAuthToken);

            string response = null;
            try
            {
                response = await SteamWeb.RequestAsync(url, "POST", postData);
            }
            catch (WebException)
            {
                return false;
            }

            if (response == null) return false;

            try
            {
                var refreshResponse = JsonConvert.DeserializeObject<RefreshSessionDataResponse>(response);
                if (refreshResponse == null || refreshResponse.Response == null
                                            || String.IsNullOrEmpty(refreshResponse.Response.Token))
                    return false;

                var token = this.Session.SteamID + "%7C%7C" + refreshResponse.Response.Token;
                var tokenSecure = this.Session.SteamID + "%7C%7C" + refreshResponse.Response.TokenSecure;

                this.Session.SteamLogin = token;
                this.Session.SteamLoginSecure = tokenSecure;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public LoginResult RefreshSession()
        {
            return this.Login(this.username, this.password);
        }

        private string _generateConfirmationHashForTime(long time, string tag)
        {
            var decode = Convert.FromBase64String(this.IdentitySecret);
            var n2 = 8;
            if (tag != null)
            {
                if (tag.Length > 32)
                {
                    n2 = 8 + 32;
                }
                else
                {
                    n2 = 8 + tag.Length;
                }
            }

            var array = new byte[n2];
            var n3 = 8;
            while (true)
            {
                var n4 = n3 - 1;
                if (n3 <= 0)
                {
                    break;
                }

                array[n4] = (byte)time;
                time >>= 8;
                n3 = n4;
            }

            if (tag != null)
            {
                Array.Copy(Encoding.UTF8.GetBytes(tag), 0, array, 8, n2 - 8);
            }

            try
            {
                var hmacGenerator = new HMACSHA1();
                hmacGenerator.Key = decode;
                var hashedData = hmacGenerator.ComputeHash(array);
                var encodedData = Convert.ToBase64String(hashedData, Base64FormattingOptions.None);
                var hash = WebUtility.UrlEncode(encodedData);
                return hash;
            }
            catch
            {
                return null;
            }
        }

        private ConfirmationDetailsResponse _getConfirmationDetails(Confirmation conf)
        {
            var url = APIEndpoints.COMMUNITY_BASE + "/mobileconf/details/" + conf.ID + "?";
            var queryString = this.GenerateConfirmationQueryParams("details");
            url += queryString;

            var cookies = new CookieContainer();
            this.Session.AddCookies(cookies);
            var referer = this.GenerateConfirmationURL();

            var response = SteamWeb.Request(url, "GET", "", cookies, null);
            if (String.IsNullOrEmpty(response)) return null;

            var confResponse = JsonConvert.DeserializeObject<ConfirmationDetailsResponse>(response);
            if (confResponse == null) return null;
            return confResponse;
        }

        private bool _sendConfirmationAjax(Confirmation conf, string op)
        {
            var url = APIEndpoints.COMMUNITY_BASE + "/mobileconf/ajaxop";
            var queryString = "?op=" + op + "&";
            queryString += this.GenerateConfirmationQueryParams(op);
            queryString += "&cid=" + conf.ID + "&ck=" + conf.Key;
            url += queryString;

            var cookies = new CookieContainer();
            this.Session.AddCookies(cookies);
            var referer = this.GenerateConfirmationURL();

            var response = SteamWeb.Request(url, "GET", "", cookies, null);
            if (response == null) return false;

            var confResponse = JsonConvert.DeserializeObject<SendConfirmationResponse>(response);
            return confResponse.Success;
        }

        private bool _sendMultiConfirmationAjax(Confirmation[] confs, string op)
        {
            var url = APIEndpoints.COMMUNITY_BASE + "/mobileconf/multiajaxop";

            var query = "op=" + op + "&" + this.GenerateConfirmationQueryParams(op);
            foreach (var conf in confs)
            {
                query += "&cid[]=" + conf.ID + "&ck[]=" + conf.Key;
            }

            var cookies = new CookieContainer();
            this.Session.AddCookies(cookies);
            var referer = this.GenerateConfirmationURL();

            var response = SteamWeb.Request(url, "POST", query, cookies, null);
            if (response == null) return false;

            var confResponse = JsonConvert.DeserializeObject<SendConfirmationResponse>(response);
            return confResponse.Success;
        }

        public class WGTokenExpiredException : Exception
        {
        }

        public class WGTokenInvalidException : Exception
        {
        }

        private class ConfirmationDetailsResponse
        {
            [JsonProperty("html")]
            public string HTML { get; set; }

            [JsonProperty("success")]
            public bool Success { get; set; }
        }

        private class RefreshSessionDataResponse
        {
            [JsonProperty("response")]
            public RefreshSessionDataInternalResponse Response { get; set; }

            internal class RefreshSessionDataInternalResponse
            {
                [JsonProperty("token")]
                public string Token { get; set; }

                [JsonProperty("token_secure")]
                public string TokenSecure { get; set; }
            }
        }

        private class RemoveAuthenticatorResponse
        {
            [JsonProperty("response")]
            public RemoveAuthenticatorInternalResponse Response { get; set; }

            internal class RemoveAuthenticatorInternalResponse
            {
                [JsonProperty("success")]
                public bool Success { get; set; }
            }
        }

        private class SendConfirmationResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }
        }
    }
}