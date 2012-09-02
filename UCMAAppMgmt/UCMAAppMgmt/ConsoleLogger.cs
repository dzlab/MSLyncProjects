using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UCMAAppMgmt
{
    public class ConsoleLogger : ILogger
    {
        public ConsoleLogger()
        {
        }
        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void Log(string message, Exception ex)
        {
            Console.WriteLine(message, ex);
        }

        public void Log(string message, params object[] arg)
        {
            Console.WriteLine(message, arg);
        }
    }
}
