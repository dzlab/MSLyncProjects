using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;
using System.Threading;

namespace LyncPBX
{
    public class SupervisorJoinCallSession
    {
        AudioVideoCall _frontEndCallLeg;
        AudioVideoCall _backEndCallLeg;
        BackToBackCall _b2bCall;

        string _recipientSipUri;
        string _supervisorSipUri;

        internal SupervisorJoinCallSession(AudioVideoCall call, string recipientSipUri, string supervisorSipUri)
        {
            _frontEndCallLeg = call;
            _recipientSipUri = recipientSipUri;
            _supervisorSipUri = supervisorSipUri;
        }

        internal void HandleIncomingCall()
        {
            Console.WriteLine("Handling incoming call.");

            // Create a new conversation for the back-end leg of the B2B call (which will connect to the conference).
            LocalEndpoint localEndpoint = _frontEndCallLeg.Conversation.Endpoint;
            Conversation backEndConversation = new Conversation(localEndpoint);
            
            // Impersonate the caller so that the caller, rather than the application, will appear to be participating in the conference
            string callerSipUri = _frontEndCallLeg.RemoteEndpoint.Participant.Uri;
            backEndConversation.Impersonate(callerSipUri, null, null);
            
            Console.WriteLine("Caller SIP Uri: " + callerSipUri);

            try
            {
                // Join the conference
                backEndConversation.ConferenceSession.BeginJoin( 
                    default(ConferenceJoinOptions), 
                    joinAsyncResult => {
                        try
                        {
                            backEndConversation.ConferenceSession.EndJoin(joinAsyncResult);
                            Console.WriteLine("Joined conference.");

                            _backEndCallLeg = new AudioVideoCall(backEndConversation);

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
            Console.WriteLine("Creating a back to back call between caller and the conference.");
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
                            Console.WriteLine("Back to back call established.");

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
            Console.WriteLine("Inviting the recipient (" + _recipientSipUri + ") to incoming call.");
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
                            Console.WriteLine("Invited recipient to the conference.");

                            InviteSupervisor();
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

        private void InviteSupervisor()
        {
            Console.WriteLine("Inviting the supervisor (" + _supervisorSipUri + ") to incoming call.");
            LocalEndpoint localEndpoint = _backEndCallLeg.Conversation.Endpoint;

            // Create a new audio call to call the supervisor.
            Conversation supervisorConversation = new Conversation(localEndpoint);
            AudioVideoCall supervisorCall = new AudioVideoCall(supervisorConversation);

            try
            {
                // Place an outbound call to the supervisor.
                supervisorCall.BeginEstablish(_supervisorSipUri,
                    default(CallEstablishOptions),
                    establishAsyncResult =>
                    {
                        try
                        {
                            supervisorCall.EndEstablish(establishAsyncResult);

                            // Wait for a couple of seconds before transferring.
                            Thread.Sleep(2000);

                            SelfTransferSupervisorCall(supervisorCall);
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

        private void SelfTransferSupervisorCall(AudioVideoCall supervisorCall)
        {
            Console.WriteLine("Self transfering the call.");
            // Put this instance of SupervisorJoinCallSession into the context property for retrieval when the application receives the self-transferred call.
            supervisorCall.Conversation.ApplicationContext = this;

            try
            {
                // Perform a self-transfer
                supervisorCall.BeginTransfer(
                    supervisorCall,
                    transferAsyncResult =>
                    {
                        try
                        {
                            supervisorCall.EndTransfer(transferAsyncResult);
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

        internal void HandleIncomingSelfTransfer(AudioVideoCall call)
        {
            Console.WriteLine("Handling the incoming self transfer call.");
            string conferenceUri = _backEndCallLeg.Conversation.ConferenceSession.ConferenceUri;

            // Create a new conversation for the back-end call leg.
            LocalEndpoint localEndpoint = _backEndCallLeg.Conversation.Endpoint;
            Conversation conferenceConversation = new Conversation(localEndpoint);

            // Prepare to join the conference as a trusted participant so as to be allowed to manipulate audio routing.
            ConferenceJoinOptions joinOptions = new ConferenceJoinOptions();
            joinOptions.JoinMode = JoinMode.TrustedParticipant;

            try
            {
                // Join the conference
                conferenceConversation.ConferenceSession.BeginJoin(conferenceUri,
                    joinOptions,
                    joinAsyncResult =>
                    {
                        try
                        {
                            conferenceConversation.ConferenceSession.EndJoin(joinAsyncResult);

                            CreateSupervisorB2BCall(call, conferenceConversation);
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

        private void CreateSupervisorB2BCall(AudioVideoCall incomingCall, Conversation conferenceConversation)
        {
            Console.WriteLine("Creating a supervisor B2B call.");
            // Create a new audio call on the back-end call leg
            AudioVideoCall callToConference = new AudioVideoCall(conferenceConversation);

            // Set up the call to be automatically removed from the default audio mix in the A/V MCU.
            AudioVideoCallEstablishOptions establishOptions = new AudioVideoCallEstablishOptions();
            establishOptions.AudioVideoMcuDialInOptions.RemoveFromDefaultRouting = true;

            // Create a back to back call between the supervisor and the conference 
            BackToBackCallSettings frontEndB2BSettings = new BackToBackCallSettings(incomingCall);
            BackToBackCallSettings backEndB2BSettings = new BackToBackCallSettings(callToConference);

            backEndB2BSettings.CallEstablishOptions = establishOptions;

            BackToBackCall supervisorB2BCall = new BackToBackCall(frontEndB2BSettings, backEndB2BSettings);

            try
            {
                // Establish the B2B call
                supervisorB2BCall.BeginEstablish(
                    establishAsyncResult =>
                    {
                        try
                        {
                            supervisorB2BCall.EndEstablish(establishAsyncResult);

                            CreateManualAudioRoutes(callToConference);
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

        private void CreateManualAudioRoutes(AudioVideoCall callToConference)
        {
            Console.WriteLine("Creating manual audio routes.");
            AudioVideoMcuSession avMcu = _backEndCallLeg.Conversation.ConferenceSession.AudioVideoMcuSession;

            // Get the participantEndpoint objects for the caller and recipient
            ParticipantEndpoint callerParticipantEndpoint = avMcu.GetRemoteParticipantEndpoints().Single(
                p => p.Participant.Uri == _backEndCallLeg.Conversation.Endpoint.OwnerUri);

            ParticipantEndpoint recipientParticipantEndpoint = avMcu.GetRemoteParticipantEndpoints().Single(
                p => p.Participant.Uri == _recipientSipUri);

            // Create incoming audio routes from the caller and recipient to the supervisor
            IncomingAudioRoute callerToSupervisorAudioRoute = new IncomingAudioRoute(callerParticipantEndpoint);
            IncomingAudioRoute recipientToSupervisorAudioRoute = new IncomingAudioRoute(recipientParticipantEndpoint);

            List<IncomingAudioRoute> routes = new List<IncomingAudioRoute>() {
                callerToSupervisorAudioRoute, recipientToSupervisorAudioRoute};

            try
            {
                // Update the MCU audio routing with the new incoming routes.
                callToConference.AudioVideoMcuRouting.BeginUpdateAudioRoutes(
                    null, routes,
                    updateRoutesAsyncResult =>
                    {
                        try
                        {
                            callToConference.AudioVideoMcuRouting.EndUpdateAudioRoutes(updateRoutesAsyncResult);
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

        private void RemoveCallerFromDefaultAudioMix()
        {
            Console.WriteLine("Removing caller from default audio mix.");
            AudioVideoMcuSession avMcu = _backEndCallLeg.Conversation.ConferenceSession.AudioVideoMcuSession;

            // Get the ParticipantEndpoint object for the caller
            ParticipantEndpoint callerParticipantEndpoint = avMcu.GetRemoteParticipantEndpoints().Single(
                p => p.Participant.Uri == _backEndCallLeg.Conversation.Endpoint.OwnerUri);

            try
            {
                // Remove the caller from the default audio mix. He or she will not hear or be heard by any other participants.
                avMcu.BeginRemoveFromDefaultRouting(callerParticipantEndpoint, removeAsyncReuslt =>
                    {
                        try
                        {
                            avMcu.EndRemoveFromDefaultRouting(removeAsyncReuslt);
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

        private void AddCallerToDefaultAudioMix()
        {
            Console.WriteLine("Adding caller to the default audio mix.");
            AudioVideoMcuSession avMcu =
                _backEndCallLeg.Conversation.ConferenceSession.AudioVideoMcuSession;

            // Get the ParticipantEndpoint object for the caller.
            ParticipantEndpoint callerParticipantEndpoint =
                avMcu.GetRemoteParticipantEndpoints().Single(p =>
                    p.Participant.Uri == _backEndCallLeg.Conversation.Endpoint.OwnerUri);

            try
            {
                // Remove the caller from the default audio mix. He or she will not hear or be heard by any other participants.
                avMcu.BeginAddToDefaultRouting(callerParticipantEndpoint,
                    addAsyncResult =>
                    {
                        try
                        {
                            avMcu.EndAddToDefaultRouting(addAsyncResult);
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
