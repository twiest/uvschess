using System;
using System.Collections.Generic;
using System.Text;

namespace UvsChess
{
    public class Logger
    {
        public static void Log(string msg)
        {
#if Console
            Console.WriteLine(msg);
#endif
        }
    }
}
