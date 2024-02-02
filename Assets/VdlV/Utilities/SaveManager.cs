using System;
using System.IO;
using TED;
using TED.Utilities;

namespace VdlV.Utilities {
    using static Char;
    using static CsvWriter;
    using static Deserializer;
    using static Directory;
    using static File;
    using static Serializer;
    using static StringProcessing;
    using Random = System.Random;

    public static class SaveManager {
        private const string SerializeFile = "SerializedObjects.json";


        private static string SavePath(string saveName) => $"Assets/Saves/{saveName}/";

        public static StreamWriter SaveStream;
        public static StreamReader LoadStream;

        public static string SerializedId(object o) => $"#{Serialize(o, SaveStream)}";

        public static void Save(string saveName, Simulation simulation) {
            var savePath = SavePath(saveName);
            CreateDirectory(savePath);
            SaveStream = CreateText($"{savePath}{SerializeFile}");
            WriteAllTables(savePath, simulation);
            SaveStream.Close();
        }

        public static void Save(Simulation simulation) => Save(NowString, simulation);

        private static bool IsSerializedId(string s) => s.Length >= 2 && s[0].Equals('#') && IsDigit(s[1]);
        public static object DeserializeIfId(string s, Func<string, object> fallback) => 
            IsSerializedId(s) ? Deserialize(s) : fallback(s);
        public static object DeserializeIfId(string s, Func<string, Random, object> fallback, Random rng) =>
            IsSerializedId(s) ? Deserialize(s) : fallback(s, rng);

        private static object DeserializeIfId<T>(string s, Func<string, T> fallback) =>
            IsSerializedId(s) ? Deserialize(s) : fallback(s);
        public static object DeserializeIfId<T>(string s) where T : ISerializableValue<T> => 
            DeserializeIfId(s, ISerializableValue<T>.FromString);

        private static object DeserializeIfId<T>(string s, Func<string, Random, T> fallback, Random rng) =>
            IsSerializedId(s) ? Deserialize(s) : fallback(s, rng);
        public static object DeserializeIfId<T>(string s, Random rng) where T : ISerializableRandomValue<T> =>
            DeserializeIfId(s, ISerializableRandomValue<T>.FromString, rng);

        public static void Load(string saveName, Simulation simulation) {
            var savePath = SavePath(saveName);
            LoadStream = new StreamReader($"{savePath}{SerializeFile}");
            while (!LoadStream.EndOfStream) Deserialize(LoadStream);
            LoadStream.Close();
            foreach (var table in simulation.Tables) table.LoadCsv(savePath);
        }
    }
}
