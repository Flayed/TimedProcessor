using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;

namespace Tigrinum.TimedProcessor
{
    public class TimedProcessor
    {
        /// <summary>
        /// Times the TimedProcessor has fired its Elapsed function
        /// </summary>
        public int TimesElapsed { get; set; }

        /// <summary>
        /// Indicates whether or not the TimedProcessor is running
        /// </summary>
        public bool IsRunning => !IsStopped;

        /// <summary>
        /// Indicates whether or not the TimedProcessor is stopped
        /// </summary>
        private bool IsStopped { get; set; } = true;

        /// <summary>
        /// Starts the TimedProcessor
        /// </summary>
        public void Start()
        {
            Timer.Start();
            IsStopped = false;
        }

        /// <summary>
        /// Stops the TimedProcessor
        /// </summary>
        public void Stop()
        {
            Timer.Stop();
            IsStopped = true;
        }

        /// <summary>
        /// Creates a TimedProcessor - a wrapper around a System.Timer - with an asynchronous elapsed function
        /// </summary>
        /// <param name="interval">The time in miliseconds to wait between firing of the elapsed function</param>
        /// <param name="processor">The asynchronous elapsed function to fire </param>
        /// <param name="whenStartedFireImmediately">Indicates that the elapsed function should fire immediately when started (instead of waiting for the initial interval)</param>
        public TimedProcessor(int interval, Func<Task> processor, bool whenStartedFireImmediately = true)
        {
            AsyncProcessor = processor;
            HasAsyncProcessor = true;
            InitializeTimedProcessor(interval, whenStartedFireImmediately);
        }

        /// <summary>
        /// Creates a TimedProcessor - a wrapper around a System.Timer - with a synchronous elapsed function
        /// </summary>
        /// <param name="interval">The time in miliseconds to wait between firing of the elapsed function</param>
        /// <param name="processor">The elapsed function to fire</param>
        /// <param name="whenStartedFireImmediately">Indicates that the elapsed function should fire immediately when started (instead of waiting for the initial interval)</param>
        public TimedProcessor(int interval, Action processor, bool whenStartedFireImmediately = true)
        {
            Processor = processor;
            HasProcessor = true;
            InitializeTimedProcessor(interval, whenStartedFireImmediately);
        }

        /// <summary>
        /// Indicates that the TimedProcessor has fired at least once
        /// </summary>
        private bool HasFired { get; set; }

        /// <summary>
        /// The time in miliseconds to wait between firings of the elapsed function
        /// </summary>
        private int Interval { get; set; }

        /// <summary>
        /// Stopwatch to keep track of the life of this TimedProcessor
        /// </summary>
        private readonly Stopwatch _duration = new Stopwatch();

        /// <summary>
        /// Indicates if this TimedProcessor has had a synchronous timer associated to it
        /// </summary>
        internal bool HasProcessor;

        /// <summary>
        /// The asynchronous elasped function to fire
        /// </summary>
        private Action Processor { get; }

        /// <summary>
        /// Indicates if this TimedProcessor has had an asynchronous timer associated to it
        /// </summary>
        internal bool HasAsyncProcessor;

        /// <summary>
        /// The asynchronous elapsed function to fire
        /// </summary>
        private Func<Task> AsyncProcessor { get; }

        /// <summary>
        /// The underlying Timer
        /// </summary>
        private Timer Timer { get; set; }

        /// <summary>
        /// Initializes the TimedProcessor
        /// </summary>
        /// <param name="interval">The time in miliseconds to wait between firings of the elapsed function</param>
        /// <param name="whenStartedFireImmediately">Indicates that the elapsed function should fire immediately when started (instead of waiting for the initial interval)</param>
        private void InitializeTimedProcessor(int interval, bool whenStartedFireImmediately)
        {
            Interval = interval;
            HasFired = !whenStartedFireImmediately;
            TimesElapsed = 0;
            _duration.Start();
            Timer = new Timer() { AutoReset = false };
            if (!whenStartedFireImmediately) Timer.Interval = interval;
            Timer.Elapsed += TimerElapsed;
        }

        /// <summary>
        /// The underlying elapsed function.  This function will in turn fire the functions described in the constructor
        /// </summary>
        private async void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            TimesElapsed++;
            var timer = (sender as Timer);
            if (HasAsyncProcessor)
            {
                await AsyncProcessor.Invoke().ConfigureAwait(false);
            }
            if (HasProcessor)
            {
                Processor.Invoke();
            }
            if (!HasFired)
            {
                HasFired = true;
                if (timer != null) timer.Interval = Math.Max(Interval, 1);
            }
            if (!IsStopped)
                timer?.Start();
        }

        public override string ToString()
        {
            return $"TimedProcessor executed {TimesElapsed} times over {_duration}, average of {Math.Round((TimesElapsed / _duration.Elapsed.TotalSeconds), 2)} per second";
        }
    }
}
