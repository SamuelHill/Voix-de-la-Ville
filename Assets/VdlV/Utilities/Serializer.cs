using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using TED.Utilities;

namespace VdlV.Utilities {
    using static BindingFlags;
    using static CsvWriter;
    using static SaveManager;

    public static class Serializer {
        /// <summary>The stream (TextWriter) to write to.</summary>
        private static TextWriter _writer;

        /// <summary>How much to indent after a newline.</summary>
        private static int _indentLevel;
        /// <summary>How many spaces make up an indent.</summary>
        private const int IndentAmount = 4;

        /// <summary>Table of objects and their id numbers that have already been serialized.</summary>
        private static readonly Dictionary<object, int> IdTable = new();
        /// <summary>id number to give to the next object that we serialize</summary>
        private static int _idCounter;

        /// <summary>Serialize an object to a text stream</summary>
        /// <param name="o">Object to write</param>
        /// <param name="w">Stream to write it to</param>
        public static int Serialize(object o, TextWriter w) {
            _writer = w;
            _writer.Flush();
            return WriteObject(o);
        }

        /// <summary>Serialize an object to a string</summary>
        /// <param name="o">Object to serialize</param>
        /// <returns>ID assigned to the object (-1 if no ID assigned)
        /// and the serialized form of the object</returns>
        public static (int, string) Serialize(object o) {
            var w = new StringWriter();
            var id = Serialize(o, w);
            return (id, w.ToString());
        }

        /// <summary>
        /// Return the ID number attached to this object for this serialization.
        /// If the object hasn't already been assigned an ID, assign one and return (id, true).
        /// Otherwise, use the previously assigned id and return (id, false).
        /// </summary>
        /// <param name="o">Object to be output</param>
        /// <returns>ID assigned to the object and whether that id was just assigned or
        /// had already been assigned before this call.</returns>
        private static (int, bool) GetId(object o) {
            if (IdTable.TryGetValue(o, out var id)) return (id, false);
            id = _idCounter++;
            IdTable[o] = id;
            return (id, true);
        }

        private static IEnumerable<KeyValuePair<string, object>> SerializeFields(object toSerialize) {
            // Crawl up the type hierarchy
            for (var type = toSerialize.GetType(); type != null && type != typeof(object); type = type.BaseType) {
                foreach (var f in type.GetFields(Instance | Public | NonPublic | DeclaredOnly))
                    // Check if that field needs to be serialized
                    if (f.IsPublic || f.GetCustomAttribute<SerializeOnSave>() != null)
                        yield return new KeyValuePair<string, object>(f.Name, f.GetValue(toSerialize));
            }
        }

        #region Write "overrides"

        /// <summary>Write a string to output.</summary>
        /// <param name="s">String to write</param>
        private static void Write(string s) => _writer.Write(s);

        /// <summary>Write an integer to output.</summary>
        /// <param name="s">Number to write</param>
        private static void Write(sbyte s) => _writer.Write(s.ToString(CultureInfo.InvariantCulture));

        /// <summary>Write an integer to output.</summary>
        /// <param name="b">Number to write</param>
        private static void Write(byte b) => _writer.Write(b.ToString(CultureInfo.InvariantCulture));

        /// <summary>Write an integer to output.</summary>
        /// <param name="s">Number to write</param>
        private static void Write(short s) => _writer.Write(s.ToString(CultureInfo.InvariantCulture));

        /// <summary>Write an integer to output.</summary>
        /// <param name="u">Number to write</param>
        private static void Write(ushort u) => _writer.Write(u.ToString(CultureInfo.InvariantCulture));

        /// <summary>Write an integer to output.</summary>
        /// <param name="i">Number to write</param>
        private static void Write(int i) => _writer.Write(i.ToString(CultureInfo.InvariantCulture));

        /// <summary>Write an integer to output.</summary>
        /// <param name="u">Number to write</param>
        private static void Write(uint u) => _writer.Write(u.ToString(CultureInfo.InvariantCulture));

        /// <summary>Write an integer to output.</summary>
        /// <param name="l">Number to write</param>
        private static void Write(long l) => _writer.Write(l.ToString(CultureInfo.InvariantCulture));

        /// <summary>Write an integer to output.</summary>
        /// <param name="u">Number to write</param>
        private static void Write(ulong u) => _writer.Write(u.ToString(CultureInfo.InvariantCulture));

        /// <summary>Write a float to output.</summary>
        /// <param name="f">Number to write</param>
        private static void Write(float f) => _writer.Write(f.ToString(CultureInfo.InvariantCulture));

        /// <summary>Write a float to output.</summary>
        /// <param name="d">Number to write</param>
        private static void Write(double d) => _writer.Write(d.ToString(CultureInfo.InvariantCulture));

        /// <summary>Write a character to output.</summary>
        /// <param name="c">Character to write</param>
        private static void Write(char c) => _writer.Write(c);

