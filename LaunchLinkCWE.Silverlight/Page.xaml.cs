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
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Microsoft.Lync.Model.Conversation;

using LaunchLinkCWE.Silverlight.SampleData;
using Microsoft.Lync.Model;

namespace LaunchLinkCWE.Silverlight
{
    public partial class Page : UserControl
    {
        string _appData = String.Empty;

        // TODO: 3.2.2 Set the application Id
        string _applicationGuid = "{E222C0E4-5C01-4975-B053-845EAAF82CFE}";

        Accounts _accounts = new Accounts();

        public Page()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(Page_Loaded);
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Conversation conversation = null;

            //TODO: 3.2.3 Get the hosting conversation
            conversation = LyncClient.GetHostingConversation() as Conversation;


            if (conversation != null)
            {
                // TODO: 3.2.4 Retrieve the contextual data from the conversation
                _appData = conversation.GetApplicationData(_applicationGuid);

                this.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        //TODO: 3.2.5 Load the selected account
                        int accountId = Convert.ToInt32(_appData.Split(new char[] { ':' })[1]);
                        Account account = _accounts.Where(a => a.ID == accountId).First();
                        this.accountName.Text = account.AccountName;
                        ordersList.ItemsSource = account.Orders;
                    }));
            }
        }
    }
}
