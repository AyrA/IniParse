using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IniParse
{
    public class IniSection
    {
        public string Name { get; set; }

        public List<IniSetting> Settings { get; private set; }

        public string[] Comments { get; set; }

        public IniSetting this[string name]
        {
            get
            {
                return Settings.FirstOrDefault(m => m.Name == name);
            }
        }

        public IniSetting this[int index]
        {
            get
            {
                return Settings[index];
            }
        }

        public IniSection() : this(null)
        {

        }

        public IniSection(string SectionName)
        {
            Name = SectionName;
            Settings = new List<IniSetting>();
        }

        public async Task ExportSection(StreamWriter SW, char CommentChar)
        {
            if (Name != null)
            {
                if(!Tools.IsEmpty(Comments))
                {
                    foreach(var L in Comments)
                    {
                        await SW.WriteLineAsync($"{CommentChar}{L}");
                    }
                }
                await SW.WriteLineAsync($"[{Name}]");
            }
            foreach(var S in Settings)
            {
                await S.ExportSetting(SW, CommentChar);
            }
        }
    }
}
