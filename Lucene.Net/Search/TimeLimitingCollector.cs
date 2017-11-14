using Lucene.Net.Support.Threading;
using System;
#if FEATURE_SERIALIZABLE_EXCEPTIONS
using System.Runtime.Serialization;
#endif
using System.Threading;

namespace Lucene.Net.Search
{
    /*
     * Licensed to the Apache Software Foundation (ASF) under one or more
     * contributor license agreements.  See the NOTICE file distributed with
     * this work for additional information regarding copyright ownership.
     * The ASF licenses this file to You under the Apache License, Version 2.0
     * (the "License"); you may not use this file except in compliance with
     * the License.  You may obtain a copy of the License at
     *
     *     http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */

    using AtomicReaderContext = Lucene.Net.Index.AtomicReaderContext;
    using Counter = Lucene.Net.Util.Counter;

    /// <summary>
    /// The <see cref="TimeLimitingCollector"/> is used to timeout search requests that
    /// take longer than the maximum allowed search time limit. After this time is
    /// exceeded, the search thread is stopped by throwing a
    /// <see cref="TimeExceededException"/>.
    /// </summary>
    public class TimeLimitingCollector : ICollector
    {
        /// <summary>
        /// Thrown when elapsed search time exceeds allowed search time. </summary>
        // LUCENENET: It is no longer good practice to use binary serialization. 
        // See: https://github.com/dotnet/corefx/issues/23584#issuecomment-325724568
#if FEATURE_SERIALIZABLE_EXCEPTIONS
        [Serializable]
#endif
        public class TimeExceededException : Exception
        {
            private long timeAllowed;
            private long timeElapsed;
            private int lastDocCollected;

            internal TimeExceededException(long timeAllowed, long timeElapsed, int lastDocCollected)
                : base("Elapsed time: " + timeElapsed + "Exceeded allowed search time: " + timeAllowed + " ms.")
            {
                this.timeAllowed = timeAllowed;
                this.timeElapsed = timeElapsed;
                this.lastDocCollected = lastDocCollected;
            }

            // For testing purposes
            internal TimeExceededException(string message)
                : base(message)
            {
            }

#if FEATURE_SERIALIZABLE_EXCEPTIONS
            /// <summary>
            /// Initializes a new instance of this class with serialized data.
            /// </summary>
            /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
            /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
            public TimeExceededException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
#endif

            /// <summary>
            /// Returns allowed time (milliseconds). </summary>
            public virtual long TimeAllowed
            {
                get
                {
                    return timeAllowed;
                }
            }

            /// <summary>
            /// Returns elapsed time (milliseconds). </summary>
            public virtual long TimeElapsed
            {
                get
                {
                    return timeElapsed;
                }
            }

            /// <summary>
            /// Returns last doc (absolute doc id) that was collected when the search time exceeded. </summary>
            public virtual int LastDocCollected
            {
                get
                {
                    return lastDocCollected;
                }
            }
        }

        private long t0 = long.MinValue;
        private long timeout = long.MinValue;
        private ICollector collector;
        private readonly Counter clock;
        private readonly long ticksAllowed;
        private bool greedy = false;
        private int docBase;

        /// <summary>
        /// Create a <see cref="TimeLimitingCollector"/> wrapper over another <see cref="ICollector"/> with a specified timeout. </summary>
        /// <param name="collector"> The wrapped <see cref="ICollector"/> </param>
        /// <param name="clock"> The timer clock </param>
        /// <param name="ticksAllowed"> Max time allowed for collecting
        /// hits after which <see cref="TimeExceededException"/> is thrown </param>
        public TimeLimitingCollector(ICollector collector, Counter clock, long ticksAllowed)
        {
            this.collector = collector;
            this.clock = clock;
            this.ticksAllowed = ticksAllowed;
        }

        /// <summary>
        /// Sets the baseline for this collector. By default the collectors baseline is
        /// initialized once the first reader is passed to the collector.
        /// To include operations executed in prior to the actual document collection
        /// set the baseline through this method in your prelude.
        /// <para>
        /// Example usage:
        /// <code>
        ///     // Counter is in the Lucene.Net.Util namespace
        ///     Counter clock = Counter.NewCounter(true);
        ///     long baseline = clock.Get();
        ///     // ... prepare search
        ///     TimeLimitingCollector collector = new TimeLimitingCollector(c, clock, numTicks);
        ///     collector.SetBaseline(baseline);
        ///     indexSearcher.Search(query, collector);
        /// </code>
        /// </para> 
        /// </summary>
        /// <seealso cref="SetBaseline()"/>
        public virtual void SetBaseline(long clockTime)
        {
            t0 = clockTime;
            timeout = t0 + ticksAllowed;
        }

        /// <summary>
        /// Syntactic sugar for <see cref="SetBaseline(long)"/> using <see cref="Counter.Get()"/>
        /// on the clock passed to the constructor.
        /// </summary>
        public virtual void SetBaseline()
        {
            SetBaseline(clock.Get());
        }

        /// <summary>
        /// Checks if this time limited collector is greedy in collecting the last hit.
        /// A non greedy collector, upon a timeout, would throw a <see cref="TimeExceededException"/>
        /// without allowing the wrapped collector to collect current doc. A greedy one would
        /// first allow the wrapped hit collector to collect current doc and only then
        /// throw a <see cref="TimeExceededException"/>. 
        /// </summary>
        public virtual bool IsGreedy
        {
            get
            {
                return greedy;
            }
            set
            {
                this.greedy = value;
            }
        }

