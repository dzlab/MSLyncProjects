using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Signaling;

namespace LyncPresence
{
    class IMCallHandler
    {
        InstantMessagingCall _frontEndCallLeg;
        InstantMessagingCall _backEndCallLeg;
        BackToBackCall _b2bCall;

        string _recipientSipUri;

        internal IMCallHandler(InstantMessagingCall call, string recipientSipUri)
        {
            _frontEndCallLeg = call;
            _recipientSipUri = recipientSipUri;
        }

        internal void HandleIncomingCall()
        {
            //Console.WriteLine("Handling incoming IM call.");
            // Create a new conversation for the back-end leg of the B2B call (which will connect to the conference).
            LocalEndpoint localEndpoint = _frontEndCallLeg.Conversation.Endpoint;
            Conversation backEndConversation = new Conversation(localEndpoint);

            // Impersonate the caller so that the caller, rather than the application, will appear to be participating in the conference
            string callerSipUri = _frontEndCallLeg.RemoteEndpoint.Participant.Uri;
            backEndConversation.Impersonate(callerSipUri, null, null);

            //Console.WriteLine("Caller SIP Uri: " + callerSipUri);

            try
            {
                // Join the conference
                backEndConversation.ConferenceSession.BeginJoin(
                    default(ConferenceJoinOptions),
                    joinAsyncResult =>
                    {
                        try
                        {
                            backEndConversation.ConferenceSession.EndJoin(joinAsyncResult);
                            Console.WriteLine("Joined conference.");

                            _backEndCallLeg = new InstantMessagingCall(backEndConversation);

                            CreateBackToBack();
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
        private void CreateBackToBack()
        {
            //Console.WriteLine("Creating a back to back call between caller and the conference.");
            // Create a back to back call between the caller and the conference.
            // This is so the caller will not see that he/she is connected to a conference.
            BackToBackCallSettings frontEndCallLegSettings = new BackToBackCallSettings(_frontEndCallLeg);
            BackToBackCallSettings backEndCallLegSettings = new BackToBackCallSettings(_backEndCallLeg);

            _b2bCall = new BackToBackCall(frontEndCallLegSettings, backEndCallLegSettings);

            try
            {
                // Establish the back to back call.
                _b2bCall.BeginEstablish(
                    establishAsyncResult =>
                    {
                        try
                        {
                            _b2bCall.EndEstablish(establishAsyncResult);
                            //Console.WriteLine("Back to back call established.");

                            InviteRecipientToConference();
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

        private void InviteRecipientToConference()
        {
            //Console.WriteLine("Inviting the recipient (" + _recipientSipUri + ") to incoming call.");
            ConferenceInvitation invitation = new ConferenceInvitation(_backEndCallLeg.Conversation);

            try
            {
                // Invite the recipient to the conference using a conference invitation.
                invitation.BeginDeliver(
                    _recipientSipUri,
                    deliverAsyncResult =>
                    {
                        try
                        {
                            invitation.EndDeliver(deliverAsyncResult);
                            //Console.WriteLine("Invited recipient to the conference.");
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

        // The Echo anables the UCMA app to respond to incoming IM messages by the received text
        internal void Echo()
        {
            InstantMessagingCall imCall = _frontEndCallLeg;
            try
            {
                imCall.BeginAccept(ar =>
                {
                    try
                    {
                        imCall.EndAccept(ar);
                        //Console.WriteLine("Incoming IM call accepted");
                    }
                    catch (RealTimeException ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        InstantMessagingFlow flow = imCall.Flow;
                        flow.MessageReceived += new EventHandler<InstantMessageReceivedEventArgs>(OnMessageReceived);
                        
                    }
                },
                    null);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void OnMessageReceived(object sender, InstantMessageReceivedEventArgs e)
        {
            //Console.WriteLine("Receiving a message");
            InstantMessagingFlow flow = sender as InstantMessagingFlow;
            string message = "Application received an empty message!";
            if (e.HasTextBody)
            {
                message = "Application received following message: \n" + e.TextBody;
            }
            SendMessage(flow, message);
        }

        private void SendMessage(InstantMessagingFlow flow, string message)
        {
            try
            {
                flow.BeginSendInstantMessage(message, ar =>
                {
                    try
                    {
                        flow.EndSendInstantMessage(ar);
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

    }
}
