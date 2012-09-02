using System;

namespace TwoPartyCallControl
{
    public class ConsoleLogger : ILogger
    {
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
