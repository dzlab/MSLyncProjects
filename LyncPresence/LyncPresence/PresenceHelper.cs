using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Collaboration.ContactsGroups;

namespace LyncPresence
{
    class PresenceHelper
    {
        private RemotePresenceView _remotePresenceView;
        private ApplicationEndpoint _appEndpoint;
        private string presencePubUri = "http://10.193.200.202:8080/Gateway/pub/supervisor/presence";

        public PresenceHelper(ApplicationEndpoint appEndpoint)
        {
            _appEndpoint = appEndpoint;
            
            // Subscribe to receive presence notifications
            _remotePresenceView = new RemotePresenceView(appEndpoint);
            _remotePresenceView.PresenceNotificationReceived += new EventHandler<RemotePresentitiesNotificationEventArgs>(PresenceReceivedEventHandler);
        }

        public void QueryUserPresence(string userSipUri)
        {
            Console.WriteLine("Querying presence of " + userSipUri);
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
                string presenceStatus = notification.AggregatedPresenceState.Availability.ToString();
                ConnectionHelper.PostData(presencePubUri, presenceStatus);

                Console.WriteLine("Availability: " + presenceStatus);
            }
            catch (RealTimeException ex)
            {
                Console.WriteLine("Presence query failed.", ex);
            }
            catch (Exception e)
            {
                Console.WriteLine("Presence query failed.", e);
            }
        }

        #region Event handlers

        private void PresenceReceivedEventHandler(object sender, RemotePresentitiesNotificationEventArgs e)
        {
            Console.WriteLine("PresenceReceivedEventHandler");
            IEnumerable<RemotePresentityNotification> notifications = e.Notifications;

            // Make sure the call has finished
            //

            // Grap the first notification in the results
            RemotePresentityNotification notification = notifications.FirstOrDefault();
            string presenceStatus = notification.AggregatedPresenceState.Availability.ToString();
            ConnectionHelper.PostData(presencePubUri, presenceStatus);

            Console.WriteLine("Availability: " + presenceStatus);
        }

        #endregion Event handlers
    }
}
