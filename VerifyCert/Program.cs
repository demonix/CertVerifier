using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace VerifyCert
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
                Console.WriteLine("Usage: VerifyCert.exe <type> <path>, type='n' or 'o'. n - new check; o - old check.");
            
            Console.WriteLine("Verifying cert " + args[0]);
            byte[] bytes = File.ReadAllBytes(args[0]);
            long arrayLength = CalculateBase64Length(bytes);
            char[] base64bytes;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(args[1]);
            webRequest.Proxy = null;
            webRequest.Method = "POST";
            Stream reqStream = webRequest.GetRequestStream();
            if (bytes[0] == 0x30)
            {
                base64bytes = new char[arrayLength];
                Convert.ToBase64CharArray(bytes, 0, bytes.Length, base64bytes, 0);
                using (StreamWriter sw = new StreamWriter(reqStream))
                {
                    sw.Write(base64bytes);
                    sw.Close();
                }
                reqStream.Close();
            }
            else
            {
                reqStream.Write(bytes, 0, bytes.Length);
                reqStream.Close();
            }

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
                Console.WriteLine(new StreamReader(ms, Encoding.UTF8).ReadToEnd());
            }
            catch (WebException exception)
            {
                if (exception.Response != null)
                {
                    HttpWebResponse rsp = (HttpWebResponse)exception.Response;
                    MemoryStream ms = new MemoryStream();
                    Stream rspStream = rsp.GetResponseStream();
                    int bytesRead;
                    byte[] buffer = new byte[1024];
                    while ((bytesRead = rspStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, bytesRead);
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                    Console.WriteLine("error while checking cert\r\nStatus code: " + rsp.StatusCode + "\r\nResponse text: " + new StreamReader(ms, Encoding.UTF8).ReadToEnd());
                }
                else throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error while checking cert: " + ex);
            }
        }

        private static long CalculateBase64Length(byte[] bytes)
        {
            long arrayLength = (long)((4.0d / 3.0d) * bytes.Length);
            if (arrayLength % 4 != 0)
            {
                arrayLength += 4 - arrayLength % 4;
            }
            return arrayLength;
        }
    }
}