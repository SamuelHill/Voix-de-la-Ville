using System;
using System.Linq;
using TED;
using TED.Interpreter;

namespace TotT.Simulog {

    /// <summary>
    /// Base class for effects of IEvent predicates
    /// </summary>
    public class Effect {
        private Goal[] _extraConditions = Array.Empty<Goal>();
        private readonly CodeGenerator _generator;

        private delegate void CodeGenerator(Effect effect, Goal eventCondition);

        private Effect(CodeGenerator generator) => _generator = generator;

        public void GenerateCode(TableGoal goal) => _generator(this, goal);

        public Effect If(params Goal[] extraConditions) {
            _extraConditions = extraConditions;
            return this;
        }

        private Goal[] JoinExtraConditions(Goal g) => _extraConditions.Prepend(g).ToArray();

        public static Effect Set<TKey, TCol>(TablePredicate t, Var<TKey> key, Var<TCol> column, Term<TCol> newValue) => 
            new((e, g) => { t.Set(key, column, newValue).If(e.JoinExtraConditions(g)); });

        public static Effect Add(TableGoal row) => new((e, g) => {
            row.TablePredicate.AddUntyped.GetGoal(row.Arguments).If(e.JoinExtraConditions(g));
        });
    }
}
