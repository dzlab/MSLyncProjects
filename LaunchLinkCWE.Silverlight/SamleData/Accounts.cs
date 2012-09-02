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
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace LaunchLinkCWE.Silverlight.SampleData
{
    public class Account
    {
        private Random rand = new Random(DateTime.Now.Millisecond);

        public Account(int id, string accountName)
        {
            ID = id;
            AccountName = accountName;
            CreationDate = getRandomDate(new DateTime(1990, 1, 1), DateTime.Now);
            Orders = new Orders();
        }

        public int ID { get; set; }
        public string AccountName { get; set; }
        public DateTime CreationDate { get; set; }
        public ObservableCollection<Order> Orders { get; set; }

        private static DateTime getRandomDate(DateTime minDate, DateTime maxDate)
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            int totalDays = (int)DateTimeUtil.DateDiff(DateInterval.Day, minDate, maxDate);
            int randomDays = rand.Next(0, totalDays);
            return minDate.AddDays(randomDays);
        }
    }

    public class Accounts : ObservableCollection<Account>
    {
        public Accounts()
        {
            this.Add(new Account(1, "Active Systems"));
            this.Add(new Account(2, "Affordable Sports Equipment"));
            this.Add(new Account(3, "Brightwork Company"));
            this.Add(new Account(4, "Bulk Discount Store"));
            this.Add(new Account(5, "Capable Sales and Service"));
            this.Add(new Account(6, "Central Discount Store"));
            this.Add(new Account(7, "Designated Distributors"));
            this.Add(new Account(8, "Distant Inn"));
            this.Add(new Account(9, "Eastside Parts Shop"));
            this.Add(new Account(10, "First Department Stores"));
            this.Add(new Account(11, "General Industries"));
            this.Add(new Account(12, "Instruments and Parts Company"));
            this.Add(new Account(13, "Metro Manufacturers"));
            this.Add(new Account(14, "Nuts and Bolts Mfg."));
            this.Add(new Account(15, "Out-of-the-Way Hotels"));
            this.Add(new Account(16, "Paint Supply"));
            this.Add(new Account(17, "Quick Delivery Service"));
            this.Add(new Account(18, "Requisite Part Supply"));
            this.Add(new Account(19, "Rewarding Activities Company"));
            this.Add(new Account(20, "Roadway Supplies"));
            this.Add(new Account(21, "Separate Parts Corporation"));
            this.Add(new Account(22, "Tachometers and Accessories"));
            this.Add(new Account(23, "Technical Parts Manufacturing"));
            this.Add(new Account(24, "Urban Sports Emporium"));
            this.Add(new Account(25, "West Side Mart"));
            this.Add(new Account(26, "World of Bikes"));
            this.Add(new Account(27, "Year-Round Sports"));
        }
    }
}
