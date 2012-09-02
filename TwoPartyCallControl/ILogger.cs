using System;

namespace TwoPartyCallControl
{
    public interface ILogger
    {
        void Log(string message);

        void Log(string message, Exception ex);

        void Log(string message, params object[] arg);
    }
}
