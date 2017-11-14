﻿using Lucene.Net.Analysis.Util;
using System.Collections.Generic;

namespace Lucene.Net.Analysis.No
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
    /// Factory for <see cref="NorwegianMinimalStemFilter"/>.
    /// <code>
    /// &lt;fieldType name="text_svlgtstem" class="solr.TextField" positionIncrementGap="100"&gt;
    ///   &lt;analyzer&gt;
    ///     &lt;tokenizer class="solr.StandardTokenizerFactory"/&gt;
    ///     &lt;filter class="solr.LowerCaseFilterFactory"/&gt;
    ///     &lt;filter class="solr.NorwegianMinimalStemFilterFactory" variant="nb"/&gt;
    ///   &lt;/analyzer&gt;
    /// &lt;/fieldType&gt;</code>
    /// </summary>
    public class NorwegianMinimalStemFilterFactory : TokenFilterFactory
    {
        private readonly NorwegianStandard flags;

        /// <summary>
        /// Creates a new <see cref="NorwegianMinimalStemFilterFactory"/> </summary>
        public NorwegianMinimalStemFilterFactory(IDictionary<string, string> args) 
            : base(args)
        {
            string variant = Get(args, "variant");
            if (variant == null || "nb".Equals(variant))
            {
                flags = NorwegianStandard.BOKMAAL;
            }
            else if ("nn".Equals(variant))
            {
                flags = NorwegianStandard.NYNORSK;
            }
            else if ("no".Equals(variant))
            {
                flags = NorwegianStandard.BOKMAAL | NorwegianStandard.NYNORSK;
            }
            else
            {
                throw new System.ArgumentException("invalid variant: " + variant);
            }
            if (args.Count > 0)
            {
                throw new System.ArgumentException("Unknown parameters: " + args);
            }
        }

        public override TokenStream Create(TokenStream input)
        {
            return new NorwegianMinimalStemFilter(input, flags);
        }
    }
}