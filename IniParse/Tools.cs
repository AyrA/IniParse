using System;
using System.Collections.Generic;
using System.Text;

namespace IniParse
{
    internal class Tools
    {
        public static bool IsEmpty(Array a)
        {
            return a == null || a.Length == 0;
        }
    }
}
