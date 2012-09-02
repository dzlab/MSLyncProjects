using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration;

namespace TwoPartyCallControl
{
    class Program
    {
        static void Main(string[] args)
        {
            ApplicationEndpoint appEndpoint;
            ILogger logger = new ConsoleLogger();

            ApplicationEndpointStarter starter = new ApplicationEndpointStarter(logger);
            starter.Start();
            starter.WaitForStartup();
            appEndpoint = starter.ApplicationEndpoint;

            Console.WriteLine("Select one of the following:");
            Console.WriteLine("1. Outbound IM Call");
            Console.WriteLine("2. Outbound A/V Call");
            Console.WriteLine("3. Toast Message");
            Console.WriteLine("4. Accept Incoming Audio Call");
            Console.WriteLine("5. Decline Incoming Audio Call");
            Console.WriteLine("6. Selectively Accept Calls");
            Console.WriteLine("7. Forward Incoming Audio Call");
            Console.WriteLine("8. Accept Transfers and Forwards");
            Console.WriteLine("9. Transfer Call");
            Console.WriteLine("10. Send Instant Message");
            Console.WriteLine("11. Recover Call From Failed Transfer");
            Console.Write("?");
            string entry = Console.ReadLine();

            ISampleComponent sample = null;

            switch (entry)
            {
                case "1":
                    sample = new OutboundInstantMessagingCallSample(appEndpoint, logger);
                    break;
                case "2":
                    sample = new OutboundAudioCallSample(appEndpoint, logger);
                    break;
                case "3":
                    sample = new ToastMessageSample(appEndpoint, logger);
                    break;
                case "4":
                    sample = new AcceptIncomingAudioCallSample(appEndpoint, logger);
                    break;
                case "5":
                    sample = new DeclineIncomingAudioCallSample(appEndpoint, logger);
                    break;
                case "6":
                    sample = new SelectivelyAcceptCallsSample(appEndpoint, logger);
                    break;
                case "7":
                    sample = new ForwardIncomingAudioCallSample(appEndpoint, logger);
                    break;
                case "8":
                    sample = new AcceptTransfersAndForwardsSample(appEndpoint, logger);
                    break;
                case "9":
                    sample = new TransferCallSample(appEndpoint, logger);
                    break;
                case "10":
                    sample = new SendInstantMessageSample(appEndpoint, logger);
                    break;
                case "11":
                    sample = new RecoverFailedTransferSample(appEndpoint, logger);
                    break;
            }

            if (sample != null)
            {
                sample.Start();
                Console.WriteLine("Press enter to finish");
                Console.ReadLine();
                sample.Stop();
            }

            starter.ShutDown();
            starter.WaitForShutdown();

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }
}
