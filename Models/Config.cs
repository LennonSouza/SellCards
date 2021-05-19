using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SellCards.SteamBot.Models {
    class Config {
        public string Api_Key { get; set; }
        public int Not_Marketable { get; set; }
        public bool NormalCard { get; set; }
        public bool FoilCard { get; set; }
        public bool Network { get; set; }
        public List<string> AppIDBlackList { get; set; }
    }
}
