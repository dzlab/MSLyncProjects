using System;
using System.Configuration;
using System.Threading;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;

namespace TwoPartyCallControl
{
    public class ForwardIncomingAudioCallSample : ISampleComponent
    {
        ApplicationEndpoint _appEndpoint;

        // A wait handle for startup and one for shutdown.
        // They are set to unsignaled to start.
        ManualResetEvent _startupWaitHandle = new ManualResetEvent(false);
        ManualResetEvent _shutdownWaitHandle = new ManualResetEvent(false);

        ILogger _logger;

        public ForwardIncomingAudioCallSample(ApplicationEndpoint endpoint, ILogger logger)
        {
            _appEndpoint = endpoint;
            _logger = logger;
        }

        public void Start()
        {
            // Register for incoming audio/video calls.
            _appEndpoint.RegisterForIncomingCall<AudioVideoCall>(OnIncomingAudioVideoCallReceived);
        }

        private void OnIncomingAudioVideoCallReceived(object sender, CallReceivedEventArgs<AudioVideoCall> e)
        {
            // Forward incoming A/V calls.
            try
            {
                _logger.Log("Forwarding incoming call.");

                e.Call.Forward("sip:georged@fabrikam.com");
            }
            catch (InvalidOperationException ioex)
            {
                _logger.Log("Failed forwarding incoming call.", ioex);
            }
        }

        public void Stop()
        {
            // Nothing to do.
        }
    }
}
