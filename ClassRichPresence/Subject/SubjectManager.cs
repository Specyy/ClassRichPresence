using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClassRichPresence.Subject
{
    public static class SubjectManager
    {
        // Default Subjects
        private static readonly Subject _frenchSubject = new Subject("French", "Cinema/Litterature 11", icon: "french-0");
        private static readonly Subject _socialsSubject = new Subject("Socials", "Histoire du Monde 12", icon: "socials-0");
        private static readonly Subject _mathSubject = new Subject("Math", "Pre-Calculus 12", icon: "math-0");
        private static readonly Subject _chemistrySubject = new Subject("Chemistry", "Chemistry 11", icon: "chemistry-0");

        private static readonly OrderedDictionary<string, Subject> _subjects = new OrderedDictionary<string, Subject>();
        private static Subject _currentSubject;
        public static string CurrentSubject
        {
            get => _currentSubject.Name;
            set
            {
                _currentSubject = _subjects[value];
            }
        }
        public static ICollection<string> SubjectNames => _subjects.Keys;
        public static ICollection<Subject> Subjects => _subjects.Values;

        public static int SubjectCount => _subjects.Count;
        public static int DefaultSubjectCount => 4;

        static SubjectManager()
        {
            // Add default subjects
            AddDefaultSubjects();

            // Load custom subjects
            AddCustomSubjects(App.CustomSubjectsFile);
        }

        private static void AddDefaultSubjects()
        {
            AddSubject(_frenchSubject);
            AddSubject(_socialsSubject);
            AddSubject(_mathSubject);
            AddSubject(_chemistrySubject);
        }

        private static void AddCustomSubjects(string file)
        {
            if (!File.Exists(file))
                return;

            using (FileStream stream = File.OpenRead(file))
            {
                byte[] buffer = new byte[1024];

                // Check header
                // No header = not my file
                stream.Read(buffer, 0, 6);
                byte[] header = App.CustomSubjectsFileHeader;

                if (buffer[0] == header[0] && buffer[1] == header[1] && buffer[2] == header[2]
                    && buffer[3] == header[3] && buffer[4] == header[4] && buffer[5] == header[5])
                {
                    stream.Read(buffer, 0, buffer.Length);
                    ReadSubjects(stream, ref buffer);
                }
            }
        }

        private static int ReadSubjects(FileStream stream, ref byte[] buffer)
        {
            int count = 0;
            int nextIndex = 0;

            while (true)
            {
                // us
                if (buffer[nextIndex] != 0xE2 || buffer[nextIndex + 1] != 0x90 || buffer[nextIndex + 2] != 0x9F)
                    throw new IOException("Cannot read corrupted subjects file!");

                if ((nextIndex += 3) == buffer.Length)
                {
                    stream.Read(buffer, 0, buffer.Length);
                    nextIndex = 0;
                }

                string name = ParseData(stream, ref buffer, ref nextIndex);
                string @class = ParseData(stream, ref buffer, ref nextIndex);
                string iconText = ParseData(stream, ref buffer, ref nextIndex);
                string icon = ParseData(stream, ref buffer, ref nextIndex);

                AddSubject(new Subject(name, @class, iconText, icon));
                count++;

                if (nextIndex + 6 == stream.Length)
                    break;
            }

            return count;
        }

        private static string ParseData(FileStream stream, ref byte[] buffer, ref int nextIndex)
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

            // stx
            if (buffer[nextIndex] != 0xE2 || buffer[nextIndex + 1] != 0x90 || buffer[nextIndex + 2] != 0x82)
                throw new IOException("Cannot read corrupted subjects file!");

            int count = buffer.Length;
            if ((nextIndex += 3) == buffer.Length)
            {
                count = stream.Read(buffer, 0, buffer.Length);
                nextIndex = 0;
            }
            
            
            LinkedList<byte> list = new LinkedList<byte>();

            for (; nextIndex < count; nextIndex++)
            {
                if (buffer[nextIndex] == 0xE2 && buffer[nextIndex + 1] == 0x90 && buffer[nextIndex + 2] == 0x83)
                {
                    if ((nextIndex += 3) == buffer.Length)
                    {
                        stream.Read(buffer, 0, buffer.Length);
                        nextIndex = 0;
                    }

                    break;
                }

                list.AddLast(buffer[nextIndex]);
            }

            byte[] stringBytes = new byte[list.Count];
            list.CopyTo(stringBytes, 0);
            return Encoding.ASCII.GetString(stringBytes);
        }

        public static bool ContainsSubject(Subject subject) => _subjects.ContainsKey(subject.Name.ToLowerInvariant());
        public static bool ContainsSubject(string subject) => _subjects.ContainsKey(subject.ToLowerInvariant());
        public static void AddSubject(Subject subject) => _subjects.Add(subject.Name.ToLowerInvariant(), subject);

        public static Subject GetSubject(string subject) => _subjects[subject.ToLowerInvariant()];

        public static Subject GetSubject(int subject) => _subjects[subject].Value;
        public static bool RemoveSubject(string subject) => _subjects.Remove(subject.ToLowerInvariant());
        public static bool RemoveSubject(Subject subject) => _subjects.Remove(subject.Name.ToLowerInvariant());
    }
}
