using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Signaling;
using System.Threading;
using System.Configuration;
using Microsoft.Rtc.Collaboration.AudioVideo;

namespace LyncPresence
{
    class Program
    {        
        static void Main(string[] args)
        {                        
            ApplicationEndpointStarter ucma = new ApplicationEndpointStarter();
            ucma.Start();
            
            Console.Read();
            PresenceHelper helper = new PresenceHelper(ucma.GetApplicationEndpoint());
            helper.QueryUserPresence("sip:bchihani@mocass.rd.francetelecom.fr");

            Console.Read();
            Console.Read();
                        
            ucma.ShutDown();
        }        
    }
}
