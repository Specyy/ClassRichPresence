using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;

namespace ClassRichPresence.Subject
{
    [Serializable]
    public class Subject
    {
        public string Name { get; }

        public string Class { get; }

        public string IconText { get; }

        public string Icon { get; }

        public Subject(string name, string @class, string iconText = null, string icon = "school-0")
        {
            Name = name;
            Class = @class;
            IconText = iconText ?? name;
            Icon = icon;
        }
    }
}
