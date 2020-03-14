using System;

namespace RichHudFramework
{
    public static partial class Utils
    {
        /// <summary>
        /// Simple stopwatch class in lieu of <see cref="System.Diagnostics.Stopwatch"/>.
        /// </summary>
        public class Stopwatch
        {
            /// <summary>
            /// The number of ticks that have elapsed since starting the stopwatch.
            /// </summary>
            public long ElapsedTicks { get { return Enabled ? (DateTime.Now.Ticks - startTime) : (stopTime - startTime); } }

            /// <summary>
            /// The number of milliseconds that have elapsed since starting the stopwatch.
            /// </summary>
            public long ElapsedMilliseconds { get { return ElapsedTicks / TimeSpan.TicksPerMillisecond; } }

            /// <summary>
            /// If true, then the stopwatch is running and hasn't been stopped.
            /// </summary>
            public bool Enabled { get; private set; }

            private long startTime, stopTime;

            public Stopwatch()
            {
                startTime = long.MaxValue;
                stopTime = long.MaxValue;
                Enabled = false;
            }

            /// <summary>
            /// Starts the stopwatch.
            /// </summary>
            public void Start()
            {
                Reset();
                Enabled = true;
            }

            /// <summary>
            /// Stops the stopwatch.
            /// </summary>
            public void Stop()
            {
                stopTime = DateTime.Now.Ticks;
                Enabled = false;
            }

            /// <summary>
            /// Resets the starting time to the current time.
            /// </summary>
            public void Reset()
            {
                startTime = DateTime.Now.Ticks;
            }
        }
    }
}
