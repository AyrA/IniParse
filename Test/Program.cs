using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //Load test INI
            var IF = new IniParse.IniFile();
            IF.WhitespaceHandling = IniParse.WhitespaceMode.TrimNames | IniParse.WhitespaceMode.TrimSections;
            IF.CaseHandling = IniParse.CaseSensitivity.CaseInsensitiveSection;
            IF.Load(@"Test.INI").Wait();
            IF.Validate();
            HighlightFile(IF);

            Console.Write(string.Empty.PadRight(Console.BufferWidth * 3, '#'));

            //Create a new INI file
            IF = new IniParse.IniFile();
            //New section
            var TestSection = IF.AddSection("TEST");
            TestSection.Settings.Add(new IniParse.IniSetting("Name", "Value 1"));
            TestSection.Settings.Add(new IniParse.IniSetting("Name", "Value 2")
            {
                Comments = new string[] { "Duplicate setting is possible" }
            });
            TestSection.Settings.Add(new IniParse.IniSetting("\tWhitespace ", " Whitespace Value")
            {
                Comments = new string[] { "Whitespace is always taken as-is when editing" }
            });

            //Adding an empty string section
            TestSection = IF.AddSection("");
            TestSection.Comments = "Empty Section Comment 1||Empty Section Comment 3".Split('|');
            TestSection.Settings.Add(new IniParse.IniSetting("Name", "Value"));

            //Adding the null-section
            TestSection = IF.AddSection((string)null);
            TestSection.Comments = new string[]
            {
                "The comments of the null section serve as file header",
                $"Current Date: {DateTime.Now}",
                $"Creator:      {Environment.UserName}"
            };
            TestSection.Settings.Add(new IniParse.IniSetting("FirstSetting", "Setting without a section"));

            //Add a setting directly
            IF["NewSection1", "NewSetting1"] = "NewValue1";
            IF["NewSection2", "NewSetting2"] = "NewValue2";
            IF["NewSection3", "NewSetting3-1"] = "NewValue3-1";
            IF["NewSection3", "NewSetting3-2"] = "NewValue3-2";
            //Delete a setting directly, this should also delete the section that is now empty
            IF["NewSection2", "NewSetting2"] = null;
            //Delete a setting directly, this should not delete the section because it's not empty
            IF["NewSection3", "NewSetting3-1"] = null;

            IF.EndComments = new string[] { "This will be the last line of the file" };

            //INI file should still be considered valid
            IF.Validate();

            HighlightFile(IF);

            //Try to store an invalid value
            IF["[THIS_IS_VALID]", "THIS=IS=INVALID"] = "THIS=IS=VALID";
            try
            {
                IF.Validate();
                throw new Exception("INI is supposed to be invalid but IF.Validate() did not throw");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Validation failed: {0}", ex.Message);
                foreach (var e in ex.Data.Keys)
                {
                    Console.WriteLine("{0}={1}", e, ex.Data[e]);
                }
            }

            Console.Error.WriteLine("#END");
            Console.ReadKey(true);
        }

        static void HighlightFile(IniParse.IniFile IF)
        {
            //Export file with text formatting for better readability
            foreach (var Section in IF.Sections)
            {
                if (Section.Comments != null)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    foreach (var L in Section.Comments)
                    {
                        Console.WriteLine("{0}{1}", IF.CommentChar, L);
                    }
                }
                if (Section.Name == null)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("<Null Section>");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[{0}]", Section.Name);
                }
                foreach (var Setting in Section.Settings)
                {
                    if (Setting.Comments != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        foreach (var L in Setting.Comments)
                        {
                            Console.WriteLine("{0}{1}", IF.CommentChar, L);
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(Setting.Name);
                    Console.ResetColor();
                    Console.Write('=');
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(Setting.Value);
                }
            }
            if (IF.EndComments != null)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                foreach (var L in IF.EndComments)
                {
                    Console.WriteLine("{0}{1}", IF.CommentChar, L);
                }
            }
            Console.ResetColor();
        }
    }
}
