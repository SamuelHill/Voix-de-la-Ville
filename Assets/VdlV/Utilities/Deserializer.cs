using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using TED.Utilities;
using TED;

namespace VdlV.Utilities {
    using static BindingFlags;
    using static Enum;

    /// <summary>
    /// Reconstructs a data structure from the output of Serializer that is equivalent to the data structure
    /// the Serializer started from.
    /// </summary>
    public static class Deserializer {
        /// <summary>The stream (TextReader) from which we are reading.</summary>
        private static TextReader _reader;

        /// <summary>Table of previously deserialized objects.</summary>
        private static readonly Dictionary<int, object> IdTable = new();

        /// <summary>Reconstruct an object or set of objects from their serialization</summary>
        /// <param name="r">TextReader for a stream containing the serialization</param>
        /// <returns>Copy of the object on which Serialize was originally called</returns>
        public static object Deserialize(TextReader r) {
            _reader = r;
            return ReadObject(-1);
        }

        /// <summary>Reconstruct an object or set of objects from their serialization</summary>
        /// <param name="s">Serialization as a string</param>
        /// <returns>Copy of the object on which Serialize was originally called</returns>
        public static object Deserialize(string s) => Deserialize(new StringReader(s));

        public static void ResetIdTable() => IdTable.Clear();

        /// <summary>
        /// Update the value of a field in an object given the name of the field as a string.
        /// </summary>
        /// <param name="o">Object whose field should be changed</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="newValue">New value to write into the field</param>
        private static void SetFieldByName(object o, string fieldName, object newValue) {
            var t = o.GetType();
            var f = t.GetField(fieldName, Instance | Public | NonPublic) ??
                    throw new MissingFieldException(t.Name, fieldName);
            f.SetValue(o, ParseType(f.FieldType, newValue));
        }

        private static object ParseType(Type t, object value) {
            if (t.IsArray) {
                var convertArray = typeof(Deserializer).GetMethod(nameof(ConvertArray), Public | NonPublic | Instance | Static);
                return convertArray == null ? throw new MissingMethodException($"Couldn't find the method {nameof(ConvertArray)}")
                           : convertArray.MakeGenericMethod(t.GetElementType()).Invoke(null, new[] { value });
            }
            if (!t.IsGenericType) { // If the type is not generic value is returned as is, unless value is a string
                if (value is not string stringValue) return value;
                if (t.IsEnum) return Parse(t, stringValue, true);
                CsvReader.TryParse(t, stringValue, out var result);
                return result ?? value; // If not an Enum OR if the CsvReader Parse failed, return value as is
            }
            var d = t.GetGenericTypeDefinition();
            if (!d.IsAssignableFrom(typeof(List<>))) { // Check if this is a List, if not return value as is
                if (d.IsAssignableFrom(typeof(Dictionary<,>)))
                    throw new NotSupportedException("Dictionaries are not a supported type");
                if (value is not string stringValue) return value;
                CsvReader.TryParse(t, stringValue, out var result);
                return result ?? value;
            }
            var convertList = typeof(Deserializer).GetMethod(nameof(ConvertList), Public | NonPublic | Instance | Static);
            return convertList == null ? throw new MissingMethodException($"Couldn't find the method {nameof(ConvertList)}")
                       : convertList.MakeGenericMethod(t.GetGenericArguments()[0]).Invoke(null, new[] { value });
        }

        private static T ConvertElement<T>(object element) {
            if (element is T typedElement) return typedElement;
            if (typeof(IConvertible).IsAssignableFrom(typeof(T)))
                return (T)Convert.ChangeType(element, typeof(T));
            if (element is string stringValue) { 
                CsvReader.TryParse(typeof(T), stringValue, out var result);
                if (result != null) return (T)result;
            }
            try {
                // ReSharper disable once PossibleInvalidCastException
                return (T)element;
            } catch (InvalidCastException) {
                throw new InvalidCastException($"Cannot convert {element} to type {typeof(T).FullName}");
            }
        }

