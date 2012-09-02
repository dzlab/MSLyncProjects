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
using System.Threading;

namespace LaunchLinkCWE.Silverlight.SampleData
{
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
