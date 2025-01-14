using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Xml.Linq;

using ClassRichPresence.Subject;

namespace ClassRichPresence.State
{
    public sealed class SubjectDeleteState : IAppState
    {
        public IAppState Run()
        {
            Console.ResetColor();
            Console.Clear();

            return HandleSubject();
        }

        private IAppState HandleSubject()
        {
            Subject.Subject subject = HandleOption(GetOption());

            if (subject is null)
                return AppStateManager.GetState<MenuState>();

            Console.ForegroundColor = App.AppColor;
            Console.WriteLine("\nDeleting subject...");

            DeleteSubject(subject, App.CustomSubjectsFile);

            Console.ForegroundColor = App.AppColor;
            Console.WriteLine("\nSubject successfully deleted!");

            Console.WriteLine("Returning to menu...");
            Thread.Yield();
            Thread.Sleep(1000);

            return AppStateManager.GetState<MenuState>();
        }

        private int GetOption()
        {
            Console.ForegroundColor = App.AppColor;
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("|                                           |");
            Console.WriteLine("|               Subject Delete              |");
            Console.WriteLine("|                                           |");
            Console.WriteLine("---------------------------------------------\n");

            if (SubjectManager.SubjectCount == SubjectManager.DefaultSubjectCount)
            {
                Console.WriteLine("No custom subjects to delete");
                Console.WriteLine("Returning to menu...");
                Thread.Yield();
                Thread.Sleep(1000);
                return -1;
            }

            Console.WriteLine("Please choose a subject to delete\n");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"0 --- All");

            Console.ForegroundColor = App.AppColor;
            for (int i = SubjectManager.DefaultSubjectCount; i < SubjectManager.SubjectCount; i++)
            {
                Subject.Subject subject = SubjectManager.GetSubject(i);
                Console.WriteLine($"{i - SubjectManager.DefaultSubjectCount + 1} --- {subject.Name} ({subject.Class})");
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{SubjectManager.SubjectCount - SubjectManager.DefaultSubjectCount + 1} --- Back");

            Console.ForegroundColor = App.AppColor;
            string line = Console.ReadLine().Trim();
            int option;

            while (!int.TryParse(line, out option) || option < 0 || option > SubjectManager.SubjectCount - SubjectManager.DefaultSubjectCount + 1 + 1)
                line = Console.ReadLine().Trim();

            return option == SubjectManager.SubjectCount - SubjectManager.DefaultSubjectCount + 1 ? -1 : option;
        }

        private Subject.Subject HandleOption(int option)
        {
            if (option == -1)
                return null;

            if (option == 0)
                return HandleAll();

            Console.Clear();

            Subject.Subject subject = SubjectManager.GetSubject(option + SubjectManager.DefaultSubjectCount - 1);

            Console.ForegroundColor = App.AppColor;
            Console.Write("Removing subject '");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(subject.Name);

            Console.ForegroundColor = App.AppColor;
            Console.Write("' with class '");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(subject.Class);

            Console.ForegroundColor = App.AppColor;
            Console.WriteLine("'. Is this OK? (y/n)");

            var answerText = Console.ReadLine().Trim().ToLowerInvariant();
            var answer = answerText[0];

            while (answerText.Length > 1 || (answer != 'y' && answer != 'n'))
            {
                answerText = Console.ReadLine().Trim().ToLowerInvariant();
                answer = answerText[0];
            }

            if (answer == 'y')
                return subject;

            Console.Clear();
            return HandleOption(GetOption());
        }

