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
                account.SteamGuard.AcceptMultipleConfirmations(confirmations);
            }
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}
