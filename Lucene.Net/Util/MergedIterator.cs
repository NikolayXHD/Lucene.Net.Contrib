using Lucene.Net.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
    /// Provides a merged sorted view from several sorted iterators.
    /// <para/>
    /// If built with <see cref="removeDuplicates"/> set to <c>true</c> and an element
    /// appears in multiple iterators then it is deduplicated, that is this iterator
    /// returns the sorted union of elements.
    /// <para/>
    /// If built with <see cref="removeDuplicates"/> set to <c>false</c> then all elements
    /// in all iterators are returned.
    /// <para/>
    /// Caveats:
    /// <list type="bullet">
    ///   <item><description>The behavior is undefined if the iterators are not actually sorted.</description></item>
    ///   <item><description>Null elements are unsupported.</description></item>
    ///   <item><description>If <see cref="removeDuplicates"/> is set to <c>true</c> and if a single iterator contains
    ///       duplicates then they will not be deduplicated.</description></item>
    ///   <item><description>When elements are deduplicated it is not defined which one is returned.</description></item>
    ///   <item><description>If <see cref="removeDuplicates"/> is set to <c>false</c> then the order in which duplicates
    ///       are returned isn't defined.</description></item>
    /// </list>
    /// <para/>
    /// @lucene.internal
    /// </summary>
    public sealed class MergedIterator<T> : IEnumerator<T>
        where T : IComparable<T>
    {
        private readonly TermMergeQueue<T> queue;
        private readonly SubIterator<T>[] top;
        private readonly bool removeDuplicates;
        private int numTop;
        private T current;

        public MergedIterator(params IEnumerator<T>[] iterators)
            : this(true, iterators)
        {
        }

        public MergedIterator(bool removeDuplicates, params IEnumerator<T>[] iterators)
        {
            this.removeDuplicates = removeDuplicates;
            queue = new TermMergeQueue<T>(iterators.Length);
            top = new SubIterator<T>[iterators.Length];
            int index = 0;
            foreach (IEnumerator<T> iter in iterators)
            {
                // If hasNext
                if (iter.MoveNext())
                {
                    SubIterator<T> sub = new SubIterator<T>();
                    sub.Current = iter.Current;
                    sub.Iterator = iter;
                    sub.Index = index++;
                    queue.Add(sub);
                }
            }
        }

        public bool MoveNext()
        {
            PushTop();

            if (queue.Count > 0)
            {
                PullTop();
            }
            else
            {
                return false;
            }

            return true;
        }

        public T Current
        {
            get
            {
                return current;
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
        }

        private void PullTop()
        {
            Debug.Assert(numTop == 0);
            top[numTop++] = queue.Pop();
            if (removeDuplicates)
            {
                //extract all subs from the queue that have the same top element
                while (queue.Count != 0 && queue.Top.Current.Equals(top[0].Current))
                {
                    top[numTop++] = queue.Pop();
                }
            }
            current = top[0].Current;
        }

        private void PushTop()
        {
            for (int i = 0; i < numTop; ++i)
            {
                if (top[i].Iterator.MoveNext())
                {
                    top[i].Current = top[i].Iterator.Current;
                    queue.Add(top[i]);
                }
                else
                {
                    top[i].Current = default(T);
                }
            }
            numTop = 0;
        }

        private class SubIterator<I>
            where I : IComparable<I>
        {
            internal IEnumerator<I> Iterator { get; set; }
            internal I Current { get; set; }
            internal int Index { get; set; }
        }

        private class TermMergeQueue<C> : PriorityQueue<SubIterator<C>>
            where C : IComparable<C>
        {
            internal TermMergeQueue(int size)
                : base(size)
            {
            }

            protected internal override bool LessThan(SubIterator<C> a, SubIterator<C> b)
            {
                int cmp;
                // LUCNENENET specific: For strings, we need to ensure we compare them ordinal
                if (typeof(C).Equals(typeof(string)))
                {
                    cmp = (a.Current as string).CompareToOrdinal(b.Current as string);
                }
                else
                {
                    cmp = a.Current.CompareTo(b.Current);
                }
                if (cmp != 0)
                {
                    return cmp < 0;
                }
                else
                {
                    return a.Index < b.Index;
                }
            }
        }
    }
}