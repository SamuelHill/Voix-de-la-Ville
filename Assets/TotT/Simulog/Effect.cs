using System;
using System.Linq;
using TED;
using TED.Interpreter;

namespace TotT.Simulog {

    /// <summary>
    /// Base class for effects of IEvent predicates
    /// </summary>
    public class Effect {
        protected Goal[] ExtraConditions = Array.Empty<Goal>();
        public delegate void CodeGenerator(Effect effect, Goal eventCondition);

        private readonly CodeGenerator _generator;

        public Effect(CodeGenerator generator) => _generator = generator;

        public void GenerateCode(TableGoal goal) => _generator(this, goal);

        public Effect If(params Goal[] extraConditions) {
            ExtraConditions = extraConditions;
            return this;
        }

        private Goal[] JoinExtraConditions(Goal g) => ExtraConditions.Prepend(g).ToArray();

        public static Effect Set<TKey, TCol>(TablePredicate t, Var<TKey> key, Var<TCol> column, Term<TCol> newValue) => 
            new((e, g) => { t.Set(key, column, newValue).If(e.JoinExtraConditions(g)); });

        public static Effect Add(TableGoal row) => new((e, g) => {
            row.TablePredicate.AddUntyped.GetGoal(row.Arguments).If(e.JoinExtraConditions(g));
        });

        public static Effect Init(TableGoal row) => new((e, g) => {
            row.TablePredicate.InitialValueTable.GetGoal(row.Arguments).If(e.JoinExtraConditions(g));
        });
    }
}
