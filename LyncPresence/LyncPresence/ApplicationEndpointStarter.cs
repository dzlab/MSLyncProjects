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
    public class ApplicationEndpointStarter
    {
        CollaborationPlatform _collaborationPlatform;
        ApplicationEndpoint _appEndpoint;
        
        AVCallHelper _avHelper = new AVCallHelper();

        // A wait handle for startup and one for shutdown
        // They are set to unsignaled to start
        ManualResetEvent _startupWaitHandle = new ManualResetEvent(false);
        ManualResetEvent _shutdownWaitHandle = new ManualResetEvent(false);

        string _recipientSipUri;

        public ApplicationEndpointStarter()
        {
            
        }

        public ApplicationEndpoint GetApplicationEndpoint()
        {
            return _appEndpoint;
        }

        public void Start()
        {
            // Get the trusted application settings from App.config
            string appUserAgent = "MyUCMAApp";
            string applicationId = "urn:application:myucmaapp";
            string localhost = System.Net.Dns.GetHostEntry("localhost").HostName;
            int listeningPort = int.Parse("10600");

            _recipientSipUri = "sip:smanai@mocass.rd.francetelecom.fr";

            // Create a settings object that will hold all the previous information
            var settings = new ProvisionedApplicationPlatformSettings(appUserAgent, applicationId);
            //ServerPlatformSettings settings = new ServerPlatformSettings(applicationName, localhost, listeningPort, gruu, CertificateHelper.GetLocalCertificate());

            // Create a new collaboration platform with the settings
            _collaborationPlatform = new CollaborationPlatform(settings);

            // Start the platform as an asynchronous operation
            _collaborationPlatform.BeginStartup(OnPlatformStartupCompleted, null);
        }

        private void OnPlatformStartupCompleted(IAsyncResult result)
        {
            try
            {
                // Finish the startup operation
                _collaborationPlatform.EndStartup(result);

                // Collaboration platform started

                EstablishApplicationEndpoint();
            }
            catch (RealTimeException ex)
            {
                Console.Write(ex); 
            }
        }

        private void EstablishApplicationEndpoint()
        {
            string contactUri = ConfigurationManager.AppSettings["contactUri"];
            string proxyServerFqdn = ConfigurationManager.AppSettings["proxyServerFqdn"];
            int tlsPort = 5061;
            
            // ApplicationEndpointSettings settings = new ApplicationEndpointSettings(contactUri);                        

            ApplicationEndpointSettings settings = new ApplicationEndpointSettings(contactUri, proxyServerFqdn, tlsPort);
            settings.UseRegistration = true;

            settings.AutomaticPresencePublicationEnabled = true;
            settings.Presence.PresentityType = "automaton";
            settings.Presence.Description = "Always available !";

            PreferredServiceCapabilities capabilities = settings.Presence.PreferredServiceCapabilities;
            capabilities.InstantMessagingSupport = CapabilitySupport.Supported;
            capabilities.AudioSupport = CapabilitySupport.Supported;
            capabilities.VideoSupport = CapabilitySupport.Supported;
            capabilities.ApplicationSharingSupport = CapabilitySupport.Supported;

            _appEndpoint = new ApplicationEndpoint(_collaborationPlatform, settings);
            _appEndpoint.StateChanged += new EventHandler<LocalEndpointStateChangedEventArgs>(_appEndpoint_StateChanged);

            // Register to be notified of incoming calls
            _appEndpoint.RegisterForIncomingCall<AudioVideoCall>(OnAudioVideoCallReceived);
            
            // Register to be notified of incoming instant messaging calls
            _appEndpoint.RegisterForIncomingCall<InstantMessagingCall>(OnInstantMessagingCallReceived);

            //Console.WriteLine("Establishing application endpoint...");
            _appEndpoint.BeginEstablish(OnApplicationEndpointEstablishCompleted, null);
        }

        private void OnApplicationEndpointEstablishCompleted(IAsyncResult result)
        {
            try
            {
                _appEndpoint.EndEstablish(result);

                Console.WriteLine("Application endpoint established.");
                //Console.WriteLine("Contact URI: {0}", _appEndpoint.OwnerUri);
                //Console.WriteLine("Endpoint URI: {0}", _appEndpoint.EndpointUri);                

                _startupWaitHandle.Set();
            }
            catch (RealTimeException ex)
            {
                Console.Write("Application endpoint establishment failed: {0}", ex);
            }
            catch (Exception e)
            {
                Console.Write("Application endpoint establishment failed: {0}", e);
            }
        }

        internal void OnInstantMessagingCallReceived(object sender, CallReceivedEventArgs<InstantMessagingCall> e)
        {
            Console.WriteLine("An incoming Insatnt Messagine call is received");

            IMCallHandler handler = new IMCallHandler(e.Call, _recipientSipUri);
            handler.HandleIncomingCall();

            //handler.Echo();
                                    
        }

        
        

        internal void OnAudioVideoCallReceived (object sender, CallReceivedEventArgs<AudioVideoCall> e)
        {
            
            
            _avHelper.AcceptAVCall(e.Call);
            
        }

        private void _appEndpoint_StateChanged(object sender, LocalEndpointStateChangedEventArgs e)
        {
            Console.WriteLine("Application endpoint state changed from {0} to {1}", e.PreviousState, e.State);
        }

        public void ShutDown()
        {
            Console.WriteLine("Terminating application endpoint...");

            _appEndpoint.BeginTerminate(OnApplicationEndpointTerminateCompleted, null);
        }
        private void OnApplicationEndpointTerminateCompleted(IAsyncResult result)
        {
            try
            {
                _appEndpoint.EndTerminate(result);

                Console.WriteLine("Application endpoint terminated.");

                ShutDownPlatform();
            }
            catch (RealTimeException ex)
            {
                Console.Write("Application endpoint terminated failed: {0}", ex);
            }
        }
        private void ShutDownPlatform()
        {
            Console.WriteLine("Shutting down platform...");
            _collaborationPlatform.BeginShutdown(OnPlatformShutdownCompleted, null);
        }
        private void OnPlatformShutdownCompleted(IAsyncResult result)
        {
            try
            {
                _collaborationPlatform.EndShutdown(result);

                Console.Write("Platform shut down");

                _shutdownWaitHandle.Set();
            }
            catch (RealTimeException ex)
            {
                Console.WriteLine("Platform shutdown failed: {0}", ex);
            }
        }

        /* This method makes caller wait (block it) until startup or application endpoint is completed */
        public void WaitForStartup()
        {
            _startupWaitHandle.WaitOne();
        }
        /* This method blocks the caller until the platform shutdown is completed */
        public void WaitForShutdown()
        {
            _shutdownWaitHandle.WaitOne();
        }

        /* Query the presence of a user, must be called after endpoint successfully established */
        public void QueryUserPresence()
        {            
            //PresenceHelper helper = new PresenceHelper(_appEndpoint);
            //helper.QueryUserPresence("sip:bchihani@mocass.rd.francetelecom.fr");
        }

        public void SendTextToSpeechMessage(string destinationSipUri, string message)
        {
            _avHelper.ExtablishOutboundAVCall(_appEndpoint, destinationSipUri, message);
        }

        public void StartConference(List<string> invitees)
        {
            ConferenceHelper helper = new ConferenceHelper(_appEndpoint);            
            helper.InitiateConference(invitees);
        }
    }
}
