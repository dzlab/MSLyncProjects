using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace UCMAAppMgmt
{
    internal static class CertificateHelper
    {
        internal static X509Certificate2 GetLocalCertificate()
        {
            // Get a reference to the local machine certificates in the store and 
            // open it in read-only mode.
            X509Store store = new X509Store(StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            // Get a reference to the collection of certificates in the store.
            X509Certificate2Collection certificates = store.Certificates;

            // Find the first certificate whose name is the FQDN of the local machine
            foreach (X509Certificate2 certificate in certificates)
            {
                if (certificate.SubjectName.Name.ToUpper().Contains(Dns.GetHostEntry("localhost").HostName.ToUpper())
                    && certificate.HasPrivateKey)
                {
                    return certificate;
                }
            }
            return null;
        }
    }
}
