using System;
using System.Configuration;
using System.Threading;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;

namespace TwoPartyCallControl
{
    public class TransferCallSample : ISampleComponent
    {
        ApplicationEndpoint _appEndpoint;
        AudioVideoCall _avCall;
        TransferTypeSelection _transferType = TransferTypeSelection.Attended;
        string _destinationSipUri = string.Empty;

        // A wait handle for startup and one for shutdown.
        // They are set to unsignaled to start.
        ManualResetEvent _startupWaitHandle = new ManualResetEvent(false);
        ManualResetEvent _shutdownWaitHandle = new ManualResetEvent(false);

        ILogger _logger;

        public TransferCallSample(ApplicationEndpoint endpoint, ILogger logger)
        {
            _appEndpoint = endpoint;
            _logger = logger;
        }

        public void Start()
        {
            Console.Write("Enter transfer destination SIP URI: ");
            _destinationSipUri = Console.ReadLine();

            Console.WriteLine("Select one of the following:");
            Console.WriteLine("1. Attended Transfer");
            Console.WriteLine("2. Unattended Transfer");
            Console.WriteLine("3. Supervised Transfer");
            Console.Write("?");
            string entry = Console.ReadLine();

            int selection = int.Parse(entry);

            if (selection < 1 || selection > 3)
            {
                _logger.Log("Invalid selection for transfer type. Defaulting to attended transfer.");
            }
            else
            {
                _transferType = (TransferTypeSelection)selection;
            }

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

                        switch (_transferType)
                        {
                            case TransferTypeSelection.Attended:
                                PerformAttendedTransfer();
                                break;
                            case TransferTypeSelection.Unattended:
                                PerformUnattendedTransfer();
                                break;
                            case TransferTypeSelection.Supervised:
                                PerformSupervisedTransfer();
                                break;
                        }
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

        private void PerformAttendedTransfer()
        {
            try
            {
                _avCall.BeginTransfer(_destinationSipUri,
                    ar =>
                    {
                        try
                        {
                            _avCall.EndTransfer(ar);
                        }
                        catch (RealTimeException rtex)
                        {
                            _logger.Log("Failed transferring call.", rtex);
                        }
                    },
                    null);
            }
            catch (InvalidOperationException ioex)
            {
                _logger.Log("Failed transferring call.", ioex);
            }
        }

        private void PerformUnattendedTransfer()
        {
            CallTransferOptions options =
                new CallTransferOptions(CallTransferType.Unattended);

            try
            {
                _avCall.BeginTransfer(_destinationSipUri, options,
                    ar =>
                    {
                        try
                        {
                            _avCall.EndTransfer(ar);
                        }
                        catch (RealTimeException rtex)
                        {
                            _logger.Log("Failed transferring call.", rtex);
                        }
                    },
                    null);
            }
            catch (InvalidOperationException ioex)
            {
                _logger.Log("Failed transferring call.", ioex);
            }
        }

        private void PerformSupervisedTransfer()
        {
            ConversationSettings settings = new ConversationSettings()
            {
                Subject = "Supervised transfer"
            };

            Conversation newConversation = new Conversation(_appEndpoint, settings);

            AudioVideoCall newCall = new AudioVideoCall(newConversation);

            try
            {
                newCall.BeginEstablish(_destinationSipUri, null,
                    ar =>
                    {
                        try
                        {
                            newCall.EndEstablish(ar);

                            ReplaceNewCallWithIncomingCall(newCall);
                        }
                        catch (RealTimeException rtex)
                        {
                            _logger.Log("Failed establishing second call.", rtex);
                        }
                    },
                    null
                );
            }
            catch (InvalidOperationException ioex)
            {
                _logger.Log("Failed establishing second call.", ioex);
            }
        }

        private void ReplaceNewCallWithIncomingCall(AudioVideoCall newCall)
        {
            // Transfer the original incoming call, replacing the new call to the destination URI.
            try
            {
                _avCall.BeginTransfer(newCall,
                    ar =>
                    {
                        try
                        {
                            _avCall.EndTransfer(ar);

                            _logger.Log("Successfully replaced call.");
                        }
                        catch (RealTimeException rtex)
                        {
                            _logger.Log("Failed replacing call.", rtex);
                        }
                    },
                    null
                );
            }
            catch (InvalidOperationException ioex)
            {
                _logger.Log("Failed replacing call.", ioex);
            }
        }

        public void Stop()
        {
            // Terminate the IM call if necessary.

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

    enum TransferTypeSelection : int
    {
        Attended = 1,
        Unattended = 2,
        Supervised = 3
    }
}
