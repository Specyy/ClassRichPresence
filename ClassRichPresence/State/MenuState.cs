using System;
using System.Collections.Generic;
using System.Text;

namespace ClassRichPresence.State
{
    public sealed class MenuState : IAppState
    {
        public IAppState Run()
        {
            Console.ResetColor();
            Console.Clear();

            Console.ForegroundColor = App.AppColor;
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("|                                           |");
            Console.WriteLine("|                    Menu                   |");
            Console.WriteLine("|                                           |");
            Console.WriteLine("---------------------------------------------\n");

            return HandleOption(GetOption());
        }

        private byte GetOption()
        {
            Console.ForegroundColor = App.AppColor;
            Console.WriteLine("1 --- Study");
            Console.WriteLine("2 --- Add Subject");
            Console.WriteLine("3 --- Delete Subject");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("4 --- Quit");

            var text = Console.ReadLine().Trim();
            var key = text[0];

            while (text.Length > 1 || key < '1' || key > '4')
            {
                text = Console.ReadLine().Trim();
                key = text[0];
            }

            return (byte)(key - '0');
        }

        private IAppState HandleOption(byte option)
        {
            switch (option)
            {
                case 1:
                    return AppStateManager.GetState<StudyState>();
                case 2:
                    return AppStateManager.GetState<SubjectAddState>();
                case 3:
                    return AppStateManager.GetState<SubjectDeleteState>();

                // Should only happen for exit
                default:
                    return null;
            };
        }
    }
}
