using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Collaboration.ContactsGroups;
using Microsoft.Rtc.Signaling;

namespace LyncPBX
{
    /// <summary>
    /// Encapsulates remote presence subscription and queries on a local endpoint. 
    /// A presence watcher can receive presence notifications using
    ///   1. Persistent subscription in real time
    ///   2. Polling subscription at prescribed fixed intervals of time
    ///   3. Querying remote presence on demand
    ///   4. Refreshing remote presence views
    ///   
    /// The subscriptions are managed using instances of the RemotePresenceView class.
    /// The query is handled using the LocalEndpointPresenceServices class.
    /// Subscription refresh are handled using LocalEndpointPresenceServices class.
    /// </summary>
    class PresenceWatcher : UcmaObject
    {
        protected LocalOwnerPresence _localPresenceServices;
        protected LocalEndpointPresenceServices _remotePresenceServices;
        protected RemotePresenceView _persistentPresenceView;
        protected RemotePresenceView _pollingPresenceView;

        #region process-related events
        public event EventHandler<AsyncOpStatusEventArgs> OnQueryPresenceCompleted;
        public event EventHandler<AsyncOpStatusEventArgs> OnSubscribeToPresenceCompleted;
        public event EventHandler<AsyncOpStatusEventArgs> OnRefreshPresenceViewsCompleted;
        #endregion process-related events

        #region results-related events
        public event EventHandler<RemotePresentitiesNotificationEventArgs> OnPersistentPresenceReceived;
        public event EventHandler<RemoteSubscriptionStateChangedEventArgs> OnPersistentSubscriptionStateChanged;

        public event EventHandler<RemotePresentitiesNotificationEventArgs> OnPollingPresenceReceived;
        public event EventHandler<RemoteSubscriptionStateChangedEventArgs> OnPollingSubscriptionStateChanged;

        public event EventHandler<RemotePresentitiesNotificationEventArgs> OnQueryingPresenceReceived;
        #endregion results-related events

        #region class constructor
        /// <summary>
        /// Constructor of a presence watcher supporting persistent, polling and on-demand subscriptions 
        /// to presence categories published by specified remote presentities. on-demand subscription is 
        /// also known as presence query. The code here also handles self-presence.
        /// </summary>
        /// <param name="endpoint"></param>
        public PresenceWatcher(LocalEndpoint endpoint)
        {
            _remotePresenceServices = endpoint.PresenceServices;

            // RemotePresenceView for persitent subscription:
            RemotePresenceViewSettings rpvs = new RemotePresenceViewSettings();
            rpvs.SubscriptionMode = RemotePresenceViewSubscriptionMode.Persistent;
            
            _persistentPresenceView = new RemotePresenceView(endpoint, rpvs);
            _persistentPresenceView.PresenceNotificationReceived += new EventHandler<RemotePresentitiesNotificationEventArgs>(PersistentPresenceReceivedEventHandler);
            
            _persistentPresenceView.SubscriptionStateChanged += new EventHandler<RemoteSubscriptionStateChangedEventArgs>(PersistentSubscriptionStateChangedEventHandler);
            
            // RemotePresenceView for polling subscription
            rpvs = new RemotePresenceViewSettings();
            rpvs.SubscriptionMode = RemotePresenceViewSubscriptionMode.Polling;
            rpvs.PollingInterval = new TimeSpan(0, 5, 0);  // every 5 minutes
            _pollingPresenceView = new RemotePresenceView(endpoint, rpvs);

            _pollingPresenceView.SetPresenceSubscriptionCategoriesForPolling(new string[] { "contactCard", "state", "note", "noteHistory" });
            
            _pollingPresenceView.PresenceNotificationReceived += new EventHandler<RemotePresentitiesNotificationEventArgs>(PollingPresenceReceivedEventHandler);

            _pollingPresenceView.SubscriptionStateChanged += new EventHandler<RemoteSubscriptionStateChangedEventArgs>(PollingSubscriptionStateChangedEventHandler);

        }
        #endregion class constructor

        #region Event handlers for persistent subscription
        void PersistentPresenceReceivedEventHandler(object sender, RemotePresentitiesNotificationEventArgs e)
        {
            if (OnPersistentPresenceReceived != null)
                RaiseEvent(OnPersistentPresenceReceived, this, e);
        }

        void PersistentSubscriptionStateChangedEventHandler(object sender, RemoteSubscriptionStateChangedEventArgs e)
        {
            if (OnPersistentSubscriptionStateChanged != null)
                RaiseEvent(OnPersistentSubscriptionStateChanged, this, e);
        }
        #endregion Event handlers for persistent subscription

        #region Event handlers for polling subscription
        void PollingPresenceReceivedEventHandler(object sender, RemotePresentitiesNotificationEventArgs e)
        {
            if (OnPollingPresenceReceived != null)
                RaiseEvent(OnPollingPresenceReceived, this, e);
        }

        void PollingSubscriptionStateChangedEventHandler(object sender, RemoteSubscriptionStateChangedEventArgs e)
        {
            if (OnPollingSubscriptionStateChanged != null)
                RaiseEvent(OnPollingSubscriptionStateChanged, this, e);
        }

        #endregion Event handlers for polling subscription

        #region Event handlers for querying subscription
        void QueryingPresenceReceivedEventHandler(object sender, RemotePresentitiesNotificationEventArgs e)
        {
            Console.WriteLine("An event received for QueryingPresenceReceivedEventHandler");
            if (OnQueryingPresenceReceived != null)
            {                
                foreach (RemotePresentityNotification presentity in e.Notifications)
                {
                    Console.WriteLine("Availability: " + presentity);
                }
                RaiseEvent(OnQueryingPresenceReceived, this, e);
            }
        }

