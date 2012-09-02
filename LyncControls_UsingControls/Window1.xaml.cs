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
using System.Configuration;

namespace LyncControls_UsingControls
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(Window1_Loaded);
        }

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            if ((String.IsNullOrEmpty(ConfigurationManager.AppSettings["PrimaryLabUserId"]))
                || (String.IsNullOrEmpty(ConfigurationManager.AppSettings["SecondaryLabUserId"])))
            {
                MessageBox.Show("Please provide values for all keys in app.config",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }

            string primaryLabUserId = ConfigurationManager.AppSettings["PrimaryLabUserId"];
            string secondaryLabUserId = ConfigurationManager.AppSettings["SecondaryLabUserId"];

            //TODO: 2.1.1 Set the Source property of the Lync controls
            presenceIndicator.Source = primaryLabUserId;

            //TODO: 2.1.4 Populate the custom contact list
            /*
            customContactList.ItemsSource = new List<String>()
            {
                secondaryLabUserId,
                "sip:brianc@fabrikam.com",
                "sip:carlosg@fabrikam.com",
                "sip:christg@fabrikam.com",
                "sip:bernart@fabrikam.com"
            };
             * */
        }

        private void btnTransferCall_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
