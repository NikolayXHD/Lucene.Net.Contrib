using Lucene.Net.Support;
using System.Diagnostics;
using System.Text;

namespace Lucene.Net.Search.Spans
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

    using ToStringUtils = Lucene.Net.Util.ToStringUtils;

    /// <summary>
    /// Matches spans near the beginning of a field.
    /// <para/>
    /// This class is a simple extension of <see cref="SpanPositionRangeQuery"/> in that it assumes the
    /// start to be zero and only checks the end boundary.
    /// </summary>
    public class SpanFirstQuery : SpanPositionRangeQuery
    {
        /// <summary>
        /// Construct a <see cref="SpanFirstQuery"/> matching spans in <paramref name="match"/> whose end
        /// position is less than or equal to <paramref name="end"/>.
        /// </summary>
        public SpanFirstQuery(SpanQuery match, int end)
            : base(match, 0, end)
        {
        }

        protected override AcceptStatus AcceptPosition(Spans spans)
        {
            Debug.Assert(spans.Start != spans.End, "start equals end: " + spans.Start);
            if (spans.Start >= m_end)
            {
                return AcceptStatus.NO_AND_ADVANCE;
            }
            else if (spans.End <= m_end)
            {
                return AcceptStatus.YES;
            }
            else
            {
                return AcceptStatus.NO;
            }
        }

        public override string ToString(string field)
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("spanFirst(");
            buffer.Append(m_match.ToString(field));
            buffer.Append(", ");
            buffer.Append(m_end);
            buffer.Append(")");
            buffer.Append(ToStringUtils.Boost(Boost));
            return buffer.ToString();
        }

        public override object Clone()
        {
            SpanFirstQuery spanFirstQuery = new SpanFirstQuery((SpanQuery)m_match.Clone(), m_end);
            spanFirstQuery.Boost = Boost;
            return spanFirstQuery;
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (!(o is SpanFirstQuery))
            {
                return false;
            }

            SpanFirstQuery other = (SpanFirstQuery)o;
            return this.m_end == other.m_end && this.m_match.Equals(other.m_match) && this.Boost == other.Boost;
        }

        public override int GetHashCode()
        {
            int h = m_match.GetHashCode();
            h ^= (h << 8) | ((int)((uint)h >> 25)); // reversible
            h ^= Number.SingleToRawInt32Bits(Boost) ^ m_end;
            return h;
        }
    }
}