using Lucene.Net.Support;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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
    /// Represents <see cref="T:char[]"/>, as a slice (offset + Length) into an existing <see cref="T:char[]"/>.
    /// The <see cref="Chars"/> property should never be <c>null</c>; use
    /// <see cref="EMPTY_CHARS"/> if necessary.
    /// <para/>
    /// @lucene.internal
    /// </summary>
#if FEATURE_SERIALIZABLE
    [Serializable]
#endif
    public sealed class CharsRef : IComparable<CharsRef>, ICharSequence
#if FEATURE_CLONEABLE
        , System.ICloneable
#endif
    {
        /// <summary>
        /// An empty character array for convenience </summary>
        public static readonly char[] EMPTY_CHARS = new char[0];

        /// <summary>
        /// The contents of the <see cref="CharsRef"/>. Should never be <c>null</c>.
        /// </summary>
        [WritableArray]
        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "Lucene's design requires some writable array properties")]
        public char[] Chars
        {
            get { return chars; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Chars cannot be null");
                }
                chars = value;
            }
        }
        private char[] chars;

        /// <summary>
        /// Offset of first valid character. </summary>
        public int Offset { get; internal set; }

        /// <summary>
        /// Length of used characters. </summary>
        public int Length { get; set; }

        /// <summary>
        /// Creates a new <see cref="CharsRef"/> initialized an empty array zero-Length
        /// </summary>
        public CharsRef()
            : this(EMPTY_CHARS, 0, 0)
        {
        }

        /// <summary>
        /// Creates a new <see cref="CharsRef"/> initialized with an array of the given
        /// <paramref name="capacity"/>.
        /// </summary>
        public CharsRef(int capacity)
        {
            chars = new char[capacity];
        }

        /// <summary>
        /// Creates a new <see cref="CharsRef"/> initialized with the given <paramref name="chars"/>, 
        /// <paramref name="offset"/> and <paramref name="length"/>.
        /// </summary>
        public CharsRef(char[] chars, int offset, int length)
        {
            this.chars = chars;
            this.Offset = offset;
            this.Length = length;
            Debug.Assert(IsValid());
        }

        /// <summary>
        /// Creates a new <see cref="CharsRef"/> initialized with the given <see cref="string"/> character
        /// array.
        /// </summary>
        public CharsRef(string @string)
        {
            this.chars = @string.ToCharArray();
            this.Offset = 0;
            this.Length = chars.Length;
        }

        /// <summary>
        /// Returns a shallow clone of this instance (the underlying characters are
        /// <b>not</b> copied and will be shared by both the returned object and this
        /// object.
        /// </summary>
        /// <seealso cref="DeepCopyOf(CharsRef)"/>
        public object Clone()
        {
            return new CharsRef(chars, Offset, Length);
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 0;
            int end = Offset + Length;
            for (int i = Offset; i < end; i++)
            {
                result = prime * result + chars[i];
            }
            return result;
        }

        public override bool Equals(object other)
        {
            if (other == null)
            {
                return false;
            }

            if (other is CharsRef)
            {
                return this.CharsEquals(((CharsRef)other));
            }
            return false;
        }

        public bool CharsEquals(CharsRef other)
        {
            if (Length == other.Length)
            {
                int otherUpto = other.Offset;
                char[] otherChars = other.chars;
                int end = Offset + Length;
                for (int upto = Offset; upto < end; upto++, otherUpto++)
                {
                    if (chars[upto] != otherChars[otherUpto])
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Signed <see cref="int"/> order comparison </summary>
        public int CompareTo(CharsRef other)
        {
            if (this == other)
            {
                return 0;
            }

            char[] aChars = this.chars;
            int aUpto = this.Offset;
            char[] bChars = other.chars;
            int bUpto = other.Offset;

            int aStop = aUpto + Math.Min(this.Length, other.Length);

            while (aUpto < aStop)
            {
                int aInt = aChars[aUpto++];
                int bInt = bChars[bUpto++];
                if (aInt > bInt)
                {
                    return 1;
                }
                else if (aInt < bInt)
                {
                    return -1;
                }
            }

            // One is a prefix of the other, or, they are equal:
            return this.Length - other.Length;
        }

        /// <summary>
        /// Copies the given <see cref="CharsRef"/> referenced content into this instance.
        /// </summary>
        /// <param name="other">
        ///          The <see cref="CharsRef"/> to copy. </param>
        public void CopyChars(CharsRef other)
        {
            CopyChars(other.chars, other.Offset, other.Length);
        }

        /// <summary>
        /// Used to grow the reference array.
        /// <para/>
        /// In general this should not be used as it does not take the offset into account.
        /// <para/>
        /// @lucene.internal
        /// </summary>
        public void Grow(int newLength)
        {
            Debug.Assert(Offset == 0);
            if (chars.Length < newLength)
            {
                chars = ArrayUtil.Grow(chars, newLength);
            }
        }

        /// <summary>
        /// Copies the given array into this <see cref="CharsRef"/>.
        /// </summary>
        public void CopyChars(char[] otherChars, int otherOffset, int otherLength)
        {
            if (Chars.Length - Offset < otherLength)
            {
                chars = new char[otherLength];
                Offset = 0;
            }
            Array.Copy(otherChars, otherOffset, chars, Offset, otherLength);
            Length = otherLength;
        }

        /// <summary>
        /// Appends the given array to this <see cref="CharsRef"/>.
        /// </summary>
        public void Append(char[] otherChars, int otherOffset, int otherLength)
        {
            int newLen = Length + otherLength;
            if (chars.Length - Offset < newLen)
            {
                var newChars = new char[newLen];
                Array.Copy(chars, Offset, newChars, 0, Length);
                Offset = 0;
                chars = newChars;
            }
            Array.Copy(otherChars, otherOffset, chars, Length + Offset, otherLength);
            Length = newLen;
        }

        public override string ToString()
        {
            return new string(chars, Offset, Length);
        }

        // LUCENENET NOTE: Length field made into property already

        //public char CharAt(int index)
        //{
        //    // NOTE: must do a real check here to meet the specs of CharSequence
        //    if (index < 0 || index >= Length)
        //    {
        //        throw new System.IndexOutOfRangeException();
        //    }
        //    return Chars[Offset + index];
        //}

        // LUCENENET specific - added to .NETify
        public char this[int index]
        {
            get
            {
                // NOTE: must do a real check here to meet the specs of CharSequence
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException();
                }
                return chars[Offset + index];
            }
        }

        public ICharSequence SubSequence(int start, int end)
        {
            // NOTE: must do a real check here to meet the specs of CharSequence
            if (start < 0 || end > Length || start > end)
            {
                throw new System.IndexOutOfRangeException();
            }
            return new CharsRef(chars, Offset + start, end - start);
        }

        /// @deprecated this comparer is only a transition mechanism
        [Obsolete("this comparer is only a transition mechanism")]
        private static readonly IComparer<CharsRef> utf16SortedAsUTF8SortOrder = new Utf16SortedAsUTF8Comparer();

        /// @deprecated this comparer is only a transition mechanism
        [Obsolete("this comparer is only a transition mechanism")]
        public static IComparer<CharsRef> UTF16SortedAsUTF8Comparer
        {
            get
            {
                return utf16SortedAsUTF8SortOrder;
            }
        }

        /// @deprecated this comparer is only a transition mechanism
        [Obsolete("this comparer is only a transition mechanism")]
        private class Utf16SortedAsUTF8Comparer : IComparer<CharsRef>
        {
            // Only singleton
            internal Utf16SortedAsUTF8Comparer()
            {
            }

            public virtual int Compare(CharsRef a, CharsRef b)
            {
                if (a == b)
                {
                    return 0;
                }

                char[] aChars = a.Chars;
                int aUpto = a.Offset;
                char[] bChars = b.Chars;
                int bUpto = b.Offset;

                int aStop = aUpto + Math.Min(a.Length, b.Length);

                while (aUpto < aStop)
                {
                    char aChar = aChars[aUpto++];
                    char bChar = bChars[bUpto++];
                    if (aChar != bChar)
                    {
                        // http://icu-project.org/docs/papers/utf16_code_point_order.html

                        /* aChar != bChar, fix up each one if they're both in or above the surrogate range, then compare them */
                        if (aChar >= 0xd800 && bChar >= 0xd800)
                        {//LUCENE TO-DO possible truncation or is char 16bit?
                            if (aChar >= 0xe000)
                            {
                                aChar -= (char)0x800;
                            }
                            else
                            {
                                aChar += (char)0x2000;
                            }

                            if (bChar >= 0xe000)
                            {
                                bChar -= (char)0x800;
                            }
                            else
                            {
                                bChar += (char)0x2000;
                            }
                        }

                        /* now aChar and bChar are in code point order */
                        return (int)aChar - (int)bChar; // int must be 32 bits wide
                    }
                }

                // One is a prefix of the other, or, they are equal:
                return a.Length - b.Length;
            }
        }

        /// <summary>
        /// Creates a new <see cref="CharsRef"/> that points to a copy of the chars from
        /// <paramref name="other"/>.
        /// <para/>
        /// The returned <see cref="CharsRef"/> will have a Length of <c>other.Length</c>
        /// and an offset of zero.
        /// </summary>
        public static CharsRef DeepCopyOf(CharsRef other)
        {
            CharsRef clone = new CharsRef();
            clone.CopyChars(other);
            return clone;
        }

        /// <summary>
        /// Performs internal consistency checks.
        /// Always returns true (or throws <see cref="InvalidOperationException"/>)
        /// </summary>
        public bool IsValid()
        {
            if (Chars == null)
            {
                throw new InvalidOperationException("chars is null");
            }
            if (Length < 0)
            {
                throw new InvalidOperationException("Length is negative: " + Length);
            }
            if (Length > Chars.Length)
            {
                throw new InvalidOperationException("Length is out of bounds: " + Length + ",chars.Length=" + Chars.Length);
            }
            if (Offset < 0)
            {
                throw new InvalidOperationException("offset is negative: " + Offset);
            }
            if (Offset > Chars.Length)
            {
                throw new InvalidOperationException("offset out of bounds: " + Offset + ",chars.Length=" + Chars.Length);
            }
            if (Offset + Length < 0)
            {
                throw new InvalidOperationException("offset+Length is negative: offset=" + Offset + ",Length=" + Length);
            }
            if (Offset + Length > Chars.Length)
            {
                throw new InvalidOperationException("offset+Length out of bounds: offset=" + Offset + ",Length=" + Length + ",chars.Length=" + Chars.Length);
            }
            return true;
        }
    }
}