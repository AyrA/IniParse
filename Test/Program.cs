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
            var IF = new IniParse.IniFile(@"E:\Projects\PlanAndPlayCore\PlanAndPlayCore\CONFIG.INI");
            //Export file with text formatting
            foreach (var Section in IF.Sections)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[{0}]", Section.Name);
                foreach (var Setting in Section.Settings)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(Setting.Name);
                    Console.ResetColor();
                    Console.Write('=');
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(Setting.Value);
                }
            }
            Console.ResetColor();
            //Export using built-in exporter
            using (var SW = new StreamWriter(Console.OpenStandardOutput()))
            {
                IF.ExportFile(SW).Wait();
            }
            Console.Error.WriteLine("#END");
            Console.ReadKey(true);
        }
    }
}
