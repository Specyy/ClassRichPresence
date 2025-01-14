using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;

using ClassRichPresence.Subject;

namespace ClassRichPresence.State
{
    public sealed class SubjectAddState : IAppState
    {
        public IAppState Run()
        {
            Console.ResetColor();
            Console.Clear();

            return HandleSubject();
        }

        private IAppState HandleSubject()
        {
            Subject.Subject subject = GetWantedSubject();

            Console.ForegroundColor = App.AppColor;
            Console.WriteLine("\nAdding subject...");

            AddSubject(subject, App.CustomSubjectsFile);

            Console.ForegroundColor = App.AppColor;
            Console.WriteLine("\nSubject successfully added!");

            Console.WriteLine("Returning to menu...");
            Thread.Yield();
            Thread.Sleep(1000);

            return AppStateManager.GetState<MenuState>();
        }

        private Subject.Subject GetWantedSubject()
        {
            while (true)
            {
                Console.ForegroundColor = App.AppColor;
                Console.WriteLine("---------------------------------------------");
                Console.WriteLine("|                                           |");
                Console.WriteLine("|                Subject Add                |");
                Console.WriteLine("|                                           |");
                Console.WriteLine("---------------------------------------------\n");

                Console.WriteLine("What is the name of the subject? (ex. Math)");

                string name = Console.ReadLine();

                while (SubjectManager.ContainsSubject(name))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nThat subject already exists!");

                    Console.ForegroundColor = App.AppColor;
                    Console.WriteLine("What is the name of the subject? (ex. Math)");

                    name = Console.ReadLine();
                }

                Console.WriteLine("\nWhat is the name of the class? (ex. Pre-Calculus 11)");
                string @class = Console.ReadLine();

                Console.Clear();
                Console.Write("Adding subject '");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(name);

                Console.ForegroundColor = App.AppColor;
                Console.Write("' with class '");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(@class);

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
                    return new Subject.Subject(name, @class);

                Console.Clear();
            }
        }

        private bool AddSubject(Subject.Subject subject, string saveFile)
        {
            SubjectManager.AddSubject(subject);

            Console.ForegroundColor = App.AppColor;
            Console.WriteLine("Saving changes...");
            SaveSubject(subject, saveFile);
            return true;
        }

        private void SaveSubject(Subject.Subject subject, string saveFile)
        {
            FileInfo file = new FileInfo(saveFile);
            FileStream stream;

            if (file.Exists)
            {
                byte[] fileBytes = new byte[(int)file.Length];
                using (var st = file.OpenRead())
                    st.Read(fileBytes, 0, fileBytes.Length);

                stream = file.OpenWrite();
                stream.Write(fileBytes, 0, fileBytes.Length);
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file.FullName));
                stream = file.Create();
                stream.Write(App.CustomSubjectsFileHeader, 0, App.CustomSubjectsFileHeader.Length);
            }

            WriteSubject(subject, stream);
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
            stream.Dispose();
        }
    }
}
