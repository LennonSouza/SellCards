namespace SteamBot.SteamWebBot.Account
{
    public class SteamID
    {
        private static readonly ulong changeValue = 76561197960265728;

        public SteamID(ulong steamID64)
        {
            this.SteamID64 = steamID64;
            this.SteamID3 = this.Convert64To3(steamID64);
            this.SteamID32 = this.Convert64To32(steamID64);
        }

        public SteamID(uint steamID3)
        {
            this.SteamID3 = steamID3;
            this.SteamID64 = this.Convert3To64(steamID3);
            this.SteamID32 = this.Convert64To32(this.SteamID64);
        }

        public uint SteamID3 { get; set; } // 8-9 U:1:xxxxxxxx

        public string SteamID32 { get; set; } //6 STEAM_0:X:XXXXXX

        public ulong SteamID64 { get; set; } // 7656119xxxxxxxxxx

        private uint Convert32To3(uint SteamID32)
        {
            return (uint)(this.SteamID64 * 2 + 1);
        }

        private ulong Convert32To64(uint steamID32)
        {
            return ((ulong)steamID32 * 2 + 1) * 2;
        }

        private ulong Convert3To64(uint steamID3)
        {
            return steamID3 + changeValue;
        }

        private uint Convert64To3(ulong steamID64)
        {
            return (uint)(steamID64 - changeValue);
        }

        private string Convert64To32(ulong steamID64)
        {
            return $"STEAM_0:{steamID64 % 2}:{(steamID64 - changeValue) / 2}";
        }
    }
}