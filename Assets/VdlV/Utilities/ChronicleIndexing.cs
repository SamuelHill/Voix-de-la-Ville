using TED.Interpreter;
using static TED.Tables.IndexMode;

namespace VdlV.Utilities {
    public static class ChronicleIndexing {
        public static IColumnSpec<T> DemoteKey<T>(IColumnSpec<T> columnSpec) => 
            columnSpec.IndexMode == Key ? columnSpec.TypedVariable.Indexed : columnSpec;
    }
}
