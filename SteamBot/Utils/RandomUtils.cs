namespace SellCards.SteamWebBot.Utils
{
    using System;

    class RandomUtils
    {
        private static Random r = new Random();

        public static int GetRandomInt(int minBound, int maxBound)
        {
            return r.Next(minBound, maxBound);
        }
    }
}