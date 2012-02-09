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
                Log.Write("Start.");
                while (!TaskQueue.Stopped)
                {
                    Log.Write("WaitForQueue");
                    Task task = TaskQueue.Dequeue();
                    if (task != null)
                    {
                        Log.Write("Before handle");
                        ((Handler) handler).Handle(task);
                        Log.Write("Handled");
                    }
                }
                Log.Write("Finish.");
            }
            catch (Exception e)
            {
                Log.Write(e);
                StartThread();
                Thread.CurrentThread.Abort();
            }
        }

       
    }
}