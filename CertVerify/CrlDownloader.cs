using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Org.BouncyCastle.X509;

namespace CertVerify
{
    internal static class CrlDownloader
    {
        public static X509Crl DownloadCrl(List<string> cdpAddresses)
        {
            foreach (string cdpAddress in cdpAddresses)
            {
                HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(cdpAddress);
                Stream response;
                try
                {
                    response = webRequest.GetResponse().GetResponseStream();
                    MemoryStream ms = new MemoryStream();
                    int bytesRead;
                    byte[] buffer = new byte[1024];
                    while ((bytesRead = response.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, bytesRead);
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                    return new X509CrlParser().ReadCrl(ms);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error while downloading crl: " + ex);
                }
            }
            return null;
        }
    }
}