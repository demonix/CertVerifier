using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CertVerifierService
{
    public static class Listener
    {
        public static void Listen(object port)
        {
            WebServer webServer = new WebServer("http://+:8989/certVerifier/");
            webServer.IncomingRequest += WebServerIncomingRequest;
            webServer.Start();

        }

        private static void WebServerIncomingRequest(object sender, HttpRequestEventArgs e)
        {
            HttpListenerContext httpContext = e.RequestContext;
            TaskQueue.Enqueue(new Task(httpContext));
        }
    }
}