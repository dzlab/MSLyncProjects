using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
/*
using CodeScales.Http;
using CodeScales.Http.Methods;
using CodeScales.Http.Entity;
using CodeScales.Http.Common;
*/
namespace LyncPresence
{
    class ConnectionHelper
    {
        public ConnectionHelper() 
        {
            
        }

        public static void PostData(string uri, string data)
        {
            Console.WriteLine("Posting data to " + uri);
            
            ASCIIEncoding encoding=new ASCIIEncoding();
            byte[] bytes = encoding.GetBytes(data);

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
            //request.Proxy = 
            
            request.Method = "POST";
            request.ContentType="application/*";
	        request.ContentLength = data.Length;
            Stream reqStream = request.GetRequestStream();
            
            reqStream.Write(bytes, 0, data.Length);
            reqStream.Close();
            
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();

            Console.WriteLine(response.StatusCode);
            Stream resStream = response.GetResponseStream();            
            Console.WriteLine(resStream.ToString());         
        }

    }
}
