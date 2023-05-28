using System.Linq;
using TED.Interpreter;

namespace TotT.Simulog {
    // ReSharper disable once InconsistentNaming
    public static class TEDHelpers {
        public static Goal Goals(params Goal[] goals) =>
            goals.Length == 0 ? null : goals.Aggregate((current, goal) => current & goal);
        public static Goal NonZero(Goal goal) => !!goal;
        public static Goal NonZero(params Goal[] goals) => NonZero(Goals(goals));
    }
}