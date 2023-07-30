using TED;
using TED.Interpreter;
using TED.Tables;
using TotT.Simulator;
using TotT.Time;
using TotT.Unity;
using UnityEngine;
using static TED.Language;

namespace TotT.Simulog {
    using static Clock;
    using static Color;
    using TimePoint = TimePoint;
    using static TimePoint;
    using static Variables;
    using static SimuLang;

    // ReSharper disable MemberCanBePrivate.Global

    public class Existent<T> : TablePredicate<T, bool, TimePoint, TimePoint> {
        public readonly Event<T> Start;
        public readonly Event<T> End;
        public readonly Event<T, TimePoint> StartWith;

        private readonly Var<TimePoint> _start;

        public Existent(string name, Var<T> existent, Var<TimePoint> start) 
            : base(name, existent.Key, exists.Indexed, start, end) {
            _start = start;
            Start = Event($"{name}Start", existent);
            End = Event($"{name}End", existent);
            StartWith = Event($"{name}StartWith", existent, _start);
            Add[existent, true, time, Eschaton].If(Start, CurrentTimePoint[time]);
            Add[existent, true, start, Eschaton].If(StartWith);
            Set(existent, end, time).If(End, CurrentTimePoint[time]);
            Set(existent, exists, false).If(End);
            this.Colorize(exists, s => s ? white : gray);
        }

        #region Count table
        private TablePredicate<bool, int> _countPredicate;
        public TablePredicate<bool, int> Count {
            get {
                if (_countPredicate != null) return _countPredicate;
                _countPredicate = CountsBy($"{Name}Count", this, exists, count);
                _countPredicate.IndexByKey(exists);
                return _countPredicate;
            }
        }
        public KeyIndex<(bool, int), bool> CountIndex =>
            (KeyIndex<(bool, int), bool>)Count.IndexFor(exists, true);
        #endregion

