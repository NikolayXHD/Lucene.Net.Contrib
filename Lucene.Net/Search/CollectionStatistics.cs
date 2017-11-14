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

    /// <summary>
    /// Contains statistics for a collection (field)
    /// <para/>
    /// @lucene.experimental
    /// </summary>
    public class CollectionStatistics
    {
        private readonly string field;
        private readonly long maxDoc;
        private readonly long docCount;
        private readonly long sumTotalTermFreq;
        private readonly long sumDocFreq;

        /// <summary>
        /// Sole constructor.
        /// </summary>
        public CollectionStatistics(string field, long maxDoc, long docCount, long sumTotalTermFreq, long sumDocFreq)
        {
            Debug.Assert(maxDoc >= 0);
            Debug.Assert(docCount >= -1 && docCount <= maxDoc); // #docs with field must be <= #docs
            Debug.Assert(sumDocFreq == -1 || sumDocFreq >= docCount); // #postings must be >= #docs with field
            Debug.Assert(sumTotalTermFreq == -1 || sumTotalTermFreq >= sumDocFreq); // #positions must be >= #postings
            this.field = field;
            this.maxDoc = maxDoc;
            this.docCount = docCount;
            this.sumTotalTermFreq = sumTotalTermFreq;
            this.sumDocFreq = sumDocFreq;
        }

        /// <summary>
        /// Returns the field name </summary>
        public string Field
        {
            get { return field; }
        }

        /// <summary>
        /// Returns the total number of documents, regardless of
        /// whether they all contain values for this field. </summary>
        /// <seealso cref="Index.IndexReader.MaxDoc"/>
        public long MaxDoc
        {
            get { return maxDoc; }
        }

        /// <summary>
        /// Returns the total number of documents that
        /// have at least one term for this field. </summary>
        /// <seealso cref="Index.Terms.DocCount"/>
        public long DocCount
        {
            get { return docCount; }
        }

        /// <summary>
        /// Returns the total number of tokens for this field </summary>
        /// <seealso cref="Index.Terms.SumTotalTermFreq"/>
        public long SumTotalTermFreq
        {
            get { return sumTotalTermFreq; }
        }

        /// <summary>
        /// Returns the total number of postings for this field </summary>
        /// <seealso cref="Index.Terms.SumDocFreq"/>
        public long SumDocFreq
        {
            get { return sumDocFreq; }
        }
    }
}