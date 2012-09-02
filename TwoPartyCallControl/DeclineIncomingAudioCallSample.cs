using System;
using System.Configuration;
using System.Threading;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;

namespace TwoPartyCallControl
{
    public class DeclineIncomingAudioCallSample : ISampleComponent
    {
        ApplicationEndpoint _appEndpoint;
        InstantMessagingCall _imCall;

        // A wait handle for startup and one for shutdown.
        // They are set to unsignaled to start.
        ManualResetEvent _startupWaitHandle = new ManualResetEvent(false);
        ManualResetEvent _shutdownWaitHandle = new ManualResetEvent(false);

        ILogger _logger;

        public DeclineIncomingAudioCallSample(ApplicationEndpoint endpoint, ILogger logger)
        {
            _appEndpoint = endpoint;
            _logger = logger;
        }

        public void Start()
        {
            // Register for incoming audio/video calls.
            _appEndpoint.RegisterForIncomingCall<InstantMessagingCall>(OnIncomingInstantMessagingCallReceived);
            _appEndpoint.RegisterForIncomingCall<AudioVideoCall>(OnIncomingAudioVideoCallReceived);
        }

        private void OnIncomingInstantMessagingCallReceived(object sender, CallReceivedEventArgs<InstantMessagingCall> e)
        {
            _imCall = e.Call;

            try
            {
                // Accept the incoming call.
                _imCall.BeginAccept(ar =>
                {
                    try
                    {
                        _imCall.EndAccept(ar);

                        _logger.Log("Accepted incoming call.");
                    }
                    catch (RealTimeException rtex)
                    {
                        _logger.Log("Failed accepting incoming IM call.", rtex);
                    }
                },
                null);
            }
            catch (InvalidOperationException ioex)
            {
                _logger.Log("Failed accepting incoming IM call.", ioex);
            }
        }

        private void OnIncomingAudioVideoCallReceived(object sender, CallReceivedEventArgs<AudioVideoCall> e)
        {
            // Decline incoming A/V calls.

            try
            {
                e.Call.Decline();
            }
            catch (InvalidOperationException ioex)
            {
                _logger.Log("Failed declining incoming call.", ioex);
            }
        }

        public void Stop()
        {
            // Terminate the IM call if necessary.

            if (_imCall != null &&
                _imCall.State != CallState.Terminating &&
                _imCall.State != CallState.Terminated)
            {
                try
                {
                    _imCall.BeginTerminate(ar =>
                    {
                        try
                        {
                            _imCall.EndTerminate(ar);
                        }
                        catch (RealTimeException rtex)
                        {
                            _logger.Log("Failed terminating A/V call.", rtex);
                        }
                    },
                    null);
                }
                catch (InvalidOperationException ioex)
                {
                    _logger.Log("Failed terminating A/V call.", ioex);
                }
            }
        }
    }
}
