using System;
using System.Configuration;
using System.Threading;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;

namespace TwoPartyCallControl
{
    public class SendInstantMessageSample : ISampleComponent
    {
        ApplicationEndpoint _appEndpoint;
        InstantMessagingCall _imCall;
        string _destinationSipUri;

        // A wait handle for startup and one for shutdown.
        // They are set to unsignaled to start.
        ManualResetEvent _startupWaitHandle = new ManualResetEvent(false);
        ManualResetEvent _shutdownWaitHandle = new ManualResetEvent(false);

        ILogger _logger;

        public SendInstantMessageSample(ApplicationEndpoint endpoint, ILogger logger)
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

                            _imCall.Flow.MessageReceived += 
                                new EventHandler<InstantMessageReceivedEventArgs>(OnMessageReceived);

                            SendMessage("Jackdaws love my big sphinx of quartz.");
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

        private void OnMessageReceived(object sender, InstantMessageReceivedEventArgs e)
        {
            InstantMessagingFlow flow = sender as InstantMessagingFlow;

            if (e.HasTextBody)
            {
                SendMessage(e.TextBody.Replace("e", "3"));
            }
        }

        private void SendMessage(string message)
        {
            try
            {
                _imCall.Flow.BeginSendInstantMessage(message,
                    ar =>
                    {
                        try
                        {
                            _imCall.Flow.EndSendInstantMessage(ar);
                        }
                        catch (RealTimeException rtex)
                        {
                            _logger.Log("Failed sending IM.", rtex);
                        }
                    },
                    null
                );
            }
            catch (InvalidOperationException ioex)
            {
                _logger.Log("Failed sending IM.", ioex);
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
