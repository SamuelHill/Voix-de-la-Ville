using System;
using TED;
using TED.Interpreter;
using TED.Tables;
using static TED.Language;

namespace TotT.Simulog {
    public class GenericExistent<T, TTimePoint, TEvent, TEventAtTime> : TablePredicate<T, bool, TTimePoint, TTimePoint> 
        where TEvent : GenericEvent<T, TTimePoint> where TEventAtTime : GenericEvent<T, TTimePoint, TTimePoint> {
        // ReSharper disable StaticMemberInGenericType
        // ReSharper disable InconsistentNaming
        internal static readonly Var<bool> _exists = (Var<bool>)"exists";
        private static readonly Var<TTimePoint> _end = (Var<TTimePoint>)"end";
        private static readonly Var<TTimePoint> _time = (Var<TTimePoint>)"time";
        private static readonly Var<int> _count = (Var<int>)"count";
        // ReSharper restore InconsistentNaming
        // ReSharper restore StaticMemberInGenericType

        private readonly TTimePoint _eschaton;
        
        // ReSharper disable MemberCanBePrivate.Global
        public readonly TEvent Start;
        public readonly TEvent End;
        public readonly TEventAtTime StartWith;
        // ReSharper restore MemberCanBePrivate.Global
        private readonly Var<TTimePoint> _start;

        protected GenericExistent(string name, Var<T> existent, Var<TTimePoint> start, TTimePoint eschaton, Function<TTimePoint> currentTime, 
                                  Func<string, Var<T>, TEvent> newEvent, Func<string, Var<T>, Var<TTimePoint>, TEventAtTime> newEventAtTime) 
            : base(name, existent.Key, _exists.Indexed, start, _end) {
            _start = start;
            _eschaton = eschaton;
            Start = newEvent($"{name}Start", existent);
            End = newEvent($"{name}End", existent);
            StartWith = newEventAtTime($"{name}StartWith", existent, _start);
            Add[existent, true, _time, _eschaton].If(Start, currentTime[_time]);
            Add[existent, true, start, _eschaton].If(StartWith);
            Set(existent, _end, _time).If(End, currentTime[_time]);
            Set(existent, _exists, false).If(End);
        }

        private TablePredicate<bool, int> _countPredicate;
        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<bool, int> Count {
            get {
                if (_countPredicate != null) return _countPredicate;
                // No using default variables on an Existent.Count table
                _countPredicate = CountsBy($"{Name}Count", this, _exists, _count);
                _countPredicate.IndexByKey(_exists);
                return _countPredicate;
            }
        }
        public KeyIndex<(bool, int), bool> CountIndex =>
            (KeyIndex<(bool, int), bool>)Count.IndexFor(_exists, true);

        public GenericExistent<T, TTimePoint, TEvent, TEventAtTime> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<T>)DefaultVariables[0], true, _start, _eschaton].Where(conditions);
            return this;
        }

        public GenericExistent<T, TTimePoint, TEvent, TEventAtTime> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, TTimePoint, TEvent, TEventAtTime> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public GenericExistent<T, TTimePoint, TEvent, TEventAtTime> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, TTimePoint, TEvent, TEventAtTime> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }
        
        public GenericExistent<T, TTimePoint, TEvent, TEventAtTime> StartWithTime(params Goal[] conditions) {
            StartWith.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, TTimePoint, TEvent, TEventAtTime> StartWithCauses(params Effect[] effects) {
            StartWith.Causes(effects);
            return this;
        }
        
        public TablePredicate<T, TTimePoint, TTimePoint> StartWithChronicle => StartWith.Chronicle;

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

        public class ExistentGoal : TableGoal<T, bool, TTimePoint, TTimePoint> {
            public ExistentGoal(TablePredicate predicate, Term<T> arg1, Term<bool> arg2, Term<TTimePoint> arg3, Term<TTimePoint> arg4) 
                : base(predicate, arg1, arg2, arg3, arg4) {}

            public ExistentGoal Ended => new(TablePredicate, Arg1, false, Arg3, Arg4);
            public ExistentGoal StartedAt(Term<TTimePoint> t) => new(TablePredicate, Arg1, __, t, Arg4);
            public ExistentGoal EndedAt(Term<TTimePoint> t) => new(TablePredicate, Arg1, __, Arg3, t);
        }
    }

    public class GenericExistent<T, T1, TTimePoint, TEvent, TEventAtTime> : TablePredicate<T, bool, TTimePoint, TTimePoint>
        where TEvent : GenericEvent<T, TTimePoint> where TEventAtTime : GenericEvent<T, TTimePoint, TTimePoint> {
        // ReSharper disable StaticMemberInGenericType
        // ReSharper disable InconsistentNaming
        internal static readonly Var<bool> _exists = (Var<bool>)"exists";
        private static readonly Var<TTimePoint> _end = (Var<TTimePoint>)"end";
        private static readonly Var<TTimePoint> _time = (Var<TTimePoint>)"time";
        private static readonly Var<int> _count = (Var<int>)"count";
        // ReSharper restore InconsistentNaming
        // ReSharper restore StaticMemberInGenericType

        private readonly TTimePoint _eschaton;

        // ReSharper disable MemberCanBePrivate.Global
        public readonly TEvent Start;
        public readonly TEvent End;
        public readonly TEventAtTime StartWith;
        // ReSharper restore MemberCanBePrivate.Global
        private readonly Var<TTimePoint> _start;

        public readonly TablePredicate<T, T1> Attributes;

        protected GenericExistent(string name, Var<T> existent, Var<TTimePoint> start, TTimePoint eschaton, Function<TTimePoint> currentTime,
                                  Func<string, Var<T>, TEvent> newEvent, Func<string, Var<T>, Var<TTimePoint>, TEventAtTime> newEventAtTime,
                                  IColumnSpec<T1> feature1)
            : base(name, existent.Key, _exists.Indexed, start, _end) {
            _start = start;
            _eschaton = eschaton;
            Start = newEvent($"{name}Start", existent);
            End = newEvent($"{name}End", existent);
            StartWith = newEventAtTime($"{name}StartWith", existent, _start);
            Add[existent, true, _time, _eschaton].If(Start, currentTime[_time]);
            Add[existent, true, start, _eschaton].If(StartWith);
            Set(existent, _end, _time).If(End, currentTime[_time]);
            Set(existent, _exists, false).If(End);
            Attributes = Predicate($"{name}Attributes", ((Var<T>)DefaultVariables[0]).Key, feature1);
        }

        private TablePredicate<bool, int> _countPredicate;
        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<bool, int> Count {
            get {
                if (_countPredicate != null) return _countPredicate;
                // No using default variables on an Existent.Count table
                _countPredicate = CountsBy($"{Name}Count", this, _exists, _count);
                _countPredicate.IndexByKey(_exists);
                return _countPredicate;
            }
        }
        public KeyIndex<(bool, int), bool> CountIndex =>
            (KeyIndex<(bool, int), bool>)Count.IndexFor(_exists, true);

        public GenericExistent<T, T1, TTimePoint, TEvent, TEventAtTime> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<T>)DefaultVariables[0], true, _start, _eschaton].Where(conditions);
            return this;
        }

        public GenericExistent<T, T1, TTimePoint, TEvent, TEventAtTime> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, TTimePoint, TEvent, TEventAtTime> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public GenericExistent<T, T1, TTimePoint, TEvent, TEventAtTime> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, TTimePoint, TEvent, TEventAtTime> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        public GenericExistent<T, T1, TTimePoint, TEvent, TEventAtTime> StartWithTime(params Goal[] conditions) {
            StartWith.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, TTimePoint, TEvent, TEventAtTime> StartWithCauses(params Effect[] effects) {
            StartWith.Causes(effects);
            return this;
        }

        public TablePredicate<T, TTimePoint, TTimePoint> StartWithChronicle => StartWith.Chronicle;

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

        public class ExistentGoal : TableGoal<T, bool, TTimePoint, TTimePoint> {
            public ExistentGoal(TablePredicate predicate, Term<T> arg1, Term<bool> arg2, Term<TTimePoint> arg3, Term<TTimePoint> arg4)
                : base(predicate, arg1, arg2, arg3, arg4) { }

            public ExistentGoal Ended => new(TablePredicate, Arg1, false, Arg3, Arg4);
            public ExistentGoal StartedAt(Term<TTimePoint> t) => new(TablePredicate, Arg1, __, t, Arg4);
            public ExistentGoal EndedAt(Term<TTimePoint> t) => new(TablePredicate, Arg1, __, Arg3, t);
        }
    }

    public class GenericExistent<T, T1, T2, TTimePoint, TEvent, TEventAtTime> : TablePredicate<T, bool, TTimePoint, TTimePoint>
        where TEvent : GenericEvent<T, TTimePoint> where TEventAtTime : GenericEvent<T, TTimePoint, TTimePoint> {
        // ReSharper disable StaticMemberInGenericType
        // ReSharper disable InconsistentNaming
        internal static readonly Var<bool> _exists = (Var<bool>)"exists";
        private static readonly Var<TTimePoint> _end = (Var<TTimePoint>)"end";
        private static readonly Var<TTimePoint> _time = (Var<TTimePoint>)"time";
        private static readonly Var<int> _count = (Var<int>)"count";
        // ReSharper restore InconsistentNaming
        // ReSharper restore StaticMemberInGenericType

        private readonly TTimePoint _eschaton;

        // ReSharper disable MemberCanBePrivate.Global
        public readonly TEvent Start;
        public readonly TEvent End;
        public readonly TEventAtTime StartWith;
        // ReSharper restore MemberCanBePrivate.Global
        private readonly Var<TTimePoint> _start;

        public readonly TablePredicate<T, T1, T2> Attributes;

        protected GenericExistent(string name, Var<T> existent, Var<TTimePoint> start, TTimePoint eschaton, Function<TTimePoint> currentTime,
                                  Func<string, Var<T>, TEvent> newEvent, Func<string, Var<T>, Var<TTimePoint>, TEventAtTime> newEventAtTime,
                                  IColumnSpec<T1> feature1, IColumnSpec<T2> feature2)
            : base(name, existent.Key, _exists.Indexed, start, _end) {
            _start = start;
            _eschaton = eschaton;
            Start = newEvent($"{name}Start", existent);
            End = newEvent($"{name}End", existent);
            StartWith = newEventAtTime($"{name}StartWith", existent, _start);
            Add[existent, true, _time, _eschaton].If(Start, currentTime[_time]);
            Add[existent, true, start, _eschaton].If(StartWith);
            Set(existent, _end, _time).If(End, currentTime[_time]);
            Set(existent, _exists, false).If(End);
            Attributes = Predicate($"{name}Attributes", ((Var<T>)DefaultVariables[0]).Key, feature1, feature2);
        }

        private TablePredicate<bool, int> _countPredicate;
        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<bool, int> Count {
            get {
                if (_countPredicate != null) return _countPredicate;
                // No using default variables on an Existent.Count table
                _countPredicate = CountsBy($"{Name}Count", this, _exists, _count);
                _countPredicate.IndexByKey(_exists);
                return _countPredicate;
            }
        }
        public KeyIndex<(bool, int), bool> CountIndex =>
            (KeyIndex<(bool, int), bool>)Count.IndexFor(_exists, true);

        public GenericExistent<T, T1, T2, TTimePoint, TEvent, TEventAtTime> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<T>)DefaultVariables[0], true, _start, _eschaton].Where(conditions);
            return this;
        }

        public GenericExistent<T, T1, T2, TTimePoint, TEvent, TEventAtTime> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, TTimePoint, TEvent, TEventAtTime> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public GenericExistent<T, T1, T2, TTimePoint, TEvent, TEventAtTime> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, TTimePoint, TEvent, TEventAtTime> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        public GenericExistent<T, T1, T2, TTimePoint, TEvent, TEventAtTime> StartWithTime(params Goal[] conditions) {
            StartWith.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, TTimePoint, TEvent, TEventAtTime> StartWithCauses(params Effect[] effects) {
            StartWith.Causes(effects);
            return this;
        }

        public TablePredicate<T, TTimePoint, TTimePoint> StartWithChronicle => StartWith.Chronicle;

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

        public class ExistentGoal : TableGoal<T, bool, TTimePoint, TTimePoint> {
            public ExistentGoal(TablePredicate predicate, Term<T> arg1, Term<bool> arg2, Term<TTimePoint> arg3, Term<TTimePoint> arg4)
                : base(predicate, arg1, arg2, arg3, arg4) { }

            public ExistentGoal Ended => new(TablePredicate, Arg1, false, Arg3, Arg4);
            public ExistentGoal StartedAt(Term<TTimePoint> t) => new(TablePredicate, Arg1, __, t, Arg4);
            public ExistentGoal EndedAt(Term<TTimePoint> t) => new(TablePredicate, Arg1, __, Arg3, t);
        }
    }

    public class GenericExistent<T, T1, T2, T3, TTimePoint, TEvent, TEventAtTime> : TablePredicate<T, bool, TTimePoint, TTimePoint>
        where TEvent : GenericEvent<T, TTimePoint> where TEventAtTime : GenericEvent<T, TTimePoint, TTimePoint> {
        // ReSharper disable StaticMemberInGenericType
        // ReSharper disable InconsistentNaming
        internal static readonly Var<bool> _exists = (Var<bool>)"exists";
        private static readonly Var<TTimePoint> _end = (Var<TTimePoint>)"end";
        private static readonly Var<TTimePoint> _time = (Var<TTimePoint>)"time";
        private static readonly Var<int> _count = (Var<int>)"count";
        // ReSharper restore InconsistentNaming
        // ReSharper restore StaticMemberInGenericType

        private readonly TTimePoint _eschaton;

        // ReSharper disable MemberCanBePrivate.Global
        public readonly TEvent Start;
        public readonly TEvent End;
        public readonly TEventAtTime StartWith;
        // ReSharper restore MemberCanBePrivate.Global
        private readonly Var<TTimePoint> _start;

        public readonly TablePredicate<T, T1, T2, T3> Attributes;

        protected GenericExistent(string name, Var<T> existent, Var<TTimePoint> start, TTimePoint eschaton, Function<TTimePoint> currentTime,
                                  Func<string, Var<T>, TEvent> newEvent, Func<string, Var<T>, Var<TTimePoint>, TEventAtTime> newEventAtTime,
                                  IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3)
            : base(name, existent.Key, _exists.Indexed, start, _end) {
            _start = start;
            _eschaton = eschaton;
            Start = newEvent($"{name}Start", existent);
            End = newEvent($"{name}End", existent);
            StartWith = newEventAtTime($"{name}StartWith", existent, _start);
            Add[existent, true, _time, _eschaton].If(Start, currentTime[_time]);
            Add[existent, true, start, _eschaton].If(StartWith);
            Set(existent, _end, _time).If(End, currentTime[_time]);
            Set(existent, _exists, false).If(End);
            Attributes = Predicate($"{name}Attributes", ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3);
        }

        private TablePredicate<bool, int> _countPredicate;
        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<bool, int> Count {
            get {
                if (_countPredicate != null) return _countPredicate;
                // No using default variables on an Existent.Count table
                _countPredicate = CountsBy($"{Name}Count", this, _exists, _count);
                _countPredicate.IndexByKey(_exists);
                return _countPredicate;
            }
        }
        public KeyIndex<(bool, int), bool> CountIndex =>
            (KeyIndex<(bool, int), bool>)Count.IndexFor(_exists, true);

        public GenericExistent<T, T1, T2, T3, TTimePoint, TEvent, TEventAtTime> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<T>)DefaultVariables[0], true, _start, _eschaton].Where(conditions);
            return this;
        }

        public GenericExistent<T, T1, T2, T3, TTimePoint, TEvent, TEventAtTime> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, T3, TTimePoint, TEvent, TEventAtTime> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public GenericExistent<T, T1, T2, T3, TTimePoint, TEvent, TEventAtTime> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, T3, TTimePoint, TEvent, TEventAtTime> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        public GenericExistent<T, T1, T2, T3, TTimePoint, TEvent, TEventAtTime> StartWithTime(params Goal[] conditions) {
            StartWith.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, T3, TTimePoint, TEvent, TEventAtTime> StartWithCauses(params Effect[] effects) {
            StartWith.Causes(effects);
            return this;
        }

        public TablePredicate<T, TTimePoint, TTimePoint> StartWithChronicle => StartWith.Chronicle;

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

        public class ExistentGoal : TableGoal<T, bool, TTimePoint, TTimePoint> {
            public ExistentGoal(TablePredicate predicate, Term<T> arg1, Term<bool> arg2, Term<TTimePoint> arg3, Term<TTimePoint> arg4)
                : base(predicate, arg1, arg2, arg3, arg4) { }

            public ExistentGoal Ended => new(TablePredicate, Arg1, false, Arg3, Arg4);
            public ExistentGoal StartedAt(Term<TTimePoint> t) => new(TablePredicate, Arg1, __, t, Arg4);
            public ExistentGoal EndedAt(Term<TTimePoint> t) => new(TablePredicate, Arg1, __, Arg3, t);
        }
    }

    public class GenericExistent<T, T1, T2, T3, T4, TTimePoint, TEvent, TEventAtTime> : TablePredicate<T, bool, TTimePoint, TTimePoint>
        where TEvent : GenericEvent<T, TTimePoint> where TEventAtTime : GenericEvent<T, TTimePoint, TTimePoint> {
        // ReSharper disable StaticMemberInGenericType
        // ReSharper disable InconsistentNaming
        internal static readonly Var<bool> _exists = (Var<bool>)"exists";
        private static readonly Var<TTimePoint> _end = (Var<TTimePoint>)"end";
        private static readonly Var<TTimePoint> _time = (Var<TTimePoint>)"time";
        private static readonly Var<int> _count = (Var<int>)"count";
        // ReSharper restore InconsistentNaming
        // ReSharper restore StaticMemberInGenericType

        private readonly TTimePoint _eschaton;

        // ReSharper disable MemberCanBePrivate.Global
        public readonly TEvent Start;
        public readonly TEvent End;
        public readonly TEventAtTime StartWith;
        // ReSharper restore MemberCanBePrivate.Global
        private readonly Var<TTimePoint> _start;

        public readonly TablePredicate<T, T1, T2, T3, T4> Attributes;

        protected GenericExistent(string name, Var<T> existent, Var<TTimePoint> start, TTimePoint eschaton, Function<TTimePoint> currentTime,
                                  Func<string, Var<T>, TEvent> newEvent, Func<string, Var<T>, Var<TTimePoint>, TEventAtTime> newEventAtTime,
                                  IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4)
            : base(name, existent.Key, _exists.Indexed, start, _end) {
            _start = start;
            _eschaton = eschaton;
            Start = newEvent($"{name}Start", existent);
            End = newEvent($"{name}End", existent);
            StartWith = newEventAtTime($"{name}StartWith", existent, _start);
            Add[existent, true, _time, _eschaton].If(Start, currentTime[_time]);
            Add[existent, true, start, _eschaton].If(StartWith);
            Set(existent, _end, _time).If(End, currentTime[_time]);
            Set(existent, _exists, false).If(End);
            Attributes = Predicate($"{name}Attributes", ((Var<T>)DefaultVariables[0]).Key,
                                   feature1, feature2, feature3, feature4);
        }

        private TablePredicate<bool, int> _countPredicate;
        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<bool, int> Count {
            get {
                if (_countPredicate != null) return _countPredicate;
                // No using default variables on an Existent.Count table
                _countPredicate = CountsBy($"{Name}Count", this, _exists, _count);
                _countPredicate.IndexByKey(_exists);
                return _countPredicate;
            }
        }
        public KeyIndex<(bool, int), bool> CountIndex =>
            (KeyIndex<(bool, int), bool>)Count.IndexFor(_exists, true);

        public GenericExistent<T, T1, T2, T3, T4, TTimePoint, TEvent, TEventAtTime> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<T>)DefaultVariables[0], true, _start, _eschaton].Where(conditions);
            return this;
        }

        public GenericExistent<T, T1, T2, T3, T4, TTimePoint, TEvent, TEventAtTime> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, T3, T4, TTimePoint, TEvent, TEventAtTime> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public GenericExistent<T, T1, T2, T3, T4, TTimePoint, TEvent, TEventAtTime> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, T3, T4, TTimePoint, TEvent, TEventAtTime> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        public GenericExistent<T, T1, T2, T3, T4, TTimePoint, TEvent, TEventAtTime> StartWithTime(params Goal[] conditions) {
            StartWith.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, T3, T4, TTimePoint, TEvent, TEventAtTime> StartWithCauses(params Effect[] effects) {
            StartWith.Causes(effects);
            return this;
        }

        public TablePredicate<T, TTimePoint, TTimePoint> StartWithChronicle => StartWith.Chronicle;

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

        public class ExistentGoal : TableGoal<T, bool, TTimePoint, TTimePoint> {
            public ExistentGoal(TablePredicate predicate, Term<T> arg1, Term<bool> arg2, Term<TTimePoint> arg3, Term<TTimePoint> arg4)
                : base(predicate, arg1, arg2, arg3, arg4) { }

            public ExistentGoal Ended => new(TablePredicate, Arg1, false, Arg3, Arg4);
            public ExistentGoal StartedAt(Term<TTimePoint> t) => new(TablePredicate, Arg1, __, t, Arg4);
            public ExistentGoal EndedAt(Term<TTimePoint> t) => new(TablePredicate, Arg1, __, Arg3, t);
        }
    }

    public class GenericExistent<T, T1, T2, T3, T4, T5, TTimePoint, TEvent, TEventAtTime> : TablePredicate<T, bool, TTimePoint, TTimePoint>
        where TEvent : GenericEvent<T, TTimePoint> where TEventAtTime : GenericEvent<T, TTimePoint, TTimePoint> {
        // ReSharper disable StaticMemberInGenericType
        // ReSharper disable InconsistentNaming
        internal static readonly Var<bool> _exists = (Var<bool>)"exists";
        private static readonly Var<TTimePoint> _end = (Var<TTimePoint>)"end";
        private static readonly Var<TTimePoint> _time = (Var<TTimePoint>)"time";
        private static readonly Var<int> _count = (Var<int>)"count";
        // ReSharper restore InconsistentNaming
        // ReSharper restore StaticMemberInGenericType

        private readonly TTimePoint _eschaton;

        // ReSharper disable MemberCanBePrivate.Global
        public readonly TEvent Start;
        public readonly TEvent End;
        public readonly TEventAtTime StartWith;
        // ReSharper restore MemberCanBePrivate.Global
        private readonly Var<TTimePoint> _start;

        public readonly TablePredicate<T, T1, T2, T3, T4, T5> Attributes;

        protected GenericExistent(string name, Var<T> existent, Var<TTimePoint> start, TTimePoint eschaton, Function<TTimePoint> currentTime,
                                  Func<string, Var<T>, TEvent> newEvent, Func<string, Var<T>, Var<TTimePoint>, TEventAtTime> newEventAtTime,
                                  IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4,
                                  IColumnSpec<T5> feature5)
            : base(name, existent.Key, _exists.Indexed, start, _end) {
            _start = start;
            _eschaton = eschaton;
            Start = newEvent($"{name}Start", existent);
            End = newEvent($"{name}End", existent);
            StartWith = newEventAtTime($"{name}StartWith", existent, _start);
            Add[existent, true, _time, _eschaton].If(Start, currentTime[_time]);
            Add[existent, true, start, _eschaton].If(StartWith);
            Set(existent, _end, _time).If(End, currentTime[_time]);
            Set(existent, _exists, false).If(End);
            Attributes = Predicate($"{name}Attributes", ((Var<T>)DefaultVariables[0]).Key,
                                   feature1, feature2, feature3, feature4, feature5);
        }

        private TablePredicate<bool, int> _countPredicate;
        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<bool, int> Count {
            get {
                if (_countPredicate != null) return _countPredicate;
                // No using default variables on an Existent.Count table
                _countPredicate = CountsBy($"{Name}Count", this, _exists, _count);
                _countPredicate.IndexByKey(_exists);
                return _countPredicate;
            }
        }
        public KeyIndex<(bool, int), bool> CountIndex =>
            (KeyIndex<(bool, int), bool>)Count.IndexFor(_exists, true);

        public GenericExistent<T, T1, T2, T3, T4, T5, TTimePoint, TEvent, TEventAtTime> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<T>)DefaultVariables[0], true, _start, _eschaton].Where(conditions);
            return this;
        }

        public GenericExistent<T, T1, T2, T3, T4, T5, TTimePoint, TEvent, TEventAtTime> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, T3, T4, T5, TTimePoint, TEvent, TEventAtTime> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public GenericExistent<T, T1, T2, T3, T4, T5, TTimePoint, TEvent, TEventAtTime> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, T3, T4, T5, TTimePoint, TEvent, TEventAtTime> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        public GenericExistent<T, T1, T2, T3, T4, T5, TTimePoint, TEvent, TEventAtTime> StartWithTime(params Goal[] conditions) {
            StartWith.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, T3, T4, T5, TTimePoint, TEvent, TEventAtTime> StartWithCauses(params Effect[] effects) {
            StartWith.Causes(effects);
            return this;
        }

        public TablePredicate<T, TTimePoint, TTimePoint> StartWithChronicle => StartWith.Chronicle;

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

        public class ExistentGoal : TableGoal<T, bool, TTimePoint, TTimePoint> {
            public ExistentGoal(TablePredicate predicate, Term<T> arg1, Term<bool> arg2, Term<TTimePoint> arg3, Term<TTimePoint> arg4)
                : base(predicate, arg1, arg2, arg3, arg4) { }

            public ExistentGoal Ended => new(TablePredicate, Arg1, false, Arg3, Arg4);
            public ExistentGoal StartedAt(Term<TTimePoint> t) => new(TablePredicate, Arg1, __, t, Arg4);
            public ExistentGoal EndedAt(Term<TTimePoint> t) => new(TablePredicate, Arg1, __, Arg3, t);
        }
    }

    public class GenericExistent<T, T1, T2, T3, T4, T5, T6, TTimePoint, TEvent, TEventAtTime> : TablePredicate<T, bool, TTimePoint, TTimePoint>
        where TEvent : GenericEvent<T, TTimePoint> where TEventAtTime : GenericEvent<T, TTimePoint, TTimePoint> {
        // ReSharper disable StaticMemberInGenericType
        // ReSharper disable InconsistentNaming
        internal static readonly Var<bool> _exists = (Var<bool>)"exists";
        private static readonly Var<TTimePoint> _end = (Var<TTimePoint>)"end";
        private static readonly Var<TTimePoint> _time = (Var<TTimePoint>)"time";
        private static readonly Var<int> _count = (Var<int>)"count";
        // ReSharper restore InconsistentNaming
        // ReSharper restore StaticMemberInGenericType

        private readonly TTimePoint _eschaton;

        // ReSharper disable MemberCanBePrivate.Global
        public readonly TEvent Start;
        public readonly TEvent End;
        public readonly TEventAtTime StartWith;
        // ReSharper restore MemberCanBePrivate.Global
        private readonly Var<TTimePoint> _start;

        public readonly TablePredicate<T, T1, T2, T3, T4, T5, T6> Attributes;

        protected GenericExistent(string name, Var<T> existent, Var<TTimePoint> start, TTimePoint eschaton, Function<TTimePoint> currentTime,
                                  Func<string, Var<T>, TEvent> newEvent, Func<string, Var<T>, Var<TTimePoint>, TEventAtTime> newEventAtTime,
                                  IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4,
                                  IColumnSpec<T5> feature5, IColumnSpec<T6> feature6)
            : base(name, existent.Key, _exists.Indexed, start, _end) {
            _start = start;
            _eschaton = eschaton;
            Start = newEvent($"{name}Start", existent);
            End = newEvent($"{name}End", existent);
            StartWith = newEventAtTime($"{name}StartWith", existent, _start);
            Add[existent, true, _time, _eschaton].If(Start, currentTime[_time]);
            Add[existent, true, start, _eschaton].If(StartWith);
            Set(existent, _end, _time).If(End, currentTime[_time]);
            Set(existent, _exists, false).If(End);
            Attributes = Predicate($"{name}Attributes", ((Var<T>)DefaultVariables[0]).Key,
                                   feature1, feature2, feature3, feature4, feature5, feature6);
        }

        private TablePredicate<bool, int> _countPredicate;
        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<bool, int> Count {
            get {
                if (_countPredicate != null) return _countPredicate;
                // No using default variables on an Existent.Count table
                _countPredicate = CountsBy($"{Name}Count", this, _exists, _count);
                _countPredicate.IndexByKey(_exists);
                return _countPredicate;
            }
        }
        public KeyIndex<(bool, int), bool> CountIndex =>
            (KeyIndex<(bool, int), bool>)Count.IndexFor(_exists, true);

        public GenericExistent<T, T1, T2, T3, T4, T5, T6, TTimePoint, TEvent, TEventAtTime> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<T>)DefaultVariables[0], true, _start, _eschaton].Where(conditions);
            return this;
        }

        public GenericExistent<T, T1, T2, T3, T4, T5, T6, TTimePoint, TEvent, TEventAtTime> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, T3, T4, T5, T6, TTimePoint, TEvent, TEventAtTime> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public GenericExistent<T, T1, T2, T3, T4, T5, T6, TTimePoint, TEvent, TEventAtTime> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, T3, T4, T5, T6, TTimePoint, TEvent, TEventAtTime> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        public GenericExistent<T, T1, T2, T3, T4, T5, T6, TTimePoint, TEvent, TEventAtTime> StartWithTime(params Goal[] conditions) {
            StartWith.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, T3, T4, T5, T6, TTimePoint, TEvent, TEventAtTime> StartWithCauses(params Effect[] effects) {
            StartWith.Causes(effects);
            return this;
        }

        public TablePredicate<T, TTimePoint, TTimePoint> StartWithChronicle => StartWith.Chronicle;

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

        public class ExistentGoal : TableGoal<T, bool, TTimePoint, TTimePoint> {
            public ExistentGoal(TablePredicate predicate, Term<T> arg1, Term<bool> arg2, Term<TTimePoint> arg3, Term<TTimePoint> arg4)
                : base(predicate, arg1, arg2, arg3, arg4) { }

            public ExistentGoal Ended => new(TablePredicate, Arg1, false, Arg3, Arg4);
            public ExistentGoal StartedAt(Term<TTimePoint> t) => new(TablePredicate, Arg1, __, t, Arg4);
            public ExistentGoal EndedAt(Term<TTimePoint> t) => new(TablePredicate, Arg1, __, Arg3, t);
        }
    }

    public class GenericExistent<T, T1, T2, T3, T4, T5, T6, T7, TTimePoint, TEvent, TEventAtTime> : TablePredicate<T, bool, TTimePoint, TTimePoint>
        where TEvent : GenericEvent<T, TTimePoint> where TEventAtTime : GenericEvent<T, TTimePoint, TTimePoint> {
        // ReSharper disable StaticMemberInGenericType
        // ReSharper disable InconsistentNaming
        internal static readonly Var<bool> _exists = (Var<bool>)"exists";
        private static readonly Var<TTimePoint> _end = (Var<TTimePoint>)"end";
        private static readonly Var<TTimePoint> _time = (Var<TTimePoint>)"time";
        private static readonly Var<int> _count = (Var<int>)"count";
        // ReSharper restore InconsistentNaming
        // ReSharper restore StaticMemberInGenericType

        private readonly TTimePoint _eschaton;

        // ReSharper disable MemberCanBePrivate.Global
        public readonly TEvent Start;
        public readonly TEvent End;
        public readonly TEventAtTime StartWith;
        // ReSharper restore MemberCanBePrivate.Global
        private readonly Var<TTimePoint> _start;

        public readonly TablePredicate<T, T1, T2, T3, T4, T5, T6, T7> Attributes;

        protected GenericExistent(string name, Var<T> existent, Var<TTimePoint> start, TTimePoint eschaton, Function<TTimePoint> currentTime,
                                  Func<string, Var<T>, TEvent> newEvent, Func<string, Var<T>, Var<TTimePoint>, TEventAtTime> newEventAtTime, 
                                  IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, 
                                  IColumnSpec<T5> feature5, IColumnSpec<T6> feature6, IColumnSpec<T7> feature7)
            : base(name, existent.Key, _exists.Indexed, start, _end) {
            _start = start;
            _eschaton = eschaton;
            Start = newEvent($"{name}Start", existent);
            End = newEvent($"{name}End", existent);
            StartWith = newEventAtTime($"{name}StartWith", existent, _start);
            Add[existent, true, _time, _eschaton].If(Start, currentTime[_time]);
            Add[existent, true, start, _eschaton].If(StartWith);
            Set(existent, _end, _time).If(End, currentTime[_time]);
            Set(existent, _exists, false).If(End);
            Attributes = Predicate($"{name}Attributes", ((Var<T>)DefaultVariables[0]).Key, 
                                   feature1, feature2, feature3, feature4, feature5, feature6, feature7);
        }

        private TablePredicate<bool, int> _countPredicate;
        // ReSharper disable once MemberCanBePrivate.Global
        public TablePredicate<bool, int> Count {
            get {
                if (_countPredicate != null) return _countPredicate;
                // No using default variables on an Existent.Count table
                _countPredicate = CountsBy($"{Name}Count", this, _exists, _count);
                _countPredicate.IndexByKey(_exists);
                return _countPredicate;
            }
        }
        public KeyIndex<(bool, int), bool> CountIndex =>
            (KeyIndex<(bool, int), bool>)Count.IndexFor(_exists, true);

        public GenericExistent<T, T1, T2, T3, T4, T5, T6, T7, TTimePoint, TEvent, TEventAtTime> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<T>)DefaultVariables[0], true, _start, _eschaton].Where(conditions);
            return this;
        }

        public GenericExistent<T, T1, T2, T3, T4, T5, T6, T7, TTimePoint, TEvent, TEventAtTime> StartWhen(params Goal[] conditions) {
            Start.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, T3, T4, T5, T6, T7, TTimePoint, TEvent, TEventAtTime> StartCauses(params Effect[] effects) {
            Start.Causes(effects);
            return this;
        }

        public GenericExistent<T, T1, T2, T3, T4, T5, T6, T7, TTimePoint, TEvent, TEventAtTime> EndWhen(params Goal[] conditions) {
            End.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, T3, T4, T5, T6, T7, TTimePoint, TEvent, TEventAtTime> EndCauses(params Effect[] effects) {
            End.Causes(effects);
            return this;
        }

        public GenericExistent<T, T1, T2, T3, T4, T5, T6, T7, TTimePoint, TEvent, TEventAtTime> StartWithTime(params Goal[] conditions) {
            StartWith.OccursWhen(conditions);
            return this;
        }
        public GenericExistent<T, T1, T2, T3, T4, T5, T6, T7, TTimePoint, TEvent, TEventAtTime> StartWithCauses(params Effect[] effects) {
            StartWith.Causes(effects);
            return this;
        }

        public TablePredicate<T, TTimePoint, TTimePoint> StartWithChronicle => StartWith.Chronicle;

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

        public class ExistentGoal : TableGoal<T, bool, TTimePoint, TTimePoint> {
            public ExistentGoal(TablePredicate predicate, Term<T> arg1, Term<bool> arg2, Term<TTimePoint> arg3, Term<TTimePoint> arg4)
                : base(predicate, arg1, arg2, arg3, arg4) { }

            public ExistentGoal Ended => new(TablePredicate, Arg1, false, Arg3, Arg4);
            public ExistentGoal StartedAt(Term<TTimePoint> t) => new(TablePredicate, Arg1, __, t, Arg4);
            public ExistentGoal EndedAt(Term<TTimePoint> t) => new(TablePredicate, Arg1, __, Arg3, t);
        }
    }
}
