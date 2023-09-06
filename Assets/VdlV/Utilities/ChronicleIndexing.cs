using TED.Interpreter;
using static TED.Tables.IndexMode;

namespace VdlV.Utilities {
    public static class ChronicleIndexing {
        public static IColumnSpec<T> DemoteKey<T>(IColumnSpec<T> columnSpec) => 
            columnSpec.IndexMode == Key ? MaintainJoint(columnSpec) : columnSpec;

        private static IColumnSpec<T> MaintainJoint<T>(IColumnSpec<T> columnSpec) =>
            columnSpec.JointPartial ? columnSpec.TypedVariable.JointIndexed : columnSpec.TypedVariable.Indexed;
    }
}
