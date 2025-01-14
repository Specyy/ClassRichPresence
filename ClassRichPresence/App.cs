using System;
using System.IO;
using ClassRichPresence.State;
using Discord;

namespace ClassRichPresence
{
    public class App
    {
        //public static string LargeText => "foo largeImageText";
        //public static string SmallText => "foo smallImageText";
        public static string AppDetailsPreamble => "Studying";
        public static ConsoleColor AppColor => ConsoleColor.Green;
        public static string CustomSubjectsFile => $"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClassRichPresence\\custom.bin")}";

        /// <summary>
        /// Header:<br/>
        /// soh = u+2401 | utf-8: 0xE2 0x90 0x81<br/>
        /// sohCRP | 0xE2 0x90 0x81 , 0x43 0x52 0x50<br/>1
        /// </summary>
        /*
                Discord.CreateFlags.Default will require Discord to be running for the game to work
                If Discord is not running, it will:
                1. Close your game
                2. Open Discord
                3. Attempt to re-open your game
                Step 3 will fail when running directly from the Unity editor
                Therefore, always keep Discord running during tests, or use Discord.CreateFlags.NoRequireDiscord
            */
        public static byte[] CustomSubjectsFileHeader = { 0xE2, 0x90, 0x81, 0x43, 0x52, 0x50 };
        public static long AppID = 885721832655835147L;
        private static Discord.Discord _discord = new Discord.Discord(885721832655835147L, (UInt64)CreateFlags.NoRequireDiscord);
        public static Discord.Discord Discord => _discord;

        public static ActivityManager ActivityManager => _discord.GetActivityManager();


        static void Main(string[] args)
        {
            // Start state
            AppStateManager.CurrentState = AppStateManager.GetState<MenuState>();

            while ((AppStateManager.CurrentState = AppStateManager.CurrentState.Run()) != null)
                ;

            // Do not affect proceeding apps
            Console.ResetColor();
        }
    }
}
