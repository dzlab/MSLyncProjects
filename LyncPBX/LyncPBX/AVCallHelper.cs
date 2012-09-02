using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Synthesis;

namespace LyncPBX
{
    class AVCallHelper
    {
        AudioVideoCall _frontEndCallLeg;
        AudioVideoCall _backEndCallLeg;
        string _recipient;

        internal AVCallHelper()
        {
            
        }
               
        public void AcceptAVCall(AudioVideoCall call)
        {
            Console.WriteLine("Accepting incoming AV call");
            
            try
            {
                call.BeginAccept(
                ar =>
                {
                    try
                    {
                        call.Flow.StateChanged += new EventHandler<MediaFlowStateChangedEventArgs>(Flow_StateChanged);

                        call.EndAccept(ar);
                        SpeakMessage(call.Flow, string.Format("Hello, {0}. Thanks for calling. "
                            + "Your SIP URI is {1}",
                            call.RemoteEndpoint.Participant.DisplayName,
                            call.RemoteEndpoint.Participant.Uri));
                    }
                    catch (RealTimeException ex)
                    {
                        Console.WriteLine("Failed tp accept call.", ex);
                    }
                },
                null);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("Failed tp accept call.", ex);
            }            
        }

        public void SpeakMessage(AudioVideoFlow flow, string message)
        {
            try
            {
                SpeechSynthesizer synth = new SpeechSynthesizer();
                SpeechAudioFormatInfo formatInfo = new SpeechAudioFormatInfo(16000, AudioBitsPerSample.Sixteen, Microsoft.Speech.AudioFormat.AudioChannel.Mono);
                SpeechSynthesisConnector connector = new SpeechSynthesisConnector();

                synth.SetOutputToAudioStream(connector.Stream, formatInfo);

                connector.AttachFlow(flow);
                connector.Start();

                synth.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(
                    (sender, args) =>
                    {
                        connector.Stop();
                        synth.Dispose();
                    });

                synth.SpeakAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to play the message. {0}", ex);
            }

        }

        private void Flow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            try
            {
                AudioVideoFlow flow = sender as AudioVideoFlow;

                if (e.State == MediaFlowState.Terminated)
                {
                    flow.SpeechSynthesisConnector.DetachFlow();
                }

                flow.StateChanged -= Flow_StateChanged;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void ExtablishOutboundAVCall(ApplicationEndpoint appEndpoint, string destinationSipUri)
        {
            Conversation conversation = new Conversation(appEndpoint);
            AudioVideoCall call = new AudioVideoCall(conversation);

            try
            {
                call.BeginEstablish(destinationSipUri,
                    new CallEstablishOptions(),
                    result =>
                    {
                        try
                        {
                            call.EndEstablish(result);
                            SpeakMessage(call.Flow, 
                                string.Format("Hello, {0}. This my UCMA app calling you. " + "Your SIP URI is {1}",
                                call.RemoteEndpoint.Participant.DisplayName,
                                call.RemoteEndpoint.Participant.Uri));
                        }
                        catch (RealTimeException ex)
                        {
                            Console.WriteLine("Failed establishing AV call {0}", ex);
                        }
                    },
                    null);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("Failed establishing AV call {0}", ex);
            }
        }

        // Escalate call to a conference
        public void EscalateCalltoConference(AudioVideoCall call, string recipient)
        {
            _frontEndCallLeg = call;
            _recipient = recipient;

            Console.WriteLine("Escalating incoming call to conference");
            LocalEndpoint localEndpoint = _frontEndCallLeg.Conversation.Endpoint;
            Conversation backEndConversation = new Conversation(localEndpoint);

            string callerSipUri = _frontEndCallLeg.RemoteEndpoint.Participant.Uri;
            backEndConversation.Impersonate(callerSipUri, null, null);
            Console.WriteLine("Caller SIP Uri: " + callerSipUri);

            try
            {
                ConferenceJoinOptions options = new ConferenceJoinOptions();
                options.JoinMode = JoinMode.TrustedParticipant;

                backEndConversation.ConferenceSession.BeginJoin(
                    options,
                    joinAsyncResult =>
                    {
                        try
                        {
                            backEndConversation.ConferenceSession.EndJoin(joinAsyncResult);
                            Console.WriteLine("Joined conference.");
                            _backEndCallLeg = new AudioVideoCall(backEndConversation);
                            CreateBackToBackCall();
                        }
                        catch (RealTimeException ex)
                        {
                            Console.WriteLine("Failed to join conference when escalating the call.\n{0}", ex);
                        }
                    },
                    null);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("Failed to escalate call to conference: {0}", ex);
            }
        }

        public void CreateBackToBackCall()
        {
            Console.WriteLine("Creating a back to back call between caller and the conference.");
            // Create a back to back call between the caller and the conference.
            // This is so the caller will not see that he/she is connected to a conference.
            BackToBackCallSettings frontEndCallLegSettings = new BackToBackCallSettings(_frontEndCallLeg);
            BackToBackCallSettings backEndCallLegSettings = new BackToBackCallSettings(_backEndCallLeg);

            BackToBackCall b2bCall = new BackToBackCall(frontEndCallLegSettings, backEndCallLegSettings);

            try
            {
                // Establish the back to back call.
                b2bCall.BeginEstablish(
                    establishAsyncResult =>
                    {
                        try
                        {
                            b2bCall.EndEstablish(establishAsyncResult);
                            Console.WriteLine("Back to back call established.");

                            // we may choose to forward the call to another destination
                            InviteRecipientToConference(_recipient);
                        }
                        catch (RealTimeException ex)
                        {
                            Console.WriteLine(ex);
                        }
                    },
                    null);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void InviteRecipientToConference(string recipientUri)
        {
            Console.WriteLine("Inviting the recipient (" + recipientUri + ") to incoming call.");
            
            try
            {
                // Invite the recipient to the conference using a conference invitation.
                ConferenceInvitation invitation = new ConferenceInvitation(_backEndCallLeg.Conversation);
                invitation.BeginDeliver(
                    recipientUri,
                    deliverAsyncResult =>
                    {
                        try
                        {
                            invitation.EndDeliver(deliverAsyncResult);
                            Console.WriteLine("Invited recipient to the conference.");

                        }
                        catch (RealTimeException ex)
                        {
                            Console.WriteLine(ex);
                        }
                    },
                    null);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void EjectRecipientFromConference(string recipientUri)
        {
            try
            {
                ConferenceSession confSession = _backEndCallLeg.Conversation.ConferenceSession;
                ParticipantEndpoint endpoint = confSession.GetRemoteParticipantEndpoints().FirstOrDefault(p => p.Participant.Uri == recipientUri);
                if (endpoint != null)
                {
                    ConversationParticipant participant = endpoint.Participant;
                    confSession.BeginEject(participant,
                        ejectResult =>
                        {
                            try
                            {
                                confSession.EndEject(ejectResult);
                            }
                            catch (RealTimeException ex)
                            {
                                Console.WriteLine("Failed to remove " + recipientUri + " from conference.\n{0}", ex);
                            }
                        },
                        null);
                }
                else
                {
                    Console.WriteLine("Failed to remove " + recipientUri + " from conference, recipient endpoint was null.");
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("Failed to remove " + recipientUri + " from conference.\n{0}", ex);
            }
        }

        public void ReplaceRecipient(string oldRecipient, string newRecipient)
        {
            InviteRecipientToConference(newRecipient);
            EjectRecipientFromConference(oldRecipient);
        }
    }
}
