﻿using System;
using System.Diagnostics;

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

    public static class Time
    {
        public const long MILLISECONDS_PER_NANOSECOND = 1000000;
        public const long TICKS_PER_NANOSECOND = 100;

        public static long NanoTime()
        {
            return DateTime.Now.Ticks * TICKS_PER_NANOSECOND;
            // LUCENENET TODO: Change to
            // return (Stopwatch.GetTimestamp() / Stopwatch.Frequency) * 1000000000;
            // for better accuracy that is not affected by the system clock
        }

        public static long CurrentTimeMilliseconds()
        {
            return (Stopwatch.GetTimestamp() / Stopwatch.Frequency) * 1000;
        }
    }
}