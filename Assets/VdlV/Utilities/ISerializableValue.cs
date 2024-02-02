using System;

namespace VdlV.Utilities {
    public interface ISerializableValue {
        public string ToString();
    }

    public interface ISerializableValue<out T> : ISerializableValue {
        public static T FromString(string fromString) {
            throw new NotImplementedException();
        }
    }

    public interface ISerializableRandomValue<out T> : ISerializableValue {
        public static T FromString(string fromString, Random rng) {
            throw new NotImplementedException();
        }
    }
}
