using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UCMAAppMgmt;

namespace ConsoleUCMA
{
    class Program
    {
        static void Main(string[] args)
        {
            ApplicationEndpointStarter sample = new ApplicationEndpointStarter (new ConsoleLogger());
            
            sample.Start();
            sample.WaitForStartup();

            System.Console.WriteLine("Hit enter to stop");
            System.Console.ReadLine();

            sample.ShutDown();
            sample.WaitForShutdown();

            System.Console.WriteLine("Hit enter to exit");
            System.Console.ReadLine();
        }
    }
}
