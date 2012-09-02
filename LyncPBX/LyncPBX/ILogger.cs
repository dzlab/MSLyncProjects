using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LyncPBX
{
    public interface ILogger
    {
        void Log(string message);
        
        void Log(string message, Exception ex);

        void Log(string message, params object[] arg);
    }
}