        private static IEnumerable<T> ConvertEnumerable<T>(object list, string typeName) => list is not List<object> itemList ?
            throw new ArgumentException($"Cannot convert {list} into type {typeName}") : itemList.Select(ConvertElement<T>);

        private static object ConvertList<T>(object list) => ConvertEnumerable<T>(list, $"List<{typeof(T).FullName}>").ToList();

        private static object ConvertArray<T>(object list) => ConvertEnumerable<T>(list, $"{typeof(T).FullName}[]").ToArray();
        

        /// <summary>
        /// Make a new instance of the type with the specified name
        /// The type must have a default constructor, i.e. one without parameters.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private static object MakeInstance(string typeName) {
            var t = Assembly.GetExecutingAssembly().GetType(typeName);
            return t == null
                ? throw new ArgumentException($"Can't find a type named {typeName}")
                : t.InvokeMember(null, Instance | Public | NonPublic | CreateInstance,
                                 null, null, null);
        }

        #region Stream reading error handlers

        /// <summary>True if there's nothing left to read.</summary>
        private static bool End => _reader.Peek() < 0;

        /// <summary>Creates EndOfStreamException with starting text "Stream ended unexpectedly"</summary>
        /// <param name="endedWhile">Description of what was being done when the stream ended</param>
        /// <returns>EndOfStreamException</returns>
        private static EndOfStreamException EndedUnexpectedly(string endedWhile) =>
            new($"Stream ended unexpectedly {endedWhile}");

        /// <summary>Throws EndOfStreamException if the stream ended.</summary>
        /// <param name="enclosingId">Object id of the object we're currently trying to read.</param>
        /// <param name="typeBeingRead">Type we're trying to read.</param>
        private static void EndedStreamWhileReading(int enclosingId, string typeBeingRead) {
            if (End) throw EndedUnexpectedly($"while reading a {typeBeingRead} in object id {enclosingId}");
        }

        /// <summary>Throws EndOfStreamException if the stream ended.</summary>
        /// <param name="enclosingId">Object id of the object we're currently trying to read.</param>
        /// <param name="fieldName">Name of the field we are trying to read.</param>
        private static void EndedStreamAfterField(int enclosingId, string fieldName) {
            if (End) throw EndedUnexpectedly($"after field name {fieldName} in object id {enclosingId}");
        }

        #endregion

        #region TextReader helper functions

        /// <summary>Reads the next character in the serialization data, without moving on to the next character.</summary>
        /// <returns>The next character</returns>
        private static char PeekChar() {
            var peek = _reader.Peek();
            return peek >= 0 ? (char)peek : throw EndedUnexpectedly("while peeking");
        }

        /// <summary>Reads the next character in the serialization data.</summary>
        /// <returns>The current character</returns>
        private static char GetChar() {
            var next = _reader.Read();
            return next >= 0 ? (char)next : throw EndedUnexpectedly("while reading the next character");
        }

        /// <summary>Skips over any spaces, tabs, or newlines, if any.</summary>
        private static void SkipWhitespace() {
            while (!End && char.IsWhiteSpace(PeekChar()))
                GetChar(); // Swallow whitespace
        }

        /// <summary>
        /// Peeks at the next character and returns false if it can't be part of a number or a word,
        /// or is at the end of the stream.
        /// </summary>
        private static bool InsideToken() {
            var p = _reader.Peek();
            if (p < 0) return false;
            var c = (char)p;
            return char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_';
        }

        /// <summary>
        /// Reads the next characters until it finds a character that can't be part of a number or a word.
        /// </summary>
        /// <returns></returns>
        private static string ReadToken() {
            var b = new StringBuilder();
            while (InsideToken()) b.Append(GetChar());
            return b.ToString();
        }

        #endregion