        #endregion Event handlers for querying subscription

        #region Persistent subscription

        public void StartPersistentSubscription(IEnumerable<string> uris)
        {
            if (uris == null)
                return;
            List<RemotePresentitySubscriptionTarget> targets = new List<RemotePresentitySubscriptionTarget>();
            foreach (string uri in uris)
                targets.Add(new RemotePresentitySubscriptionTarget(uri));
            _persistentPresenceView.StartSubscribingToPresentities(targets);
        }

        public void StopPersistentSubscription(IEnumerable<string> uris)
        {
            if (uris == null)
                return;
            _persistentPresenceView.StartUnsubscribingToPresentities(uris);
        }


        public void StopPersistentSubscription()
        {
            this.StopPersistentSubscription(_persistentPresenceView.GetPresentities());
        }


        #endregion Persistent subscription

        #region Polling subscription
        public void SetPollingCategories(params string[] categoryNames)
        {
            if (categoryNames == null || categoryNames.Length == 0)
                return;
            _pollingPresenceView.SetPresenceSubscriptionCategoriesForPolling(categoryNames);
        }

        public void StartPollingSubscription(params string[] sipUris)
        {
            IEnumerable<string> uris = sipUris as IEnumerable<string>;
            this.StartPollingSubscription(uris);
        }

        public void StartPollingSubscription(IEnumerable<string> sipUris)
        {
            List<RemotePresentitySubscriptionTarget> sipUriList = new List<RemotePresentitySubscriptionTarget>();
            foreach (string sipUri in sipUris)
                sipUriList.Add(new RemotePresentitySubscriptionTarget(sipUri));
            _pollingPresenceView.StartSubscribingToPresentities(sipUriList.ToArray());
        }

        public void StopPollingSubscription(IEnumerable<string> sipUris)
        {
            _pollingPresenceView.StartUnsubscribingToPresentities(sipUris);
        }

        public void StopPollingSubscription()
        {
            IEnumerable<string> sipUris = _pollingPresenceView.GetPresentities();
            if (sipUris != null)
                this.StopPollingSubscription(sipUris);
        }

        #endregion polling subscription

        #region Querying remote presence
        public void QueryRemotePresence(string[] targets, string[] categories)
        {
            QueryRemotePresence(targets, categories, QueryingPresenceReceivedEventHandler);
        }

        public void QueryRemotePresence(string[] presentityUris, string[] categoryNames,
            EventHandler<RemotePresentitiesNotificationEventArgs> queryResultHandler)
        {
            _remotePresenceServices.BeginPresenceQuery(
                presentityUris,
                categoryNames,
                queryResultHandler,
                CallbackOnBeginPresenceQueryReturned,
                _remotePresenceServices);
        }

        private void CallbackOnBeginPresenceQueryReturned(IAsyncResult result)
        {
            Console.WriteLine("CallbackOnBeginPresenceQueryReturned");
            try
            {
                if (_remotePresenceServices == result.AsyncState as LocalEndpointPresenceServices)
                {
                    _remotePresenceServices.EndPresenceQuery(result);
                    // inform the caller of the OK status of the PresenceQuery operation.
                    if (OnQueryPresenceCompleted != null)
                    {

                        RaiseEvent(OnQueryPresenceCompleted, this, new AsyncOpStatusEventArgs(AsyncOpStatus.OK, null));
                    }
                }
            }
            catch (Exception ex)
            {
                // inform the caller of the Error status of the presence querying operation.
                if (OnQueryPresenceCompleted != null)
                    RaiseEvent(OnQueryPresenceCompleted, this,
                        new AsyncOpStatusEventArgs(AsyncOpStatus.Error, ex));
            }
        }
        #endregion Querying remote presence

        #region Refresh all remote presence views
        public void RefreshRemoteSubscription()
        {
            _remotePresenceServices.BeginRefreshRemotePresenceViews(CallbackOnBeginRefreshReturned, _remotePresenceServices);
        }

        private void CallbackOnBeginRefreshReturned(IAsyncResult result)
        {
            try
            {
                LocalEndpointPresenceServices localEndpointPresenceServices = result.AsyncState as LocalEndpointPresenceServices;
                if (localEndpointPresenceServices == _remotePresenceServices)
                {
                    _remotePresenceServices.EndRefreshRemotePresenceViews(result);
                    if (OnRefreshPresenceViewsCompleted != null)
                        RaiseEvent(OnRefreshPresenceViewsCompleted, this,
                            new AsyncOpStatusEventArgs(AsyncOpStatus.OK, null));
                }
            }
            catch (Exception ex)
            {
                if (OnRefreshPresenceViewsCompleted != null)
                    RaiseEvent(OnRefreshPresenceViewsCompleted, this,
                        new AsyncOpStatusEventArgs(AsyncOpStatus.Error, ex));
            }
        }
        #endregion Refresh all remote presence views

        public enum AsyncOpStatus { OK, Error };

        public class AsyncOpStatusEventArgs : EventArgs
        {
            Exception _exception;
            AsyncOpStatus _status;
            public AsyncOpStatusEventArgs(AsyncOpStatus status, Exception exception)
            {
                _exception = exception;
                _status = status;
            }
            public Exception Exception { get { return this._exception; } }
            public AsyncOpStatus Status { get { return this._status; } }
        }


    }
}
