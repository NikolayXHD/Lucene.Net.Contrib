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
    public class IrishStemmer : SnowballProgram
    {
        private readonly static IrishStemmer methodObject = new IrishStemmer();

        private readonly static Among[] a_0 = {
                    new Among ( "b'", -1, 4, "", methodObject ),
                    new Among ( "bh", -1, 14, "", methodObject ),
                    new Among ( "bhf", 1, 9, "", methodObject ),
                    new Among ( "bp", -1, 11, "", methodObject ),
                    new Among ( "ch", -1, 15, "", methodObject ),
                    new Among ( "d'", -1, 2, "", methodObject ),
                    new Among ( "d'fh", 5, 3, "", methodObject ),
                    new Among ( "dh", -1, 16, "", methodObject ),
                    new Among ( "dt", -1, 13, "", methodObject ),
                    new Among ( "fh", -1, 17, "", methodObject ),
                    new Among ( "gc", -1, 7, "", methodObject ),
                    new Among ( "gh", -1, 18, "", methodObject ),
                    new Among ( "h-", -1, 1, "", methodObject ),
                    new Among ( "m'", -1, 4, "", methodObject ),
                    new Among ( "mb", -1, 6, "", methodObject ),
                    new Among ( "mh", -1, 19, "", methodObject ),
                    new Among ( "n-", -1, 1, "", methodObject ),
                    new Among ( "nd", -1, 8, "", methodObject ),
                    new Among ( "ng", -1, 10, "", methodObject ),
                    new Among ( "ph", -1, 20, "", methodObject ),
                    new Among ( "sh", -1, 5, "", methodObject ),
                    new Among ( "t-", -1, 1, "", methodObject ),
                    new Among ( "th", -1, 21, "", methodObject ),
                    new Among ( "ts", -1, 12, "", methodObject )
                };

        private readonly static Among[] a_1 = {
                    new Among ( "\u00EDochta", -1, 1, "", methodObject ),
                    new Among ( "a\u00EDochta", 0, 1, "", methodObject ),
                    new Among ( "ire", -1, 2, "", methodObject ),
                    new Among ( "aire", 2, 2, "", methodObject ),
                    new Among ( "abh", -1, 1, "", methodObject ),
                    new Among ( "eabh", 4, 1, "", methodObject ),
                    new Among ( "ibh", -1, 1, "", methodObject ),
                    new Among ( "aibh", 6, 1, "", methodObject ),
                    new Among ( "amh", -1, 1, "", methodObject ),
                    new Among ( "eamh", 8, 1, "", methodObject ),
                    new Among ( "imh", -1, 1, "", methodObject ),
                    new Among ( "aimh", 10, 1, "", methodObject ),
                    new Among ( "\u00EDocht", -1, 1, "", methodObject ),
                    new Among ( "a\u00EDocht", 12, 1, "", methodObject ),
                    new Among ( "ir\u00ED", -1, 2, "", methodObject ),
                    new Among ( "air\u00ED", 14, 2, "", methodObject )
                };

        private readonly static Among[] a_2 = {
                    new Among ( "\u00F3ideacha", -1, 6, "", methodObject ),
                    new Among ( "patacha", -1, 5, "", methodObject ),
                    new Among ( "achta", -1, 1, "", methodObject ),
                    new Among ( "arcachta", 2, 2, "", methodObject ),
                    new Among ( "eachta", 2, 1, "", methodObject ),
                    new Among ( "grafa\u00EDochta", -1, 4, "", methodObject ),
                    new Among ( "paite", -1, 5, "", methodObject ),
                    new Among ( "ach", -1, 1, "", methodObject ),
                    new Among ( "each", 7, 1, "", methodObject ),
                    new Among ( "\u00F3ideach", 8, 6, "", methodObject ),
                    new Among ( "gineach", 8, 3, "", methodObject ),
                    new Among ( "patach", 7, 5, "", methodObject ),
                    new Among ( "grafa\u00EDoch", -1, 4, "", methodObject ),
                    new Among ( "pataigh", -1, 5, "", methodObject ),
                    new Among ( "\u00F3idigh", -1, 6, "", methodObject ),
                    new Among ( "acht\u00FAil", -1, 1, "", methodObject ),
                    new Among ( "eacht\u00FAil", 15, 1, "", methodObject ),
                    new Among ( "gineas", -1, 3, "", methodObject ),
                    new Among ( "ginis", -1, 3, "", methodObject ),
                    new Among ( "acht", -1, 1, "", methodObject ),
                    new Among ( "arcacht", 19, 2, "", methodObject ),
                    new Among ( "eacht", 19, 1, "", methodObject ),
                    new Among ( "grafa\u00EDocht", -1, 4, "", methodObject ),
                    new Among ( "arcachta\u00ED", -1, 2, "", methodObject ),
                    new Among ( "grafa\u00EDochta\u00ED", -1, 4, "", methodObject )
                };

        private readonly static Among[] a_3 = {
                    new Among ( "imid", -1, 1, "", methodObject ),
                    new Among ( "aimid", 0, 1, "", methodObject ),
                    new Among ( "\u00EDmid", -1, 1, "", methodObject ),
                    new Among ( "a\u00EDmid", 2, 1, "", methodObject ),
                    new Among ( "adh", -1, 2, "", methodObject ),
                    new Among ( "eadh", 4, 2, "", methodObject ),
                    new Among ( "faidh", -1, 1, "", methodObject ),
                    new Among ( "fidh", -1, 1, "", methodObject ),
                    new Among ( "\u00E1il", -1, 2, "", methodObject ),
                    new Among ( "ain", -1, 2, "", methodObject ),
                    new Among ( "tear", -1, 2, "", methodObject ),
                    new Among ( "tar", -1, 2, "", methodObject )
                };

        private static readonly char[] g_v = { (char)17, (char)65, (char)16, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)1, (char)17, (char)4, (char)2 };

        private int I_p2;
        private int I_p1;
        private int I_pV;

        private void copy_from(IrishStemmer other)
        {
            I_p2 = other.I_p2;
            I_p1 = other.I_p1;
            I_pV = other.I_pV;
            base.CopyFrom(other);
        }

        private bool r_mark_regions()
        {
            int v_1;
            int v_3;
            // (, line 28
            I_pV = m_limit;
            I_p1 = m_limit;
            I_p2 = m_limit;
            // do, line 34
            v_1 = m_cursor;
            do
            {
                // (, line 34
                // gopast, line 35
                while (true)
                {
                    do
                    {
                        if (!(InGrouping(g_v, 97, 250)))
                        {
                            goto lab2;
                        }
                        goto golab1;
                    } while (false);
                    lab2:
                    if (m_cursor >= m_limit)
                    {
                        goto lab0;
                    }
                    m_cursor++;
                }
                golab1:
                // setmark pV, line 35
                I_pV = m_cursor;
            } while (false);
            lab0:
            m_cursor = v_1;
            // do, line 37
            v_3 = m_cursor;
            do
            {
                // (, line 37
                // gopast, line 38
                while (true)
                {
                    do
                    {
                        if (!(InGrouping(g_v, 97, 250)))
                        {
                            goto lab5;
                        }
                        goto golab4;
                    } while (false);
                    lab5:
                    if (m_cursor >= m_limit)
                    {
                        goto lab3;
                    }
                    m_cursor++;
                }
                golab4:
                // gopast, line 38
                while (true)
                {
                    do
                    {
                        if (!(OutGrouping(g_v, 97, 250)))
                        {
                            goto lab7;
                        }
                        goto golab6;
                    } while (false);
                    lab7:
                    if (m_cursor >= m_limit)
                    {
                        goto lab3;
                    }
                    m_cursor++;
                }
                golab6:
                // setmark p1, line 38
                I_p1 = m_cursor;
                // gopast, line 39
                while (true)
                {
                    do
                    {
                        if (!(InGrouping(g_v, 97, 250)))
                        {
                            goto lab9;
                        }
                        goto golab8;
                    } while (false);
                    lab9:
                    if (m_cursor >= m_limit)
                    {
                        goto lab3;
                    }
                    m_cursor++;
                }
                golab8:
                // gopast, line 39
                while (true)
                {
                    do
                    {
                        if (!(OutGrouping(g_v, 97, 250)))
                        {
                            goto lab11;
                        }
                        goto golab10;
                    } while (false);
                    lab11:
                    if (m_cursor >= m_limit)
                    {
                        goto lab3;
                    }
                    m_cursor++;
                }
                golab10:
                // setmark p2, line 39
                I_p2 = m_cursor;
            } while (false);
            lab3:
            m_cursor = v_3;
            return true;
        }

        private bool r_initial_morph()
        {
            int among_var;
            // (, line 43
            // [, line 44
            m_bra = m_cursor;
            // substring, line 44
            among_var = FindAmong(a_0, 24);
            if (among_var == 0)
            {
                return false;
            }
            // ], line 44
            m_ket = m_cursor;
            switch (among_var)
            {
                case 0:
                    return false;
                case 1:
                    // (, line 46
                    // delete, line 46
                    SliceDel();
                    break;
                case 2:
                    // (, line 50
                    // delete, line 50
                    SliceDel();
                    break;
                case 3:
                    // (, line 52
                    // <-, line 52
                    SliceFrom("f");
                    break;
                case 4:
                    // (, line 55
                    // delete, line 55
                    SliceDel();
                    break;
                case 5:
                    // (, line 58
                    // <-, line 58
                    SliceFrom("s");
                    break;
                case 6:
                    // (, line 61
                    // <-, line 61
                    SliceFrom("b");
                    break;
                case 7:
                    // (, line 63
                    // <-, line 63
                    SliceFrom("c");
                    break;
                case 8:
                    // (, line 65
                    // <-, line 65
                    SliceFrom("d");
                    break;
                case 9:
                    // (, line 67
                    // <-, line 67
                    SliceFrom("f");
                    break;
                case 10:
                    // (, line 69
                    // <-, line 69
                    SliceFrom("g");
                    break;
                case 11:
                    // (, line 71
                    // <-, line 71
                    SliceFrom("p");
                    break;
                case 12:
                    // (, line 73
                    // <-, line 73
                    SliceFrom("s");
                    break;
                case 13:
                    // (, line 75
                    // <-, line 75
                    SliceFrom("t");
                    break;
                case 14:
                    // (, line 79
                    // <-, line 79
                    SliceFrom("b");
                    break;
                case 15:
                    // (, line 81
                    // <-, line 81
                    SliceFrom("c");
                    break;
                case 16:
                    // (, line 83
                    // <-, line 83
                    SliceFrom("d");
                    break;
                case 17:
                    // (, line 85
                    // <-, line 85
                    SliceFrom("f");
                    break;
                case 18:
                    // (, line 87
                    // <-, line 87
                    SliceFrom("g");
                    break;
                case 19:
                    // (, line 89
                    // <-, line 89
                    SliceFrom("m");
                    break;
                case 20:
                    // (, line 91
                    // <-, line 91
                    SliceFrom("p");
                    break;
                case 21:
                    // (, line 93
                    // <-, line 93
                    SliceFrom("t");
                    break;
            }
            return true;
        }

        private bool r_RV()
        {
            if (!(I_pV <= m_cursor))
            {
                return false;
            }
            return true;
        }

        private bool r_R1()
        {
            if (!(I_p1 <= m_cursor))
            {
                return false;
            }
            return true;
        }

        private bool r_R2()
        {
            if (!(I_p2 <= m_cursor))
            {
                return false;
            }
            return true;
        }

        private bool r_noun_sfx()
        {
            int among_var;
            // (, line 103
            // [, line 104
            m_ket = m_cursor;
            // substring, line 104
            among_var = FindAmongB(a_1, 16);
            if (among_var == 0)
            {
                return false;
            }
            // ], line 104
            m_bra = m_cursor;
            switch (among_var)
            {
                case 0:
                    return false;
                case 1:
                    // (, line 108
                    // call R1, line 108
                    if (!r_R1())
                    {
                        return false;
                    }
                    // delete, line 108
                    SliceDel();
                    break;
                case 2:
                    // (, line 110
                    // call R2, line 110
                    if (!r_R2())
                    {
                        return false;
                    }
                    // delete, line 110
                    SliceDel();
                    break;
            }
            return true;
        }

        private bool r_deriv()
        {
            int among_var;
            // (, line 113
            // [, line 114
            m_ket = m_cursor;
            // substring, line 114
            among_var = FindAmongB(a_2, 25);
            if (among_var == 0)
            {
                return false;
            }
            // ], line 114
            m_bra = m_cursor;
            switch (among_var)
            {
                case 0:
                    return false;
                case 1:
                    // (, line 116
                    // call R2, line 116
                    if (!r_R2())
                    {
                        return false;
                    }
                    // delete, line 116
                    SliceDel();
                    break;
                case 2:
                    // (, line 118
                    // <-, line 118
                    SliceFrom("arc");
                    break;
                case 3:
                    // (, line 120
                    // <-, line 120
                    SliceFrom("gin");
                    break;
                case 4:
                    // (, line 122
                    // <-, line 122
                    SliceFrom("graf");
                    break;
                case 5:
                    // (, line 124
                    // <-, line 124
                    SliceFrom("paite");
                    break;
                case 6:
                    // (, line 126
                    // <-, line 126
                    SliceFrom("\u00F3id");
                    break;
            }
            return true;
        }

        private bool r_verb_sfx()
        {
            int among_var;
            // (, line 129
            // [, line 130
            m_ket = m_cursor;
            // substring, line 130
            among_var = FindAmongB(a_3, 12);
            if (among_var == 0)
            {
                return false;
            }
            // ], line 130
            m_bra = m_cursor;
            switch (among_var)
            {
                case 0:
                    return false;
                case 1:
                    // (, line 133
                    // call RV, line 133
                    if (!r_RV())
                    {
                        return false;
                    }
                    // delete, line 133
                    SliceDel();
                    break;
                case 2:
                    // (, line 138
                    // call R1, line 138
                    if (!r_R1())
                    {
                        return false;
                    }
                    // delete, line 138
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
            int v_5;
            // (, line 143
            // do, line 144
            v_1 = m_cursor;
            do
            {
                // call initial_morph, line 144
                if (!r_initial_morph())
                {
                    goto lab0;
                }
            } while (false);
            lab0:
            m_cursor = v_1;
            // do, line 145
            v_2 = m_cursor;
            do
            {
                // call mark_regions, line 145
                if (!r_mark_regions())
                {
                    goto lab1;
                }
            } while (false);
            lab1:
            m_cursor = v_2;
            // backwards, line 146
            m_limit_backward = m_cursor; m_cursor = m_limit;
            // (, line 146
            // do, line 147
            v_3 = m_limit - m_cursor;
            do
            {
                // call noun_sfx, line 147
                if (!r_noun_sfx())
                {
                    goto lab2;
                }
            } while (false);
            lab2:
            m_cursor = m_limit - v_3;
            // do, line 148
            v_4 = m_limit - m_cursor;
            do
            {
                // call deriv, line 148
                if (!r_deriv())
                {
                    goto lab3;
                }
            } while (false);
            lab3:
            m_cursor = m_limit - v_4;
            // do, line 149
            v_5 = m_limit - m_cursor;
            do
            {
                // call verb_sfx, line 149
                if (!r_verb_sfx())
                {
                    goto lab4;
                }
            } while (false);
            lab4:
            m_cursor = m_limit - v_5;
            m_cursor = m_limit_backward; return true;
        }

        public override bool Equals(object o)
        {
            return o is IrishStemmer;
        }

        public override int GetHashCode()
        {
            return this.GetType().FullName.GetHashCode();
        }
    }
}
