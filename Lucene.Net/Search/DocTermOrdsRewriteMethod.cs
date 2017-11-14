using System.Collections.Generic;
using System.Diagnostics;

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
    using IBits = Lucene.Net.Util.IBits;
    using BytesRef = Lucene.Net.Util.BytesRef;
    using IndexReader = Lucene.Net.Index.IndexReader;
    using Int64BitSet = Lucene.Net.Util.Int64BitSet;
    using SortedSetDocValues = Lucene.Net.Index.SortedSetDocValues;
    using Terms = Lucene.Net.Index.Terms;
    using TermsEnum = Lucene.Net.Index.TermsEnum;

    /// <summary>
    /// Rewrites <see cref="MultiTermQuery"/>s into a filter, using DocTermOrds for term enumeration.
    /// <para>
    /// This can be used to perform these queries against an unindexed docvalues field.
    /// </para>
    /// @lucene.experimental
    /// </summary>
    public sealed class DocTermOrdsRewriteMethod : MultiTermQuery.RewriteMethod
    {
        public override Query Rewrite(IndexReader reader, MultiTermQuery query)
        {
            Query result = new ConstantScoreQuery(new MultiTermQueryDocTermOrdsWrapperFilter(query));
            result.Boost = query.Boost;
            return result;
        }

        internal class MultiTermQueryDocTermOrdsWrapperFilter : Filter
        {
            protected readonly MultiTermQuery m_query;

            /// <summary>
            /// Wrap a <see cref="MultiTermQuery"/> as a <see cref="Filter"/>.
            /// </summary>
            protected internal MultiTermQueryDocTermOrdsWrapperFilter(MultiTermQuery query)
            {
                this.m_query = query;
            }

            public override string ToString()
            {
                // query.toString should be ok for the filter, too, if the query boost is 1.0f
                return m_query.ToString();
            }

            public override sealed bool Equals(object o)
            {
                if (o == this)
                {
                    return true;
                }
                if (o == null)
                {
                    return false;
                }
                if (this.GetType().Equals(o.GetType()))
                {
                    return this.m_query.Equals(((MultiTermQueryDocTermOrdsWrapperFilter)o).m_query);
                }
                return false;
            }

            public override sealed int GetHashCode()
            {
                return m_query.GetHashCode();
            }

            /// <summary>
            /// Returns the field name for this query </summary>
            public string Field
            {
                get
                {
                    return m_query.Field;
                }
            }

            /// <summary>
            /// Returns a <see cref="DocIdSet"/> with documents that should be permitted in search
            /// results.
            /// </summary>
            public override DocIdSet GetDocIdSet(AtomicReaderContext context, IBits acceptDocs)
            {
                SortedSetDocValues docTermOrds = FieldCache.DEFAULT.GetDocTermOrds((context.AtomicReader), m_query.m_field);
                // Cannot use FixedBitSet because we require long index (ord):
                Int64BitSet termSet = new Int64BitSet(docTermOrds.ValueCount);
                TermsEnum termsEnum = m_query.GetTermsEnum(new TermsAnonymousInnerClassHelper(this, docTermOrds));

                Debug.Assert(termsEnum != null);
                if (termsEnum.Next() != null)
                {
                    // fill into a bitset
                    do
                    {
                        termSet.Set(termsEnum.Ord);
                    } while (termsEnum.Next() != null);
                }
                else
                {
                    return null;
                }

                return new FieldCacheDocIdSetAnonymousInnerClassHelper(this, context.Reader.MaxDoc, acceptDocs, docTermOrds, termSet);
            }

            private class TermsAnonymousInnerClassHelper : Terms
            {
                private readonly MultiTermQueryDocTermOrdsWrapperFilter outerInstance;

                private SortedSetDocValues docTermOrds;

                public TermsAnonymousInnerClassHelper(MultiTermQueryDocTermOrdsWrapperFilter outerInstance, SortedSetDocValues docTermOrds)
                {
                    this.outerInstance = outerInstance;
                    this.docTermOrds = docTermOrds;
                }

                public override IComparer<BytesRef> Comparer
                {
                    get
                    {
                        return BytesRef.UTF8SortedAsUnicodeComparer;
                    }
                }

                public override TermsEnum GetIterator(TermsEnum reuse)
                {
                    return docTermOrds.GetTermsEnum();
                }

                public override long SumTotalTermFreq
                {
                    get
                    {
                        return -1;
                    }
                }

                public override long SumDocFreq
                {
                    get
                    {
                        return -1;
                    }
                }

                public override int DocCount
                {
                    get
                    {
                        return -1;
                    }
                }

                public override long Count
                {
                    get { return -1; }
                }

                public override bool HasFreqs
                {
                    get { return false; }
                }

                public override bool HasOffsets
                {
                    get { return false; }
                }

                public override bool HasPositions
                {
                    get { return false; }
                }

                public override bool HasPayloads
                {
                    get { return false; }
                }
            }

            private class FieldCacheDocIdSetAnonymousInnerClassHelper : FieldCacheDocIdSet
            {
                private readonly MultiTermQueryDocTermOrdsWrapperFilter outerInstance;

                private SortedSetDocValues docTermOrds;
                private Int64BitSet termSet;

                public FieldCacheDocIdSetAnonymousInnerClassHelper(MultiTermQueryDocTermOrdsWrapperFilter outerInstance, int maxDoc, IBits acceptDocs, SortedSetDocValues docTermOrds, Int64BitSet termSet)
                    : base(maxDoc, acceptDocs)
                {
                    this.outerInstance = outerInstance;
                    this.docTermOrds = docTermOrds;
                    this.termSet = termSet;
                }

                protected internal override sealed bool MatchDoc(int doc)
                {
                    docTermOrds.SetDocument(doc);
                    long ord;
                    // TODO: we could track max bit set and early terminate (since they come in sorted order)
                    while ((ord = docTermOrds.NextOrd()) != SortedSetDocValues.NO_MORE_ORDS)
                    {
                        if (termSet.Get(ord))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (this.GetType() != obj.GetType())
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return 877;
        }
    }
}