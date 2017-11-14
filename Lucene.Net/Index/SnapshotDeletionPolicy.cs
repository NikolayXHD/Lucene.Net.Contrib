using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lucene.Net.Index
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

    using Directory = Lucene.Net.Store.Directory;

    /// <summary>
    /// An <see cref="IndexDeletionPolicy"/> that wraps any other
    /// <see cref="IndexDeletionPolicy"/> and adds the ability to hold and later release
    /// snapshots of an index. While a snapshot is held, the <see cref="IndexWriter"/> will
    /// not remove any files associated with it even if the index is otherwise being
    /// actively, arbitrarily changed. Because we wrap another arbitrary
    /// <see cref="IndexDeletionPolicy"/>, this gives you the freedom to continue using
    /// whatever <see cref="IndexDeletionPolicy"/> you would normally want to use with your
    /// index.
    ///
    /// <para/>
    /// This class maintains all snapshots in-memory, and so the information is not
    /// persisted and not protected against system failures. If persistence is
    /// important, you can use <see cref="PersistentSnapshotDeletionPolicy"/>.
    /// <para/>
    /// @lucene.experimental
    /// </summary>
    public class SnapshotDeletionPolicy : IndexDeletionPolicy
    {
        /// <summary>
        /// Records how many snapshots are held against each
        /// commit generation
        /// </summary>
        protected IDictionary<long, int> m_refCounts = new Dictionary<long, int>();

        /// <summary>
        /// Used to map gen to <see cref="IndexCommit"/>. </summary>
        protected IDictionary<long?, IndexCommit> m_indexCommits = new Dictionary<long?, IndexCommit>();

        /// <summary>
        /// Wrapped <see cref="IndexDeletionPolicy"/> </summary>
        private IndexDeletionPolicy primary;

        /// <summary>
        /// Most recently committed <see cref="IndexCommit"/>. </summary>
        protected IndexCommit m_lastCommit;

        /// <summary>
        /// Used to detect misuse </summary>
        private bool initCalled;

        /// <summary>
        /// Sole constructor, taking the incoming 
        /// <see cref="IndexDeletionPolicy"/> to wrap.
        /// </summary>
        public SnapshotDeletionPolicy(IndexDeletionPolicy primary)
        {
            this.primary = primary;
        }

        public override void OnCommit<T>(IList<T> commits)
        {
            lock (this)
            {
                primary.OnCommit(WrapCommits(commits));
                m_lastCommit = commits[commits.Count - 1];
            }
        }

        public override void OnInit<T>(IList<T> commits)
        {
            lock (this)
            {
                initCalled = true;
                primary.OnInit(WrapCommits(commits));
                foreach (IndexCommit commit in commits)
                {
                    if (m_refCounts.ContainsKey(commit.Generation))
                    {
                        m_indexCommits[commit.Generation] = commit;
                    }
                }
                if (commits.Count > 0)
                {
                    m_lastCommit = commits[commits.Count - 1];
                }
            }
        }

        /// <summary>
        /// Release a snapshotted commit.
        /// </summary>
        /// <param name="commit">
        ///          the commit previously returned by <see cref="Snapshot()"/> </param>
        public virtual void Release(IndexCommit commit)
        {
            lock (this)
            {
                long gen = commit.Generation;
                ReleaseGen(gen);
            }
        }

        /// <summary>
        /// Release a snapshot by generation. </summary>
        protected internal virtual void ReleaseGen(long gen)
        {
            if (!initCalled)
            {
                throw new InvalidOperationException("this instance is not being used by IndexWriter; be sure to use the instance returned from writer.getConfig().getIndexDeletionPolicy()");
            }
            int? refCount = m_refCounts[gen];
            if (refCount == null)
            {
                throw new System.ArgumentException("commit gen=" + gen + " is not currently snapshotted");
            }
            int refCountInt = (int)refCount;
            Debug.Assert(refCountInt > 0);
            refCountInt--;
            if (refCountInt == 0)
            {
                m_refCounts.Remove(gen);
                m_indexCommits.Remove(gen);
            }
            else
            {
                m_refCounts[gen] = refCountInt;
            }
        }

        /// <summary>
        /// Increments the refCount for this <see cref="IndexCommit"/>. </summary>
        protected internal virtual void IncRef(IndexCommit ic)
        {
            lock (this)
            {
                long gen = ic.Generation;
                int refCount;
                int refCountInt;
                if (!m_refCounts.TryGetValue(gen, out refCount))
                {
                    m_indexCommits[gen] = m_lastCommit;
                    refCountInt = 0;
                }
                else
                {
                    refCountInt = (int)refCount;
                }
                m_refCounts[gen] = refCountInt + 1;
            }
        }

        /// <summary>
        /// Snapshots the last commit and returns it. Once a commit is 'snapshotted,' it is protected
        /// from deletion (as long as this <see cref="IndexDeletionPolicy"/> is used). The
        /// snapshot can be removed by calling <see cref="Release(IndexCommit)"/> followed
        /// by a call to <see cref="IndexWriter.DeleteUnusedFiles()"/>.
        ///
        /// <para/>
        /// <b>NOTE:</b> while the snapshot is held, the files it references will not
        /// be deleted, which will consume additional disk space in your index. If you
        /// take a snapshot at a particularly bad time (say just before you call
        /// <see cref="IndexWriter.ForceMerge(int)"/>) then in the worst case this could consume an extra 1X of your
        /// total index size, until you release the snapshot.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///           if this index does not have any commits yet </exception>
        /// <returns> the <see cref="IndexCommit"/> that was snapshotted. </returns>
        public virtual IndexCommit Snapshot()
        {
            lock (this)
            {
                if (!initCalled)
                {
                    throw new InvalidOperationException("this instance is not being used by IndexWriter; be sure to use the instance returned from writer.getConfig().getIndexDeletionPolicy()");
                }
                if (m_lastCommit == null)
                {
                    // No commit yet, eg this is a new IndexWriter:
                    throw new InvalidOperationException("No index commit to snapshot");
                }

                IncRef(m_lastCommit);

                return m_lastCommit;
            }
        }

        /// <summary>
        /// Returns all <see cref="IndexCommit"/>s held by at least one snapshot. </summary>
        public virtual IList<IndexCommit> GetSnapshots()
        {
            lock (this)
            {
                return new List<IndexCommit>(m_indexCommits.Values);
            }
        }

        /// <summary>
        /// Returns the total number of snapshots currently held. </summary>
        public virtual int SnapshotCount
        {
            get
            {
                lock (this)
                {
                    int total = 0;
                    foreach (var refCount in m_refCounts.Values)
                    {
                        total += refCount;
                    }

                    return total;
                }
            }
        }

        /// <summary>
        /// Retrieve an <see cref="IndexCommit"/> from its generation;
        /// returns <c>null</c> if this <see cref="IndexCommit"/> is not currently
        /// snapshotted
        /// </summary>
        public virtual IndexCommit GetIndexCommit(long gen)
        {
            lock (this)
            {
                return m_indexCommits[gen];
            }
        }

        public override object Clone()
        {
            lock (this)
            {
                SnapshotDeletionPolicy other = (SnapshotDeletionPolicy)base.Clone();
                other.primary = (IndexDeletionPolicy)this.primary.Clone();
                other.m_lastCommit = null;
                other.m_refCounts = new Dictionary<long, int>(m_refCounts);
                other.m_indexCommits = new Dictionary<long?, IndexCommit>(m_indexCommits);
                return other;
            }
        }

        /// <summary>
        /// Wraps each <see cref="IndexCommit"/> as a 
        /// <see cref="SnapshotCommitPoint"/>.
        /// </summary>
        private IList<IndexCommit> WrapCommits<T>(IList<T> commits)
            where T : IndexCommit
        {
            IList<IndexCommit> wrappedCommits = new List<IndexCommit>(commits.Count);
            foreach (IndexCommit ic in commits)
            {
                wrappedCommits.Add(new SnapshotCommitPoint(this, ic));
            }
            return wrappedCommits;
        }

        /// <summary>
        /// Wraps a provided <see cref="IndexCommit"/> and prevents it
        /// from being deleted.
        /// </summary>
        private class SnapshotCommitPoint : IndexCommit
        {
            private readonly SnapshotDeletionPolicy outerInstance;

            /// <summary>
            /// The <see cref="IndexCommit"/> we are preventing from deletion. </summary>
            protected IndexCommit m_cp;

            /// <summary>
            /// Creates a <see cref="SnapshotCommitPoint"/> wrapping the provided
            /// <see cref="IndexCommit"/>.
            /// </summary>
            protected internal SnapshotCommitPoint(SnapshotDeletionPolicy outerInstance, IndexCommit cp)
            {
                this.outerInstance = outerInstance;
                this.m_cp = cp;
            }

            public override string ToString()
            {
                return "SnapshotDeletionPolicy.SnapshotCommitPoint(" + m_cp + ")";
            }

            public override void Delete()
            {
                lock (outerInstance)
                {
                    // Suppress the delete request if this commit point is
                    // currently snapshotted.
                    if (!outerInstance.m_refCounts.ContainsKey(m_cp.Generation))
                    {
                        m_cp.Delete();
                    }
                }
            }

            public override Directory Directory
            {
                get
                {
                    return m_cp.Directory;
                }
            }

            public override ICollection<string> FileNames
            {
                get
                {
                    return m_cp.FileNames;
                }
            }

            public override long Generation
            {
                get
                {
                    return m_cp.Generation;
                }
            }

            public override string SegmentsFileName
            {
                get
                {
                    return m_cp.SegmentsFileName;
                }
            }

            public override IDictionary<string, string> UserData
            {
                get
                {
                    return m_cp.UserData;
                }
            }

            public override bool IsDeleted
            {
                get
                {
                    return m_cp.IsDeleted;
                }
            }

            public override int SegmentCount
            {
                get
                {
                    return m_cp.SegmentCount;
                }
            }
        }
    }
}