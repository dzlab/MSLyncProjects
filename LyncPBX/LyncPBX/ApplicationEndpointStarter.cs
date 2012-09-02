using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Signaling;
using System.Threading;
using System.Configuration;
using Microsoft.Rtc.Collaboration.AudioVideo;

namespace LyncPBX
{
    
    public class ApplicationEndpointStarter
    {
        CollaborationPlatform _collaborationPlatform;
        ApplicationEndpoint _appEndpoint;

        ILogger _logger;

        AVCallHelper _avHelper = new AVCallHelper();

        // A wait handle for startup and one for shutdown
        // They are set to unsignaled to start
        ManualResetEvent _startupWaitHandle = new ManualResetEvent(false);
        ManualResetEvent _shutdownWaitHandle = new ManualResetEvent(false);

        string _recipientSipUri;
        string _supervisorSipUri;

        public ApplicationEndpointStarter(ILogger logger)
        {
            _logger = logger;
        }        

        public void Start()
        {
            // Get the trusted application settings from App.config
            string appUserAgent = "MyUCMAApp";
            string applicationId = ConfigurationManager.AppSettings["applicationId"];
            string applicationName = ConfigurationManager.AppSettings["applicationName"];
            string localhost = System.Net.Dns.GetHostEntry("localhost").HostName;
            int listeningPort = int.Parse(ConfigurationManager.AppSettings["listeningPort"]);
            string gruu = ConfigurationManager.AppSettings["gruu"];

            _recipientSipUri = ConfigurationManager.AppSettings["recipientSipUri"];

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

                _logger.Log("Collaboration platform started.");

                EstablishApplicationEndpoint();
            }
            catch (RealTimeException ex)
            {
                _logger.Log("Platform startup failed: {0}", ex);
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

            _logger.Log("Establishing application endpoint...");
            _appEndpoint.BeginEstablish(OnApplicationEndpointEstablishCompleted, null);
        }

        private void OnApplicationEndpointEstablishCompleted(IAsyncResult result)
        {
            try
            {
                _appEndpoint.EndEstablish(result);

                _logger.Log("Application endpoint established.");
                _logger.Log("Contact URI: {0}", _appEndpoint.OwnerUri);
                _logger.Log("Endpoint URI: {0}", _appEndpoint.EndpointUri);

                //_avHelper.ExtablishOutboundAVCall(_appEndpoint, "sip:bsamson@mocass.rd.francetelecom.fr");

                _startupWaitHandle.Set();
            }
            catch (RealTimeException ex)
            {
                _logger.Log("Application endpoint establishment failed: {0}", ex);
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
            
            
            //_avHelper.AcceptAVCall(e.Call);
            _avHelper.EscalateCalltoConference(e.Call, "sip:bsamson@mocass.rd.francetelecom.fr" );  
            /*
            if (e.CallToBeReplaced == null)
            {
                Console.WriteLine("An initial audio/video call is received");
                // initial call (not a self-transfer).
                SupervisorJoinCallSession session = new SupervisorJoinCallSession(e.Call, _recipientSipUri, _supervisorSipUri);
                session.HandleIncomingCall();
            }
            else
            {
                Console.WriteLine("A self-transfer for a supervisor join audio/video call is received");
                // Self-transfer for a supervisor join. Get the session object out of the ApplicationContext property
                // on the replaced call (where we put it before doing the self-transfer).
                SupervisorJoinCallSession session = e.CallToBeReplaced.Conversation.ApplicationContext as SupervisorJoinCallSession;
                session.HandleIncomingSelfTransfer(e.Call);
            }
            */
        }

        private void _appEndpoint_StateChanged(object sender, LocalEndpointStateChangedEventArgs e)
        {
            _logger.Log("Application endpoint state changed from {0} to {1}", e.PreviousState, e.State);
        }

        public void ShutDown()
        {
            _logger.Log("Terminating application endpoint...");

            _appEndpoint.BeginTerminate(OnApplicationEndpointTerminateCompleted, null);
        }
        private void OnApplicationEndpointTerminateCompleted(IAsyncResult result)
        {
            try
            {
                _appEndpoint.EndTerminate(result);

                _logger.Log("Application endpoint terminated.");

                ShutDownPlatform();
            }
            catch (RealTimeException ex)
            {
                _logger.Log("Application endpoint terminated failed: {0}", ex);
            }
        }
        private void ShutDownPlatform()
        {
            _logger.Log("Shutting down platform...");
            _collaborationPlatform.BeginShutdown(OnPlatformShutdownCompleted, null);
        }
        private void OnPlatformShutdownCompleted(IAsyncResult result)
        {
            try
            {
                _collaborationPlatform.EndShutdown(result);

                _logger.Log("Platform shut down");

                _shutdownWaitHandle.Set();
            }
            catch (RealTimeException ex)
            {
                _logger.Log("Platform shutdown failed: {0}", ex);
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
            /*
            PresenceWatcher watchdog = new PresenceWatcher(_appEndpoint);
            string[] targets = new string[] { "sip:bchihani@mocass.rd.francetelecom.fr" };
            string[] categories = new string[] { "state" };
            watchdog.QueryRemotePresence(targets, categories);
            */
            PresenceHelper helper = new PresenceHelper(_appEndpoint);
            helper.QueryUserPresence("sip:bsamson@mocass.rd.francetelecom.fr");
        }

        public void BeginTransferEndpoint(string oldSipUri, string newSipUri)
        {
            _avHelper.ReplaceRecipient(oldSipUri, newSipUri);
        }
    }
}
