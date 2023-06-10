using System;
using System.Linq;
using TED;
using TED.Interpreter;

namespace TotT.Simulog
{
    /// <summary>
    /// Base class for effects of IEvent predicates
    /// </summary>
    public class Effect
    {
        protected Goal[] ExtraConditions = Array.Empty<Goal>();
        public delegate void CodeGenerator(Effect effect, Goal eventCondition);

        private readonly CodeGenerator generator;

        public Effect(CodeGenerator generator)
        {
            this.generator = generator;
        }

        public void GenerateCode(TableGoal goal) => generator(this, goal);

        public Effect If(params Goal[] extraConditions)
        {
            ExtraConditions = extraConditions;
            return this;
        }

        public static Effect Set<TKey, TCol>(TablePredicate t, Var<TKey> key, Var<TCol> column, Term<TCol> newValue)
            => new Effect((e, g) =>
            {
                t.Set(key, column, newValue).If(e.JoinExtraConditions(g));
            });

        private Goal[] JoinExtraConditions(Goal g)
        {
            return ExtraConditions.Prepend(g).ToArray();
        }

        public static Effect Add(TableGoal row)
            => new Effect((e, g) =>
            {
                row.TablePredicate.AddUntyped.GetGoal(row.Arguments).If(e.JoinExtraConditions(g));
            });
    }
}
