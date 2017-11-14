﻿/*

Copyright (c) 2001, Dr Martin Porter
Copyright (c) 2002, Richard Boulton
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice,
    * this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
    * notice, this list of conditions and the following disclaimer in the
    * documentation and/or other materials provided with the distribution.
    * Neither the name of the copyright holders nor the names of its contributors
    * may be used to endorse or promote products derived from this software
    * without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

 */

namespace Lucene.Net.Tartarus.Snowball.Ext
{
    /// <summary>
    /// This class was automatically generated by a Snowball to Java compiler
    /// It implements the stemming algorithm defined by a snowball script.
    /// </summary>
    public class NorwegianStemmer : SnowballProgram
    {
        private readonly static NorwegianStemmer methodObject = new NorwegianStemmer();

        private readonly static Among[] a_0 = {
                    new Among ( "a", -1, 1, "", methodObject ),
                    new Among ( "e", -1, 1, "", methodObject ),
                    new Among ( "ede", 1, 1, "", methodObject ),
                    new Among ( "ande", 1, 1, "", methodObject ),
                    new Among ( "ende", 1, 1, "", methodObject ),
                    new Among ( "ane", 1, 1, "", methodObject ),
                    new Among ( "ene", 1, 1, "", methodObject ),
                    new Among ( "hetene", 6, 1, "", methodObject ),
                    new Among ( "erte", 1, 3, "", methodObject ),
                    new Among ( "en", -1, 1, "", methodObject ),
                    new Among ( "heten", 9, 1, "", methodObject ),
                    new Among ( "ar", -1, 1, "", methodObject ),
                    new Among ( "er", -1, 1, "", methodObject ),
                    new Among ( "heter", 12, 1, "", methodObject ),
                    new Among ( "s", -1, 2, "", methodObject ),
                    new Among ( "as", 14, 1, "", methodObject ),
                    new Among ( "es", 14, 1, "", methodObject ),
                    new Among ( "edes", 16, 1, "", methodObject ),
                    new Among ( "endes", 16, 1, "", methodObject ),
                    new Among ( "enes", 16, 1, "", methodObject ),
                    new Among ( "hetenes", 19, 1, "", methodObject ),
                    new Among ( "ens", 14, 1, "", methodObject ),
                    new Among ( "hetens", 21, 1, "", methodObject ),
                    new Among ( "ers", 14, 1, "", methodObject ),
                    new Among ( "ets", 14, 1, "", methodObject ),
                    new Among ( "et", -1, 1, "", methodObject ),
                    new Among ( "het", 25, 1, "", methodObject ),
                    new Among ( "ert", -1, 3, "", methodObject ),
                    new Among ( "ast", -1, 1, "", methodObject )
                };

        private readonly static Among[] a_1 = {
                    new Among ( "dt", -1, -1, "", methodObject ),
                    new Among ( "vt", -1, -1, "", methodObject )
                };

        private readonly static Among[] a_2 = {
                    new Among ( "leg", -1, 1, "", methodObject ),
                    new Among ( "eleg", 0, 1, "", methodObject ),
                    new Among ( "ig", -1, 1, "", methodObject ),
                    new Among ( "eig", 2, 1, "", methodObject ),
                    new Among ( "lig", 2, 1, "", methodObject ),
                    new Among ( "elig", 4, 1, "", methodObject ),
                    new Among ( "els", -1, 1, "", methodObject ),
                    new Among ( "lov", -1, 1, "", methodObject ),
                    new Among ( "elov", 7, 1, "", methodObject ),
                    new Among ( "slov", 7, 1, "", methodObject ),
                    new Among ( "hetslov", 9, 1, "", methodObject )
                };

        private static readonly char[] g_v = { (char)17, (char)65, (char)16, (char)1, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)48, (char)0, (char)128 };

        private static readonly char[] g_s_ending = { (char)119, (char)125, (char)149, (char)1 };

        private int I_x;
        private int I_p1;

        private void copy_from(NorwegianStemmer other)
        {
            I_x = other.I_x;
            I_p1 = other.I_p1;
            base.CopyFrom(other);
        }

        private bool r_mark_regions()
        {
            int v_1;
            int v_2;
            // (, line 26
            I_p1 = m_limit;
            // test, line 30
            v_1 = m_cursor;
            // (, line 30
            // hop, line 30
            {
                int c = m_cursor + 3;
                if (0 > c || c > m_limit)
                {
                    return false;
                }
                m_cursor = c;
            }
            // setmark x, line 30
            I_x = m_cursor;
            m_cursor = v_1;
            // goto, line 31
            while (true)
            {
                v_2 = m_cursor;
                do
                {
                    if (!(InGrouping(g_v, 97, 248)))
                    {
                        goto lab1;
                    }
                    m_cursor = v_2;
                    goto golab0;
                } while (false);
                lab1:
                m_cursor = v_2;
                if (m_cursor >= m_limit)
                {
                    return false;
                }
                m_cursor++;
            }
            golab0:
            // gopast, line 31
            while (true)
            {
                do
                {
                    if (!(OutGrouping(g_v, 97, 248)))
                    {
                        goto lab3;
                    }
                    goto golab2;
                } while (false);
                lab3:
                if (m_cursor >= m_limit)
                {
                    return false;
                }
                m_cursor++;
            }
            golab2:
            // setmark p1, line 31
            I_p1 = m_cursor;
            // try, line 32
            do
            {
                // (, line 32
                if (!(I_p1 < I_x))
                {
                    goto lab4;
                }
                I_p1 = I_x;
            } while (false);
            lab4:
            return true;
        }

