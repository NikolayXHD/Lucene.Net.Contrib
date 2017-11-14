using Lucene.Net.Documents;

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

    using BytesRef = Lucene.Net.Util.BytesRef;
    using NumericDocValuesField = NumericDocValuesField;
    using RamUsageEstimator = Lucene.Net.Util.RamUsageEstimator;

    /// <summary>
    /// An in-place update to a <see cref="DocValues"/> field. </summary>
    public abstract class DocValuesUpdate
    {
        /* Rough logic: OBJ_HEADER + 3*PTR + INT
         * Term: OBJ_HEADER + 2*PTR
         *   Term.field: 2*OBJ_HEADER + 4*INT + PTR + string.length*CHAR
         *   Term.bytes: 2*OBJ_HEADER + 2*INT + PTR + bytes.length
         * String: 2*OBJ_HEADER + 4*INT + PTR + string.length*CHAR
         * T: OBJ_HEADER
         */
        private static readonly int RAW_SIZE_IN_BYTES = 8 * RamUsageEstimator.NUM_BYTES_OBJECT_HEADER + 8 * RamUsageEstimator.NUM_BYTES_OBJECT_REF + 8 * RamUsageEstimator.NUM_BYTES_INT32;

        internal readonly DocValuesFieldUpdatesType type;
        internal readonly Term term;
        internal readonly string field;
        internal readonly object value;
        internal int docIDUpto = -1; // unassigned until applied, and confusing that it's here, when it's just used in BufferedDeletes...

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type"> the <see cref="DocValuesFieldUpdatesType"/> </param>
        /// <param name="term"> the <see cref="Term"/> which determines the documents that will be updated </param>
        /// <param name="field"> the <see cref="NumericDocValuesField"/> to update </param>
        /// <param name="value"> the updated value </param>
        protected DocValuesUpdate(DocValuesFieldUpdatesType type, Term term, string field, object value)
        {
            this.type = type;
            this.term = term;
            this.field = field;
            this.value = value;
        }

        internal abstract long GetValueSizeInBytes();

        internal int GetSizeInBytes()
        {
            int sizeInBytes = RAW_SIZE_IN_BYTES;
            sizeInBytes += term.Field.Length * RamUsageEstimator.NUM_BYTES_CHAR;
            sizeInBytes += term.Bytes.Bytes.Length;
            sizeInBytes += field.Length * RamUsageEstimator.NUM_BYTES_CHAR;
            sizeInBytes += (int)GetValueSizeInBytes();
            return sizeInBytes;
        }

        public override string ToString()
        {
            return "term=" + term + ",field=" + field + ",value=" + value;
        }

        /// <summary>
        /// An in-place update to a binary <see cref="DocValues"/> field </summary>
        public sealed class BinaryDocValuesUpdate : DocValuesUpdate
        {
            /* Size of BytesRef: 2*INT + ARRAY_HEADER + PTR */
            private static readonly long RAW_VALUE_SIZE_IN_BYTES = RamUsageEstimator.NUM_BYTES_ARRAY_HEADER + 2 * RamUsageEstimator.NUM_BYTES_INT32 + RamUsageEstimator.NUM_BYTES_OBJECT_REF;

            internal static readonly BytesRef MISSING = new BytesRef();

            internal BinaryDocValuesUpdate(Term term, string field, BytesRef value)
                : base(DocValuesFieldUpdatesType.BINARY, term, field, value == null ? MISSING : value)
            {
            }

            internal override long GetValueSizeInBytes()
            {
                return RAW_VALUE_SIZE_IN_BYTES + ((BytesRef)value).Bytes.Length;
            }
        }

        /// <summary>
        /// An in-place update to a numeric <see cref="DocValues"/> field </summary>
        public sealed class NumericDocValuesUpdate : DocValuesUpdate // LUCENENET NOTE: Made public rather than internal because it is on a public API
        {
            internal static readonly long? MISSING = new long?(0);

            public NumericDocValuesUpdate(Term term, string field, long? value)
                : base(DocValuesFieldUpdatesType.NUMERIC, term, field, value == null ? MISSING : value)
            {
            }

            internal override long GetValueSizeInBytes()
            {
                return RamUsageEstimator.NUM_BYTES_INT64;
            }
        }
    }
}