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
    public class IniSection : Validateable
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
        /// Gets or sets the case sensitivity handling when searching for names
        /// </summary>
        public CaseSensitivity CaseHandling { get; set; }

        /// <summary>
        /// Gets the first setting with the given name
        /// </summary>
        /// <param name="name">Setting name</param>
        /// <returns>Setting</returns>
        public IniSetting this[string name]
        {
            get
            {
                return Settings.FirstOrDefault(m => SetEq(m.Name, name));
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
        /// Gets all settings with the given name
        /// </summary>
        /// <param name="SettingName">Setting name</param>
        /// <returns>Found settings</returns>
        public IniSetting[] GetAll(string SettingName)
        {
            return Settings
                .Where(m => SetEq(m.Name, SettingName))
                .ToArray();
        }

        /// <summary>
        /// Gets all values with the given name
        /// </summary>
        /// <param name="SettingName">Setting name</param>
        /// <returns>Found Values</returns>
        public string[] GetValues(string SettingName)
        {
            return GetAll(SettingName)
                .Select(m => m.Value)
                .ToArray();
        }

        /// <summary>
        /// Removes all settings with the given name according to 
        /// </summary>
        /// <param name="SettingName"></param>
        public void RemoveSettings(string SettingName)
        {
            Settings.RemoveAll(m => SetEq(m.Name, SettingName));
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
                if (!Tools.IsEmpty(Comments))
                {
                    foreach (var L in Comments)
                    {
                        await SW.WriteLineAsync($"{CommentChar}{L}");
                    }
                }
                await SW.WriteLineAsync($"[{Name}]");
            }
            foreach (var S in Settings)
            {
                await S.ExportSetting(SW, CommentChar);
            }
        }

        /// <summary>
        /// Sorts the settings in this section
        /// </summary>
        /// <param name="ascending">Sort ascending</param>
        public void Sort(bool Ascending = true)
        {
            if (Ascending)
            {
                Settings.Sort((m, n) => m.Name.CompareTo(n.Name));
            }
            else
            {
                Settings.Sort((n, m) => m.Name.CompareTo(n.Name));
            }
        }

        /// <summary>
        /// Checks if two setting names are equal under the current <see cref="CaseHandling"/> flags
        /// </summary>
        /// <param name="A">Setting Name</param>
        /// <param name="B">Setting name</param>
        /// <returns>true, if considered identical</returns>
        private bool SetEq(string A, string B)
        {
            if (A == null && B == null)
            {
                return true;
            }
            else if (A == null)
            {
                return false;
            }
            else if (B == null)
            {
                return false;
            }
            if (CaseHandling.HasFlag(CaseSensitivity.CaseInsensitiveSetting))
            {
                return A.ToLower() == B.ToLower();
            }
            return A == B;
        }

        #region Overrides

        /// <summary>
        /// Gets a string representation of this instance
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"INI Section \"{Name}\" with {Settings.Count} settings";
        }

        /// <summary>
        /// Validates this section
        /// </summary>
        /// <remarks>
        /// Sections are allowed to have duplicate settings by default.
        /// Check for duplicates yourself if you don't want them.
        /// </remarks>
        public override void Validate()
        {
            if (Name != null && Name.Any(m => Tools.ForbiddenHeaderChar.Contains(m)))
            {
                var ex = new ValidationException("Forbidden character in section name");
                ex.Data.Add("Section", Name);
                throw ex;
            }
            foreach (var S in Settings)
            {
                try
                {
                    S.Validate();
                }
                catch (ValidationException ex)
                {
                    ex.Data.Add("Section", Name);
                    throw ex;
                }
            }
        }

        #endregion
    }
}
