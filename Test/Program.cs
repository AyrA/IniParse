using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var IF = new IniParse.IniFile();
            IF.WhitespaceHandling = IniParse.WhitespaceMode.TrimNames | IniParse.WhitespaceMode.TrimSections;
            IF.Load(@"Test.INI").Wait();
            IF.Validate();
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
            /*
            //Export using built-in exporter
            using (var SW = new StreamWriter(Console.OpenStandardOutput()))
            {
                IF.ExportFile(SW).Wait();
            }
            //*/
            Console.Error.WriteLine("#END");
            Console.ReadKey(true);
        }
    }
}