        private Subject.Subject HandleAll()
        {
            Console.Clear();

            Console.ForegroundColor = App.AppColor;
            Console.Write("Removing ");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("All");

            Console.ForegroundColor = App.AppColor;
            Console.Write(" subjects");

            Console.ForegroundColor = App.AppColor;
            Console.WriteLine(". Is this OK? (y/n)");

            var answerText = Console.ReadLine().Trim().ToLowerInvariant();
            var answer = answerText[0];

            while (answerText.Length > 1 || (answer != 'y' && answer != 'n'))
            {
                answerText = Console.ReadLine().Trim().ToLowerInvariant();
                answer = answerText[0];
            }

            if (answer == 'y')
            {
                Console.ForegroundColor = App.AppColor;
                Console.WriteLine("\nDeleting subjects...");

                int count = SubjectManager.SubjectCount;
                for (int i = SubjectManager.DefaultSubjectCount; i < count; i++)
                {
                    SubjectManager.RemoveSubject(SubjectManager.GetSubject(SubjectManager.DefaultSubjectCount));
                }

                DeleteSubjects(App.CustomSubjectsFile);

                Console.ForegroundColor = App.AppColor;
                Console.WriteLine("\nSubject successfully deleted!");

                Console.WriteLine("Returning to menu...");
                Thread.Yield();
                Thread.Sleep(1000);

                return null;
            }

            Console.Clear();
            return HandleOption(GetOption());
        }

        private bool DeleteSubject(Subject.Subject subject, string saveFile)
        {
            SubjectManager.RemoveSubject(subject);

            Console.ForegroundColor = App.AppColor;
            Console.WriteLine("Saving changes...");
            DeleteSubjects(saveFile);

            return true;
        }

        private void DeleteSubjects(string saveFile)
        {
            FileInfo file = new FileInfo(saveFile);
            FileStream stream;
            file.Delete();
            Directory.Delete(Path.GetDirectoryName(file.FullName));

            if (SubjectManager.SubjectCount != SubjectManager.DefaultSubjectCount)
            {
                stream = file.OpenWrite();
                stream.Write(App.CustomSubjectsFileHeader, 0, App.CustomSubjectsFileHeader.Length);

                for (int i = SubjectManager.DefaultSubjectCount; i < SubjectManager.SubjectCount; i++)
                {
                    WriteSubject(SubjectManager.GetSubject(i), stream);
                }

                stream.Dispose();
            }
        }

        private void WriteSubject(Subject.Subject subject, FileStream stream)
        {
            // class:
            // us = u + 241f | 0xE2 0x90 0x9F
            // stx = u+2402 | 0xE2 0x90 0x82
            // etx = u+2403 | 0xE2 0x90 0x83
            //
            // us stx name etx
            // stx class etx
            // stx iconText etx
            // stx icon etx

            // us
            byte[] us = new byte[] { 0xE2, 0x90, 0x9F };
            stream.Write(us, 0, us.Length);

            //
            // stx
            byte[] stx = new byte[] { 0xE2, 0x90, 0x82 };
            stream.Write(stx, 0, stx.Length);

            byte[] nameAscii = Encoding.ASCII.GetBytes(subject.Name);
            stream.Write(nameAscii, 0, nameAscii.Length);

            // etx
            byte[] etx = new byte[] { 0xE2, 0x90, 0x83 };
            stream.Write(etx, 0, etx.Length);

            //
            // stx
            stream.Write(stx, 0, stx.Length);

            byte[] classAscii = Encoding.ASCII.GetBytes(subject.Class);
            stream.Write(classAscii, 0, classAscii.Length);

            // etx
            stream.Write(etx, 0, etx.Length);

            //
            // stx
            stream.Write(stx, 0, stx.Length);

            byte[] iconTextAscii = Encoding.ASCII.GetBytes(subject.IconText);
            stream.Write(iconTextAscii, 0, iconTextAscii.Length);

            // etx
            stream.Write(etx, 0, etx.Length);

            //
            // stx
            stream.Write(stx, 0, stx.Length);

            byte[] iconAscii = Encoding.ASCII.GetBytes(subject.Icon);
            stream.Write(iconAscii, 0, iconAscii.Length);

            stream.Write(etx, 0, stx.Length);
        }
    }
}
