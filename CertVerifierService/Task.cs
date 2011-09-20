using System.Diagnostics;

namespace CertVerifierService
{
    public class Task
    {
        private readonly Stopwatch _timer = new Stopwatch();
        private readonly object _taskData;

        public Task(object taskData)
        {
            _taskData = taskData;
            _timer.Start();
        }

        public Stopwatch Timer {get { return _timer; }}
        public object TaskData { get { return _taskData; } }
    }
}