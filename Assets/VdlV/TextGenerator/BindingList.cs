using System.Collections.Generic;

namespace VdlV.TextGenerator {
    public class BindingList {
        public static BindingList Global = new(new Parameter<string>("_"), "_", null);

        public readonly Parameter Parameter;
        public readonly object Value;
        public readonly BindingList Next;

        public static void BindGlobal<T>(Parameter<T> p, T value) => Global = Global.Bind(p, value);

        public BindingList Bind<T>(Parameter<T> parameter, T value) => new(parameter, value, this);

        public BindingList(Parameter parameter, object value, BindingList next = null) {
            Parameter = parameter;
            Value = value;
            Next = next;
        }

        public T Lookup<T>(Parameter<T> p) {
            for (var b = this; b != null; b = b.Next)
                if (b.Parameter == p) return (T)b.Value;
            throw new KeyNotFoundException($"Not value given for text generation parameter {p}");
        }
    }
}
