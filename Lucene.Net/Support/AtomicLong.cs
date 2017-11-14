﻿using System;
using System.Threading;

namespace Lucene.Net.Support
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
    /// NOTE: This was AtomicLong in the JDK
    /// </summary>
#if FEATURE_SERIALIZABLE
    [Serializable]
#endif
    public class AtomicInt64
    {
        private long value;

        public AtomicInt64()
            : this(0)
        {
        }

        public AtomicInt64(long value)
        {
            Interlocked.Exchange(ref this.value, value);
        }

        public long IncrementAndGet()
        {
            return Interlocked.Increment(ref value);
        }

        public long GetAndIncrement()
        {
            return Interlocked.Increment(ref value) - 1;
        }

        public long DecrementAndGet()
        {
            return Interlocked.Decrement(ref value);
        }

        public void Set(int value)
        {
            Interlocked.Exchange(ref this.value, value);
        }

        public long AddAndGet(long value)
        {
            return Interlocked.Add(ref this.value, value);
        }

        public long Get()
        {
            //LUCENE TO-DO read operations atomic in 64 bit
            return value;
        }

        public bool CompareAndSet(long expect, long update)
        {
            long rc = Interlocked.CompareExchange(ref value, update, expect);
            return rc == expect;
        }

        public override string ToString()
        {
            return Get().ToString();
        }
    }
}