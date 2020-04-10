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
    public class IniFile
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
                return Sections.FirstOrDefault(m => m.Name == name);
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
                .Where(m => m.Name != SectionName)
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
            if (_sections.Any(m => m.Name == Section.Name))
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
                if (_sections.Any(m => m.Name == Section.Name))
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
            var Setting = new Regex(@"^\s*([^=]+)=(.*)$");
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
                        Comments.Add(Line.Substring(1));
                    }
                    else if (Section.IsMatch(Line))
                    {
                        var SectionName = Section.Match(Line).Groups[1].Value;
                        //Save old section
                        if (CurrentSection != null)
                        {
                            TempSections.Add(CurrentSection);
                        }
                        //Get existing section of the same name (if any)
                        CurrentSection = TempSections.FirstOrDefault(m => SectionName.Equals(m.Name));
                        if (CurrentSection == null)
                        {
                            CurrentSection = new IniSection(Section.Match(Line).Groups[1].Value);
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
                        var CurrentSetting = new IniSetting(Matches.Groups[1].Value, Matches.Groups[2].Value);
                        if (Comments.Count > 0)
                        {
                            CurrentSection.Comments = Comments.ToArray();
                            Comments.Clear();
                        }
                        //Create a "null" section for settings that appear before the first section
                        if (CurrentSection == null)
                        {
                            CurrentSection = new IniSection();
                        }
                        CurrentSection.Settings.Add(CurrentSetting);
                    }
                    else
                    {
                        throw new InvalidDataException($"Line {LineCount} is neither section, setting, comment, or empty: {Line}");
                    }
                }
            } while (Line != null);
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
