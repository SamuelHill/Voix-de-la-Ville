using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Step;
using Step.Interpreter;
using TED;
using VdlV.Simulator;
using VdlV.Utilities;
using VdlV.ValueTypes;

// ReSharper disable once CheckNamespace
namespace Vldv.Step
{
    public static class StepCode
    {
        public static Module Module;
        private static readonly Random Rng;

        static StepCode()
        {
            Module = new Module("VdlV", Module.Global);
            Module.AddBindHook((Module.BindHook)ImportTedPredicate);
            Module.LoadDirectory(Path.Combine(UnityEngine.Application.dataPath, "Step"));
            Rng = Randomize.MakeRng();
            Module["RomanticInteractions"] = StaticTables.romanticInteractions;
            Module["MaleSex"] = Sex.Male;
            Module["FemaleSex"] = Sex.Female;
        }

        private static IEnumerable<object[]> TableRowsInRandomOrder(TablePredicate predicate)
        {
            var buffer = new object[predicate.Arity];
            var row = (uint)Randomize.Integer((int)predicate.Length, Rng);
            var stride = (uint)Randomize.RandomPrime(Rng);
            var length = predicate.Length;
            for (var i = 0; i < length; i++)
            {
                predicate.GetUntypedRowData(row % length, buffer);
                yield return buffer;
                row += stride;
            }
        }

        private static bool ImportTedPredicate(StateVariableName variableName, out object stepTask)
        {
            var name = variableName.Name;
            var table = VoixDeLaVille.Simulation.Tables.FirstOrDefault(t => t.Name == name);

            if (table != null)
            {
                stepTask = new GeneralPrimitive(name, (args, output, env, frame, k) =>
                {
                    // ReSharper disable once CoVariantArrayConversion
                    ArgumentCountException.Check(name, table.Arity, table.DefaultVariables);
                    foreach (var tuple in TableRowsInRandomOrder(table))
                    {
                        if (env.UnifyArrays(args, tuple, out BindingList u) &&
                            k(output, u, env.State, frame))
                            return true;
                    }

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
