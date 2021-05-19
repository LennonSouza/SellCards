namespace SteamBot.SteamWebBot.Utils
{
    using System;
    using System.Threading;

    static class Waiter
    {
        public static void waitForMiliSec(int value)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(value));
        }

        public static void waitForMin(int value)
        {
            Thread.Sleep(TimeSpan.FromMinutes(value));
        }

        public static void waitForSec(int value)
        {
            Thread.Sleep(TimeSpan.FromSeconds(value));
        }
    }
}