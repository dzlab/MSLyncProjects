using System;
using System.Configuration;
using System.Threading;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;

namespace TwoPartyCallControl
{
    public class AcceptTransfersAndForwardsSample : ISampleComponent
    {
        ApplicationEndpoint _appEndpoint;
        AudioVideoCall _avCall;
        string _destinationSipUri;

        // A wait handle for startup and one for shutdown.
        // They are set to unsignaled to start.
        ManualResetEvent _startupWaitHandle = new ManualResetEvent(false);
        ManualResetEvent _shutdownWaitHandle = new ManualResetEvent(false);

        ILogger _logger;

        public AcceptTransfersAndForwardsSample(ApplicationEndpoint endpoint, ILogger logger)
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
            _avCall = new AudioVideoCall(conversation);
            _avCall.Forwarded += new EventHandler<CallForwardReceivedEventArgs>(OnCallForwarded);
            _avCall.TransferReceived += new EventHandler<AudioVideoCallTransferReceivedEventArgs>(OnTransferReceived);

            try
            {
                // Establish the IM call.
                _avCall.BeginEstablish(_destinationSipUri,
                    new CallEstablishOptions(),
                    result =>
                    {
                        try
                        {
                            // Finish the asynchronous operation.
                            _avCall.EndEstablish(result);
                        }
                        catch (RealTimeException ex)
                        {
                            // Catch and log exceptions.
                            _logger.Log("Failed establishing A/V call", ex);
                        }
                    },
                    null
                );
            }
            catch (InvalidOperationException ioex)
            {
                _logger.Log("Failed establishing A/V call", ioex);
            }
        }

        private void OnTransferReceived(object sender, AudioVideoCallTransferReceivedEventArgs e)
        {
            // Unregister the event handlers.
            _avCall.TransferReceived -= OnTransferReceived;
            _avCall.Forwarded -= OnCallForwarded;

            // Accept the REFER request with no special headers.
            e.Accept(null);

            // Create a new A/V call with the transfer-to URI using the
            // pre-initialized Conversation object.
            AudioVideoCall newCall = new AudioVideoCall(e.NewConversation);

            try
            {
                // Establish the call to the transfer-to endpoint.
                newCall.BeginEstablish(e.TransferDestination, null, 
                    ar =>
                    {
                        try
                        {
                            newCall.EndEstablish(ar);
                        }
                        catch (RealTimeException rtex)
                        {
                            _logger.Log("Failed establishing new call following transfer.", rtex);
                        }
                    },
                    null
                );
            }
            catch (InvalidOperationException ioex)
            {
                _logger.Log("Failed establishing new call following transfer.", ioex);
            }
        }

        private void OnCallForwarded(object sender, CallForwardReceivedEventArgs e)
        {
            // Unregister the event handlers.
            _avCall.TransferReceived -= OnTransferReceived;
            _avCall.Forwarded -= OnCallForwarded;

            // Accept the forward response from the remote endpoint.
            e.Accept();

            // Change the destination to the new URI.
            _destinationSipUri = e.ForwardDestination;

            // Establish a new call to the forwarding destination.
            EstablishCall();
        }

        public void Stop()
        {
            // Terminate the IM call if necessary.

            if (_avCall.State != CallState.Terminating &&
                _avCall.State != CallState.Terminated &&
                _avCall.State != CallState.Idle)
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
