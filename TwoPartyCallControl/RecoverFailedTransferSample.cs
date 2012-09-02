using System;
using System.Configuration;
using System.Threading;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;

namespace TwoPartyCallControl
{
    public class RecoverFailedTransferSample : ISampleComponent
    {
        ApplicationEndpoint _appEndpoint;
        AudioVideoCall _avCall;
        string _destinationSipUri = string.Empty;

        // A wait handle for startup and one for shutdown.
        // They are set to unsignaled to start.
        ManualResetEvent _startupWaitHandle = new ManualResetEvent(false);
        ManualResetEvent _shutdownWaitHandle = new ManualResetEvent(false);

        ILogger _logger;

        public RecoverFailedTransferSample(ApplicationEndpoint endpoint, ILogger logger)
        {
            _appEndpoint = endpoint;
            _logger = logger;
        }

        public void Start()
        {
            Console.Write("Enter transfer destination SIP URI: ");
            _destinationSipUri = Console.ReadLine();

            // Register for incoming audio/video calls.
            _appEndpoint.RegisterForIncomingCall<AudioVideoCall>(OnIncomingAudioVideoCallReceived);
        }

        private void OnIncomingAudioVideoCallReceived(object sender, CallReceivedEventArgs<AudioVideoCall> e)
        {
            _avCall = e.Call;

            _avCall.StateChanged += new EventHandler<CallStateChangedEventArgs>(OnCallStateChanged);

            try
            {
                // Accept the incoming call.
                _avCall.BeginAccept(ar =>
                {
                    try
                    {
                        _avCall.EndAccept(ar);

                        _logger.Log("Accepted incoming call.");

                        PerformAttendedTransfer();
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

        private void OnCallStateChanged(object sender, CallStateChangedEventArgs e)
        {
            _logger.Log("Call state changed to {0}", e.State);
        }

        private void PerformAttendedTransfer()
        {
            try
            {
                _avCall.BeginTransfer(_destinationSipUri,
                    transferResult =>
                    {
                        try
                        {
                            _avCall.EndTransfer(transferResult);
                        }
                        catch (RealTimeException rtex)
                        {
                            _logger.Log("Failed transferring call; retrieving it.", rtex);

                            RetrieveCallAfterTransferFailure();
                        }
                    },
                    null);
            }
            catch (InvalidOperationException ioex)
            {
                _logger.Log("Failed transferring call.", ioex);
            }
        }

        private void RetrieveCallAfterTransferFailure()
        {
            // Take the call off of hold after a transfer fails.

            try
            {
                _avCall.Flow.BeginRetrieve(retrieveResult =>
                {
                    try
                    {
                        _avCall.Flow.EndRetrieve(retrieveResult);

                        _logger.Log("Successfully retrieved call.");
                    }
                    catch (RealTimeException rtex)
                    {
                        _logger.Log("Failed retrieving call.", rtex);
                    }
                },
                null);
            }
            catch (InvalidOperationException ioex)
            {
                _logger.Log("Failed retrieving call.", ioex);
            }
        }

        public void Stop()
        {
            // Terminate the A/V call if necessary.

            if (_avCall != null)
            {
                _avCall.StateChanged -= OnCallStateChanged;
            }

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
