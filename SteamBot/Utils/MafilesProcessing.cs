using Newtonsoft.Json;
using SellCards.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SellCards {
    class MafilesProcessing
    {
        public static Dictionary<string, MafileProcessingModel> GetAllMafiles()
        {
            Logger.info("Mafiles processing started.");
            var mafiles = Directory.GetFiles(@"Config\Mafiles");
            var allBots = new Dictionary<string, MafileProcessingModel>();

            foreach (var mafilePath in mafiles)
            {
                if (!mafilePath.ToLower().Contains("mafile")) continue;
                try
                {
                    var mafileString = File.ReadAllText(mafilePath);
                    var mafileJson = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(mafileString);
                    var repail = JsonConvert.DeserializeObject<mafile_repail>(mafileString);

                    if (repail.Session == null) 
                    {
                        Logger.info("Repair MaFile from account" + mafileJson.account_name.Value);

                        Session session = new Session { OAuthToken = "", SessionID = "", SteamID = 123, SteamLogin = "", SteamLoginSecure = "", WebCookie = "" };

                        mafile_repail new_reparado = new mafile_repail
                        {
                            account_name = repail.account_name,
                            device_id = repail.device_id,
                            fully_enrolled = repail.fully_enrolled,
                            identity_secret = repail.identity_secret,
                            revocation_code = repail.revocation_code,
                            secret_1 = repail.secret_1,
                            serial_number = repail.serial_number,
                            server_time = repail.server_time,
                            Session = session,
                            shared_secret = repail.shared_secret,
                            status = repail.status,
                            token_gid = repail.token_gid,
                            uri = repail.uri
                        };
                        mafileString = JsonConvert.SerializeObject(new_reparado);
                    }
                    allBots.Add(mafileJson.account_name.Value, new MafileProcessingModel { MafilePath = mafilePath, MafileContent = mafileString });
                }
                catch (Exception e)
                {
                    Logger.error("Error on processing mafile", e);
                }
            }

            Logger.info(allBots.Count() + " mafiles processed.");

            return allBots;
        }
    }

    public class Session
    {
        public string OAuthToken { get; set; }
        public string SessionID { get; set; }
        public long SteamID { get; set; }
        public string SteamLogin { get; set; }
        public string SteamLoginSecure { get; set; }
        public string WebCookie { get; set; }
    }

    public class mafile_repail
    {
        public string account_name { get; set; }
        public string device_id { get; set; }
        public bool fully_enrolled { get; set; }
        public string identity_secret { get; set; }
        public string revocation_code { get; set; }
        public string secret_1 { get; set; }
        public string serial_number { get; set; }
        public int server_time { get; set; }
        public Session Session { get; set; }
        public string shared_secret { get; set; }
        public int status { get; set; }
        public string token_gid { get; set; }
        public string uri { get; set; }
    }
}