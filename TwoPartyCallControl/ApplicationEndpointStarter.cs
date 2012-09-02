using System;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Signaling;
using System.Threading;
using System.Configuration;

namespace TwoPartyCallControl
{
    public class ApplicationEndpointStarter
    {
        CollaborationPlatform _collaborationPlatform;
        ApplicationEndpoint _appEndpoint;
        int _endpointsDiscovered = 0;

        // A wait handle for startup and one for shutdown.
        // They are set to unsignaled to start.
        ManualResetEvent _startupWaitHandle = new ManualResetEvent(false);
        ManualResetEvent _shutdownWaitHandle = new ManualResetEvent(false);

        ILogger _logger;

        public ApplicationEndpointStarter(ILogger logger)
        {
            _logger = logger;
        }

        public ApplicationEndpoint ApplicationEndpoint
        {
            get
            {
                return _appEndpoint;
            }
        }

        public void Start()
        {
            // Get the application ID from App.config.
            string applicationUserAgent = "maximillian";
            string applicationId = ConfigurationManager.AppSettings["applicationId"];

            // Create a settings object.
            ProvisionedApplicationPlatformSettings settings =
                new ProvisionedApplicationPlatformSettings(applicationUserAgent, applicationId);

            // Create a new collaboration platform with the settings.
            _collaborationPlatform = new CollaborationPlatform(settings);
            _collaborationPlatform.RegisterForApplicationEndpointSettings(OnApplicationEndpointSettingsDiscovered);

            _logger.Log("Starting collaboration platform...");

            // Start the platform as an asynchronous operation.
            _collaborationPlatform.BeginStartup(OnPlatformStartupCompleted, null);
        }

        public void WaitForStartup()
        {
            _startupWaitHandle.WaitOne();
        }

        private void OnPlatformStartupCompleted(IAsyncResult result)
        {
            try
            {
                // Finish the startup operation.
                _collaborationPlatform.EndStartup(result);

                _logger.Log("Collaboration platform started.");
            }
            catch (RealTimeException ex)
            {
                _logger.Log("Platform startup failed: {0}", ex);
            }
        }

        private void OnApplicationEndpointSettingsDiscovered(object sender, ApplicationEndpointSettingsDiscoveredEventArgs args)
        {
            // Keep track of how many endpoints we've found
            // so that we only take one.
            Interlocked.Increment(ref _endpointsDiscovered);

            if (_endpointsDiscovered > 1)
            {
                // We've already found an endpoint
                // and we don't need another one. Sorry!
                return;
            }

            _appEndpoint = new ApplicationEndpoint(_collaborationPlatform, args.ApplicationEndpointSettings);

            _appEndpoint.BeginEstablish(OnApplicationEndpointEstablishCompleted, null);
        }

        private void OnApplicationEndpointEstablishCompleted(IAsyncResult result)
        {
            try
            {
                _appEndpoint.EndEstablish(result);

                _logger.Log("Application endpoint established.");

                _startupWaitHandle.Set();
            }
            catch (RealTimeException ex)
            {
                _logger.Log("Application endpoint establishment failed: {0}", ex);
            }
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

        public void WaitForShutdown()
        {
            _shutdownWaitHandle.WaitOne();
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
                _logger.Log("Application endpoint termination failed: {0}", ex);
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

                _logger.Log("Platform shut down.");

                _shutdownWaitHandle.Set();
            }
            catch (RealTimeException ex)
            {
                _logger.Log("Platform shutdown failed: {0}", ex);
            }
        }
    }
}