        /// <summary>
        /// Reads a null or Boolean from the input.
        /// Generates an exception if the next token in the input is not in fact "null", "True", or "False".
        /// </summary>
        /// <param name="enclosingId">Object id of the object we're currently trying to read.</param>
        /// <returns></returns>
        private static object ReadSpecialName(int enclosingId) {
            var token = ReadToken();
            return token switch {
                "null" => null,
                "True" => true,
                "False" => false,
                _ => throw new Exception($"Unknown token found while reading object id {enclosingId}: {token}")
            };
        }

        /// <summary>
        /// Returns the next number in the input.
        /// Generates an exception if the next characters are not in fact a number.
        /// </summary>
        /// <param name="enclosingId">Object id of the object we're currently trying to read.</param>
        /// <returns></returns>
        private static object ReadNumber(int enclosingId) {
            var token = ReadToken();
            return int.TryParse(token, out var i) ? i :
                   float.TryParse(token, out var f) ? f :
                   throw new Exception($"Unknown number format while reading object {enclosingId}: {token}");
        }

        /// <summary>
        /// Reads a string value from the input. The first character must be a quote.
        /// It return all the characters between it and the following quote.
        /// </summary>
        /// <param name="enclosingId">Object id of the object we're currently trying to read.</param>
        /// <returns></returns>
        // TODO: this doesn't understand backslashes to escape quotes in a string.
        private static object ReadString(int enclosingId) {
            GetChar();  // Swallow the quote
            var b = new StringBuilder();
            while (!End && PeekChar() != '"')
                b.Append(GetChar());
            EndedStreamWhileReading(enclosingId, "string");
            GetChar();  // Swallow the quote
            return b.ToString();
        }

        /// <summary>Read whatever data object is next in the stream</summary>
        /// <param name="enclosingId">Object id of the object we're currently trying to read.</param>
        /// <returns>The deserialized object</returns>
        public static object ReadObject(int enclosingId) {
            SkipWhitespace();
            EndedStreamWhileReading(enclosingId, "object");
            return PeekChar() switch {
                '#' => ReadComplexObject(enclosingId),
                '[' => ReadList(enclosingId),
                '(' => ReadTuple(enclosingId),
                '"' => ReadString(enclosingId),
                '-' => ReadNumber(enclosingId),
                '.' => ReadNumber(enclosingId),
                var c when char.IsDigit(c) => ReadNumber(enclosingId),
                var c when char.IsLetter(c) => ReadSpecialName(enclosingId),
                _ => throw new Exception($"Unexpected character {PeekChar()} found while reading object id {enclosingId}")
            };
        }

        /// <summary>Reads a list represented in [ ] format and returns its data as an object list.</summary>
        /// <param name="enclosingId">Object id of the object we're currently trying to read.</param>
        private static List<object> ReadList(int enclosingId) {
            var result = new List<object>();
            GetChar(); // Swallow open bracket
            SkipWhitespace();
            while (!End && PeekChar() != ']') {
                result.Add(ReadObject(enclosingId));
                SkipWhitespace();
                EndedStreamWhileReading(enclosingId, "list");
                var c = PeekChar();
                if (c != ',' && c != ']')
                    throw new Exception($"Expected comma between list elements, but got {c} while reading object id {enclosingId}");
                if (c == ',') GetChar(); // Swallow comma
                SkipWhitespace();
            }
            EndedStreamWhileReading(enclosingId, "list");
            GetChar(); // Swallow close bracket
            return result;
        }

