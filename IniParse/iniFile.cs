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

        public char CommentChar { get; set; } = ';';

        public IniSection[] Sections
        {
            get
            {
                return _sections.ToArray();
            }
        }

        public string[] EndComments { get; set; }

        public string[] Names
        {
            get
            {
                return Sections.Select(m => m.Name).ToArray();
            }
        }

        public IniSection this[string name]
        {
            get
            {
                return Sections.FirstOrDefault(m => m.Name == name);
            }
        }

        public IniFile()
        {
            _sections = new List<IniSection>();
        }

        public IniFile(string FileName)
        {
            using (var SR = File.OpenText(FileName))
            {
                ReadData(SR).Wait();
            }
        }

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

        public void RemoveSection(string SectionName)
        {
            _sections = _sections
                .Where(m => m.Name != SectionName)
                .ToList();
        }

        public void RemoveSectionAt(int Index)
        {
            _sections.RemoveAt(Index);
        }

        public IniSection InsertSection(int Index, string SectionName)
        {
            var Section = new IniSection(SectionName);
            InsertSection(Index, Section);
            return Section;
        }

        public void InsertSection(int Index, IniSection Section)
        {
            if (_sections.Any(m => m.Name == Section.Name))
            {
                throw new ArgumentException("A section with this name already exists");
            }
            _sections.Insert(Index, Section);
        }

        public IniSection AddSection(string SectionName)
        {
            var Section = new IniSection(SectionName);
            AddSection(Section);
            return Section;
        }

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

        public static async Task<IniFile> FromFile(string FileName)
        {
            var IF = new IniFile();
            using (var SR = File.OpenText(FileName))
            {
                await IF.ReadData(SR);
            }
            return IF;
        }
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
