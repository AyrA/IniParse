using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IniParse
{
    /// <summary>
    /// Provides Read/Write access to an ini file
    /// </summary>
    public class IniFile : Validateable
    {
        /// <summary>
        /// INI sections
        /// </summary>
        private List<IniSection> _sections;

        /// <summary>
        /// Character to introduce commented lines
        /// </summary>
        /// <remarks>The default is a semicolon, but a '#' is also common</remarks>
        public char CommentChar { get; set; } = ';';

        /// <summary>
        /// Gets or sets the handling of whitespace in the INI file
        /// </summary>
        /// <remarks>
        /// This has only an effect when loading the file.
        /// Writing is always done "as-is"
        /// </remarks>
        public WhitespaceMode WhitespaceHandling { get; set; } = WhitespaceMode.AsIs;

        /// <summary>
        /// Gets or sets case sensitivity handling in the INI file
        /// </summary>
        /// <remarks>
        /// This has no effect when exporting to file,
        /// it only affects loading, searching and editing.
        /// This setting propagates to the sections when loading but not when it's changed later.
        /// </remarks>
        public CaseSensitivity CaseHandling { get; set; } = CaseSensitivity.AsIs;

        public InvalidLineMode InvalidLineHandling { get; set; } = InvalidLineMode.Throw;

        /// <summary>
        /// Gets all sections of this file
        /// </summary>
        public IniSection[] Sections
        {
            get
            {
                return _sections.ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the comments that follow the last section
        /// </summary>
        public string[] EndComments { get; set; }

        /// <summary>
        /// Gets all section names
        /// </summary>
        /// <remarks>This is in the order they're defined in the INI file</remarks>
        public string[] Names
        {
            get
            {
                return Sections.Select(m => m.Name).ToArray();
            }
        }

        /// <summary>
        /// Gets a named section
        /// </summary>
        /// <param name="name">Section name</param>
        /// <returns>Section, or null if not found</returns>
        public IniSection this[string name]
        {
            get
            {
                return Sections.FirstOrDefault(m => SecEq(m.Name, name));
            }
        }

        /// <summary>
        /// Gets or sets a setting directly
        /// </summary>
        /// <param name="SectionName">Section name</param>
        /// <param name="SettingName">Setting name</param>
        /// <returns>Setting value, null if setting and/or section not found</returns>
        /// <remarks>Setting a value to null deletes it, and the section if no settings remain</remarks>
        public string this[string SectionName, string SettingName]
        {
            get
            {
                var S = _sections.FirstOrDefault(m => SecEq(m.Name, SectionName));
                if (S != null)
                {
                    var Setting = S[SettingName];
                    if (Setting != null)
                    {
                        return Setting.Value;
                    }
                }
                return null;
            }
            set
            {
                //Setting name must be set
                if (SettingName == null)
                {
                    throw new ArgumentException("Setting name can't be null");
                }
                var S = _sections.FirstOrDefault(m => SecEq(m.Name, SectionName));
                if (S == null)
                {
                    //This is an attempt to delete. Cancel here because the section doesn't exists
                    if (value == null)
                    {
                        return;
                    }
                    //Add new section
                    S = AddSection(SectionName);
                }
                if (value == null)
                {
                    //Delete a setting
                    var Setting = S[SettingName];
                    if (Setting != null)
                    {
                        S.Settings.Remove(Setting);
                    }
                    if (S.Settings.Count == 0)
                    {
                        _sections.Remove(S);
                    }
                }
                else
                {
                    //Add/Update a setting
                    var Setting = S[SettingName];
                    if (Setting == null)
                    {
                        S.Settings.Add(new IniSetting(SettingName, value));
                    }
                    else
                    {
                        Setting.Value = value;
                    }
                }
            }
        }

        /// <summary>
        /// Creates an empty ini file
        /// </summary>
        public IniFile()
        {
            _sections = new List<IniSection>();
        }

        /// <summary>
        /// Loads an ini file from the given file name
        /// </summary>
        /// <param name="FileName">File name</param>
        public IniFile(string FileName)
        {
            using (var SR = File.OpenText(FileName))
            {
                ReadData(SR).Wait();
            }
        }

        /// <summary>
        /// Loads an ini file from the given stream
        /// </summary>
        /// <param name="IniContent">INI content</param>
        /// <param name="IniEncoding">Stream Encoding, default is UTF-8</param>
        /// <remarks>The stream is not closed</remarks>
        public IniFile(Stream IniContent, Encoding IniEncoding = null)
        {
            if (IniEncoding == null)
            {
                IniEncoding = Encoding.UTF8;
            }
            using (var SR = new StreamReader(IniContent, IniEncoding, true, 1024, true))
            {
                ReadData(SR).Wait();
            }
        }

        /// <summary>
        /// Removes a named section
        /// </summary>
        /// <param name="SectionName">Section name</param>
        public void RemoveSection(string SectionName)
        {
            _sections = _sections
                .Where(m => !SecEq(m.Name, SectionName))
                .ToList();
        }

        /// <summary>
        /// Removes the section at the given position
        /// </summary>
        /// <param name="Index">Section position</param>
        public void RemoveSectionAt(int Index)
        {
            _sections.RemoveAt(Index);
        }

        /// <summary>
        /// Inserts a new section at the given position
        /// </summary>
        /// <param name="Index">Position</param>
        /// <param name="SectionName">Section name</param>
        /// <returns>New section</returns>
        public IniSection InsertSection(int Index, string SectionName)
        {
            var Section = new IniSection(SectionName);
            InsertSection(Index, Section);
            return Section;
        }

        /// <summary>
        /// Inserts a section at the given position
        /// </summary>
        /// <param name="Index">Position</param>
        /// <param name="Section">Existing section</param>
        public void InsertSection(int Index, IniSection Section)
        {
            if (_sections.Any(m => SecEq(m.Name, Section.Name)))
            {
                throw new ArgumentException("A section with this name already exists");
            }
            _sections.Insert(Index, Section);
        }

        /// <summary>
        /// Adds a new section to the end of the list
        /// </summary>
        /// <param name="SectionName">Section name</param>
        /// <returns>New Section</returns>
        public IniSection AddSection(string SectionName)
        {
            var Section = new IniSection(SectionName);
            AddSection(Section);
            return Section;
        }

        /// <summary>
        /// Adds an existing section to the list
        /// </summary>
        /// <param name="Section">Existing section</param>
        public void AddSection(IniSection Section)
        {
            //Null section is always first
            if (Section.Name == null)
            {
                InsertSection(0, Section);
            }
            else
            {
                if (_sections.Any(m => SecEq(m.Name, Section.Name)))
                {
                    throw new ArgumentException("A section with this name already exists");
                }
                _sections.Add(Section);
            }
        }

        /// <summary>
        /// Exports the contents in this instance as an INI file
        /// </summary>
        /// <param name="SW">Location to write contents to</param>
        /// <returns>Task</returns>
        public async Task ExportFile(StreamWriter SW)
        {
            Validate();

            var EmptySection = Sections.FirstOrDefault(m => m.Name == null);
            if (EmptySection != null)
            {
                await EmptySection.ExportSection(SW, CommentChar);
            }
            foreach (var Section in Sections.Where(m => m.Name != null))
            {
                await Section.ExportSection(SW, CommentChar);
            }
            if (!Tools.IsEmpty(EndComments))
            {
                foreach (var L in EndComments)
                {
                    await SW.WriteLineAsync($"{CommentChar}{L}");
                }
            }
        }

        /// <summary>
        /// Loads ini content from an existing file
        /// </summary>
        /// <param name="FileName">File name</param>
        /// <returns>Task</returns>
        /// <remarks>This discards any existing data</remarks>
        public async Task Load(string FileName)
        {
            using (var SR = File.OpenText(FileName))
            {
                await Load(SR);
            }
        }

        /// <summary>
        /// Loads ini content 
        /// </summary>
        /// <param name="SR">Open stream reader</param>
        /// <returns>Task</returns>
        /// <remarks>This discards any existing data</remarks>
        public async Task Load(StreamReader SR)
        {
            EndComments = null;
            _sections.Clear();
            await ReadData(SR);
        }

        /// <summary>
        /// Sorts the sections in this file
        /// </summary>
        /// <param name="Ascending">Sort section names ascending</param>
        /// <param name="Recursive">Recursively sort sections too</param>
        /// <remarks>The "null section" (if existing) is always first</remarks>
        public void Sort(bool Ascending = true, bool Recursive = false)
        {
            var NullSection = _sections.FirstOrDefault(m => m.Name == null);

            if (Ascending)
            {
                _sections = _sections.OrderBy(m => m.Name).ToList();
            }
            else
            {
                _sections = _sections.OrderByDescending(m => m.Name).ToList();
            }
            if (Recursive)
            {
                foreach (var s in _sections)
                {
                    s.Sort(Ascending);
                }
            }
            //Null section is always first
            if (NullSection != null)
            {
                _sections.Remove(NullSection);
                _sections.Insert(0, NullSection);
            }
        }

        /// <summary>
        /// Checks if two section names are equal under the current <see cref="CaseHandling"/> flags
        /// </summary>
        /// <param name="A">Section Name</param>
        /// <param name="B">Section name</param>
        /// <returns>true, if considered identical</returns>
        private bool SecEq(string A, string B)
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
            if (CaseHandling.HasFlag(CaseSensitivity.CaseInsensitiveSection))
            {
                return A.ToLower() == B.ToLower();
            }
            return A == B;
        }

        /// <summary>
        /// Reads an ini file asynchronously
        /// </summary>
        /// <param name="SR">Open Stream Reader</param>
        /// <returns>Task</returns>
        private async Task ReadData(StreamReader SR)
        {
            int LineCount = 0;
            var TempSections = new List<IniSection>();
            //Currently active section
            IniSection CurrentSection = null;
            //Comment cache
            var Comments = new List<string>();
            //Matches sections and extracts the name
            var Section = new Regex(@"^\s*\[(.*)\]\s*$");
            //Matches settings and extracts name + value
            var Setting = new Regex(@"^([^=]*)=(.*)$");
            //Currently processed line
            string Line;
            do
            {
                ++LineCount;
                Line = await SR.ReadLineAsync();
                if (Line != null)
                {
                    //Skip over empty lines
                    if (Line.Trim() == "")
                    {
                        continue;
                    }
                    if (Line[0] == CommentChar)
                    {
                        //Add the comment without the comment character
                        //Comments are always preserved regardless of the IgnoreWhitespace setting
                        Comments.Add(Line.Substring(1));
                    }
                    else if (Section.IsMatch(Line))
                    {
                        var SectionName = Section.Match(Line).Groups[1].Value;
                        if (WhitespaceHandling.HasFlag(WhitespaceMode.TrimSections))
                        {
                            SectionName = SectionName.Trim();
                        }
                        //Save old section
                        if (CurrentSection != null && !TempSections.Contains(CurrentSection))
                        {
                            TempSections.Add(CurrentSection);
                        }
                        //Get existing section of the same name (if any)
                        CurrentSection = TempSections.FirstOrDefault(m => SecEq(SectionName, m.Name));
                        if (CurrentSection == null)
                        {
                            CurrentSection = new IniSection(SectionName);
                        }
                        if (Comments.Count > 0)
                        {
                            //Add new comments (if any) to the existing section
                            if (Tools.IsEmpty(CurrentSection.Comments))
                            {
                                CurrentSection.Comments = Comments.ToArray();
                            }
                            else
                            {
                                CurrentSection.Comments = CurrentSection.Comments.Concat(Comments).ToArray();
                            }
                            Comments.Clear();
                        }
                    }
                    else if (Setting.IsMatch(Line))
                    {
                        //Setting for the current section
                        var Matches = Setting.Match(Line);
                        var SettingName = Matches.Groups[1].Value;
                        var SettingValue = Matches.Groups[2].Value;
                        if (WhitespaceHandling.HasFlag(WhitespaceMode.TrimNames))
                        {
                            SettingName = SettingName.Trim();
                        }
                        if (WhitespaceHandling.HasFlag(WhitespaceMode.TrimValues))
                        {
                            SettingValue = SettingValue.Trim();
                        }
                        var CurrentSetting = new IniSetting(SettingName, SettingValue);
                        //Create a "null" section for settings that appear before the first section
                        if (CurrentSection == null)
                        {
                            CurrentSection = new IniSection();
                        }
                        if (Comments.Count > 0)
                        {
                            CurrentSetting.Comments = Comments.ToArray();
                            Comments.Clear();
                        }
                        CurrentSection.Settings.Add(CurrentSetting);
                    }
                    else
                    {
                        switch (InvalidLineHandling)
                        {
                            case InvalidLineMode.Throw:
                                throw new InvalidDataException($"Line {LineCount} is neither section, setting, comment, or empty: {Line}");
                            case InvalidLineMode.Skip:
                                break;
                            case InvalidLineMode.Convert:
                                Comments.Add(Line);
                                break;
                            default:
                                throw new NotImplementedException(nameof(InvalidLineMode));
                        }
                    }
                }
            } while (Line != null);
            //Apply case handling to all sections
            foreach (var Entry in TempSections)
            {
                Entry.CaseHandling = CaseHandling;
            }
            _sections = TempSections;
            //Add last section
            if (CurrentSection != null)
            {
                _sections.Add(CurrentSection);
            }
            if (Comments.Count > 0)
            {
                EndComments = Comments.ToArray();
            }
        }

        #region Overrides

        /// <summary>
        /// Gets a string representation of this instance
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"INI file with {_sections.Count} sections";
        }

        /// <summary>
        /// Validates this file
        /// </summary>
        public override void Validate()
        {
            var N = Names;
            if (N.Count(m => m == null) > 1)
            {
                var ex = new ValidationException($"Duplicate null-section");
                var Indexes = _sections
                        .Select((v, i) => v.Name == null ? i : -1)
                        .Where(m => m >= 0)
                        .ToArray();
                ex.Data.Add("Sections", Indexes);
                throw ex;
            }
            foreach (var Name in N)
            {
                if (N.Count(m => m == Name) > 1)
                {
                    var ex = new ValidationException($"Duplicate section: {Name}");
                    var Indexes = _sections
                        .Select((v, i) => v.Name == Name ? i : -1)
                        .Where(m => m >= 0)
                        .ToArray();
                    ex.Data.Add("Sections", Indexes);
                    throw ex;
                }
            }
            foreach (var S in Sections)
            {
                S.Validate();
            }
        }

        #endregion

        #region Static Members

        /// <summary>
        /// Loads an ini file asynchronously from a file
        /// </summary>
        /// <param name="FileName">File name</param>
        /// <returns>INI file</returns>
        public static async Task<IniFile> FromFile(string FileName)
        {
            var IF = new IniFile();
            using (var SR = File.OpenText(FileName))
            {
                await IF.ReadData(SR);
            }
            return IF;
        }
        /// <summary>
        /// Loads an ini file asynchronously from a stream
        /// </summary>
        /// <param name="IniContent">INI stream</param>
        /// <param name="IniEncoding">Stream encoding, UTF-8 by default</param>
        /// <returns>INI file</returns>
        public static async Task<IniFile> FromFile(Stream IniContent, Encoding IniEncoding = null)
        {
            var IF = new IniFile();
            if (IniEncoding == null)
            {
                IniEncoding = Encoding.UTF8;
            }
            using (var SR = new StreamReader(IniContent, IniEncoding, true, 1024, true))
            {
                await IF.ReadData(SR);
            }
            return IF;
        }

        #endregion
    }
}
