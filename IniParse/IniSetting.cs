using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IniParse
{
    public class IniSetting
    {
        public string[] Comments { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public IniSetting()
        {

        }

        public IniSetting(string SettingName, string SettingValue)
        {
            Name = SettingName;
            Value = SettingValue;
        }

        public async Task Export(StreamWriter SW, char CommentChar)
        {
            if(!Tools.IsEmpty(Comments))
            {
                foreach(var L in Comments)
                {
                    await SW.WriteLineAsync($"{CommentChar}{L}");
                }
            }
            await SW.WriteLineAsync($"{Name}={Value}");
        }
    }
}
