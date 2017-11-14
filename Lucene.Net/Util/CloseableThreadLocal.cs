using Lucene.Net.Support;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Lucene.Net.Util
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

    /// <summary>
    /// Java's builtin ThreadLocal has a serious flaw:
    /// it can take an arbitrarily long amount of time to
    /// dereference the things you had stored in it, even once the
    /// ThreadLocal instance itself is no longer referenced.
    /// This is because there is single, master map stored for
    /// each thread, which all ThreadLocals share, and that
    /// master map only periodically purges "stale" entries.
    /// <para/>
    /// While not technically a memory leak, because eventually
    /// the memory will be reclaimed, it can take a long time
    /// and you can easily hit <see cref="OutOfMemoryException"/> because from the
    /// GC's standpoint the stale entries are not reclaimable.
    /// <para/>
    /// This class works around that, by only enrolling
    /// WeakReference values into the ThreadLocal, and
    /// separately holding a hard reference to each stored
    /// value.  When you call <see cref="Dispose()"/>, these hard
    /// references are cleared and then GC is freely able to
    /// reclaim space by objects stored in it.
    /// <para/>
    /// You should not call <see cref="Dispose()"/> until all
    /// threads are done using the instance.
    /// <para/>
    /// @lucene.internal
    /// </summary>
    public class DisposableThreadLocal<T> : IDisposable
    {
        private ThreadLocal<WeakReference> t = new ThreadLocal<WeakReference>();

        // Use a WeakHashMap so that if a Thread exits and is
        // GC'able, its entry may be removed:
        private IDictionary<Thread, T> hardRefs = new HashMap<Thread, T>();

        // Increase this to decrease frequency of purging in get:
        private static int PURGE_MULTIPLIER = 20;

        // On each get or set we decrement this; when it hits 0 we
        // purge.  After purge, we set this to
        // PURGE_MULTIPLIER * stillAliveCount.  this keeps
        // amortized cost of purging linear.
        private readonly AtomicInt32 countUntilPurge = new AtomicInt32(PURGE_MULTIPLIER);

        protected internal virtual T InitialValue() // LUCENENET NOTE: Sometimes returns new instance - not a good candidate for a property
        {
            return default(T);
        }

        public virtual T Get()
        {
            WeakReference weakRef = t.Value;
            if (weakRef == null)
            {
                T iv = InitialValue();
                if (iv != null)
                {
                    Set(iv);
                    return iv;
                }
                else
                {
                    return default(T);
                }
            }
            else
            {
                MaybePurge();
                return (T)weakRef.Target;
            }
        }

        public virtual void Set(T @object)
        {
            t.Value = new WeakReference(@object);

            lock (hardRefs)
            {
                hardRefs[Thread.CurrentThread] = @object;
                MaybePurge();
            }
        }

        private void MaybePurge()
        {
            if (countUntilPurge.GetAndDecrement() == 0)
            {
                Purge();
            }
        }

        // Purge dead threads
        private void Purge()
        {
            lock (hardRefs)
            {
                int stillAliveCount = 0;
                //Placing in try-finally to ensure HardRef threads are removed in the case of an exception
                List<Thread> Removed = new List<Thread>();
                try
                {
                    for (IEnumerator<Thread> it = hardRefs.Keys.GetEnumerator(); it.MoveNext(); )
                    {
                        Thread t = it.Current;
                        if (!t.IsAlive)
                        {
                            Removed.Add(t);
                        }
                        else
                        {
                            stillAliveCount++;
                        }
                    }
                }
                finally
                {
                    foreach (Thread thd in Removed)
                    {
                        hardRefs.Remove(thd);
                    }
                }

                int nextCount = (1 + stillAliveCount) * PURGE_MULTIPLIER;
                if (nextCount <= 0)
                {
                    // defensive: int overflow!
                    nextCount = 1000000;
                }

                countUntilPurge.Set(nextCount);
            }
        }

        public void Dispose()
        {
            // Clear the hard refs; then, the only remaining refs to
            // all values we were storing are weak (unless somewhere
            // else is still using them) and so GC may reclaim them:
            hardRefs = null;
            // Take care of the current thread right now; others will be
            // taken care of via the WeakReferences.
            if (t != null)
            {
                t.Dispose();
            }
            t = null;
        }
    }
}