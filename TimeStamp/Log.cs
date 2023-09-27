using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeStamp
{
    public static class Log
    {
        public static readonly string LogFilePath = ".\\StampLog.txt";

        public static void Add(string line)
        {
            File.AppendAllLines(LogFilePath, new[] { DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " " + line });
        }

    }
}
