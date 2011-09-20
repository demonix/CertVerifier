using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CertVerifierService
{
    class Program
    {
        private static int port;

        public static int Main(string[] args)
        {
            port = GetPort();
            if (args.Length > 0) return ControlMode(args);

            Start();
            return 0;
        }

        private static int GetPort()
        {
            const string fileName = "port";
            if (!File.Exists(fileName)) return 10080;

            using (var stream = new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
                return int.Parse(stream.ReadLine());
        }

        private static int ControlMode(string[] args)
        {
            if (args[0] == "--stop") Stop();
            return 0;
        }

        private static void Stop()
        {
            using (var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, port));
                    clientSocket.Send(Encoding.UTF8.GetBytes("STOP\r\n\r\n"));
                }
                catch (SocketException)
                {
                    Log.Write("Nothing to stop.");
                }
            }
        }

        private static void Start()
        {

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Verifier.GetInstance();
            ThreadPool.Start(16);
            new Thread(Listener.Listen).Start(port);
        }
    }
}
