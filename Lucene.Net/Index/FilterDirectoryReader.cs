using System.Collections.Generic;
using System.Linq;

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

    /// <summary>
    /// A <see cref="FilterDirectoryReader"/> wraps another <see cref="DirectoryReader"/>, allowing implementations
    /// to transform or extend it.
    /// <para/>
    /// Subclasses should implement <see cref="DoWrapDirectoryReader(DirectoryReader)"/> to return an instance of the
    /// subclass.
    /// <para/>
    /// If the subclass wants to wrap the <see cref="DirectoryReader"/>'s subreaders, it should also
    /// implement a <see cref="SubReaderWrapper"/> subclass, and pass an instance to its base
    /// constructor.
    /// </summary>
    public abstract class FilterDirectoryReader : DirectoryReader
    {
        /// <summary>
        /// Factory class passed to <see cref="FilterDirectoryReader"/> constructor that allows
        /// subclasses to wrap the filtered <see cref="DirectoryReader"/>'s subreaders.  You
        /// can use this to, e.g., wrap the subreaders with specialized
        /// <see cref="FilterAtomicReader"/> implementations.
        /// </summary>
        public abstract class SubReaderWrapper
        {
            internal virtual AtomicReader[] Wrap(IList<AtomicReader> readers)
            {
                AtomicReader[] wrapped = new AtomicReader[readers.Count];
                for (int i = 0; i < readers.Count; i++)
                {
                    wrapped[i] = Wrap(readers[i]);
                }
                return wrapped;
            }

            /// <summary>
            /// Constructor </summary>
            public SubReaderWrapper()
            {
            }

            /// <summary>
            /// Wrap one of the parent <see cref="DirectoryReader"/>'s subreaders </summary>
            /// <param name="reader"> the subreader to wrap </param>
            /// <returns> a wrapped/filtered <see cref="AtomicReader"/> </returns>
            public abstract AtomicReader Wrap(AtomicReader reader);
        }

        /// <summary>
        /// A no-op <see cref="SubReaderWrapper"/> that simply returns the parent
        /// <see cref="DirectoryReader"/>'s original subreaders.
        /// </summary>
        public class StandardReaderWrapper : SubReaderWrapper
        {
            /// <summary>
            /// Constructor </summary>
            public StandardReaderWrapper()
            {
            }

            public override AtomicReader Wrap(AtomicReader reader)
            {
                return reader;
            }
        }

        /// <summary>
        /// The filtered <see cref="DirectoryReader"/> </summary>
        protected readonly DirectoryReader m_input;

        /// <summary>
        /// Create a new <see cref="FilterDirectoryReader"/> that filters a passed in <see cref="DirectoryReader"/>. </summary>
        /// <param name="input"> the <see cref="DirectoryReader"/> to filter </param>
        public FilterDirectoryReader(DirectoryReader input)
            : this(input, new StandardReaderWrapper())
        {
        }

        /// <summary>
        /// Create a new <see cref="FilterDirectoryReader"/> that filters a passed in <see cref="DirectoryReader"/>,
        /// using the supplied <see cref="SubReaderWrapper"/> to wrap its subreader. </summary>
        /// <param name="input"> the <see cref="DirectoryReader"/> to filter </param>
        /// <param name="wrapper"> the <see cref="SubReaderWrapper"/> to use to wrap subreaders </param>
        public FilterDirectoryReader(DirectoryReader input, SubReaderWrapper wrapper)
            : base(input.Directory, wrapper.Wrap(input.GetSequentialSubReaders().OfType<AtomicReader>().ToList()))
        {
            this.m_input = input;
        }

        /// <summary>
        /// Called by the <see cref="DoOpenIfChanged()"/> methods to return a new wrapped <see cref="DirectoryReader"/>.
        /// <para/>
        /// Implementations should just return an instance of themselves, wrapping the
        /// passed in <see cref="DirectoryReader"/>.
        /// </summary>
        /// <param name="input"> the <see cref="DirectoryReader"/> to wrap </param>
        /// <returns> the wrapped <see cref="DirectoryReader"/> </returns>
        protected abstract DirectoryReader DoWrapDirectoryReader(DirectoryReader input);

        private DirectoryReader WrapDirectoryReader(DirectoryReader input)
        {
            return input == null ? null : DoWrapDirectoryReader(input);
        }

        protected internal override sealed DirectoryReader DoOpenIfChanged()
        {
            return WrapDirectoryReader(m_input.DoOpenIfChanged());
        }

        protected internal override sealed DirectoryReader DoOpenIfChanged(IndexCommit commit)
        {
            return WrapDirectoryReader(m_input.DoOpenIfChanged(commit));
        }

        protected internal override sealed DirectoryReader DoOpenIfChanged(IndexWriter writer, bool applyAllDeletes)
        {
            return WrapDirectoryReader(m_input.DoOpenIfChanged(writer, applyAllDeletes));
        }

        public override long Version
        {
            get
            {
                return m_input.Version;
            }
        }

        public override bool IsCurrent()
        {
            return m_input.IsCurrent();
        }

        public override IndexCommit IndexCommit
        {
            get
            {
                return m_input.IndexCommit;
            }
        }

        protected internal override void DoClose()
        {
            m_input.DoClose();
        }
    }
}