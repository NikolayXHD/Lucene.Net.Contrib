using Lucene.Net.Support;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

/*
 * dk.brics.automaton
 *
 * Copyright (c) 2001-2009 Anders Moeller
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * this SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * this SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace Lucene.Net.Util.Automaton
{
    /// <summary>
    /// Finite-state automaton with regular expression operations.
    /// <para/>
    /// Class invariants:
    /// <list type="bullet">
    ///     <item><description>An automaton is either represented explicitly (with <see cref="State"/> and
    ///         <see cref="Transition"/> objects) or with a singleton string (see
    ///         <see cref="Singleton"/> and <see cref="ExpandSingleton()"/>) in case the automaton
    ///         is known to accept exactly one string. (Implicitly, all states and
    ///         transitions of an automaton are reachable from its initial state.)</description></item>
    ///     <item><description>Automata are always reduced (see <see cref="Reduce()"/>) and have no
    ///         transitions to dead states (see <see cref="RemoveDeadTransitions()"/>).</description></item>
    ///     <item><description>If an automaton is nondeterministic, then <see cref="IsDeterministic"/>
    ///         returns <c>false</c> (but the converse is not required).</description></item>
    ///     <item><description>Automata provided as input to operations are generally assumed to be
    ///         disjoint.</description></item>
    /// </list>
    /// <para/>
    /// If the states or transitions are manipulated manually, the
    /// <see cref="RestoreInvariant()"/> method and <see cref="IsDeterministic"/> setter
    /// should be used afterwards to restore representation invariants that are
    /// assumed by the built-in automata operations.
    ///
    /// <para/>
    /// <para>
    /// Note: this class has internal mutable state and is not thread safe. It is
    /// the caller's responsibility to ensure any necessary synchronization if you
    /// wish to use the same Automaton from multiple threads. In general it is instead
    /// recommended to use a <see cref="RunAutomaton"/> for multithreaded matching: it is immutable,
    /// thread safe, and much faster.
    /// </para>
    /// @lucene.experimental
    /// </summary>
    public class Automaton
#if FEATURE_CLONEABLE
        : System.ICloneable
#endif
    {
        /// <summary>
        /// Minimize using Hopcroft's O(n log n) algorithm. this is regarded as one of
        /// the most generally efficient algorithms that exist.
        /// </summary>
        /// <seealso cref="SetMinimization(int)"/>
        public const int MINIMIZE_HOPCROFT = 2;

        /// <summary>
        /// Selects minimization algorithm (default: <c>MINIMIZE_HOPCROFT</c>). </summary>
        internal static int minimization = MINIMIZE_HOPCROFT;

        /// <summary>
        /// Initial state of this automaton. </summary>
        internal State initial;

        /// <summary>
        /// If <c>true</c>, then this automaton is definitely deterministic (i.e., there are
        /// no choices for any run, but a run may crash).
        /// </summary>
        internal bool deterministic;

        /// <summary>
        /// Extra data associated with this automaton. </summary>
        internal object info;

        ///// <summary>
        ///// Hash code. Recomputed by <see cref="MinimizationOperations#minimize(Automaton)"/>
        ///// </summary>
        //int hash_code;

        /// <summary>
        /// Singleton string. Null if not applicable. </summary>
        internal string singleton;

        /// <summary>
        /// Minimize always flag. </summary>
        internal static bool minimize_always = false;

        /// <summary>
        /// Selects whether operations may modify the input automata (default:
        /// <c>false</c>).
        /// </summary>
        internal static bool allow_mutation = false;

        /// <summary>
        /// Constructs a new automaton that accepts the empty language. Using this
        /// constructor, automata can be constructed manually from <see cref="State"/> and
        /// <see cref="Transition"/> objects.
        /// </summary>
        /// <seealso cref="State"/>
        /// <seealso cref="Transition"/>
        public Automaton(State initial)
        {
            this.initial = initial;
            deterministic = true;
            singleton = null;
        }

        public Automaton()
            : this(new State())
        {
        }

        /// <summary>
        /// Selects minimization algorithm (default: <c>MINIMIZE_HOPCROFT</c>).
        /// </summary>
        /// <param name="algorithm"> minimization algorithm </param>
        public static void SetMinimization(int algorithm)
        {
            minimization = algorithm;
        }

        /// <summary>
        /// Sets or resets minimize always flag. If this flag is set, then
        /// <see cref="MinimizationOperations.Minimize(Automaton)"/> will automatically be
        /// invoked after all operations that otherwise may produce non-minimal
        /// automata. By default, the flag is not set.
        /// </summary>
        /// <param name="flag"> if <c>true</c>, the flag is set </param>
        public static void SetMinimizeAlways(bool flag)
        {
            minimize_always = flag;
        }

        /// <summary>
        /// Sets or resets allow mutate flag. If this flag is set, then all automata
        /// operations may modify automata given as input; otherwise, operations will
        /// always leave input automata languages unmodified. By default, the flag is
        /// not set.
        /// </summary>
        /// <param name="flag"> if <c>true</c>, the flag is set </param>
        /// <returns> previous value of the flag </returns>
        public static bool SetAllowMutate(bool flag)
        {
            bool b = allow_mutation;
            allow_mutation = flag;
            return b;
        }

        /// <summary>
        /// Returns the state of the allow mutate flag. If this flag is set, then all
        /// automata operations may modify automata given as input; otherwise,
        /// operations will always leave input automata languages unmodified. By
        /// default, the flag is not set.
        /// </summary>
        /// <returns> current value of the flag </returns>
        internal static bool AllowMutate
        {
            get
            {
                return allow_mutation;
            }
        }

        internal virtual void CheckMinimizeAlways()
        {
            if (minimize_always)
            {
                MinimizationOperations.Minimize(this);
            }
        }

        internal bool IsSingleton
        {
            get
            {
                return singleton != null;
            }
        }

        /// <summary>
        /// Returns the singleton string for this automaton. An automaton that accepts
        /// exactly one string <i>may</i> be represented in singleton mode. In that
        /// case, this method may be used to obtain the string.
        /// </summary>
        /// <returns> String, <c>null</c> if this automaton is not in singleton mode. </returns>
        public virtual string Singleton
        {
            get
            {
                return singleton;
            }
        }

        ///// <summary>
        ///// Sets initial state.
        ///// </summary>
        ///// <param name="s"> state </param>
        /*
        public void setInitialState(State s) {
          initial = s;
          singleton = null;
        }
        */

        /// <summary>
        /// Gets initial state.
        /// </summary>
        /// <returns> state </returns>
        public virtual State GetInitialState()
        {
            ExpandSingleton();
            return initial;
        }

        /// <summary>
        /// Returns deterministic flag for this automaton.
        /// </summary>
        /// <returns> <c>true</c> if the automaton is definitely deterministic, <c>false</c> if the
        ///         automaton may be nondeterministic </returns>
        public virtual bool IsDeterministic
        {
            get
            {
                return deterministic;
            }
            set
            {
                this.deterministic = value;
            }
        }

        /// <summary>
        /// Associates extra information with this automaton.
        /// </summary>
        /// <param name="value"> extra information </param>
        public virtual object Info
        {
            set
            {
                this.info = value;
            }
            get
            {
                return info;
            }
        }

        // cached
        private State[] numberedStates;

        public virtual State[] GetNumberedStates()
        {
            if (numberedStates == null)
            {
                ExpandSingleton();
                HashSet<State> visited = new HashSet<State>();
                LinkedList<State> worklist = new LinkedList<State>();
                State[] states = new State[4];
                int upto = 0;
                worklist.AddLast(initial);
                visited.Add(initial);
                initial.number = upto;
                states[upto] = initial;
                upto++;
                while (worklist.Count > 0)
                {
                    State s = worklist.First.Value;
                    worklist.Remove(s);
                    for (int i = 0; i < s.numTransitions; i++)
                    {
                        Transition t = s.TransitionsArray[i];
                        if (!visited.Contains(t.to))
                        {
                            visited.Add(t.to);
                            worklist.AddLast(t.to);
                            t.to.number = upto;
                            if (upto == states.Length)
                            {
                                State[] newArray = new State[ArrayUtil.Oversize(1 + upto, RamUsageEstimator.NUM_BYTES_OBJECT_REF)];
                                Array.Copy(states, 0, newArray, 0, upto);
                                states = newArray;
                            }
                            states[upto] = t.to;
                            upto++;
                        }
                    }
                }
                if (states.Length != upto)
                {
                    State[] newArray = new State[upto];
                    Array.Copy(states, 0, newArray, 0, upto);
                    states = newArray;
                }
                numberedStates = states;
            }

            return numberedStates;
        }

        public virtual void SetNumberedStates(State[] states)
        {
            SetNumberedStates(states, states.Length);
        }

        public virtual void SetNumberedStates(State[] states, int count)
        {
            Debug.Assert(count <= states.Length);
            // TODO: maybe we can eventually allow for oversizing here...
            if (count < states.Length)
            {
                State[] newArray = new State[count];
                Array.Copy(states, 0, newArray, 0, count);
                numberedStates = newArray;
            }
            else
            {
                numberedStates = states;
            }
        }

        public virtual void ClearNumberedStates()
        {
            numberedStates = null;
        }

        /// <summary>
        /// Returns the set of reachable accept states.
        /// </summary>
        /// <returns> Set of <see cref="State"/> objects. </returns>
        public virtual ISet<State> GetAcceptStates()
        {
            ExpandSingleton();
            HashSet<State> accepts = new HashSet<State>();
            HashSet<State> visited = new HashSet<State>();
            LinkedList<State> worklist = new LinkedList<State>();
            worklist.AddLast(initial);
            visited.Add(initial);
            while (worklist.Count > 0)
            {
                State s = worklist.First.Value;
                worklist.Remove(s);
                if (s.accept)
                {
                    accepts.Add(s);
                }
                foreach (Transition t in s.GetTransitions())
                {
                    if (!visited.Contains(t.to))
                    {
                        visited.Add(t.to);
                        worklist.AddLast(t.to);
                    }
                }
            }
            return accepts;
        }

        /// <summary>
        /// Adds transitions to explicit crash state to ensure that transition function
        /// is total.
        /// </summary>
        internal virtual void Totalize()
        {
            State s = new State();
            s.AddTransition(new Transition(Character.MIN_CODE_POINT, Character.MAX_CODE_POINT, s));
            foreach (State p in GetNumberedStates())
            {
                int maxi = Character.MIN_CODE_POINT;
                p.SortTransitions(Transition.COMPARE_BY_MIN_MAX_THEN_DEST);
                foreach (Transition t in p.GetTransitions())
                {
                    if (t.min > maxi)
                    {
                        p.AddTransition(new Transition(maxi, (t.min - 1), s));
                    }
                    if (t.max + 1 > maxi)
                    {
                        maxi = t.max + 1;
                    }
                }
                if (maxi <= Character.MAX_CODE_POINT)
                {
                    p.AddTransition(new Transition(maxi, Character.MAX_CODE_POINT, s));
                }
            }
            ClearNumberedStates();
        }

        /// <summary>
        /// Restores representation invariant. This method must be invoked before any
        /// built-in automata operation is performed if automaton states or transitions
        /// are manipulated manually.
        /// </summary>
        /// <seealso cref="IsDeterministic"/>
        public virtual void RestoreInvariant()
        {
            RemoveDeadTransitions();
        }

        /// <summary>
        /// Reduces this automaton. An automaton is "reduced" by combining overlapping
        /// and adjacent edge intervals with same destination.
        /// </summary>
        public virtual void Reduce()
        {
            State[] states = GetNumberedStates();
            if (IsSingleton)
            {
                return;
            }
            foreach (State s in states)
            {
                s.Reduce();
            }
        }

        /// <summary>
        /// Returns sorted array of all interval start points.
        /// </summary>
        public virtual int[] GetStartPoints()
        {
            State[] states = GetNumberedStates();
            HashSet<int> pointset = new HashSet<int>();
            pointset.Add(Character.MIN_CODE_POINT);
            foreach (State s in states)
            {
                foreach (Transition t in s.GetTransitions())
                {
                    pointset.Add(t.min);
                    if (t.max < Character.MAX_CODE_POINT)
                    {
                        pointset.Add((t.max + 1));
                    }
                }
            }
            int[] points = new int[pointset.Count];
            int n = 0;
            foreach (int m in pointset)
            {
                points[n++] = m;
            }
            Array.Sort(points);
            return points;
        }

        /// <summary>
        /// Returns the set of live states. A state is "live" if an accept state is
        /// reachable from it.
        /// </summary>
        /// <returns> Set of <see cref="State"/> objects. </returns>
        private State[] GetLiveStates()
        {
            State[] states = GetNumberedStates();
            HashSet<State> live = new HashSet<State>();
            foreach (State q in states)
            {
                if (q.Accept)
                {
                    live.Add(q);
                }
            }
            // map<state, set<state>>
            ISet<State>[] map = new HashSet<State>[states.Length];
            for (int i = 0; i < map.Length; i++)
            {
                map[i] = new HashSet<State>();
            }
            foreach (State s in states)
            {
                for (int i = 0; i < s.numTransitions; i++)
                {
                    map[s.TransitionsArray[i].to.Number].Add(s);
                }
            }
            LinkedList<State> worklist = new LinkedList<State>(live);
            while (worklist.Count > 0)
            {
                State s = worklist.First.Value;
                worklist.Remove(s);
                foreach (State p in map[s.number])
                {
                    if (!live.Contains(p))
                    {
                        live.Add(p);
                        worklist.AddLast(p);
                    }
                }
            }

            return live.ToArray(/*new State[live.Count]*/);
        }

        /// <summary>
        /// Removes transitions to dead states and calls <see cref="Reduce()"/>.
        /// (A state is "dead" if no accept state is
        /// reachable from it.)
        /// </summary>
        public virtual void RemoveDeadTransitions()
        {
            State[] states = GetNumberedStates();
            //clearHashCode();
            if (IsSingleton)
            {
                return;
            }
            State[] live = GetLiveStates();

            OpenBitSet liveSet = new OpenBitSet(states.Length);
            foreach (State s in live)
            {
                liveSet.Set(s.number);
            }

            foreach (State s in states)
            {
                // filter out transitions to dead states:
                int upto = 0;
                for (int i = 0; i < s.numTransitions; i++)
                {
                    Transition t = s.TransitionsArray[i];
                    if (liveSet.Get(t.to.number))
                    {
                        s.TransitionsArray[upto++] = s.TransitionsArray[i];
                    }
                }
                s.numTransitions = upto;
            }
            for (int i = 0; i < live.Length; i++)
            {
                live[i].number = i;
            }
            if (live.Length > 0)
            {
                SetNumberedStates(live);
            }
            else
            {
                // sneaky corner case -- if machine accepts no strings
                ClearNumberedStates();
            }
            Reduce();
        }

        /// <summary>
        /// Returns a sorted array of transitions for each state (and sets state
        /// numbers).
        /// </summary>
        public virtual Transition[][] GetSortedTransitions()
        {
            State[] states = GetNumberedStates();
            Transition[][] transitions = new Transition[states.Length][];
            foreach (State s in states)
            {
                s.SortTransitions(Transition.COMPARE_BY_MIN_MAX_THEN_DEST);
                s.TrimTransitionsArray();
                transitions[s.number] = s.TransitionsArray;
                Debug.Assert(s.TransitionsArray != null);
            }
            return transitions;
        }

        /// <summary>
        /// Expands singleton representation to normal representation. Does nothing if
        /// not in singleton representation.
        /// </summary>
        public virtual void ExpandSingleton()
        {
            if (IsSingleton)
            {
                State p = new State();
                initial = p;
                for (int i = 0, cp = 0; i < singleton.Length; i += Character.CharCount(cp))
                {
                    State q = new State();
                    p.AddTransition(new Transition(cp = Character.CodePointAt(singleton, i), q));
                    p = q;
                }
                p.accept = true;
                deterministic = true;
                singleton = null;
            }
        }

        /// <summary>
        /// Returns the number of states in this automaton.
        /// </summary>
        public virtual int GetNumberOfStates()
        {
            if (IsSingleton)
            {
                return singleton.CodePointCount(0, singleton.Length) + 1;
            }
            return GetNumberedStates().Length;
        }

        /// <summary>
        /// Returns the number of transitions in this automaton. This number is counted
        /// as the total number of edges, where one edge may be a character interval.
        /// </summary>
        public virtual int GetNumberOfTransitions()
        {
            if (IsSingleton)
            {
                return singleton.CodePointCount(0, singleton.Length);
            }
            int c = 0;
            foreach (State s in GetNumberedStates())
            {
                c += s.NumTransitions;
            }
            return c;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Automaton;
            return BasicOperations.SameLanguage(this, other);
            //throw new System.NotSupportedException("use BasicOperations.sameLanguage instead");
        }

        // LUCENENET specific - in .NET, we can't simply throw an exception here because 
        // collections use this to determine equality. Most of this code was pieced together from
        // BasicOperations.SubSetOf (which, when done both ways determines equality).
        public override int GetHashCode() 
        {
            if (IsSingleton)
            {
                return singleton.GetHashCode();
            }

            int hash = 31; // arbitrary prime

            this.Determinize(); // LUCENENET: should we do this ?

            Transition[][] transitions = this.GetSortedTransitions();
            LinkedList<State> worklist = new LinkedList<State>();
            HashSet<State> visited = new HashSet<State>();

            State current = this.initial;
            worklist.AddLast(this.initial);
            visited.Add(this.initial);
            while (worklist.Count > 0)
            {
                current = worklist.First.Value;
                worklist.Remove(current);
                hash = 31 * hash + current.accept.GetHashCode();

                Transition[] t1 = transitions[current.number];

                for (int n1 = 0; n1 < t1.Length; n1++)
                {
                    int min1 = t1[n1].min, max1 = t1[n1].max;

                    hash = 31 * hash + min1.GetHashCode();
                    hash = 31 * hash + max1.GetHashCode();

                    State next = t1[n1].to;
                    if (!visited.Contains(next))
                    {
                        worklist.AddLast(next);
                        visited.Add(next);
                    }
                }
            }

            return hash;
            //throw new System.NotSupportedException();
        }

        ///// <summary>
        ///// Must be invoked when the stored hash code may no longer be valid.
        ///// </summary>
        /*
        void clearHashCode() {
          hash_code = 0;
        }
        */

        /// <summary>
        /// Returns a string representation of this automaton.
        /// </summary>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            if (IsSingleton)
            {
                b.Append("singleton: ");
                int length = singleton.CodePointCount(0, singleton.Length);
                int[] codepoints = new int[length];
                for (int i = 0, j = 0, cp = 0; i < singleton.Length; i += Character.CharCount(cp))
                {
                    codepoints[j++] = cp = singleton.CodePointAt(i);
                }
                foreach (int c in codepoints)
                {
                    Transition.AppendCharString(c, b);
                }
                b.Append("\n");
            }
            else
            {
                State[] states = GetNumberedStates();
                b.Append("initial state: ").Append(initial.number).Append("\n");
                foreach (State s in states)
                {
                    b.Append(s.ToString());
                }
            }
            return b.ToString();
        }

        /// <summary>
        /// Returns <a href="http://www.research.att.com/sw/tools/graphviz/"
        /// target="_top">Graphviz Dot</a> representation of this automaton.
        /// </summary>
        public virtual string ToDot()
        {
            StringBuilder b = new StringBuilder("digraph Automaton {\n");
            b.Append("  rankdir = LR;\n");
            State[] states = GetNumberedStates();
            foreach (State s in states)
            {
                b.Append("  ").Append(s.number);
                if (s.accept)
                {
                    b.Append(" [shape=doublecircle,label=\"\"];\n");
                }
                else
                {
                    b.Append(" [shape=circle,label=\"\"];\n");
                }
                if (s == initial)
                {
                    b.Append("  initial [shape=plaintext,label=\"\"];\n");
                    b.Append("  initial -> ").Append(s.number).Append("\n");
                }
                foreach (Transition t in s.GetTransitions())
                {
                    b.Append("  ").Append(s.number);
                    t.AppendDot(b);
                }
            }
            return b.Append("}\n").ToString();
        }

        /// <summary>
        /// Returns a clone of this automaton, expands if singleton.
        /// </summary>
        internal virtual Automaton CloneExpanded()
        {
            Automaton a = (Automaton)Clone();
            a.ExpandSingleton();
            return a;
        }

        /// <summary>
        /// Returns a clone of this automaton unless <see cref="allow_mutation"/> is
        /// set, expands if singleton.
        /// </summary>
        internal virtual Automaton CloneExpandedIfRequired()
        {
            if (allow_mutation)
            {
                ExpandSingleton();
                return this;
            }
            else
            {
                return CloneExpanded();
            }
        }

        /// <summary>
        /// Returns a clone of this automaton.
        /// </summary>
        public virtual object Clone()
        {
            Automaton a = (Automaton)base.MemberwiseClone();
            if (!IsSingleton)
            {
                Dictionary<State, State> m = new Dictionary<State, State>();
                State[] states = GetNumberedStates();
                foreach (State s in states)
                {
                    m[s] = new State();
                }
                foreach (State s in states)
                {
                    State p = m[s];
                    p.accept = s.accept;
                    if (s == initial)
                    {
                        a.initial = p;
                    }
                    foreach (Transition t in s.GetTransitions())
                    {
                        p.AddTransition(new Transition(t.min, t.max, m[t.to]));
                    }
                }
            }
            a.ClearNumberedStates();
            return a;
        }

        /// <summary>
        /// Returns a clone of this automaton, or this automaton itself if
        /// <see cref="allow_mutation"/> flag is set.
        /// </summary>
        internal virtual Automaton CloneIfRequired()
        {
            if (allow_mutation)
            {
                return this;
            }
            else
            {
                return (Automaton)Clone();
            }
        }

        /// <summary>
        /// See <see cref="BasicOperations.Concatenate(Automaton, Automaton)"/>.
        /// </summary>
        public virtual Automaton Concatenate(Automaton a)
        {
            return BasicOperations.Concatenate(this, a);
        }

        /// <summary>
        /// See <see cref="BasicOperations.Concatenate(IList{Automaton})"/>.
        /// </summary>
        public static Automaton Concatenate(IList<Automaton> l)
        {
            return BasicOperations.Concatenate(l);
        }

        /// <summary>
        /// See <see cref="BasicOperations.Optional(Automaton)"/>.
        /// </summary>
        public virtual Automaton Optional()
        {
            return BasicOperations.Optional(this);
        }

        /// <summary>
        /// See <see cref="BasicOperations.Repeat(Automaton)"/>.
        /// </summary>
        public virtual Automaton Repeat()
        {
            return BasicOperations.Repeat(this);
        }

        /// <summary>
        /// See <see cref="BasicOperations.Repeat(Automaton, int)"/>.
        /// </summary>
        public virtual Automaton Repeat(int min)
        {
            return BasicOperations.Repeat(this, min);
        }

        /// <summary>
        /// See <see cref="BasicOperations.Repeat(Automaton, int, int)"/>.
        /// </summary>
        public virtual Automaton Repeat(int min, int max)
        {
            return BasicOperations.Repeat(this, min, max);
        }

        /// <summary>
        /// See <see cref="BasicOperations.Complement(Automaton)"/>.
        /// </summary>
        public virtual Automaton Complement()
        {
            return BasicOperations.Complement(this);
        }

        /// <summary>
        /// See <see cref="BasicOperations.Minus(Automaton, Automaton)"/>.
        /// </summary>
        public virtual Automaton Minus(Automaton a)
        {
            return BasicOperations.Minus(this, a);
        }

        /// <summary>
        /// See <see cref="BasicOperations.Intersection(Automaton, Automaton)"/>.
        /// </summary>
        public virtual Automaton Intersection(Automaton a)
        {
            return BasicOperations.Intersection(this, a);
        }

        /// <summary>
        /// See <see cref="BasicOperations.SubsetOf(Automaton, Automaton)"/>.
        /// </summary>
        public virtual bool SubsetOf(Automaton a)
        {
            return BasicOperations.SubsetOf(this, a);
        }

        /// <summary>
        /// See <see cref="BasicOperations.Union(Automaton, Automaton)"/>.
        /// </summary>
        public virtual Automaton Union(Automaton a)
        {
            return BasicOperations.Union(this, a);
        }

        /// <summary>
        /// See <see cref="BasicOperations.Union(ICollection{Automaton})"/>.
        /// </summary>
        public static Automaton Union(ICollection<Automaton> l)
        {
            return BasicOperations.Union(l);
        }

        /// <summary>
        /// See <see cref="BasicOperations.Determinize(Automaton)"/>.
        /// </summary>
        public virtual void Determinize()
        {
            BasicOperations.Determinize(this);
        }

        /// <summary>
        /// See <see cref="BasicOperations.IsEmptyString(Automaton)"/>.
        /// </summary>
        public virtual bool IsEmptyString
        {
            get
            {
                return BasicOperations.IsEmptyString(this);
            }
        }

        /// <summary>
        /// See <see cref="MinimizationOperations.Minimize(Automaton)"/>. Returns the
        /// automaton being given as argument.
        /// </summary>
        public static Automaton Minimize(Automaton a)
        {
            MinimizationOperations.Minimize(a);
            return a;
        }
    }
}