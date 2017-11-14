using Lucene.Net.Documents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
    using Counter = Lucene.Net.Util.Counter;
    using DocValuesConsumer = Lucene.Net.Codecs.DocValuesConsumer;
    using DocValuesFormat = Lucene.Net.Codecs.DocValuesFormat;
    using IOUtils = Lucene.Net.Util.IOUtils;

    internal sealed class DocValuesProcessor : StoredFieldsConsumer
    {
        // TODO: somewhat wasteful we also keep a map here; would
        // be more efficient if we could "reuse" the map/hash
        // lookup DocFieldProcessor already did "above"
        private readonly IDictionary<string, DocValuesWriter> writers = new Dictionary<string, DocValuesWriter>();

        private readonly Counter bytesUsed;

        public DocValuesProcessor(Counter bytesUsed)
        {
            this.bytesUsed = bytesUsed;
        }

        public override void StartDocument()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal override void FinishDocument()
        {
        }

        public override void AddField(int docID, IIndexableField field, FieldInfo fieldInfo)
        {
            DocValuesType dvType = field.IndexableFieldType.DocValueType;
            if (dvType != DocValuesType.NONE)
            {
                fieldInfo.DocValuesType = dvType;
                if (dvType == DocValuesType.BINARY)
                {
                    AddBinaryField(fieldInfo, docID, field.GetBinaryValue());
                }
                else if (dvType == DocValuesType.SORTED)
                {
                    AddSortedField(fieldInfo, docID, field.GetBinaryValue());
                }
                else if (dvType == DocValuesType.SORTED_SET)
                {
                    AddSortedSetField(fieldInfo, docID, field.GetBinaryValue());
                }
                else if (dvType == DocValuesType.NUMERIC)
                {
                    if (field.NumericType != NumericFieldType.INT64)
                    {
                        throw new System.ArgumentException("illegal type " + field.NumericType + ": DocValues types must be " + NumericFieldType.INT64);
                    }
                    AddNumericField(fieldInfo, docID, field.GetInt64ValueOrDefault());
                }
                else
                {
                    Debug.Assert(false, "unrecognized DocValues.Type: " + dvType);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Flush(SegmentWriteState state)
        {
            if (writers.Count > 0)
            {
                DocValuesFormat fmt = state.SegmentInfo.Codec.DocValuesFormat;
                DocValuesConsumer dvConsumer = fmt.FieldsConsumer(state);
                bool success = false;
                try
                {
                    foreach (DocValuesWriter writer in writers.Values)
                    {
                        writer.Finish(state.SegmentInfo.DocCount);
                        writer.Flush(state, dvConsumer);
                    }
                    // TODO: catch missing DV fields here?  else we have
                    // null/"" depending on how docs landed in segments?
                    // but we can't detect all cases, and we should leave
                    // this behavior undefined. dv is not "schemaless": its column-stride.
                    writers.Clear();
                    success = true;
                }
                finally
                {
                    if (success)
                    {
                        IOUtils.Dispose(dvConsumer);
                    }
                    else
                    {
                        IOUtils.DisposeWhileHandlingException(dvConsumer);
                    }
                }
            }
        }

        internal void AddBinaryField(FieldInfo fieldInfo, int docID, BytesRef value)
        {
            DocValuesWriter writer;
            writers.TryGetValue(fieldInfo.Name, out writer);
            BinaryDocValuesWriter binaryWriter;
            if (writer == null)
            {
                binaryWriter = new BinaryDocValuesWriter(fieldInfo, bytesUsed);
                writers[fieldInfo.Name] = binaryWriter;
            }
            else if (!(writer is BinaryDocValuesWriter))
            {
                throw new System.ArgumentException("Incompatible DocValues type: field \"" + fieldInfo.Name + "\" changed from " + GetTypeDesc(writer) + " to binary");
            }
            else
            {
                binaryWriter = (BinaryDocValuesWriter)writer;
            }
            binaryWriter.AddValue(docID, value);
        }

        internal void AddSortedField(FieldInfo fieldInfo, int docID, BytesRef value)
        {
            DocValuesWriter writer;
            writers.TryGetValue(fieldInfo.Name, out writer);
            SortedDocValuesWriter sortedWriter;
            if (writer == null)
            {
                sortedWriter = new SortedDocValuesWriter(fieldInfo, bytesUsed);
                writers[fieldInfo.Name] = sortedWriter;
            }
            else if (!(writer is SortedDocValuesWriter))
            {
                throw new System.ArgumentException("Incompatible DocValues type: field \"" + fieldInfo.Name + "\" changed from " + GetTypeDesc(writer) + " to sorted");
            }
            else
            {
                sortedWriter = (SortedDocValuesWriter)writer;
            }
            sortedWriter.AddValue(docID, value);
        }

        internal void AddSortedSetField(FieldInfo fieldInfo, int docID, BytesRef value)
        {
            DocValuesWriter writer;
            writers.TryGetValue(fieldInfo.Name, out writer);
            SortedSetDocValuesWriter sortedSetWriter;
            if (writer == null)
            {
                sortedSetWriter = new SortedSetDocValuesWriter(fieldInfo, bytesUsed);
                writers[fieldInfo.Name] = sortedSetWriter;
            }
            else if (!(writer is SortedSetDocValuesWriter))
            {
                throw new System.ArgumentException("Incompatible DocValues type: field \"" + fieldInfo.Name + "\" changed from " + GetTypeDesc(writer) + " to sorted");
            }
            else
            {
                sortedSetWriter = (SortedSetDocValuesWriter)writer;
            }
            sortedSetWriter.AddValue(docID, value);
        }

        internal void AddNumericField(FieldInfo fieldInfo, int docID, long value)
        {
            DocValuesWriter writer;
            writers.TryGetValue(fieldInfo.Name, out writer);
            NumericDocValuesWriter numericWriter;
            if (writer == null)
            {
                numericWriter = new NumericDocValuesWriter(fieldInfo, bytesUsed, true);
                writers[fieldInfo.Name] = numericWriter;
            }
            else if (!(writer is NumericDocValuesWriter))
            {
                throw new System.ArgumentException("Incompatible DocValues type: field \"" + fieldInfo.Name + "\" changed from " + GetTypeDesc(writer) + " to numeric");
            }
            else
            {
                numericWriter = (NumericDocValuesWriter)writer;
            }
            numericWriter.AddValue(docID, value);
        }

        private string GetTypeDesc(DocValuesWriter obj)
        {
            if (obj is BinaryDocValuesWriter)
            {
                return "binary";
            }
            else if (obj is NumericDocValuesWriter)
            {
                return "numeric";
            }
            else
            {
                Debug.Assert(obj is SortedDocValuesWriter);
                return "sorted";
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Abort()
        {
            foreach (DocValuesWriter writer in writers.Values)
            {
                try
                {
                    writer.Abort();
                }
#pragma warning disable 168
                catch (Exception t)
#pragma warning restore 168
                {
                }
            }
            writers.Clear();
        }
    }
}