        public Existent<T> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<T>)DefaultVariables[0], true, _start, Eschaton].Where(conditions);
            return this;
        }

        public Existent<T> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public Existent<T> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public Existent<T> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public Existent<T> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }
        
        public Existent<T> StartWithTime(params Goal[] conditions) {
            StartWith.OccursWhen(conditions);
            return this;
        }
        public Existent<T> StartWithCauses(params Effect[] effects) {
            StartWith.Causes(effects);
            return this;
        }
        
        public TablePredicate<T, TimePoint, TimePoint> StartWithChronicle => StartWith.Chronicle;

        #region Feature Tables

        public TablePredicate<T, T1> Features<T1>(string name, IColumnSpec<T1> feature) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature);
        public TablePredicate<T, T1, T2> Features<T1, T2>(string name, IColumnSpec<T1> feature1, IColumnSpec<T2> feature2) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2);
        public TablePredicate<T, T1, T2, T3> Features<T1, T2, T3>(string name, IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3);
        public TablePredicate<T, T1, T2, T3, T4> Features<T1, T2, T3, T4>(string name, IColumnSpec<T1> feature1,
            IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4);
        public TablePredicate<T, T1, T2, T3, T4, T5> Features<T1, T2, T3, T4, T5>(string name, IColumnSpec<T1> feature1,
            IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, IColumnSpec<T5> feature5) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5);
        public TablePredicate<T, T1, T2, T3, T4, T5, T6> Features<T1, T2, T3, T4, T5, T6>(string name, IColumnSpec<T1> feature1, IColumnSpec<T2> feature2,
            IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, IColumnSpec<T5> feature5, IColumnSpec<T6> feature6) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6);
        public TablePredicate<T, T1, T2, T3, T4, T5, T6, T7> Features<T1, T2, T3, T4, T5, T6, T7>(string name, IColumnSpec<T1> feature1, IColumnSpec<T2> feature2,
            IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, IColumnSpec<T5> feature5, IColumnSpec<T6> feature6, IColumnSpec<T7> feature7) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6, feature7);

        public TablePredicate<T, T1> FeaturesMultiple<T1>(string name, IColumnSpec<T1> feature) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature);
        public TablePredicate<T, T1, T2> FeaturesMultiple<T1, T2>(string name, IColumnSpec<T1> feature1, IColumnSpec<T2> feature2) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2);
        public TablePredicate<T, T1, T2, T3> FeaturesMultiple<T1, T2, T3>(string name, IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3);
        public TablePredicate<T, T1, T2, T3, T4> FeaturesMultiple<T1, T2, T3, T4>(string name, IColumnSpec<T1> feature1,
            IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4);
        public TablePredicate<T, T1, T2, T3, T4, T5> FeaturesMultiple<T1, T2, T3, T4, T5>(string name, IColumnSpec<T1> feature1,
            IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, IColumnSpec<T5> feature5) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5);
        public TablePredicate<T, T1, T2, T3, T4, T5, T6> FeaturesMultiple<T1, T2, T3, T4, T5, T6>(string name, IColumnSpec<T1> feature1, IColumnSpec<T2> feature2,
            IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, IColumnSpec<T5> feature5, IColumnSpec<T6> feature6) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5, feature6);
        public TablePredicate<T, T1, T2, T3, T4, T5, T6, T7> FeaturesMultiple<T1, T2, T3, T4, T5, T6, T7>(string name, IColumnSpec<T1> feature1, IColumnSpec<T2> feature2,
            IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, IColumnSpec<T5> feature5, IColumnSpec<T6> feature6, IColumnSpec<T7> feature7) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5, feature6, feature7);

        #endregion

        public ExistentGoal this[Term<T> arg] => new(this, arg, true, __, __);

        public class ExistentGoal : TableGoal<T, bool, TimePoint, TimePoint> {
            public ExistentGoal(TablePredicate predicate, Term<T> arg1, Term<bool> arg2, Term<TimePoint> arg3, Term<TimePoint> arg4) 
                : base(predicate, arg1, arg2, arg3, arg4) {}

            public ExistentGoal Ended => new(TablePredicate, Arg1, false, Arg3, Arg4);
            public ExistentGoal StartedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, t, Arg4);
            public ExistentGoal EndedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, Arg3, t);
        }
    }

    public class Existent<T, T1> : TablePredicate<T, bool, TimePoint, TimePoint> {
        public readonly Event<T> Start;
        public readonly Event<T> End;
        public readonly Event<T, TimePoint> StartWith;

        private readonly Var<TimePoint> _start;

        public readonly TablePredicate<T, T1> Attributes;

        public Existent(string name, Var<T> existent, Var<TimePoint> start, IColumnSpec<T1> feature1)
            : base(name, existent.Key, exists.Indexed, start, end) {
            _start = start;
            Start = Event($"{name}Start", existent);
            End = Event($"{name}End", existent);
            StartWith = Event($"{name}StartWith", existent, _start);
            Add[existent, true, time, Eschaton].If(Start, CurrentTimePoint[time]);
            Add[existent, true, start, Eschaton].If(StartWith);
            Set(existent, end, time).If(End, CurrentTimePoint[time]);
            Set(existent, exists, false).If(End);
            Attributes = Predicate($"{name}Attributes", ((Var<T>)DefaultVariables[0]).Key, feature1);
            this.Colorize(exists, s => s ? white : gray);
        }

        #region Count table
        private TablePredicate<bool, int> _countPredicate;
        public TablePredicate<bool, int> Count {
            get {
                if (_countPredicate != null) return _countPredicate;
                _countPredicate = CountsBy($"{Name}Count", this, exists, count);
                _countPredicate.IndexByKey(exists);
                return _countPredicate;
            }
        }
        public KeyIndex<(bool, int), bool> CountIndex =>
            (KeyIndex<(bool, int), bool>)Count.IndexFor(exists, true);
        #endregion

        public Existent<T, T1> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<T>)DefaultVariables[0], true, _start, Eschaton].Where(conditions);
            return this;
        }

        public Existent<T, T1> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public Existent<T, T1> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        public Existent<T, T1> StartWithTime(params Goal[] conditions) {
            StartWith.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1> StartWithCauses(params Effect[] effects) {
            StartWith.Causes(effects);
            return this;
        }

        public TablePredicate<T, TimePoint, TimePoint> StartWithChronicle => StartWith.Chronicle;

        #region Feature Tables

        public TablePredicate<T, T01> Features<T01>(string name, IColumnSpec<T01> feature) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature);
        public TablePredicate<T, T01, T02> Features<T01, T02>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2);
        public TablePredicate<T, T01, T02, T03> Features<T01, T02, T03>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2, IColumnSpec<T03> feature3) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3);
        public TablePredicate<T, T01, T02, T03, T04> Features<T01, T02, T03, T04>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4);
        public TablePredicate<T, T01, T02, T03, T04, T05> Features<T01, T02, T03, T04, T05>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06> Features<T01, T02, T03, T04, T05, T06>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06, T07> Features<T01, T02, T03, T04, T05, T06, T07>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6, IColumnSpec<T07> feature7) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6, feature7);

        public TablePredicate<T, T01> FeaturesMultiple<T01>(string name, IColumnSpec<T01> feature) =>
                    Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature);
        public TablePredicate<T, T01, T02> FeaturesMultiple<T01, T02>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2);
        public TablePredicate<T, T01, T02, T03> FeaturesMultiple<T01, T02, T03>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2, IColumnSpec<T03> feature3) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3);
        public TablePredicate<T, T01, T02, T03, T04> FeaturesMultiple<T01, T02, T03, T04>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4);
        public TablePredicate<T, T01, T02, T03, T04, T05> FeaturesMultiple<T01, T02, T03, T04, T05>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06> FeaturesMultiple<T01, T02, T03, T04, T05, T06>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5, feature6);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06, T07> FeaturesMultiple<T01, T02, T03, T04, T05, T06, T07>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6, IColumnSpec<T07> feature7) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5, feature6, feature7);

        #endregion

        public ExistentGoal this[Term<T> arg] => new(this, arg, true, __, __);

        public class ExistentGoal : TableGoal<T, bool, TimePoint, TimePoint> {
            public ExistentGoal(TablePredicate predicate, Term<T> arg1, Term<bool> arg2, Term<TimePoint> arg3, Term<TimePoint> arg4)
                : base(predicate, arg1, arg2, arg3, arg4) { }

            public ExistentGoal Ended => new(TablePredicate, Arg1, false, Arg3, Arg4);
            public ExistentGoal StartedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, t, Arg4);
            public ExistentGoal EndedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, Arg3, t);
        }
    }

    public class Existent<T, T1, T2> : TablePredicate<T, bool, TimePoint, TimePoint> {
        public readonly Event<T> Start;
        public readonly Event<T> End;
        public readonly Event<T, TimePoint> StartWith;

        private readonly Var<TimePoint> _start;

        public readonly TablePredicate<T, T1, T2> Attributes;

        public Existent(string name, Var<T> existent, Var<TimePoint> start, IColumnSpec<T1> feature1, IColumnSpec<T2> feature2)
            : base(name, existent.Key, exists.Indexed, start, end) {
            _start = start;
            Start = Event($"{name}Start", existent);
            End = Event($"{name}End", existent);
            StartWith = Event($"{name}StartWith", existent, _start);
            Add[existent, true, time, Eschaton].If(Start, CurrentTimePoint[time]);
            Add[existent, true, start, Eschaton].If(StartWith);
            Set(existent, end, time).If(End, CurrentTimePoint[time]);
            Set(existent, exists, false).If(End);
            Attributes = Predicate($"{name}Attributes", ((Var<T>)DefaultVariables[0]).Key, feature1, feature2);
            this.Colorize(exists, s => s ? white : gray);
        }

        private TablePredicate<bool, int> _countPredicate;
        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<bool, int> Count {
            get {
                if (_countPredicate != null) return _countPredicate;
                // No using default variables on an Existent.Count table
                _countPredicate = CountsBy($"{Name}Count", this, exists, count);
                _countPredicate.IndexByKey(exists);
                return _countPredicate;
            }
        }
        public KeyIndex<(bool, int), bool> CountIndex =>
            (KeyIndex<(bool, int), bool>)Count.IndexFor(exists, true);

        public Existent<T, T1, T2> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<T>)DefaultVariables[0], true, _start, Eschaton].Where(conditions);
            return this;
        }

        public Existent<T, T1, T2> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public Existent<T, T1, T2> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        public Existent<T, T1, T2> StartWithTime(params Goal[] conditions) {
            StartWith.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2> StartWithCauses(params Effect[] effects) {
            StartWith.Causes(effects);
            return this;
        }

        public TablePredicate<T, TimePoint, TimePoint> StartWithChronicle => StartWith.Chronicle;

        #region Feature Tables

        public TablePredicate<T, T01> Features<T01>(string name, IColumnSpec<T01> feature) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature);
        public TablePredicate<T, T01, T02> Features<T01, T02>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2);
        public TablePredicate<T, T01, T02, T03> Features<T01, T02, T03>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2, IColumnSpec<T03> feature3) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3);
        public TablePredicate<T, T01, T02, T03, T04> Features<T01, T02, T03, T04>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4);
        public TablePredicate<T, T01, T02, T03, T04, T05> Features<T01, T02, T03, T04, T05>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06> Features<T01, T02, T03, T04, T05, T06>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06, T07> Features<T01, T02, T03, T04, T05, T06, T07>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6, IColumnSpec<T07> feature7) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6, feature7);

        public TablePredicate<T, T01> FeaturesMultiple<T01>(string name, IColumnSpec<T01> feature) =>
                    Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature);
        public TablePredicate<T, T01, T02> FeaturesMultiple<T01, T02>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2);
        public TablePredicate<T, T01, T02, T03> FeaturesMultiple<T01, T02, T03>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2, IColumnSpec<T03> feature3) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3);
        public TablePredicate<T, T01, T02, T03, T04> FeaturesMultiple<T01, T02, T03, T04>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4);
        public TablePredicate<T, T01, T02, T03, T04, T05> FeaturesMultiple<T01, T02, T03, T04, T05>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06> FeaturesMultiple<T01, T02, T03, T04, T05, T06>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5, feature6);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06, T07> FeaturesMultiple<T01, T02, T03, T04, T05, T06, T07>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6, IColumnSpec<T07> feature7) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5, feature6, feature7);

        #endregion

        public ExistentGoal this[Term<T> arg] => new(this, arg, true, __, __);

        public class ExistentGoal : TableGoal<T, bool, TimePoint, TimePoint> {
            public ExistentGoal(TablePredicate predicate, Term<T> arg1, Term<bool> arg2, Term<TimePoint> arg3, Term<TimePoint> arg4)
                : base(predicate, arg1, arg2, arg3, arg4) { }

            public ExistentGoal Ended => new(TablePredicate, Arg1, false, Arg3, Arg4);
            public ExistentGoal StartedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, t, Arg4);
            public ExistentGoal EndedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, Arg3, t);
        }
    }

    public class Existent<T, T1, T2, T3> : TablePredicate<T, bool, TimePoint, TimePoint> {
        public readonly Event<T> Start;
        public readonly Event<T> End;
        public readonly Event<T, TimePoint> StartWith;

        private readonly Var<TimePoint> _start;

        public readonly TablePredicate<T, T1, T2, T3> Attributes;

        public Existent(string name, Var<T> existent, Var<TimePoint> start,
                        IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3)
            : base(name, existent.Key, exists.Indexed, start, end) {
            _start = start;
            Start = Event($"{name}Start", existent);
            End = Event($"{name}End", existent);
            StartWith = Event($"{name}StartWith", existent, _start);
            Add[existent, true, time, Eschaton].If(Start, CurrentTimePoint[time]);
            Add[existent, true, start, Eschaton].If(StartWith);
            Set(existent, end, time).If(End, CurrentTimePoint[time]);
            Set(existent, exists, false).If(End);
            Attributes = Predicate($"{name}Attributes", ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3);
            this.Colorize(exists, s => s ? white : gray);
        }

        private TablePredicate<bool, int> _countPredicate;
        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<bool, int> Count {
            get {
                if (_countPredicate != null) return _countPredicate;
                // No using default variables on an Existent.Count table
                _countPredicate = CountsBy($"{Name}Count", this, exists, count);
                _countPredicate.IndexByKey(exists);
                return _countPredicate;
            }
        }
        public KeyIndex<(bool, int), bool> CountIndex =>
            (KeyIndex<(bool, int), bool>)Count.IndexFor(exists, true);

        public Existent<T, T1, T2, T3> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<T>)DefaultVariables[0], true, _start, Eschaton].Where(conditions);
            return this;
        }

        public Existent<T, T1, T2, T3> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2, T3> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public Existent<T, T1, T2, T3> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2, T3> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        public Existent<T, T1, T2, T3> StartWithTime(params Goal[] conditions) {
            StartWith.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2, T3> StartWithCauses(params Effect[] effects) {
            StartWith.Causes(effects);
            return this;
        }

        public TablePredicate<T, TimePoint, TimePoint> StartWithChronicle => StartWith.Chronicle;

        #region Feature Tables

        public TablePredicate<T, T01> Features<T01>(string name, IColumnSpec<T01> feature) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature);
        public TablePredicate<T, T01, T02> Features<T01, T02>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2);
        public TablePredicate<T, T01, T02, T03> Features<T01, T02, T03>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2, IColumnSpec<T03> feature3) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3);
        public TablePredicate<T, T01, T02, T03, T04> Features<T01, T02, T03, T04>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4);
        public TablePredicate<T, T01, T02, T03, T04, T05> Features<T01, T02, T03, T04, T05>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06> Features<T01, T02, T03, T04, T05, T06>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06, T07> Features<T01, T02, T03, T04, T05, T06, T07>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6, IColumnSpec<T07> feature7) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6, feature7);

        public TablePredicate<T, T01> FeaturesMultiple<T01>(string name, IColumnSpec<T01> feature) =>
                    Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature);
        public TablePredicate<T, T01, T02> FeaturesMultiple<T01, T02>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2);
        public TablePredicate<T, T01, T02, T03> FeaturesMultiple<T01, T02, T03>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2, IColumnSpec<T03> feature3) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3);
        public TablePredicate<T, T01, T02, T03, T04> FeaturesMultiple<T01, T02, T03, T04>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4);
        public TablePredicate<T, T01, T02, T03, T04, T05> FeaturesMultiple<T01, T02, T03, T04, T05>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06> FeaturesMultiple<T01, T02, T03, T04, T05, T06>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5, feature6);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06, T07> FeaturesMultiple<T01, T02, T03, T04, T05, T06, T07>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6, IColumnSpec<T07> feature7) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5, feature6, feature7);

        #endregion

        public ExistentGoal this[Term<T> arg] => new(this, arg, true, __, __);

        public class ExistentGoal : TableGoal<T, bool, TimePoint, TimePoint> {
            public ExistentGoal(TablePredicate predicate, Term<T> arg1, Term<bool> arg2, Term<TimePoint> arg3, Term<TimePoint> arg4)
                : base(predicate, arg1, arg2, arg3, arg4) { }

            public ExistentGoal Ended => new(TablePredicate, Arg1, false, Arg3, Arg4);
            public ExistentGoal StartedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, t, Arg4);
            public ExistentGoal EndedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, Arg3, t);
        }
    }

    public class Existent<T, T1, T2, T3, T4> : TablePredicate<T, bool, TimePoint, TimePoint> {
        public readonly Event<T> Start;
        public readonly Event<T> End;
        public readonly Event<T, TimePoint> StartWith;

        private readonly Var<TimePoint> _start;

        public readonly TablePredicate<T, T1, T2, T3, T4> Attributes;

        public Existent(string name, Var<T> existent, Var<TimePoint> start, IColumnSpec<T1> feature1, 
                        IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4)
            : base(name, existent.Key, exists.Indexed, start, end) {
            _start = start;
            Start = Event($"{name}Start", existent);
            End = Event($"{name}End", existent);
            StartWith = Event($"{name}StartWith", existent, _start);
            Add[existent, true, time, Eschaton].If(Start, CurrentTimePoint[time]);
            Add[existent, true, start, Eschaton].If(StartWith);
            Set(existent, end, time).If(End, CurrentTimePoint[time]);
            Set(existent, exists, false).If(End);
            Attributes = Predicate($"{name}Attributes", ((Var<T>)DefaultVariables[0]).Key,
                                   feature1, feature2, feature3, feature4);
            this.Colorize(exists, s => s ? white : gray);
        }

        private TablePredicate<bool, int> _countPredicate;
        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<bool, int> Count {
            get {
                if (_countPredicate != null) return _countPredicate;
                // No using default variables on an Existent.Count table
                _countPredicate = CountsBy($"{Name}Count", this, exists, count);
                _countPredicate.IndexByKey(exists);
                return _countPredicate;
            }
        }
        public KeyIndex<(bool, int), bool> CountIndex =>
            (KeyIndex<(bool, int), bool>)Count.IndexFor(exists, true);

        public Existent<T, T1, T2, T3, T4> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<T>)DefaultVariables[0], true, _start, Eschaton].Where(conditions);
            return this;
        }

        public Existent<T, T1, T2, T3, T4> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2, T3, T4> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public Existent<T, T1, T2, T3, T4> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2, T3, T4> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        public Existent<T, T1, T2, T3, T4> StartWithTime(params Goal[] conditions) {
            StartWith.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2, T3, T4> StartWithCauses(params Effect[] effects) {
            StartWith.Causes(effects);
            return this;
        }

        public TablePredicate<T, TimePoint, TimePoint> StartWithChronicle => StartWith.Chronicle;

        #region Feature Tables

        public TablePredicate<T, T01> Features<T01>(string name, IColumnSpec<T01> feature) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature);
        public TablePredicate<T, T01, T02> Features<T01, T02>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2);
        public TablePredicate<T, T01, T02, T03> Features<T01, T02, T03>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2, IColumnSpec<T03> feature3) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3);
        public TablePredicate<T, T01, T02, T03, T04> Features<T01, T02, T03, T04>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4);
        public TablePredicate<T, T01, T02, T03, T04, T05> Features<T01, T02, T03, T04, T05>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06> Features<T01, T02, T03, T04, T05, T06>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06, T07> Features<T01, T02, T03, T04, T05, T06, T07>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6, IColumnSpec<T07> feature7) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6, feature7);

        public TablePredicate<T, T01> FeaturesMultiple<T01>(string name, IColumnSpec<T01> feature) =>
                    Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature);
        public TablePredicate<T, T01, T02> FeaturesMultiple<T01, T02>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2);
        public TablePredicate<T, T01, T02, T03> FeaturesMultiple<T01, T02, T03>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2, IColumnSpec<T03> feature3) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3);
        public TablePredicate<T, T01, T02, T03, T04> FeaturesMultiple<T01, T02, T03, T04>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4);
        public TablePredicate<T, T01, T02, T03, T04, T05> FeaturesMultiple<T01, T02, T03, T04, T05>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06> FeaturesMultiple<T01, T02, T03, T04, T05, T06>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5, feature6);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06, T07> FeaturesMultiple<T01, T02, T03, T04, T05, T06, T07>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6, IColumnSpec<T07> feature7) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5, feature6, feature7);

        #endregion

        public ExistentGoal this[Term<T> arg] => new(this, arg, true, __, __);

        public class ExistentGoal : TableGoal<T, bool, TimePoint, TimePoint> {
            public ExistentGoal(TablePredicate predicate, Term<T> arg1, Term<bool> arg2, Term<TimePoint> arg3, Term<TimePoint> arg4)
                : base(predicate, arg1, arg2, arg3, arg4) { }

            public ExistentGoal Ended => new(TablePredicate, Arg1, false, Arg3, Arg4);
            public ExistentGoal StartedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, t, Arg4);
            public ExistentGoal EndedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, Arg3, t);
        }
    }

    public class Existent<T, T1, T2, T3, T4, T5> : TablePredicate<T, bool, TimePoint, TimePoint> {
        public readonly Event<T> Start;
        public readonly Event<T> End;
        public readonly Event<T, TimePoint> StartWith;

        private readonly Var<TimePoint> _start;

        public readonly TablePredicate<T, T1, T2, T3, T4, T5> Attributes;

        public Existent(string name, Var<T> existent, Var<TimePoint> start, IColumnSpec<T1> feature1,
                        IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4,
                        IColumnSpec<T5> feature5)
            : base(name, existent.Key, exists.Indexed, start, end) {
            _start = start;
            Start = Event($"{name}Start", existent);
            End = Event($"{name}End", existent);
            StartWith = Event($"{name}StartWith", existent, _start);
            Add[existent, true, time, Eschaton].If(Start, CurrentTimePoint[time]);
            Add[existent, true, start, Eschaton].If(StartWith);
            Set(existent, end, time).If(End, CurrentTimePoint[time]);
            Set(existent, exists, false).If(End);
            Attributes = Predicate($"{name}Attributes", ((Var<T>)DefaultVariables[0]).Key,
                                   feature1, feature2, feature3, feature4, feature5);
            this.Colorize(exists, s => s ? white : gray);
        }

        private TablePredicate<bool, int> _countPredicate;
        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<bool, int> Count {
            get {
                if (_countPredicate != null) return _countPredicate;
                // No using default variables on an Existent.Count table
                _countPredicate = CountsBy($"{Name}Count", this, exists, count);
                _countPredicate.IndexByKey(exists);
                return _countPredicate;
            }
        }
        public KeyIndex<(bool, int), bool> CountIndex =>
            (KeyIndex<(bool, int), bool>)Count.IndexFor(exists, true);

        public Existent<T, T1, T2, T3, T4, T5> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<T>)DefaultVariables[0], true, _start, Eschaton].Where(conditions);
            return this;
        }

        public Existent<T, T1, T2, T3, T4, T5> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2, T3, T4, T5> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public Existent<T, T1, T2, T3, T4, T5> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2, T3, T4, T5> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        public Existent<T, T1, T2, T3, T4, T5> StartWithTime(params Goal[] conditions) {
            StartWith.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2, T3, T4, T5> StartWithCauses(params Effect[] effects) {
            StartWith.Causes(effects);
            return this;
        }

        public TablePredicate<T, TimePoint, TimePoint> StartWithChronicle => StartWith.Chronicle;

        #region Feature Tables

        public TablePredicate<T, T01> Features<T01>(string name, IColumnSpec<T01> feature) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature);
        public TablePredicate<T, T01, T02> Features<T01, T02>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2);
        public TablePredicate<T, T01, T02, T03> Features<T01, T02, T03>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2, IColumnSpec<T03> feature3) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3);
        public TablePredicate<T, T01, T02, T03, T04> Features<T01, T02, T03, T04>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4);
        public TablePredicate<T, T01, T02, T03, T04, T05> Features<T01, T02, T03, T04, T05>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06> Features<T01, T02, T03, T04, T05, T06>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06, T07> Features<T01, T02, T03, T04, T05, T06, T07>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6, IColumnSpec<T07> feature7) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6, feature7);

        public TablePredicate<T, T01> FeaturesMultiple<T01>(string name, IColumnSpec<T01> feature) =>
                    Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature);
        public TablePredicate<T, T01, T02> FeaturesMultiple<T01, T02>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2);
        public TablePredicate<T, T01, T02, T03> FeaturesMultiple<T01, T02, T03>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2, IColumnSpec<T03> feature3) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3);
        public TablePredicate<T, T01, T02, T03, T04> FeaturesMultiple<T01, T02, T03, T04>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4);
        public TablePredicate<T, T01, T02, T03, T04, T05> FeaturesMultiple<T01, T02, T03, T04, T05>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06> FeaturesMultiple<T01, T02, T03, T04, T05, T06>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5, feature6);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06, T07> FeaturesMultiple<T01, T02, T03, T04, T05, T06, T07>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6, IColumnSpec<T07> feature7) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5, feature6, feature7);

        #endregion

        public ExistentGoal this[Term<T> arg] => new(this, arg, true, __, __);

        public class ExistentGoal : TableGoal<T, bool, TimePoint, TimePoint> {
            public ExistentGoal(TablePredicate predicate, Term<T> arg1, Term<bool> arg2, Term<TimePoint> arg3, Term<TimePoint> arg4)
                : base(predicate, arg1, arg2, arg3, arg4) { }

            public ExistentGoal Ended => new(TablePredicate, Arg1, false, Arg3, Arg4);
            public ExistentGoal StartedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, t, Arg4);
            public ExistentGoal EndedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, Arg3, t);
        }
    }

    public class Existent<T, T1, T2, T3, T4, T5, T6> : TablePredicate<T, bool, TimePoint, TimePoint> {
        public readonly Event<T> Start;
        public readonly Event<T> End;
        public readonly Event<T, TimePoint> StartWith;

        private readonly Var<TimePoint> _start;

        public readonly TablePredicate<T, T1, T2, T3, T4, T5, T6> Attributes;

        public Existent(string name, Var<T> existent, Var<TimePoint> start, IColumnSpec<T1> feature1, 
                        IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4,
                        IColumnSpec<T5> feature5, IColumnSpec<T6> feature6)
            : base(name, existent.Key, exists.Indexed, start, end) {
            _start = start;
            Start = Event($"{name}Start", existent);
            End = Event($"{name}End", existent);
            StartWith = Event($"{name}StartWith", existent, _start);
            Add[existent, true, time, Eschaton].If(Start, CurrentTimePoint[time]);
            Add[existent, true, start, Eschaton].If(StartWith);
            Set(existent, end, time).If(End, CurrentTimePoint[time]);
            Set(existent, exists, false).If(End);
            Attributes = Predicate($"{name}Attributes", ((Var<T>)DefaultVariables[0]).Key,
                                   feature1, feature2, feature3, feature4, feature5, feature6);
            this.Colorize(exists, s => s ? white : gray);
        }

        private TablePredicate<bool, int> _countPredicate;
        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<bool, int> Count {
            get {
                if (_countPredicate != null) return _countPredicate;
                // No using default variables on an Existent.Count table
                _countPredicate = CountsBy($"{Name}Count", this, exists, count);
                _countPredicate.IndexByKey(exists);
                return _countPredicate;
            }
        }
        public KeyIndex<(bool, int), bool> CountIndex =>
            (KeyIndex<(bool, int), bool>)Count.IndexFor(exists, true);

        public Existent<T, T1, T2, T3, T4, T5, T6> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<T>)DefaultVariables[0], true, _start, Eschaton].Where(conditions);
            return this;
        }

        public Existent<T, T1, T2, T3, T4, T5, T6> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2, T3, T4, T5, T6> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public Existent<T, T1, T2, T3, T4, T5, T6> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2, T3, T4, T5, T6> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        public Existent<T, T1, T2, T3, T4, T5, T6> StartWithTime(params Goal[] conditions) {
            StartWith.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2, T3, T4, T5, T6> StartWithCauses(params Effect[] effects) {
            StartWith.Causes(effects);
            return this;
        }

        public TablePredicate<T, TimePoint, TimePoint> StartWithChronicle => StartWith.Chronicle;

        #region Feature Tables

        public TablePredicate<T, T01> Features<T01>(string name, IColumnSpec<T01> feature) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature);
        public TablePredicate<T, T01, T02> Features<T01, T02>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2);
        public TablePredicate<T, T01, T02, T03> Features<T01, T02, T03>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2, IColumnSpec<T03> feature3) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3);
        public TablePredicate<T, T01, T02, T03, T04> Features<T01, T02, T03, T04>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4);
        public TablePredicate<T, T01, T02, T03, T04, T05> Features<T01, T02, T03, T04, T05>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06> Features<T01, T02, T03, T04, T05, T06>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06, T07> Features<T01, T02, T03, T04, T05, T06, T07>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6, IColumnSpec<T07> feature7) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6, feature7);

        public TablePredicate<T, T01> FeaturesMultiple<T01>(string name, IColumnSpec<T01> feature) =>
                    Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature);
        public TablePredicate<T, T01, T02> FeaturesMultiple<T01, T02>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2);
        public TablePredicate<T, T01, T02, T03> FeaturesMultiple<T01, T02, T03>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2, IColumnSpec<T03> feature3) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3);
        public TablePredicate<T, T01, T02, T03, T04> FeaturesMultiple<T01, T02, T03, T04>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4);
        public TablePredicate<T, T01, T02, T03, T04, T05> FeaturesMultiple<T01, T02, T03, T04, T05>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06> FeaturesMultiple<T01, T02, T03, T04, T05, T06>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5, feature6);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06, T07> FeaturesMultiple<T01, T02, T03, T04, T05, T06, T07>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6, IColumnSpec<T07> feature7) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5, feature6, feature7);

        #endregion

        public ExistentGoal this[Term<T> arg] => new(this, arg, true, __, __);

        public class ExistentGoal : TableGoal<T, bool, TimePoint, TimePoint> {
            public ExistentGoal(TablePredicate predicate, Term<T> arg1, Term<bool> arg2, Term<TimePoint> arg3, Term<TimePoint> arg4)
                : base(predicate, arg1, arg2, arg3, arg4) { }

            public ExistentGoal Ended => new(TablePredicate, Arg1, false, Arg3, Arg4);
            public ExistentGoal StartedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, t, Arg4);
            public ExistentGoal EndedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, Arg3, t);
        }
    }

    public class Existent<T, T1, T2, T3, T4, T5, T6, T7> : TablePredicate<T, bool, TimePoint, TimePoint> {
        public readonly Event<T> Start;
        public readonly Event<T> End;
        public readonly Event<T, TimePoint> StartWith;

        private readonly Var<TimePoint> _start;

        public readonly TablePredicate<T, T1, T2, T3, T4, T5, T6, T7> Attributes;

        public Existent(string name, Var<T> existent, Var<TimePoint> start, IColumnSpec<T1> feature1, 
                        IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, 
                        IColumnSpec<T5> feature5, IColumnSpec<T6> feature6, IColumnSpec<T7> feature7)
            : base(name, existent.Key, exists.Indexed, start, end) {
            _start = start;
            Start = Event($"{name}Start", existent);
            End = Event($"{name}End", existent);
            StartWith = Event($"{name}StartWith", existent, _start);
            Add[existent, true, time, Eschaton].If(Start, CurrentTimePoint[time]);
            Add[existent, true, start, Eschaton].If(StartWith);
            Set(existent, end, time).If(End, CurrentTimePoint[time]);
            Set(existent, exists, false).If(End);
            Attributes = Predicate($"{name}Attributes", ((Var<T>)DefaultVariables[0]).Key, 
                                   feature1, feature2, feature3, feature4, feature5, feature6, feature7);
            this.Colorize(exists, s => s ? white : gray);
        }

        private TablePredicate<bool, int> _countPredicate;
        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<bool, int> Count {
            get {
                if (_countPredicate != null) return _countPredicate;
                // No using default variables on an Existent.Count table
                _countPredicate = CountsBy($"{Name}Count", this, exists, count);
                _countPredicate.IndexByKey(exists);
                return _countPredicate;
            }
        }
        public KeyIndex<(bool, int), bool> CountIndex =>
            (KeyIndex<(bool, int), bool>)Count.IndexFor(exists, true);

        public Existent<T, T1, T2, T3, T4, T5, T6, T7> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<T>)DefaultVariables[0], true, _start, Eschaton].Where(conditions);
            return this;
        }

        public Existent<T, T1, T2, T3, T4, T5, T6, T7> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2, T3, T4, T5, T6, T7> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public Existent<T, T1, T2, T3, T4, T5, T6, T7> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2, T3, T4, T5, T6, T7> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        public Existent<T, T1, T2, T3, T4, T5, T6, T7> StartWithTime(params Goal[] conditions) {
            StartWith.OccursWhen(conditions);
            return this;
        }
        public Existent<T, T1, T2, T3, T4, T5, T6, T7> StartWithCauses(params Effect[] effects) {
            StartWith.Causes(effects);
            return this;
        }

        public TablePredicate<T, TimePoint, TimePoint> StartWithChronicle => StartWith.Chronicle;

        #region Feature Tables

        public TablePredicate<T, T01> Features<T01>(string name, IColumnSpec<T01> feature) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature);
        public TablePredicate<T, T01, T02> Features<T01, T02>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2);
        public TablePredicate<T, T01, T02, T03> Features<T01, T02, T03>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2, IColumnSpec<T03> feature3) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3);
        public TablePredicate<T, T01, T02, T03, T04> Features<T01, T02, T03, T04>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4);
        public TablePredicate<T, T01, T02, T03, T04, T05> Features<T01, T02, T03, T04, T05>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06> Features<T01, T02, T03, T04, T05, T06>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06, T07> Features<T01, T02, T03, T04, T05, T06, T07>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6, IColumnSpec<T07> feature7) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6, feature7);

        public TablePredicate<T, T01> FeaturesMultiple<T01>(string name, IColumnSpec<T01> feature) =>
                    Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature);
        public TablePredicate<T, T01, T02> FeaturesMultiple<T01, T02>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2);
        public TablePredicate<T, T01, T02, T03> FeaturesMultiple<T01, T02, T03>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2, IColumnSpec<T03> feature3) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3);
        public TablePredicate<T, T01, T02, T03, T04> FeaturesMultiple<T01, T02, T03, T04>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4);
        public TablePredicate<T, T01, T02, T03, T04, T05> FeaturesMultiple<T01, T02, T03, T04, T05>(string name, IColumnSpec<T01> feature1,
            IColumnSpec<T02> feature2, IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06> FeaturesMultiple<T01, T02, T03, T04, T05, T06>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5, feature6);
        public TablePredicate<T, T01, T02, T03, T04, T05, T06, T07> FeaturesMultiple<T01, T02, T03, T04, T05, T06, T07>(string name, IColumnSpec<T01> feature1, IColumnSpec<T02> feature2,
            IColumnSpec<T03> feature3, IColumnSpec<T04> feature4, IColumnSpec<T05> feature5, IColumnSpec<T06> feature6, IColumnSpec<T07> feature7) =>
            Predicate(name, ((Var<T>)DefaultVariables[0]).Indexed, feature1, feature2, feature3, feature4, feature5, feature6, feature7);

        #endregion

        public ExistentGoal this[Term<T> arg] => new(this, arg, true, __, __);

        public class ExistentGoal : TableGoal<T, bool, TimePoint, TimePoint> {
            public ExistentGoal(TablePredicate predicate, Term<T> arg1, Term<bool> arg2, Term<TimePoint> arg3, Term<TimePoint> arg4)
                : base(predicate, arg1, arg2, arg3, arg4) { }

            public ExistentGoal Ended => new(TablePredicate, Arg1, false, Arg3, Arg4);
            public ExistentGoal StartedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, t, Arg4);
            public ExistentGoal EndedAt(Term<TimePoint> t) => new(TablePredicate, Arg1, __, Arg3, t);
        }
    }
}
