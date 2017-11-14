using Lucene.Net.Support.Threading;
using System;
using System.Diagnostics;
using System.Threading;

namespace Lucene.Net.Index
{
    /*
     * Licensed to the Apache Software Foundation (ASF) under one or more
     * contributor license agreements. See the NOTICE file distributed with
     * this work for additional information regarding copyright ownership.
     * The ASF licenses this file to You under the Apache License, Version 2.0
     * (the "License"); you may not use this file except in compliance with
     * the License. You may obtain a copy of the License at
     *
     * http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */

    /// <summary>
    /// <see cref="DocumentsWriterPerThreadPool"/> controls <see cref="ThreadState"/> instances
    /// and their thread assignments during indexing. Each <see cref="ThreadState"/> holds
    /// a reference to a <see cref="DocumentsWriterPerThread"/> that is once a
    /// <see cref="ThreadState"/> is obtained from the pool exclusively used for indexing a
    /// single document by the obtaining thread. Each indexing thread must obtain
    /// such a <see cref="ThreadState"/> to make progress. Depending on the
    /// <see cref="DocumentsWriterPerThreadPool"/> implementation <see cref="ThreadState"/>
    /// assignments might differ from document to document.
    /// <para/>
    /// Once a <see cref="DocumentsWriterPerThread"/> is selected for flush the thread pool
    /// is reusing the flushing <see cref="DocumentsWriterPerThread"/>s <see cref="ThreadState"/> with a
    /// new <see cref="DocumentsWriterPerThread"/> instance.
    /// </summary>
    internal abstract class DocumentsWriterPerThreadPool
#if FEATURE_CLONEABLE
        : System.ICloneable
#endif
    {
        /// <summary>
        /// <see cref="ThreadState"/> references and guards a
        /// <see cref="Index.DocumentsWriterPerThread"/> instance that is used during indexing to
        /// build a in-memory index segment. <see cref="ThreadState"/> also holds all flush
        /// related per-thread data controlled by <see cref="DocumentsWriterFlushControl"/>.
        /// <para/>
        /// A <see cref="ThreadState"/>, its methods and members should only accessed by one
        /// thread a time. Users must acquire the lock via <see cref="ReentrantLock.Lock()"/>
        /// and release the lock in a finally block via <see cref="ReentrantLock.Unlock()"/>
        /// (on the <see cref="ThreadState"/> instance) before accessing the state.
        /// </summary>
        internal sealed class ThreadState : ReentrantLock
        {
            internal DocumentsWriterPerThread dwpt;

            // TODO this should really be part of DocumentsWriterFlushControl
            // write access guarded by DocumentsWriterFlushControl
            internal volatile bool flushPending = false;

            // TODO this should really be part of DocumentsWriterFlushControl
            // write access guarded by DocumentsWriterFlushControl
            internal long bytesUsed = 0;

            // guarded by Reentrant lock
            internal bool isActive = true;

            internal ThreadState(DocumentsWriterPerThread dpwt)
            {
                this.dwpt = dpwt;
            }

            /// <summary>
            /// Resets the internal <see cref="DocumentsWriterPerThread"/> with the given one.
            /// if the given DWPT is <c>null</c> this <see cref="ThreadState"/> is marked as inactive and should not be used
            /// for indexing anymore. </summary>
            /// <seealso cref="IsActive"/>
            internal void Deactivate() // LUCENENET NOTE: Made internal because it is called outside of this context
            {
                //Debug.Assert(this.HeldByCurrentThread);
                isActive = false;
                Reset();
            }

            internal void Reset() // LUCENENET NOTE: Made internal because it is called outside of this context
            {
                //Debug.Assert(this.HeldByCurrentThread);
                this.dwpt = null;
                this.bytesUsed = 0;
                this.flushPending = false;
            }

            /// <summary>
            /// Returns <c>true</c> if this <see cref="ThreadState"/> is still open. This will
            /// only return <c>false</c> iff the DW has been disposed and this
            /// <see cref="ThreadState"/> is already checked out for flush.
            /// </summary>
            internal bool IsActive
            {
                get
                {
                    //Debug.Assert(this.HeldByCurrentThread);
                    return isActive;
                }
            }

            internal bool IsInitialized
            {
                get
                {
                    //Debug.Assert(this.HeldByCurrentThread);
                    return IsActive && dwpt != null;
                }
            }

            /// <summary>
            /// Returns the number of currently active bytes in this ThreadState's
            /// <see cref="DocumentsWriterPerThread"/>
            /// </summary>
            public long BytesUsedPerThread
            {
                get
                {
                    //Debug.Assert(this.HeldByCurrentThread);
                    // public for FlushPolicy
                    return bytesUsed;
                }
            }

            /// <summary>
            /// Returns this <see cref="ThreadState"/>s <see cref="DocumentsWriterPerThread"/>
            /// </summary>
            public DocumentsWriterPerThread DocumentsWriterPerThread
            {
                get
                {
                    //Debug.Assert(this.HeldByCurrentThread);
                    // public for FlushPolicy
                    return dwpt;
                }
            }

            /// <summary>
            /// Returns <c>true</c> iff this <see cref="ThreadState"/> is marked as flush
            /// pending otherwise <c>false</c>
            /// </summary>
            public bool IsFlushPending
            {
                get
                {
                    return flushPending;
                }
            }
        }

        private ThreadState[] threadStates;
        private volatile int numThreadStatesActive;

        /// <summary>
        /// Creates a new <see cref="DocumentsWriterPerThreadPool"/> with a given maximum of <see cref="ThreadState"/>s.
        /// </summary>
        internal DocumentsWriterPerThreadPool(int maxNumThreadStates)
        {
            if (maxNumThreadStates < 1)
            {
                throw new System.ArgumentException("maxNumThreadStates must be >= 1 but was: " + maxNumThreadStates);
            }
            threadStates = new ThreadState[maxNumThreadStates];
            numThreadStatesActive = 0;
            for (int i = 0; i < threadStates.Length; i++)
            {
                threadStates[i] = new ThreadState(null);
            }
        }

        public virtual object Clone()
        {
            // We should only be cloned before being used:
            if (numThreadStatesActive != 0)
            {
                throw new InvalidOperationException("clone this object before it is used!");
            }

            DocumentsWriterPerThreadPool clone;

            clone = (DocumentsWriterPerThreadPool)base.MemberwiseClone();

            clone.threadStates = new ThreadState[threadStates.Length];
            for (int i = 0; i < threadStates.Length; i++)
            {
                clone.threadStates[i] = new ThreadState(null);
            }
            return clone;
        }

        /// <summary>
        /// Returns the max number of <see cref="ThreadState"/> instances available in this
        /// <see cref="DocumentsWriterPerThreadPool"/>
        /// </summary>
        public virtual int MaxThreadStates
        {
            get
            {
                return threadStates.Length;
            }
        }

        /// <summary>
        /// Returns the active number of <see cref="ThreadState"/> instances.
        /// </summary>
        public virtual int NumThreadStatesActive // LUCENENET NOTE: Changed from getActiveThreadState() because the name wasn't clear
        {
            get
            {
                return numThreadStatesActive;
            }
        }

        /// <summary>
        /// Returns a new <see cref="ThreadState"/> iff any new state is available otherwise
        /// <c>null</c>.
        /// <para/>
        /// NOTE: the returned <see cref="ThreadState"/> is already locked iff non-<c>null</c>.
        /// </summary>
        /// <returns> a new <see cref="ThreadState"/> iff any new state is available otherwise
        ///         <c>null</c> </returns>
        public virtual ThreadState NewThreadState()
        {
            lock (this)
            {
                if (numThreadStatesActive < threadStates.Length)
                {
                    ThreadState threadState = threadStates[numThreadStatesActive];
                    threadState.@Lock(); // lock so nobody else will get this ThreadState
                    bool unlock = true;
                    try
                    {
                        if (threadState.IsActive)
                        {
                            // unreleased thread states are deactivated during DW#close()
                            numThreadStatesActive++; // increment will publish the ThreadState
                            Debug.Assert(threadState.dwpt == null);
                            unlock = false;
                            return threadState;
                        }
                        // unlock since the threadstate is not active anymore - we are closed!
                        Debug.Assert(AssertUnreleasedThreadStatesInactive());
                        return null;
                    }
                    finally
                    {
                        if (unlock)
                        {
                            // in any case make sure we unlock if we fail
                            threadState.Unlock();
                        }
                    }
                }
                return null;
            }
        }

        private bool AssertUnreleasedThreadStatesInactive()
        {
            lock (this)
            {
                for (int i = numThreadStatesActive; i < threadStates.Length; i++)
                {
                    Debug.Assert(threadStates[i].TryLock(), "unreleased threadstate should not be locked");
                    try
                    {
                        Debug.Assert(!threadStates[i].IsInitialized, "expected unreleased thread state to be inactive");
                    }
                    finally
                    {
                        threadStates[i].Unlock();
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Deactivate all unreleased threadstates
        /// </summary>
        internal virtual void DeactivateUnreleasedStates()
        {
            lock (this)
            {
                for (int i = numThreadStatesActive; i < threadStates.Length; i++)
                {
                    ThreadState threadState = threadStates[i];
                    threadState.@Lock();
                    try
                    {
                        threadState.Deactivate();
                    }
                    finally
                    {
                        threadState.Unlock();
                    }
                }
            }
        }

        internal virtual DocumentsWriterPerThread Reset(ThreadState threadState, bool closed)
        {
            //Debug.Assert(threadState.HeldByCurrentThread);
            DocumentsWriterPerThread dwpt = threadState.dwpt;
            if (!closed)
            {
                threadState.Reset();
            }
            else
            {
                threadState.Deactivate();
            }
            return dwpt;
        }

        internal virtual void Recycle(DocumentsWriterPerThread dwpt)
        {
            // don't recycle DWPT by default
        }

        // you cannot subclass this without being in o.a.l.index package anyway, so
        // the class is already pkg-private... fix me: see LUCENE-4013
        public abstract ThreadState GetAndLock(Thread requestingThread, DocumentsWriter documentsWriter); // LUCENENET NOTE: Made public rather than internal

        /// <summary>
        /// Returns the <i>i</i>th active <seealso cref="ThreadState"/> where <i>i</i> is the
        /// given ord.
        /// </summary>
        /// <param name="ord">
        ///          the ordinal of the <seealso cref="ThreadState"/> </param>
        /// <returns> the <i>i</i>th active <seealso cref="ThreadState"/> where <i>i</i> is the
        ///         given ord. </returns>
        internal virtual ThreadState GetThreadState(int ord)
        {
            return threadStates[ord];
        }

        /// <summary>
        /// Returns the <see cref="ThreadState"/> with the minimum estimated number of threads
        /// waiting to acquire its lock or <c>null</c> if no <see cref="ThreadState"/>
        /// is yet visible to the calling thread.
        /// </summary>
        internal virtual ThreadState MinContendedThreadState()
        {
            ThreadState minThreadState = null;
            int limit = numThreadStatesActive;
            for (int i = 0; i < limit; i++)
            {
                ThreadState state = threadStates[i];
                if (minThreadState == null || state.QueueLength < minThreadState.QueueLength)
                {
                    minThreadState = state;
                }
            }
            return minThreadState;
        }

        /// <summary>
        /// Returns the number of currently deactivated <see cref="ThreadState"/> instances.
        /// A deactivated <see cref="ThreadState"/> should not be used for indexing anymore.
        /// </summary>
        /// <returns> the number of currently deactivated <see cref="ThreadState"/> instances. </returns>
        internal virtual int NumDeactivatedThreadStates()
        {
            int count = 0;
            for (int i = 0; i < threadStates.Length; i++)
            {
                ThreadState threadState = threadStates[i];
                threadState.@Lock();
                try
                {
                    if (!threadState.isActive)
                    {
                        count++;
                    }
                }
                finally
                {
                    threadState.Unlock();
                }
            }
            return count;
        }

        /// <summary>
        /// Deactivates an active <see cref="ThreadState"/>. Inactive <see cref="ThreadState"/> can
        /// not be used for indexing anymore once they are deactivated. This method should only be used
        /// if the parent <see cref="DocumentsWriter"/> is closed or aborted.
        /// </summary>
        /// <param name="threadState"> the state to deactivate </param>
        internal virtual void DeactivateThreadState(ThreadState threadState)
        {
            Debug.Assert(threadState.IsActive);
            threadState.Deactivate();
        }
    }
}