using System;
using System.Collections.Generic;

namespace GraphVisualization
{
    public class DictionaryWithDefault<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public readonly Func<TKey,TValue> Default;

        public DictionaryWithDefault(Func<TKey,TValue> @default)
        {
            Default = @default;
        }

        public new TValue this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out var value))
                    return value;
                this[key] = value = Default(key);
                return value;
            }

            set => ((Dictionary<TKey,TValue>)this)[key] = value;
        }
    }
}
