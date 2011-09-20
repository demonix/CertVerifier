using System;
using System.IO;
using System.Text;
using System.Threading;

namespace CertVerifierService
{
    public static class Log
    {
        public static void Write(Exception e)
        {
            Write(string.Format("{0}: {1}", e.GetType(), e.Message));
        }

        public static void Write(string message)
        {
            lock (output) output.WriteLine(string.Format("{0} at {1:o}. {2}", Thread.CurrentThread.Name, DateTime.Now.ToUniversalTime(), message));
        }

        static Log()
        {
            output = new StreamWriter(GetLogName(), false, Encoding.Default) { AutoFlush = true };
        }

        static string GetLogName()
        {
            if (!Directory.Exists("Logs")) Directory.CreateDirectory("Logs");
            return string.Format("Logs\\{0:yyyy-MM-dd-HH-mm-ss-fffffff}.log", DateTime.Now.ToUniversalTime());
        }

        static readonly StreamWriter output;
    }
}