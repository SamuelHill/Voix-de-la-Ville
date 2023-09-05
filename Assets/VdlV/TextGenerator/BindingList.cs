using System.Collections.Generic;

namespace VdlV.TextGenerator {
    public class BindingList {
        public static BindingList Global = new(new Parameter<string>("_"), "_", null);

        private readonly Parameter _parameter;
        private readonly object _value;
        private readonly BindingList _next;

        public static void BindGlobal<T>(Parameter<T> p, T value) => Global = Global.Bind(p, value);

        public BindingList Bind<T>(Parameter<T> parameter, T value) => new(parameter, value, this);

        public BindingList(Parameter parameter, object value, BindingList next = null) {
            _parameter = parameter;
            _value = value;
            _next = next;
        }

        public T Lookup<T>(Parameter<T> p) {
            for (var b = this; b != null; b = b._next)
                if (b._parameter == p) return (T)b._value;
            throw new KeyNotFoundException($"Value not given for text generation parameter {p}");
        }
    }
}
