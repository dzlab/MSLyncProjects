using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration;

namespace MyUCMAApp
{
    class MyUCMAApp
    {
        private String _AppID;
        private CollaborationPlatform _CollabPlatform;
        private ApplicationEndpoint _applicationEndpoint;

        static void Main(string[] args)
        {
            MyUCMAApp myucmaapp = new MyUCMAApp();
            myucmaapp.Run();
        }

        void Run()
        {
            _AppID = System.Configuration.ConfigurationManager.AppSettings["ApplicationID"];
            ProvisionedApplicationPlatformSettings settings = new ProvisionedApplicationPlatformSettings("My UCMA Application", _AppID);

            _CollabPlatform = new CollaborationPlatform(settings);
            _CollabPlatform.RegisterForApplicationEndpointSettings(Platform_ApplicationEndpointOwnerDiscovered);
            
            Console.WriteLine("Starting collaboration platform");
            _CollabPlatform.BeginStartup(PlatformStartupComplete, _CollabPlatform);

            //PauseBeforeContinuing("Press enter to shutdown and exit");

            Console.WriteLine("Shutting down the platform.");
            //ShutdownPlatform();
        }
        private void Platform_ApplicationEndpointOwnerDiscovered(object sender, ApplicationEndpointSettingsDiscoveredEventArgs e)
        {
            Console.WriteLine("Contact discovered: {0}", e.ApplicationEndpointSettings.OwnerUri);
            
            var settings = e.ApplicationEndpointSettings;
            
            settings.AutomaticPresencePublicationEnabled = true;
            settings.Presence.PresentityType = "automaton";
            settings.Presence.Description = "Toujours disponible !";

            _applicationEndpoint = new ApplicationEndpoint(_CollabPlatform, settings);
            Console.WriteLine("Establishing the endopint");
            _applicationEndpoint.BeginEstablish(EndpointEstablishCompleted, null);
        }

        private void PlatformStartupComplete(IAsyncResult ar)
        {
            CollaborationPlatform collabPlatform = ar.AsyncState as CollaborationPlatform;
            collabPlatform.EndStartup(ar);
            Console.WriteLine("Collaboration platform associated with the provisioned application with ID {0} has been started", _AppID);
        }

        private void EndpointEstablishCompleted(IAsyncResult ar)
        {
            _applicationEndpoint.EndEstablish(ar);
            Console.WriteLine("The application endpoint owned by URI {0} is established and registered", _applicationEndpoint);
        }
    }
}
