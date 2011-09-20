using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CertVerifierService
{
    internal static class TaskQueue
    {
        private static readonly Queue<Task> taskQueue;
        private static volatile bool stopped;
        private static readonly ManualResetEvent stopEvent;

        static TaskQueue()
        {
            taskQueue = new Queue<Task>();
            stopped = false;
            stopEvent = new ManualResetEvent(false);
        }

        public static bool Stopped { get { return stopped; } }
        public static ManualResetEvent StopEvent { get { return stopEvent; } }

        public static void Enqueue(Task task)
        {
            lock (taskQueue)
                if (!stopped)
                {
                    taskQueue.Enqueue(task);
                    Monitor.Pulse(taskQueue);
                }
        }

        public static Task Dequeue()
        {
            lock (taskQueue)
            {
                while (taskQueue.Count == 0 && !stopped)
                    Monitor.Wait(taskQueue);
                return stopped ? null : taskQueue.Dequeue();
            }
        }

        public static void Stop()
        {
            lock (taskQueue)
            {
                stopped = true;
                stopEvent.Set();
                Monitor.PulseAll(taskQueue);
            }
        }
    }
}