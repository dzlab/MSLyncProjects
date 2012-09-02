// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using Microsoft.Lync.Controls;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Extensibility;

using LaunchLink.SampleData;

namespace LaunchLink
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private Accounts _accounts;

        private Microsoft.Lync.Model.LyncClient _lyncClient;
        private Microsoft.Lync.Model.Conversation.ConversationManager _conversationManager;

        private string _primaryLabUserId;
        private string _secondaryLabUserId;
        private string _applicationGuid;
        private string _applicationName;

        // TODO: 3.1.5 Application registration
        private ApplicationRegistration _applicationRegistration;
        

        public Window1()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(Window1_Loaded);
        }

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            if ((String.IsNullOrEmpty(ConfigurationManager.AppSettings["PrimaryLabUserId"]))
                || (String.IsNullOrEmpty(ConfigurationManager.AppSettings["SecondaryLabUserId"]))
                || (String.IsNullOrEmpty(ConfigurationManager.AppSettings["ApplicationGuid"]))
                || (String.IsNullOrEmpty(ConfigurationManager.AppSettings["ApplicationName"])))
            {
                MessageBox.Show("Please provide values for all keys in app.config",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

            _primaryLabUserId = ConfigurationManager.AppSettings["PrimaryLabUserId"];
            _secondaryLabUserId = ConfigurationManager.AppSettings["SecondaryLabUserId"];
            _applicationGuid = ConfigurationManager.AppSettings["ApplicationGuid"];
            _applicationName = ConfigurationManager.AppSettings["ApplicationName"];

            _accounts = new Accounts();
            accountsList.ItemsSource = _accounts;

            try
            {
                _lyncClient = Microsoft.Lync.Model.LyncClient.GetClient();

                // TODO: 3.1.3 Get the command line arguments
                if (App.CommandLineArgs.Count > 0 && App.CommandLineArgs["AccountId"] != null)
                {
                    LoadSelectedAccount(Convert.ToInt32(App.CommandLineArgs["AccountId"]));
                }

                InitializeClient();
            }
            catch (Exception)
            {
                throw;
            }
        }

        void InitializeClient()
        {
            _conversationManager = _lyncClient.ConversationManager;

            // TODO: 3.1.6 Runtime registration
            _applicationRegistration = _lyncClient.CreateApplicationRegistration(_applicationGuid, _applicationName);

            this._conversationManager.ConversationAdded +=
                new EventHandler<Microsoft.Lync.Model.Conversation.ConversationManagerEventArgs>(ConversationManager_ConversationAdded);
        }

        void ConversationManager_ConversationAdded(object sender, Microsoft.Lync.Model.Conversation.ConversationManagerEventArgs e)
        {
            e.Conversation.ConversationContextLinkClicked +=
                new EventHandler<Microsoft.Lync.Model.Conversation.InitialContextEventArgs>(Conversation_ConversationContextLinkClicked);
        }


        void Conversation_ConversationContextLinkClicked(object sender, Microsoft.Lync.Model.Conversation.InitialContextEventArgs e)
        {
            this.Dispatcher.Invoke(
                DispatcherPriority.Input,
                new Action(() =>
                {
                    int accountId = Convert.ToInt32(
                        e.ApplicationData.Split(new char[] { ':' })[1]);
                    LoadSelectedAccount(accountId);
                }));
        }

        void LoadSelectedAccount(int accountId)
        {
            // TODO: 3.1.4 Select the account
            foreach (var account in _accounts)
            {
                if(account.ID == accountId)
                {
                    accountsList.SelectedItem = account;
                    break;
                }
            }
            
        }

        private void accountsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (accountsList.SelectedIndex != -1)
            {
                Account selectedAccount = accountsList.SelectedItem as Account;

                secondaryLabUser.Text = _secondaryLabUserId;

                presenceIndicator.Source = _secondaryLabUserId;
                startInstantMessagingButton.Source = _secondaryLabUserId;
                startAudioCallButton.Source = _secondaryLabUserId;

                // TODO: 3.1.1 Create conversation context
                ConversationContextualInfo context = new ConversationContextualInfo();
                context.Subject = selectedAccount.AccountName;
                context.ApplicationId = _applicationGuid;
                context.ApplicationData = "AccountId:" + selectedAccount.ID.ToString();

                // TODO: 3.1.2 Attach context to Communicator Controls
                startInstantMessagingButton.ContextualInformation = context;
                startAudioCallButton.ContextualInformation = context;
            }
        }
    }
}