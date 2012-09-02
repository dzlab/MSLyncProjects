using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LyncPBX
{
    class Program
    {
        static void Main(string[] args)
        {
            ApplicationEndpointStarter sample = new ApplicationEndpointStarter(new ConsoleLogger());

            sample.Start();
            sample.WaitForStartup();

            System.Console.WriteLine("Hit enter to transfer call");
            System.Console.ReadLine();

            sample.BeginTransferEndpoint("sip:bsamson@mocass.rd.francetelecom.fr", "sip:smanai@mocass.rd.francetelecom.fr");

            System.Console.WriteLine("Hit enter to stop");
            System.Console.ReadLine();
            
            sample.ShutDown();
            sample.WaitForShutdown();

            System.Console.WriteLine("Hit enter to exit");
            System.Console.ReadLine();
        }
    }
}