        /// <summary>Reads a tuple represented in ( ) format and returns its data as an object tuple.</summary>
        /// <param name="enclosingId">Object id of the object we're currently trying to read.</param>
        // TODO : UNTESTED!! Maybe this works, added to fix Csv read issue when the bug was really in TED.CsvReader
        private static ValueTuple ReadTuple(int enclosingId) {
            var result = new List<object>();
            GetChar(); // Swallow open bracket
            SkipWhitespace();
            while (!End && PeekChar() != ')') {
                result.Add(ReadObject(enclosingId));
                SkipWhitespace();
                EndedStreamWhileReading(enclosingId, "tuple");
                var c = PeekChar();
                if (c != ',' && c != ')')
                    throw new Exception($"Expected comma between tuple elements, but got {c} while reading object id {enclosingId}");
                if (c == ',') GetChar(); // Swallow comma
                SkipWhitespace();
            }
            EndedStreamWhileReading(enclosingId, "tuple");
            GetChar(); // Swallow close bracket
            
            var generic = result.Count switch {
                1 => typeof(ValueTuple<>),
                2 => typeof(ValueTuple<,>),
                3 => typeof(ValueTuple<,,>),
                4 => typeof(ValueTuple<,,,>),
                5 => typeof(ValueTuple<,,,,>),
                6 => typeof(ValueTuple<,,,,,,>),
                7 => typeof(ValueTuple<,,,,,,>),
                8 => typeof(ValueTuple<,,,,,,,>),
                _ => throw new ArgumentException($"Cannot create a tuple with {result.Count} elements")
            };
            var types = result.Select(v => v.GetType()).ToArray();
            var t = generic.MakeGenericType(types);
            return (ValueTuple)t.InvokeMember(null, CreateInstance, null, null, result.ToArray());
        }

        /// <summary>Reads an object field in the format "fieldName: value".</summary>
        /// <param name="enclosingId">Object id of the object we're currently trying to read.</param>
        /// <returns>A tuple of the field's name and the field's value</returns>
        private static (string fieldName, object value) ReadField(int enclosingId) {
            SkipWhitespace();
            var fieldName = ReadToken();
            EndedStreamAfterField(enclosingId, fieldName);
            var c = GetChar();
            if (c != ':')
                throw new Exception($"Expected a colon after {fieldName} in object id {enclosingId}");
            SkipWhitespace();
            EndedStreamAfterField(enclosingId, fieldName);
            var value = ReadObject(enclosingId);
            SkipWhitespace();
            if (End || PeekChar() != ',') return (fieldName, value);
            GetChar();        // Swallow comma
            SkipWhitespace(); // TODO : is this needed?
            return (fieldName, value);
        }

        /// <summary>
        /// Called when the next character is a #.  Read the object id of the object and return the
        /// object.  If that object id has already been read, return the object previously returned.
        /// Otherwise, there will be a { } expression after the object id.  Read it, create the object
        /// it represents, and return it.
        /// </summary>
        /// <param name="enclosingId">Object id of the object we're currently trying to read.</param>
        /// <returns>The object referred to by this #id expression.</returns>
        private static object ReadComplexObject(int enclosingId) {
            GetChar();  // Swallow the #
            var id = int.Parse(ReadNumber(enclosingId).ToString());
            SkipWhitespace();
            if (IdTable.TryGetValue(id, out var serializedObject)) return serializedObject;
            SkipWhitespace();
            if (End) throw EndedUnexpectedly($"after reference to unknown ID {id}");
            var c = GetChar();
            if (c != '{') throw new Exception($"Expected '{'{'}' after #{id} but instead got {c}");
            (var hopefullyType, var typeName) = ReadField(id);
            if (hopefullyType != "type")
                throw new Exception($"Expected type name at the beginning of complex object id {id} but instead got {typeName}");
            if (typeName is not string type)
                throw new Exception($"Expected a type name (a string) in 'type: ...' expression for object id {id}, but instead got {typeName}");
            var toReturn = MakeInstance(type);
            IdTable[id] = toReturn;
            while (!End && PeekChar() != '}') {
                (var field, var value) = ReadField(id);
                SetFieldByName(toReturn, field, value);
            }
            EndedStreamWhileReading(enclosingId, "{ } expression");
            GetChar();  // Swallow close bracket
            return toReturn;
        }
    }
}
