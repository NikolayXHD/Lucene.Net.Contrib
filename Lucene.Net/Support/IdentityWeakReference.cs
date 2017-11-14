﻿using System;
using System.Runtime.CompilerServices;

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

    public class IdentityWeakReference<T> : WeakReference
        where T : class
    {
        private readonly int hash;
        private static readonly object NULL = new object();

        public IdentityWeakReference(T target)
            : base(target == null ? NULL : target)
        {
            hash = RuntimeHelpers.GetHashCode(target);
        }

        public override int GetHashCode()
        {
            return hash;
        }

        public override bool Equals(object o)
        {
            if (ReferenceEquals(this, o))
            {
                return true;
            }
            if (o is IdentityWeakReference<T>)
            {
                IdentityWeakReference<T> iwr = (IdentityWeakReference<T>)o;
                if (ReferenceEquals(this.Target, iwr.Target))
                {
                    return true;
                }
            }
            return false;
        }

        public new T Target
        {
            get
            {
                // note: if this.NULL is the target, it will not cast to T, so the "as" will return null as we would expect.
                return base.Target as T;
            }
            set
            {
                base.Target = value;
            }
        }
    }
}