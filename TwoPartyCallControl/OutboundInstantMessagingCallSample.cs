using System;
using System.Configuration;
using System.Threading;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;

namespace TwoPartyCallControl
{
    public class OutboundInstantMessagingCallSample : ISampleComponent
    {
        ApplicationEndpoint _appEndpoint;
        InstantMessagingCall _imCall;
        string _destinationSipUri;

        // A wait handle for startup and one for shutdown.
        // They are set to unsignaled to start.
        ManualResetEvent _startupWaitHandle = new ManualResetEvent(false);
        ManualResetEvent _shutdownWaitHandle = new ManualResetEvent(false);

        ILogger _logger;

        public OutboundInstantMessagingCallSample(ApplicationEndpoint endpoint, ILogger logger)
        {
            _appEndpoint = endpoint;
            _logger = logger;
        }

        public void Start()
        {
            Console.Write("Enter destination URI: ");
            _destinationSipUri = Console.ReadLine();

            EstablishCall();
        }

        private void EstablishCall()
        {
            // Create a new Conversation.
            Conversation conversation = new Conversation(_appEndpoint);

            // Create a new IM call.
            _imCall = new InstantMessagingCall(conversation);

            try
            {
                // Establish the IM call.
                _imCall.BeginEstablish(_destinationSipUri,
                    new CallEstablishOptions(),
                    result =>
                    {
                        try
                        {
                            // Finish the asynchronous operation.
                            _imCall.EndEstablish(result);
                        }
                        catch (RealTimeException ex)
                        {
                            // Catch and log exceptions.
                            _logger.Log("Failed establishing IM call", ex);
                        }
                    },
                    null
                );
            }
            catch (InvalidOperationException ioex)
            {
                _logger.Log("Failed establishing IM call", ioex);
            }
        }

        public void Stop()
        {
            // Terminate the IM call if necessary.

            if (_imCall.State != CallState.Terminating &&
                _imCall.State != CallState.Terminated &&
                _imCall.State != CallState.Idle)
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
                            _logger.Log("Failed terminating IM call.", rtex);
                        }
                    },
                    null);
                }
                catch (InvalidOperationException ioex)
                {
                    _logger.Log("Failed terminating IM call.", ioex);
                }
            }
        }
    }
}
