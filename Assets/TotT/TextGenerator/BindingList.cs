using System.Collections.Generic;

namespace TotT.TextGenerator
{
    public class BindingList
    {
        public readonly Parameter Parameter;
        public readonly object Value;
        public readonly BindingList Next;

        public BindingList Bind<T>(Parameter<T> parameter, T value) => new BindingList(parameter, value, this);

        private BindingList(Parameter parameter, object value, BindingList next)
        {
            Parameter = parameter;
            Value = value;
            Next = next;
        }

        public T Lookup<T>(Parameter<T> p)
        {
            for (var b = this; b != null; b = b.Next)
                if (b.Parameter == p)
                    return (T)b.Value;
            throw new KeyNotFoundException($"Not value given for text generation parameter {p}");
        }
    }
}
