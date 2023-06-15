﻿using System;
using System.Text;

namespace TotT.TextGenerator
{
    public abstract class Parameter : TextGenerator
    {
        public readonly string Name;

        protected Parameter(string name)
        {
            Name = name;
        }
    }

    public class Parameter<T> : Parameter
    {
        public Parameter(string name) : this(name, t => t.ToString()) { }

        public Parameter(string name, Func<T, string> generator) : base(name)
        {
            this.generator = generator;
        }

        private readonly Func<T, string> generator;

        public override bool Generate(StringBuilder output, BindingList b)
        { 
            output.Append(generator(b.Lookup(this)));
            return true;
        }
    }
}
