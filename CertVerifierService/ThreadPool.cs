using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CertVerifierService
{
    public static class ThreadPool
    {
        public delegate void WorkCallback();

        public static void Start(int poolSize)
        {
            for (int i = 0; i < poolSize; ++i)
                StartThread();
        }

        private static void StartThread()
        {
            var thread = new Thread(Work) { Name = "th-" + Guid.NewGuid() };
            thread.Start(new Handler());
        }

        private static void Work(object handler)
        {
            try
            {
                Log("Start.");
                while (!TaskQueue.Stopped)
                {
                    Task task = TaskQueue.Dequeue();
                    if (task != null)
                        
                            ((Handler)handler).Handle(task);
                }
                Log("Finish.");
            }
            catch (Exception e)
            {
                Log(e);
                StartThread();
                Thread.CurrentThread.Abort();
            }
        }

        private static void Log(string message)
        {
            CertVerifierService.Log.Write(message);
        }


        private static void Log(Exception e)
        {
            CertVerifierService.Log.Write(string.Format("Unhandled exception. Exception details: {0}",e));
        }
    }
}