        /// <summary>
        /// Calls <see cref="ICollector.Collect(int)"/> on the decorated <see cref="ICollector"/>
        /// unless the allowed time has passed, in which case it throws an exception.
        /// </summary>
        /// <exception cref="TimeExceededException">
        ///           If the time allowed has exceeded. </exception>
        public virtual void Collect(int doc)
        {
            long time = clock.Get();
            if (timeout < time)
            {
                if (greedy)
                {
                    //System.out.println(this+"  greedy: before failing, collecting doc: "+(docBase + doc)+"  "+(time-t0));
                    collector.Collect(doc);
                }
                //System.out.println(this+"  failing on:  "+(docBase + doc)+"  "+(time-t0));
                throw new TimeExceededException(timeout - t0, time - t0, docBase + doc);
            }
            //System.out.println(this+"  collecting: "+(docBase + doc)+"  "+(time-t0));
            collector.Collect(doc);
        }

        public virtual void SetNextReader(AtomicReaderContext context)
        {
            collector.SetNextReader(context);
            this.docBase = context.DocBase;
            if (long.MinValue == t0)
            {
                SetBaseline();
            }
        }
        
        public virtual void SetScorer(Scorer scorer)
        {
            collector.SetScorer(scorer);
        }

        public virtual bool AcceptsDocsOutOfOrder
        {
            get { return collector.AcceptsDocsOutOfOrder; }
        }

        /// <summary>
        /// This is so the same timer can be used with a multi-phase search process such as grouping.
        /// We don't want to create a new <see cref="TimeLimitingCollector"/> for each phase because that would
        /// reset the timer for each phase.  Once time is up subsequent phases need to timeout quickly.
        /// </summary>
        /// <param name="collector"> The actual collector performing search functionality. </param>
        public virtual void SetCollector(ICollector collector)
        {
            this.collector = collector;
        }

        /// <summary>
        /// Returns the global <see cref="TimerThread"/>'s <see cref="Counter"/>
        /// <para>
        /// Invoking this creates may create a new instance of <see cref="TimerThread"/> iff
        /// the global <see cref="TimerThread"/> has never been accessed before. The thread
        /// returned from this method is started on creation and will be alive unless
        /// you stop the <see cref="TimerThread"/> via <see cref="TimerThread.StopTimer()"/>.
        /// </para> 
        /// @lucene.experimental
        /// </summary>
        /// <returns> the global TimerThreads <seealso cref="Counter"/> </returns>
        public static Counter GlobalCounter
        {
            get
            {
                return TimerThreadHolder.THREAD.counter;
            }
        }

        /// <summary>
        /// Returns the global <see cref="TimerThread"/>.
        /// <para>
        /// Invoking this creates may create a new instance of <see cref="TimerThread"/> iff
        /// the global <see cref="TimerThread"/> has never been accessed before. The thread
        /// returned from this method is started on creation and will be alive unless
        /// you stop the <see cref="TimerThread"/> via <see cref="TimerThread.StopTimer()"/>.
        /// </para>
        /// @lucene.experimental
        /// </summary>
        /// <returns> the global <see cref="TimerThread"/> </returns>
        public static TimerThread GlobalTimerThread
        {
            get
            {
                return TimerThreadHolder.THREAD;
            }
        }

        private sealed class TimerThreadHolder
        {
            internal static readonly TimerThread THREAD;

            static TimerThreadHolder()
            {
                THREAD = new TimerThread(Counter.NewCounter(true));
                THREAD.Start();
            }
        }

        /// <summary>
        /// Thread used to timeout search requests.
        /// Can be stopped completely with <see cref="TimerThread.StopTimer()"/>
        /// <para/>
        /// @lucene.experimental
        /// </summary>
        public sealed class TimerThread : ThreadClass
        {
            public const string THREAD_NAME = "TimeLimitedCollector timer thread";
            public const int DEFAULT_RESOLUTION = 20;

            // NOTE: we can avoid explicit synchronization here for several reasons:
            // * updates to volatile long variables are atomic
            // * only single thread modifies this value
            // * use of volatile keyword ensures that it does not reside in
            //   a register, but in main memory (so that changes are visible to
            //   other threads).
            // * visibility of changes does not need to be instantaneous, we can
            //   afford losing a tick or two.
            //
            // See section 17 of the Java Language Specification for details.
            private long time = 0;

            private volatile bool stop = false;
            private long resolution;
            internal readonly Counter counter;

            public TimerThread(long resolution, Counter counter)
                : base(THREAD_NAME)
            {
                this.resolution = resolution;
                this.counter = counter;
                this.SetDaemon(true);
            }

            public TimerThread(Counter counter)
                : this(DEFAULT_RESOLUTION, counter)
            {
            }

            public override void Run()
            {
                while (!stop)
                {
                    // TODO: Use System.nanoTime() when Lucene moves to Java SE 5.
                    counter.AddAndGet(resolution);
//#if !NETSTANDARD1_6
//                    try
//                    {
//#endif
                        Thread.Sleep(TimeSpan.FromMilliseconds(Interlocked.Read(ref resolution)));
//#if !NETSTANDARD1_6 // LUCENENET NOTE: Senseless to catch and rethrow the same exception type
//                    }
//                    catch (ThreadInterruptedException ie)
//                    {
//                        throw new ThreadInterruptedException("Thread Interrupted Exception", ie);
//                    }
//#endif
                }
            }

            /// <summary>
            /// Get the timer value in milliseconds.
            /// </summary>
            public long Milliseconds
            {
                get
                {
                    return time;
                }
            }

            /// <summary>
            /// Stops the timer thread
            /// </summary>
            public void StopTimer()
            {
                stop = true;
            }

            /// <summary>
            /// Return the timer resolution. </summary>
            public long Resolution
            {
                get
                {
                    return resolution;
                }
                set
                {
                    this.resolution = Math.Max(value, 5); // 5 milliseconds is about the minimum reasonable time for a Object.wait(long) call.
                }
            }
        }
    }
}