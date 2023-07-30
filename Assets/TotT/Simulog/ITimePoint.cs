using System;

namespace TotT.Simulog {
    public interface ITimePoint<out T> where T : IComparable<T>, IEquatable<T> {
        // for this interface to work as intended, these would need to be static abstract properties
        // but that is only available as of C# 11 (which the netstandard unity uses doesn't support)
        public T Eschaton { get; }

        public T CurrentTimePoint { get; }
    }
}
