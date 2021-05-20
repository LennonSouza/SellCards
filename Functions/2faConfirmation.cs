using SteamBot.SteamWebBot.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SellCards.Functions {
    class _2faConfirmation {

        public static void Get(SteamWebBotAccount account, bool confirmation) {
            if (confirmation == true) {
                SteamAuth.Confirmation[] confirmations = account.SteamGuard.FetchConfirmations();

                var responseTrade = account.SteamGuard.AcceptMultipleConfirmations(confirmations);

                if (responseTrade) {
                    Logger.info($"Mobile Confirmation = {responseTrade.ToString().Replace("True", "Success")} - {Program.countAnuncio++}");
                } else {
                    Logger.info($"Mobile Confirmation = {responseTrade.ToString().Replace("False", "Fail")} or AutoAccept - {Program.countAnuncio++}");
                }
            }
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}
