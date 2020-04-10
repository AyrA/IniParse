using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IniParse
{
    /// <summary>
    /// Represents a single setting in an INI file
    /// </summary>
    public class IniSetting : Validateable
    {
        /// <summary>
        /// Gets or sets the comments for this setting
        /// </summary>
        public string[] Comments { get; set; }
        /// <summary>
        /// Gets or sets the name of this setting
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the value of this setting
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Initializes an empty setting
        /// </summary>
        public IniSetting()
        {
            Name = "";
            Value = "";
        }

        /// <summary>
        /// Initilizes a given setting
        /// </summary>
        /// <param name="SettingName">Setting name</param>
        /// <param name="SettingValue">Setting value</param>
        public IniSetting(string SettingName, string SettingValue)
        {
            Name = SettingName;
            Value = SettingValue;
        }

        /// <summary>
        /// Exports this setting
        /// </summary>
        /// <param name="SW">Location to write contents to</param>
        /// <param name="CommentChar">Character for comments</param>
        /// <returns>Task</returns>
        public async Task ExportSetting(StreamWriter SW, char CommentChar)
        {
            if (!Tools.IsEmpty(Comments))
            {
                foreach (var L in Comments)
                {
                    await SW.WriteLineAsync($"{CommentChar}{L}");
                }
            }
            await SW.WriteLineAsync($"{Name}={Value}");
        }

        /// <summary>
        /// Validates this setting
        /// </summary>
        public override void Validate()
        {
            ValidationException ex = null;
            if (Name == null)
            {
                ex = new ValidationException("Setting name can't be null.");
            }
            if (Value == null && ex == null)
            {
                ex = new ValidationException("Setting value can't be null.");
            }
            if (ex != null)
            {
                ex.Data.Add("Setting.Name", Name);
                ex.Data.Add("Setting.Value", Value);
                throw ex;
            }
        }
    }
}
