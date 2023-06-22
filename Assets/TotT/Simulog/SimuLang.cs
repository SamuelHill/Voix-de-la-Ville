using System.Linq;
using TED;
using TED.Interpreter;

namespace TotT.Simulog {

    // ReSharper disable once InconsistentNaming
    public static class SimuLang {
        public static Goal Goals(params Goal[] goals) =>
            goals.Length == 0 ? null : goals.Aggregate((current, goal) => current & goal);

        public static Existent<T> Exists<T>(string name, Var<T> arg) => new(name, arg);

        public static Event<T1> Event<T1>(string name, IColumnSpec<T1> arg) => new(name, arg);

        public static Event<T1, T2> Event<T1, T2>(string name, 
            IColumnSpec<T1> arg1, IColumnSpec<T2> arg2) => new(name, arg1, arg2);

        public static Event<T1, T2, T3> Event<T1, T2, T3>(string name, IColumnSpec<T1> arg1, 
            IColumnSpec<T2> arg2, IColumnSpec<T3> arg3) => new(name, arg1, arg2, arg3);

        public static Event<T1, T2, T3, T4> Event<T1, T2, T3, T4>(string name, 
            IColumnSpec<T1> arg1, IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4)
            => new(name, arg1, arg2, arg3, arg4);

        public static Event<T1, T2, T3, T4, T5> Event<T1, T2, T3, T4, T5>(string name, IColumnSpec<T1> arg1,
            IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5)
            => new(name, arg1, arg2, arg3, arg4, arg5);

        public static Event<T1, T2, T3, T4, T5, T6> Event<T1, T2, T3, T4, T5, T6>(string name, IColumnSpec<T1> arg1,
            IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6)
            => new(name, arg1, arg2, arg3, arg4, arg5, arg6);

        public static Event<T1, T2, T3, T4, T5, T6, T7> Event<T1, T2, T3, T4, T5, T6, T7>(string name, IColumnSpec<T1> arg1, 
            IColumnSpec<T2> arg2, IColumnSpec<T3> arg3, IColumnSpec<T4> arg4, IColumnSpec<T5> arg5, IColumnSpec<T6> arg6, IColumnSpec<T7> arg7)
            => new(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
    }
}
