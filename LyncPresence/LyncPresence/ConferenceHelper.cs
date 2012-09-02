using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Collaboration.ContactsGroups;
using Microsoft.Rtc.Collaboration.AudioVideo;

namespace LyncPresence
{
    class ConferenceHelper
    {
        private RemotePresenceView _remotePresenceView;
        private ApplicationEndpoint _appEndpoint;
        private Conversation _conversation;
        private List<string> _invitees;
        

        public ConferenceHelper(ApplicationEndpoint appEndpoint)
        {
            _appEndpoint = appEndpoint;
            _conversation = new Conversation(_appEndpoint);
            
            // Subscribe to receive presence notifications
            _remotePresenceView = new RemotePresenceView(appEndpoint);
            _remotePresenceView.PresenceNotificationReceived += new EventHandler<RemotePresentitiesNotificationEventArgs>(PresenceReceivedEventHandler);

        }

        public void QueryUserPresence(string userSipUri)
        {
            _appEndpoint.PresenceServices.BeginPresenceQuery(
                new List<string>() { userSipUri },
                new string[] { "state" },
                PresenceReceivedEventHandler,
                OnPresenceQueryCompletedCB,
                null);            
            
            // Create a list with subscription targets
            List<RemotePresentitySubscriptionTarget> targets = new List<RemotePresentitySubscriptionTarget>();
            targets.Add(new RemotePresentitySubscriptionTarget(userSipUri));

            // Initiate the presence subscription
            _remotePresenceView.StartSubscribingToPresentities(targets);
        }
        

        private void OnPresenceQueryCompletedCB(IAsyncResult result)
        {            
            try
            {
                // Retrieve the results of the query
                IEnumerable<RemotePresentityNotification> notifications = _appEndpoint.PresenceServices.EndPresenceQuery(result);

                // Make sure the call has finished
                //

                // Grap the first notification in the results
                RemotePresentityNotification notification = notifications.FirstOrDefault();
                string availability = notification.AggregatedPresenceState.Availability.ToString().ToLower();

                Console.Out.Write("Availability: " + availability);

                if (availability.Equals("available"))
                {
                    // start conference between supervisor and driver

                }
            }
            catch (RealTimeException ex)
            {
                Console.Out.Write("Presence query failed.", ex);
            }
        }

        /* Called to initiate a conference */
        public void InitiateConference(List<string> invitees)
        {
            Console.Out.Write("Initiating a conference for " + invitees.ElementAt(0) + " and " + invitees.ElementAt(1));
            _invitees = invitees;
            //AudioVideoCall call = new AudioVideoCall(_conversation);
            InstantMessagingCall call = new InstantMessagingCall(_conversation);
            try
            {
                call.StateChanged += new EventHandler<CallStateChangedEventArgs>(CallStateChanged);
                call.BeginEstablish(_invitees.ElementAt(0),
                    new CallEstablishOptions(),
                    result =>
                    {
                        try
                        {
                            call.EndEstablish(result);                                                        
                        }
                        catch (RealTimeException ex)
                        {
                            Console.Out.Write(ex);
                        }
                    },
                    null);
            }
            catch (InvalidOperationException ex)
            {
                Console.Out.Write(ex);
            }
        }

        private void InviteToConference(string invitee)
        {
            try
            {
                
                    ConferenceInvitation invitation = new ConferenceInvitation(_conversation);
                    invitation.BeginDeliver(
                    invitee,
                    deliverAsyncResult =>
                    {
                        try
                        {
                            /* Invited recipient to the conference. */
                            invitation.EndDeliver(deliverAsyncResult);
                        }
                        catch (RealTimeException ex)
                        {
                            Console.Out.Write(ex);
                        }
                    },
                    null);
                
            }
            catch (InvalidOperationException ex)
            {
                Console.Out.Write(ex);
            }

        }


        #region Event handlers

        private void PresenceReceivedEventHandler(object sender, RemotePresentitiesNotificationEventArgs e)
        {
            Console.Out.Write("PresenceReceivedEventHandler");
            IEnumerable<RemotePresentityNotification> notifications = e.Notifications;

            // Make sure the call has finished
            //

            // Grap the first notification in the results
            RemotePresentityNotification notification = notifications.FirstOrDefault();

            Console.Out.Write("Availability: " + notification.AggregatedPresenceState.Availability.ToString());
        }

        private void CallStateChanged(object sender, CallStateChangedEventArgs e)
        {
            Console.Out.Write("Outgoing call - state change.\n" 
                + "Previous state: " + e.PreviousState + "\n" 
                + "Current state: " + e.State + "\n" 
                + "TransitionReason: " + e.TransitionReason + "\n");
            if (e.State == CallState.Established)
            {
                _conversation.Impersonate(_invitees.ElementAt(0), null, null);
                try
                {
                    ConferenceJoinOptions options = new ConferenceJoinOptions();
                    options.JoinMode = JoinMode.TrustedParticipant;

                    _conversation.ConferenceSession.BeginJoin(
                        options,
                        joinAsyncResult =>
                        {
                            try
                            {
                                _conversation.ConferenceSession.EndJoin(joinAsyncResult);
                                Console.Out.Write("Joined conference.");
                                for (int i = 1; i < _invitees.Count; i++)
                                {
                                    string invitee = _invitees.ElementAt(i);
                                    InviteToConference(invitee);
                                }
                            }
                            catch (RealTimeException ex)
                            {
                                Console.Out.Write("Failed to join conference when escalating the call.\n{0}", ex);
                            }
                        },
                        null);
                }
                catch (InvalidOperationException ex)
                {
                    Console.Out.Write("Failed to escalate call to conference: {0}", ex);
                }
            }

        }

        #endregion Event handlers
    }
}
