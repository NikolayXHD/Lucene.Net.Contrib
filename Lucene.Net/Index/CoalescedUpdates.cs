using Lucene.Net.Util;
using System;
using System.Collections;
using System.Collections.Generic;

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

    using BinaryDocValuesUpdate = Lucene.Net.Index.DocValuesUpdate.BinaryDocValuesUpdate;
    using BytesRef = Lucene.Net.Util.BytesRef;
    using NumericDocValuesUpdate = Lucene.Net.Index.DocValuesUpdate.NumericDocValuesUpdate;
    using Query = Lucene.Net.Search.Query;
    using QueryAndLimit = Lucene.Net.Index.BufferedUpdatesStream.QueryAndLimit;

    internal class CoalescedUpdates
    {
        internal readonly IDictionary<Query, int> queries = new Dictionary<Query, int>();
        internal readonly IList<IEnumerable<Term>> iterables = new List<IEnumerable<Term>>();
        internal readonly IList<NumericDocValuesUpdate> numericDVUpdates = new List<NumericDocValuesUpdate>();
        internal readonly IList<BinaryDocValuesUpdate> binaryDVUpdates = new List<BinaryDocValuesUpdate>();

        public override string ToString()
        {
            // note: we could add/collect more debugging information
            return "CoalescedUpdates(termSets=" + iterables.Count + ",queries=" + queries.Count + ",numericDVUpdates=" + numericDVUpdates.Count + ",binaryDVUpdates=" + binaryDVUpdates.Count + ")";
        }

        internal virtual void Update(FrozenBufferedUpdates @in)
        {
            iterables.Add(@in.GetTermsEnumerable());

            for (int queryIdx = 0; queryIdx < @in.queries.Length; queryIdx++)
            {
                Query query = @in.queries[queryIdx];
                queries[query] = BufferedUpdates.MAX_INT32;
            }

            foreach (NumericDocValuesUpdate nu in @in.numericDVUpdates)
            {
                NumericDocValuesUpdate clone = new NumericDocValuesUpdate(nu.term, nu.field, (long?)nu.value);
                clone.docIDUpto = int.MaxValue;
                numericDVUpdates.Add(clone);
            }

            foreach (BinaryDocValuesUpdate bu in @in.binaryDVUpdates)
            {
                BinaryDocValuesUpdate clone = new BinaryDocValuesUpdate(bu.term, bu.field, (BytesRef)bu.value);
                clone.docIDUpto = int.MaxValue;
                binaryDVUpdates.Add(clone);
            }
        }

        public virtual IEnumerable<Term> TermsIterable()
        {
            return new IterableAnonymousInnerClassHelper(this);
        }

        private class IterableAnonymousInnerClassHelper : IEnumerable<Term>
        {
            private readonly CoalescedUpdates outerInstance;

            public IterableAnonymousInnerClassHelper(CoalescedUpdates outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public virtual IEnumerator<Term> GetEnumerator()
            {
                IEnumerator<Term>[] subs = new IEnumerator<Term>[outerInstance.iterables.Count];
                for (int i = 0; i < outerInstance.iterables.Count; i++)
                {
                    subs[i] = outerInstance.iterables[i].GetEnumerator();
                }
                return new MergedIterator<Term>(subs);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public virtual IEnumerable<QueryAndLimit> QueriesIterable()
        {
            return new IterableAnonymousInnerClassHelper2(this);
        }

        private class IterableAnonymousInnerClassHelper2 : IEnumerable<QueryAndLimit>
        {
            private readonly CoalescedUpdates outerInstance;

            public IterableAnonymousInnerClassHelper2(CoalescedUpdates outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public virtual IEnumerator<QueryAndLimit> GetEnumerator()
            {
                return new IteratorAnonymousInnerClassHelper(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private class IteratorAnonymousInnerClassHelper : IEnumerator<QueryAndLimit>
            {
                private readonly IterableAnonymousInnerClassHelper2 outerInstance;
                private readonly IEnumerator<KeyValuePair<Query, int>> iter;
                private QueryAndLimit current;

                public IteratorAnonymousInnerClassHelper(IterableAnonymousInnerClassHelper2 outerInstance)
                {
                    this.outerInstance = outerInstance;
                    iter = this.outerInstance.outerInstance.queries.GetEnumerator();
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    if (!iter.MoveNext())
                    {
                        return false;
                    }
                    KeyValuePair<Query, int> ent = iter.Current;
                    current = new QueryAndLimit(ent.Key, ent.Value);
                    return true;
                }

                public void Reset()
                {
                    throw new NotSupportedException();
                }

                public QueryAndLimit Current
                {
                    get { return current; }
                }

                object IEnumerator.Current
                {
                    get { return Current; }
                }
            }
        }
    }
}