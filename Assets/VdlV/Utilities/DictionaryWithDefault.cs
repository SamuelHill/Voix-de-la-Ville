using System;
using System.Collections.Generic;

namespace VdlV.Utilities {
    public class DictionaryWithDefault<TKey, TValue> : Dictionary<TKey, TValue> {
        private readonly Func<TKey,TValue> _default;

        public DictionaryWithDefault(Func<TKey,TValue> @default) => _default = @default;

        public new TValue this[TKey key] {
            get {
                if (TryGetValue(key, out var value)) return value;
                this[key] = value = _default(key);
                return value;
            }
            private set => ((Dictionary<TKey,TValue>)this)[key] = value;
        }
    }
}
