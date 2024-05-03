using System.Collections.Generic;
using System.Linq;
using Step;
using Step.Interpreter;
using TED;
using UnityEngine;
using Random = System.Random;

namespace VdlV.Step {
    using Simulator;
    using ValueTypes;
    using static Time.Clock;
    using static Simulator.StaticTables;
    using static Utilities.Randomize;
    using static ArgumentCountException; // from Step.Interpreter
    using static Module;                 // from Step
    using static Application;            // from UnityEngine
    using static Debug;                  // from UnityEngine
    using static System.IO.Path;
    
    public static class StepCode {
        private static readonly Module Module;
        private static readonly Random Rng;

        static StepCode() {
            Module = new Module("VdlV", Global);
            Module.AddBindHook((BindHook)ImportTedPredicate);
            Module.LoadDirectory(Combine(dataPath, "Step"));
            Rng = MakeRng();
            Module["RomanticInteractions"] = romanticInteractions;
            Module["MaleSex"] = Sex.Male;
            Module["FemaleSex"] = Sex.Female;
            Module["CurrentDate"] = new SimpleFunction<int, string>("CurrentDate", _ => Date().ToString());
            Module["Log"] = new SimplePredicate<object>("Log", o => { Log(o); return true; });
        }

        private static IEnumerable<object[]> TableRowsInRandomOrder(TablePredicate predicate) {
            var buffer = new object[predicate.Arity];
            var row = (uint)Integer((int)predicate.Length, Rng);
            var stride = (uint)RandomPrime(Rng);
            var length = predicate.Length;
            for (var i = 0; i < length; i++) {
                predicate.GetUntypedRowData(row % length, buffer);
                yield return buffer;
                row += stride;
            }
        }

        private static bool ImportTedPredicate(StateVariableName variableName, out object stepTask) {
            var name = variableName.Name;
            var table = VoixDeLaVille.Simulation.Tables.FirstOrDefault(t => t.Name == name);
            if (table != null) {
                stepTask = new GeneralPrimitive(name, (args, output, env, frame, k) => {
                    // ReSharper disable once CoVariantArrayConversion
                    Check(name, table.Arity, table.DefaultVariables);
                    foreach (var tuple in TableRowsInRandomOrder(table))
                        if (env.UnifyArrays(args, tuple, out BindingList u) &&
                            k(output, u, env.State, frame))
                            return true;
                    return false;
                });
                return true;
            }
            stepTask = null;
            return false;
        }

        public static string Run(string task, params object[] args) => Module.Call(task, args);
    }
}
