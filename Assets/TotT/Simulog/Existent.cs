using TED;
using TED.Interpreter;
using TED.Tables;
using TotT.Simulator;
using TotT.Unity;
using TotT.ValueTypes;
using UnityEngine;
using static TED.Language;

namespace TotT.Simulog {
    using static SimuLang;
    using static Variables;
    
    public class Existent<T> : TablePredicate<T, bool, TimePoint, TimePoint> {
        // ReSharper disable StaticMemberInGenericType
        // ReSharper disable InconsistentNaming
        private static readonly Var<bool> _exists = (Var<bool>)"exists";
        private static readonly Var<TimePoint> _start = (Var<TimePoint>)"start";
        private static readonly Var<TimePoint> _end = (Var<TimePoint>)"end";
        // ReSharper restore InconsistentNaming
        // ReSharper restore StaticMemberInGenericType

        // We don't want to chronicle the start and end events, that is the purpose of an Existent...
        public readonly Event<T> Start;
        public readonly Event<T> End;
        // PreviousStart is the special case start (new to existent while already existing)
        public readonly Event<T, TimePoint> StartWith;
        private readonly Var<TimePoint> _startVar;

        public Existent(string name, Var<T> existent, Var<TimePoint> startTime) : base(name, existent.Key, _exists.Indexed, startTime, _end) {
            Start = Event($"{name}Start", existent);
            End = Event($"{name}End", existent);
            StartWith = Event($"{name}StartWith", existent, startTime);
            _startVar = startTime;
            Add[existent, true, time, TimePoint.Eschaton].If(Start, TalkOfTheTown.Time.CurrentTimePoint[time]);
            Add[existent, true, startTime, TimePoint.Eschaton].If(StartWith);
            Set(existent, _end, time).If(End, TalkOfTheTown.Time.CurrentTimePoint[time]);
            Set(existent, _exists, false).If(End);
            this.Colorize(_exists, s => s ? Color.white : Color.gray);
        }
        // If you will never be using InitiallyWhere or StartWithTime, this is fine...
        public Existent(string name, Var<T> existent) : this(name, existent, _start) { }

        private TablePredicate<bool, int> _countPredicate;
        public TablePredicate<bool, int> Count {
            get {
                if (_countPredicate != null) return _countPredicate;
                _countPredicate = CountsBy($"{Name}Count", this, _exists, count);
                _countPredicate.IndexByKey(_exists);
                return _countPredicate;
            }
        }
        public KeyIndex<(bool, int), bool> CountIndex =>
            (KeyIndex<(bool, int), bool>)Count.IndexFor(_exists, true);

        public Existent<T> InitiallyWhere(params Goal[] conditions) {
            Initially[(Var<T>)DefaultVariables[0], true, _startVar, TimePoint.Eschaton].Where(conditions);
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
        
        public TablePredicate<T, T1> Features<T1>(IColumnSpec<T1> feature) => 
            Predicate($"{Name}Features", ((Var<T>)DefaultVariables[0]).Key, feature);
        public TablePredicate<T, T1, T2> Features<T1, T2>(IColumnSpec<T1> feature1, IColumnSpec<T2> feature2) =>
            Predicate($"{Name}Features", ((Var<T>)DefaultVariables[0]).Key, feature1, feature2);
        public TablePredicate<T, T1, T2, T3> Features<T1, T2, T3>(IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, IColumnSpec<T3> feature3) =>
            Predicate($"{Name}Features", ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3);
        public TablePredicate<T, T1, T2, T3, T4> Features<T1, T2, T3, T4>(IColumnSpec<T1> feature1,
            IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4) =>
            Predicate($"{Name}Features", ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4);
        public TablePredicate<T, T1, T2, T3, T4, T5> Features<T1, T2, T3, T4, T5>(IColumnSpec<T1> feature1, 
            IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, IColumnSpec<T5> feature5) =>
            Predicate($"{Name}Features", ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5);
        public TablePredicate<T, T1, T2, T3, T4, T5, T6> Features<T1, T2, T3, T4, T5, T6>(IColumnSpec<T1> feature1, IColumnSpec<T2> feature2,
            IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, IColumnSpec<T5> feature5, IColumnSpec<T6> feature6) =>
            Predicate($"{Name}Features", ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6);
        public TablePredicate<T, T1, T2, T3, T4, T5, T6, T7> Features<T1, T2, T3, T4, T5, T6, T7>(IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, 
            IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, IColumnSpec<T5> feature5, IColumnSpec<T6> feature6, IColumnSpec<T7> feature7) =>
            Predicate($"{Name}Features", ((Var<T>)DefaultVariables[0]).Key, feature1, feature2, feature3, feature4, feature5, feature6, feature7);

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
}