        private bool r_main_suffix()
        {
            int among_var;
            int v_1;
            int v_2;
            int v_3;
            // (, line 37
            // setlimit, line 38
            v_1 = m_limit - m_cursor;
            // tomark, line 38
            if (m_cursor < I_p1)
            {
                return false;
            }
            m_cursor = I_p1;
            v_2 = m_limit_backward;
            m_limit_backward = m_cursor;
            m_cursor = m_limit - v_1;
            // (, line 38
            // [, line 38
            m_ket = m_cursor;
            // substring, line 38
            among_var = FindAmongB(a_0, 29);
            if (among_var == 0)
            {
                m_limit_backward = v_2;
                return false;
            }
            // ], line 38
            m_bra = m_cursor;
            m_limit_backward = v_2;
            switch (among_var)
            {
                case 0:
                    return false;
                case 1:
                    // (, line 44
                    // delete, line 44
                    SliceDel();
                    break;
                case 2:
                    // (, line 46
                    // or, line 46
                    do
                    {
                        v_3 = m_limit - m_cursor;
                        do
                        {
                            if (!(InGroupingB(g_s_ending, 98, 122)))
                            {
                                goto lab1;
                            }
                            goto lab0;
                        } while (false);
                        lab1:
                        m_cursor = m_limit - v_3;
                        // (, line 46
                        // literal, line 46
                        if (!(Eq_S_B(1, "k")))
                        {
                            return false;
                        }
                        if (!(OutGroupingB(g_v, 97, 248)))
                        {
                            return false;
                        }
                    } while (false);
                    lab0:
                    // delete, line 46
                    SliceDel();
                    break;
                case 3:
                    // (, line 48
                    // <-, line 48
                    SliceFrom("er");
                    break;
            }
            return true;
        }

        private bool r_consonant_pair()
        {
            int v_1;
            int v_2;
            int v_3;
            // (, line 52
            // test, line 53
            v_1 = m_limit - m_cursor;
            // (, line 53
            // setlimit, line 54
            v_2 = m_limit - m_cursor;
            // tomark, line 54
            if (m_cursor < I_p1)
            {
                return false;
            }
            m_cursor = I_p1;
            v_3 = m_limit_backward;
            m_limit_backward = m_cursor;
            m_cursor = m_limit - v_2;
            // (, line 54
            // [, line 54
            m_ket = m_cursor;
            // substring, line 54
            if (FindAmongB(a_1, 2) == 0)
            {
                m_limit_backward = v_3;
                return false;
            }
            // ], line 54
            m_bra = m_cursor;
            m_limit_backward = v_3;
            m_cursor = m_limit - v_1;
            // next, line 59
            if (m_cursor <= m_limit_backward)
            {
                return false;
            }
            m_cursor--;
            // ], line 59
            m_bra = m_cursor;
            // delete, line 59
            SliceDel();
            return true;
        }

        private bool r_other_suffix()
        {
            int among_var;
            int v_1;
            int v_2;
            // (, line 62
            // setlimit, line 63
            v_1 = m_limit - m_cursor;
            // tomark, line 63
            if (m_cursor < I_p1)
            {
                return false;
            }
            m_cursor = I_p1;
            v_2 = m_limit_backward;
            m_limit_backward = m_cursor;
            m_cursor = m_limit - v_1;
            // (, line 63
            // [, line 63
            m_ket = m_cursor;
            // substring, line 63
            among_var = FindAmongB(a_2, 11);
            if (among_var == 0)
            {
                m_limit_backward = v_2;
                return false;
            }
            // ], line 63
            m_bra = m_cursor;
            m_limit_backward = v_2;
            switch (among_var)
            {
                case 0:
                    return false;
                case 1:
                    // (, line 67
                    // delete, line 67
                    SliceDel();
                    break;
            }
            return true;
        }


        public override bool Stem()
        {
            int v_1;
            int v_2;
            int v_3;
            int v_4;
            // (, line 72
            // do, line 74
            v_1 = m_cursor;
            do
            {
                // call mark_regions, line 74
                if (!r_mark_regions())
                {
                    goto lab0;
                }
            } while (false);
            lab0:
            m_cursor = v_1;
            // backwards, line 75
            m_limit_backward = m_cursor; m_cursor = m_limit;
            // (, line 75
            // do, line 76
            v_2 = m_limit - m_cursor;
            do
            {
                // call main_suffix, line 76
                if (!r_main_suffix())
                {
                    goto lab1;
                }
            } while (false);
            lab1:
            m_cursor = m_limit - v_2;
            // do, line 77
            v_3 = m_limit - m_cursor;
            do
            {
                // call consonant_pair, line 77
                if (!r_consonant_pair())
                {
                    goto lab2;
                }
            } while (false);
            lab2:
            m_cursor = m_limit - v_3;
            // do, line 78
            v_4 = m_limit - m_cursor;
            do
            {
                // call other_suffix, line 78
                if (!r_other_suffix())
                {
                    goto lab3;
                }
            } while (false);
            lab3:
            m_cursor = m_limit - v_4;
            m_cursor = m_limit_backward; return true;
        }

        public override bool Equals(object o)
        {
            return o is NorwegianStemmer;
        }

        public override int GetHashCode()
        {
            return this.GetType().FullName.GetHashCode();
        }
    }
}