        /// <summary>Write a Boolean to output.</summary>
        /// <param name="b">Boolean to write</param>
        private static void Write(bool b) => _writer.Write(b);

        #endregion

        /// <summary>Start a new line in the output and indent to indentLevel</summary>
        private static void NewLine() {
            _writer.WriteLine(); 
            Write(new string(' ', IndentAmount * _indentLevel));
            /* Need the indent after the newline to handle variable indent level in
            WriteBracketedExpression - specifically after the call to generator()...
            The options are; make every generator method end in a line terminator
            (which is obnoxious) OR always call WriteLine after generator. If you
            always call WriteLine and need to indent the end bracket anyway combining
            the two functions into one call makes sense, and if you already have a
            function that does both things you can structure the other few uses of
            a newline to follow this norm. */
        }

        /// <summary>
        /// Write a bracketed expression whose contents is produced by the specified generator procedure.
        /// </summary>
        /// <param name="start">Open bracket to use</param>
        /// <param name="generator">Procedure to print the contents inside the brackets</param>
        /// <param name="end">Close bracket to use</param>
        private static void WriteBracketedExpression(string start, Action generator, string end) {
            Write(start);
            _indentLevel++;
            NewLine();
            generator();
            _indentLevel--;
            NewLine();
            Write(end);
        }

        /// <summary>Write the serialization data for the specified object.</summary>
        /// <param name="o">Object to serialize</param>
        private static int WriteObject(object o) {
            switch (o) {
                case null:         Write("null"); break;
                case sbyte s:      Write(s); break;
                case byte b:       Write(b); break;
                case short s:      Write(s); break;
                case ushort u:     Write(u); break;
                case int i:        Write(i); break;
                case uint u:       Write(u); break;
                case long l:       Write(l); break;
                case ulong u:      Write(u); break;
                case float f:      Write(f); break;
                case double d:     Write(d); break;
                case string s:     Write($"\"{s}\""); break; 
                case char c:       Write(c); break;
                case bool b:       Write(b); break;
                case IList list:   WriteList(list); break;
                case ITuple tuple: WriteTuple(tuple); break;
                default:
                    if (!o.GetType().IsValueType) return WriteComplexObject(o);
                    if (o is ISerializableValue || o.GetType().IsEnum) {
                        Write(o.ToString()); break; }
                    // ReSharper disable once InvertIf
                    if (HasDeclaredWriter(o.GetType())) {
                        Write(GetDeclaredWriter(o.GetType())(o)); break; }
                    throw new ArgumentException($"Can't write unsupported value type: {o.GetType().Name}");
            }
            return -1;
        }

        /// <summary>
        /// Write a list or array in the format: [ elt, elt, elt, ... ]
        /// </summary>
        /// <param name="list"></param>
        private static void WriteList(ICollection list) {
            if (list.Count == 0) Write("[ ]");
            else WriteBracketedExpression("[ ",
                () => {
                    var firstItem = true;
                    foreach (var item in list) {
                        if (firstItem) firstItem = false;
                        else Write(", ");
                        WriteObject(item);
                    }
                },
                " ]");
        }

        /// <summary>
        /// Write a tuple in the format: ( elt, elt, elt, ... )
        /// </summary>
        /// <param name="tuple"></param>
        private static void WriteTuple(ITuple tuple) {
            if (tuple.Length == 0) Write("( )");
            else WriteBracketedExpression("( ",
                () => {
                    var firstItem = true;
                    for (var i = 0; i < tuple.Length; i++) {
                        if (firstItem) firstItem = false;
                        else Write(", ");
                        WriteObject(tuple[i]);
                    }
                },
                " )");
        }

        /// <summary>Write a field of an object in format "fieldName: fieldValue"</summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="fieldValue">Value of the field</param>
        /// <param name="firstOne">If this is the first field printed inside of the object.
        /// This is just so it knows whether to print a comma before the field</param>
        private static void WriteField(string fieldName, object fieldValue, bool firstOne) {
            if (!firstOne) { // Simultaneously handles single fields (no comma) AND
                // fits the NewLine format of being called before the content that
                // needs to be indented on the next line.
                Write(",");
                NewLine();
            }
            Write(fieldName);
            Write(": ");
            WriteObject(fieldValue);
        }

        /// <summary>
        /// Serialize a complex object (i.e. a class object)
        /// If this object has already been output, just output #id, where is is it's id from GetID.
        /// If it hasn't then output #id { type: "typename", field: value ... }
        /// </summary>
        /// <param name="o">Object to serialize</param>
        private static int WriteComplexObject(object o) {
            (var id, var isNew) = GetId(o);
            if (isNew || _writer != SaveStream) {
                Write("#");
                Write(id);
            }
            if (!isNew) return id;
            var dict = SerializeFields(o);
            WriteBracketedExpression("{ ", 
                () => {
                    WriteField("type", o.GetType().FullName, true);
                    foreach ((var k, var v) in dict) 
                        WriteField(k, v, false);
                },
                " }");
            return id;
        }
    }
}
