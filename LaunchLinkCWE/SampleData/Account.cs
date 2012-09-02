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
using System.Text;
using System.Collections.ObjectModel;
using System.Threading;

namespace LaunchLinkCWE.SampleData
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

        public double TotalSales
        {
            get
            {
                return Orders.Sum(o => o.Total);
            }
        }

        private static DateTime getRandomDate(DateTime minDate, DateTime maxDate)
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            int totalDays = (int)DateTimeUtil.DateDiff(DateInterval.Day, minDate, maxDate);
            int randomDays = rand.Next(0, totalDays);
            return minDate.AddDays(randomDays);
        }

        public override string ToString()
        {
            return AccountName;
        }
    }

    public class Order
    {
        public Order() { }

        public string SalesNumber { get; set; }
        public DateTime SaleDate { get; set; }
        public double Total { get; set; }

        public double Tax
        {
            get
            {
                if (Total != default(double))
                    return Math.Round(Total * 0.08, 2);
                else
                    return 0.00;
            }
        }

        public double SubTotal
        {
            get
            {
                return Math.Round(Total - Tax, 2);
            }
        }
    }

    public class Orders : ObservableCollection<Order>
    {
        public Orders()
        {
            for (int i = 0; i < 20; i++)
            {
                Order o = new Order();
                o.SalesNumber = "SO7177" + i.ToString();
                o.SaleDate = getRandomDate(new DateTime(2008, 1, 1), DateTime.Now);
                Thread.Sleep(1);
                o.Total = getRandomTotal();
                this.Add(o);
            }
        }

        private static double getRandomTotal()
        {
            Random rand = new Random();
            int dollar = rand.Next(85, 50000);
            double cents = rand.NextDouble();
            return Math.Round(dollar + (cents * 10), 2);
        }

        private static DateTime getRandomDate(DateTime minDate, DateTime maxDate)
        {
            Random rand = new Random();
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

    public enum DateInterval
    {
        Day,
        DayOfYear,
        Hour,
        Minute,
        Month,
        Quarter,
        Second,
        Weekday,
        WeekOfYear,
        Year
    }

    public sealed class DateTimeUtil
    {
        #region DateDiff Methods

        public static long DateDiff(DateInterval interval,
            DateTime dt1, DateTime dt2)
        {

            return DateDiff(interval, dt1, dt2, System.Globalization
                .DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek);

        }

        private static int GetQuarter(int nMonth)
        {
            if (nMonth <= 3)
                return 1;
            if (nMonth <= 6)
                return 2;
            if (nMonth <= 9)
                return 3;
            return 4;
        }

        public static long DateDiff(DateInterval interval,
            DateTime dt1, DateTime dt2, DayOfWeek eFirstDayOfWeek)
        {

            if (interval == DateInterval.Year)

                return dt2.Year - dt1.Year;

            if (interval == DateInterval.Month)

                return (dt2.Month - dt1.Month) +
                    (12 * (dt2.Year - dt1.Year));

            TimeSpan ts = dt2 - dt1;

            if (interval == DateInterval.Day ||
                interval == DateInterval.DayOfYear)
                return Round(ts.TotalDays);

            if (interval == DateInterval.Hour)
                return Round(ts.TotalHours);

            if (interval == DateInterval.Minute)
                return Round(ts.TotalMinutes);

            if (interval == DateInterval.Second)

                return Round(ts.TotalSeconds);

            if (interval == DateInterval.Weekday)
            {
                return Round(ts.TotalDays / 7.0);
            }

            if (interval == DateInterval.WeekOfYear)
            {
                while (dt2.DayOfWeek != eFirstDayOfWeek)
                    dt2 = dt2.AddDays(-1);
                while (dt1.DayOfWeek != eFirstDayOfWeek)
                    dt1 = dt1.AddDays(-1);
                ts = dt2 - dt1;
                return Round(ts.TotalDays / 7.0);
            }

            if (interval == DateInterval.Quarter)
            {
                double d1Quarter = GetQuarter(dt1.Month);
                double d2Quarter = GetQuarter(dt2.Month);
                double d1 = d2Quarter - d1Quarter;
                double d2 = (4 * (dt2.Year - dt1.Year));
                return Round(d1 + d2);
            }
            return 0;
        }

        private static long Round(double dVal)
        {
            if (dVal >= 0)
                return (long)Math.Floor(dVal);
            return (long)Math.Ceiling(dVal);
        }

        #endregion
    }
}
