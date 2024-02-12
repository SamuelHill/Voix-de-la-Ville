using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TED;
using TED.Utilities;
using TMPro;
using UnityEngine;

namespace VdlV.Utilities {
    using static Application;
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
        private static StreamReader _deserializerLoadStream;
        public static StreamWriter SerializerSaveStream;

        private const string SaveDataFile = "SaveData.dat";
        private static StreamReader _saveDataReadStream;
        private static StreamWriter _saveDataWriteStream;
        public static Action<StreamReader> ReadSaveData;
        public static Action<StreamWriter> WriteSaveData;

        private static string _saveToLoad; // defaults to null (not the empty string...?)
        private static readonly List<TablePredicate> TablesToIgnore = new();

        private static string SavePath(string saveName) => $"{persistentDataPath}/{saveName}/";

        public static IEnumerable<string> SavesList() => GetDirectories(persistentDataPath).Select(d => new DirectoryInfo(d).Name);

        public static void UpdateSaveToLoad(TMP_Dropdown saveOptions) {
            if (saveOptions.options[saveOptions.value].text != "New game") 
                _saveToLoad = saveOptions.options[saveOptions.value].text;
        }

        public static void LoadFromSave(Simulation simulation) {
            if (_saveToLoad != null) Load(_saveToLoad, simulation);
        }

        public static void IgnoreTable(TablePredicate table) => TablesToIgnore.Add(table);

        public static string SerializedId(object o) => $"#{Serialize(o, SerializerSaveStream)}";
        private static bool IsSerializedId(string s) => s.Length >= 2 && s[0].Equals('#') && IsDigit(s[1]);

        public static object DeserializeIfId(string s, Func<string, object> fallback) => IsSerializedId(s) ? Deserialize(s) : fallback(s);
        public static object DeserializeIfId(string s, Func<string, Random, object> fallback, Random rng) =>
            IsSerializedId(s) ? Deserialize(s) : fallback(s, rng);

        private static object DeserializeIfId<T>(string s, Func<string, T> fallback) => IsSerializedId(s) ? Deserialize(s) : fallback(s);
        public static object DeserializeIfId<T>(string s) where T : ISerializableValue<T> => DeserializeIfId(s, ISerializableValue<T>.FromString);

        private static object DeserializeIfId<T>(string s, Func<string, Random, T> fallback, Random rng) =>
            IsSerializedId(s) ? Deserialize(s) : fallback(s, rng);
        public static object DeserializeIfId<T>(string s, Random rng) where T : ISerializableRandomValue<T> =>
            DeserializeIfId(s, ISerializableRandomValue<T>.FromString, rng);

        public static void Save(string saveName, Simulation simulation) {
            var savePath = SavePath(saveName);
            CreateDirectory(savePath);
            _saveDataWriteStream = CreateText($"{savePath}{SaveDataFile}");
            WriteSaveData(_saveDataWriteStream);
            _saveDataWriteStream.Close();
            SerializerSaveStream = CreateText($"{savePath}{SerializeFile}");
            foreach (var table in simulation.Tables)
                if (!TablesToIgnore.Contains(table))
                    TableToCsv(savePath, table);
            WriteAllTables(savePath, simulation);
            SerializerSaveStream.Close();
            Serializer.ResetIdTable();
        }

        public static void Save(Simulation simulation) => Save(NowString, simulation);

        public static void Load(string saveName, Simulation simulation) {
            var savePath = SavePath(saveName);
            _saveDataReadStream = new StreamReader($"{savePath}{SaveDataFile}");
            ReadSaveData(_saveDataReadStream);
            _saveDataReadStream.Close();
            _deserializerLoadStream = new StreamReader($"{savePath}{SerializeFile}");
            while (!_deserializerLoadStream.EndOfStream) Deserialize(_deserializerLoadStream);
            _deserializerLoadStream.Close();
            foreach (var table in simulation.Tables)
                if (!TablesToIgnore.Contains(table))
                    table.LoadCsv(savePath);
            Deserializer.ResetIdTable();
        }
    }
}
