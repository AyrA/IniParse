using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IniParse
{
    /// <summary>
    /// Represents a section in an ini file
    /// </summary>
    public class IniSection
    {
        /// <summary>
        /// Gets or sets the name of the section
        /// </summary>
        /// <remarks>This can be null for an unnamed section</remarks>
        public string Name { get; set; }

        /// <summary>
        /// Gets the list of settings
        /// </summary>
        public List<IniSetting> Settings { get; private set; }

        /// <summary>
        /// Gets or sets the section header comments
        /// </summary>
        public string[] Comments { get; set; }

        /// <summary>
        /// Gets the setting with the given name
        /// </summary>
        /// <param name="name">Setting name</param>
        /// <returns>Setting</returns>
        public IniSetting this[string name]
        {
            get
            {
                return Settings.FirstOrDefault(m => m.Name == name);
            }
        }

        /// <summary>
        /// Gets the setting at the given position
        /// </summary>
        /// <param name="index">Setting position</param>
        /// <returns>Setting</returns>
        public IniSetting this[int index]
        {
            get
            {
                return Settings[index];
            }
        }

        /// <summary>
        /// Initializes an empty section
        /// </summary>
        public IniSection() : this(null)
        {

        }

        /// <summary>
        /// Initializes a named section
        /// </summary>
        /// <param name="SectionName">Section name</param>
        public IniSection(string SectionName)
        {
            Name = SectionName;
            Settings = new List<IniSetting>();
        }

        /// <summary>
        /// Exports this section
        /// </summary>
        /// <param name="SW">Location to write contents to</param>
        /// <param name="CommentChar">Character for comments</param>
        /// <returns>Task</returns>
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
