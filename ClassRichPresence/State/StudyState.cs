using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using ClassRichPresence.Subject;
using Discord;

namespace ClassRichPresence.State
{
    public sealed class StudyState : IAppState
    {
        private volatile bool _statusStarted;

        public IAppState Run()
        {
            Console.ResetColor();
            Console.Clear();

            Console.ForegroundColor = App.AppColor;
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("|                                           |");
            Console.WriteLine("|                   Study                   |");
            Console.WriteLine("|                                           |");
            Console.WriteLine("---------------------------------------------\n");

            return HandleOption(GetOption());
        }

        private int GetOption()
        {
            Console.ForegroundColor = App.AppColor;
            Console.WriteLine("Please choose a subject to study\n");

            for (int i = 0; i < SubjectManager.SubjectCount; i++)
            {
                Subject.Subject subject = SubjectManager.GetSubject(i);
                Console.WriteLine($"{i + 1} --- {subject.Name} ({subject.Class})");
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{SubjectManager.SubjectCount + 1} --- Back");

            Console.ForegroundColor = App.AppColor;
            string line = Console.ReadLine().Trim();
            int option;

            while (!int.TryParse(line, out option) || option < 1 || option > SubjectManager.SubjectCount + 1)
                line = Console.ReadLine().Trim();

            return option == SubjectManager.SubjectCount + 1 ? -1 : option;
        }

        private IAppState HandleOption(int option)
        {
            if (option == -1)
                return AppStateManager.GetState<MenuState>();

            Console.Clear();

            Subject.Subject subject = SubjectManager.GetSubject(option - 1);

            if (!AddActivity(subject).Equals(default(Activity)))
                return HandleStatus();
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Press any key to continue...");
            return AppStateManager.GetState<MenuState>();
        }

        private IAppState HandleStatus()
        {
            Console.ForegroundColor = App.AppColor;
            Console.WriteLine("\nTo return to the menu, type 'stop'.\n");

            Thread inputThread = new Thread(() =>
            {
                while (!_statusStarted)
                    ;

                string line = Console.ReadLine();

                while (!line.Equals("stop", StringComparison.OrdinalIgnoreCase))
                    line = Console.ReadLine();
            });
            inputThread.Start();

            while (inputThread.IsAlive)
            {
                App.Discord.RunCallbacks();
                Thread.Yield();
                Thread.Sleep(1000);
            }

            App.Discord.GetActivityManager().ClearActivity((result) => { });
            inputThread.Join();
            return AppStateManager.GetState<MenuState>();
        }

        private Activity AddActivity(Subject.Subject subject)
        {
            var activity = new Activity
            {
                Details = $"{App.AppDetailsPreamble} {subject.Name.ToLowerInvariant()}",
                State = $"{subject.Class}",

                Timestamps =
                {
                    Start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000L,
                },

                Assets =
                {
                    LargeImage = subject.Icon,
                    LargeText = subject.IconText,
                },

                Instance = true,
            };

            Console.ForegroundColor = App.AppColor;
            Console.WriteLine("Please wait...");

            bool error = false;

            App.ActivityManager.UpdateActivity(activity, (result) =>
            {
                if (result == Result.NotRunning)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Discord is not running!");
                    error = true;
                }
                else if (result == Result.Ok)
                {
                    Console.ForegroundColor = App.AppColor;
                    Console.WriteLine("Success - your status has now been set!");
                    Console.Write($"You are currently studying: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"{subject.Name} ({subject.Class})");
                    Console.ForegroundColor = ConsoleColor.Green;
                    _statusStarted = true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("An unexpected error occured!");
                    error = true;
                }
            });

            if (error)
                return default;

            return activity;
        }
    }
}