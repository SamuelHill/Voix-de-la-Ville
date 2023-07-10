using TED;
using TED.Interpreter;
using TotT.Simulator;
using TotT.Unity;
using TotT.ValueTypes;
using UnityEngine;

namespace TotT.Simulog {
    using static SimuLang;

    public class Existent<T> : GenericExistent<T, TimePoint, Event<T>, Event<T, TimePoint>> {
        public Existent(string name, Var<T> existent, Var<TimePoint> start) : 
            base(name, existent, start, TimePoint.Eschaton, TalkOfTheTown.Time.CurrentTimePoint, Event, Event) => 
            this.Colorize(_exists, s => s ? Color.white : Color.gray);
    }

    public class Existent<T, T1> : GenericExistent<T, T1, TimePoint, Event<T>, Event<T, TimePoint>> {
        public Existent(string name, Var<T> existent, Var<TimePoint> start, IColumnSpec<T1> feature1) :
            base(name, existent, start, TimePoint.Eschaton, TalkOfTheTown.Time.CurrentTimePoint, Event, Event, feature1) =>
            this.Colorize(_exists, s => s ? Color.white : Color.gray);
    }

    public class Existent<T, T1, T2> : GenericExistent<T, T1, T2, TimePoint, Event<T>, Event<T, TimePoint>> {
        public Existent(string name, Var<T> existent, Var<TimePoint> start, IColumnSpec<T1> feature1, IColumnSpec<T2> feature2) :
            base(name, existent, start, TimePoint.Eschaton, TalkOfTheTown.Time.CurrentTimePoint, Event, Event, feature1, feature2) =>
            this.Colorize(_exists, s => s ? Color.white : Color.gray);
    }

    public class Existent<T, T1, T2, T3> : GenericExistent<T, T1, T2, T3, TimePoint, Event<T>, Event<T, TimePoint>> {
        public Existent(string name, Var<T> existent, Var<TimePoint> start, IColumnSpec<T1> feature1, 
                        IColumnSpec<T2> feature2, IColumnSpec<T3> feature3) :
            base(name, existent, start, TimePoint.Eschaton, TalkOfTheTown.Time.CurrentTimePoint, Event, Event,
                 feature1, feature2, feature3) =>
            this.Colorize(_exists, s => s ? Color.white : Color.gray);
    }

    public class Existent<T, T1, T2, T3, T4> : GenericExistent<T, T1, T2, T3, T4, TimePoint, Event<T>, Event<T, TimePoint>> {
        public Existent(string name, Var<T> existent, Var<TimePoint> start, IColumnSpec<T1> feature1, 
                        IColumnSpec<T2> feature2, IColumnSpec<T3> feature3, IColumnSpec<T4> feature4) :
            base(name, existent, start, TimePoint.Eschaton, TalkOfTheTown.Time.CurrentTimePoint, Event, Event,
                 feature1, feature2, feature3, feature4) =>
            this.Colorize(_exists, s => s ? Color.white : Color.gray);
    }

    public class Existent<T, T1, T2, T3, T4, T5> : GenericExistent<T, T1, T2, T3, T4, T5, TimePoint, Event<T>, Event<T, TimePoint>> {
        public Existent(string name, Var<T> existent, Var<TimePoint> start, IColumnSpec<T1> feature1, IColumnSpec<T2> feature2,
                        IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, IColumnSpec<T5> feature5) :
            base(name, existent, start, TimePoint.Eschaton, TalkOfTheTown.Time.CurrentTimePoint, Event, Event,
                 feature1, feature2, feature3, feature4, feature5) =>
            this.Colorize(_exists, s => s ? Color.white : Color.gray);
    }

    public class Existent<T, T1, T2, T3, T4, T5, T6> : GenericExistent<T, T1, T2, T3, T4, T5, T6, TimePoint, Event<T>, Event<T, TimePoint>> {
        public Existent(string name, Var<T> existent, Var<TimePoint> start, IColumnSpec<T1> feature1, IColumnSpec<T2> feature2,
                        IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, IColumnSpec<T5> feature5, IColumnSpec<T6> feature6) :
            base(name, existent, start, TimePoint.Eschaton, TalkOfTheTown.Time.CurrentTimePoint, Event, Event,
                 feature1, feature2, feature3, feature4, feature5, feature6) =>
            this.Colorize(_exists, s => s ? Color.white : Color.gray);
    }

    public class Existent<T, T1, T2, T3, T4, T5, T6, T7> : GenericExistent<T, T1, T2, T3, T4, T5, T6, T7, TimePoint, Event<T>, Event<T, TimePoint>> {
        public Existent(string name, Var<T> existent, Var<TimePoint> start, IColumnSpec<T1> feature1, IColumnSpec<T2> feature2, 
                        IColumnSpec<T3> feature3, IColumnSpec<T4> feature4, IColumnSpec<T5> feature5, IColumnSpec<T6> feature6, IColumnSpec<T7> feature7) :
            base(name, existent, start, TimePoint.Eschaton, TalkOfTheTown.Time.CurrentTimePoint, Event, Event, 
                 feature1, feature2, feature3, feature4, feature5, feature6, feature7) =>
            this.Colorize(_exists, s => s ? Color.white : Color.gray);
    }
}
