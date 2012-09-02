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
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;

namespace LaunchLink
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Indexed command line args using hash table
        public static Hashtable CommandLineArgs = new Hashtable();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Don't bother if no command line args were passed
            // NOTE: e.Args is never null - if no command line args were passed, 
            //       the length of e.Args is 0.
            if (e.Args.Length == 0) return;

            // Parse command line args for args in the following format:
            //   /argname:argvalue /argname:argvalue /argname:argvalue ...
            //
            // Note: This sample uses regular expressions to parse the command line arguments.
            // For regular expressions, see:
            // http://msdn.microsoft.com/library/en-us/cpgenref/html/cpconRegularExpressionsLanguageElements.asp
            string pattern = @"(?<argname>\w+):(?<argvalue>\w+)";
            foreach (string arg in e.Args)
            {
                Match match = Regex.Match(arg, pattern);

                // If match not found, command line args are improperly formed.
                if (!match.Success) throw new ArgumentException("The command line arguments are improperly formed. Use argname:argvalue.");

                // Store command line arg and value
                CommandLineArgs[match.Groups["argname"].Value] = match.Groups["argvalue"].Value;
            }
        }
    }
}