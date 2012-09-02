using System;
using System.Configuration;
using System.Threading;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;

namespace TwoPartyCallControl
{
    public class AcceptIncomingAudioCallSample : ISampleComponent
    {
        ApplicationEndpoint _appEndpoint;
        AudioVideoCall _avCall;

        // A wait handle for startup and one for shutdown.
        // They are set to unsignaled to start.
        ManualResetEvent _startupWaitHandle = new ManualResetEvent(false);
        ManualResetEvent _shutdownWaitHandle = new ManualResetEvent(false);

        ILogger _logger;

        public AcceptIncomingAudioCallSample(ApplicationEndpoint endpoint, ILogger logger)
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
            _avCall = e.Call;

            try
            {
                // Accept the incoming call.
                _avCall.BeginAccept(ar =>
                {
                    try
                    {
                        _avCall.EndAccept(ar);

                        _logger.Log("Accepted incoming call.");
                    }
                    catch (RealTimeException rtex)
                    {
                        _logger.Log("Failed accepting incoming A/V call.", rtex);
                    }
                },
                null);
            }
            catch (InvalidOperationException ioex)
            {
                _logger.Log("Failed accepting incoming A/V call.", ioex);
            }
        }

        public void Stop()
        {
            // Terminate the A/V call if necessary.

            if (_avCall != null &&
                _avCall.State != CallState.Terminating &&
                _avCall.State != CallState.Terminated)
            {
                try
                {
                    _avCall.BeginTerminate(ar =>
                    {
                        try
                        {
                            _avCall.EndTerminate(ar);
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
