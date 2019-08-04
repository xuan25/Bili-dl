using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace JsonUtil
{
    public class Json
    {
        #region Value

        public abstract class Value : IEnumerable
        {
            #region Internal fileds

            internal object Data { get; set; }
            internal enum ValueType { Object, Array, String, Boolean, Number, Null }
            internal ValueType Type { get; set; }

            #endregion

            #region Public fileds

            /// <summary>
            /// Gets the number of key/value pairs or elements contained in the <see cref="Object"/> or <see cref="Array"/>.
            /// </summary>
            public int Count
            {
                get
                {
                    switch (Type)
                    {
                        case ValueType.Object:
                            return ((Dictionary<string, Value>)Data).Count;
                        case ValueType.Array:
                            return ((List<Value>)Data).Count;
                        default:
                            throw new InvalidOperationException("Count property only exists in Objects or Arrays.");
                    }
                }
            }

            /// <summary>
            /// Adds the specified kay and value to the <see cref="Object"/>.
            /// </summary>
            /// <param name="key">The key of the element to add.</param>
            /// <param name="value">The value of the element to add.</param>
            public void Add(string key, Value value)
            {
                switch (Type)
                {
                    case ValueType.Object:
                        ((Dictionary<string, Value>)Data).Add(key, value);
                        break;
                    case ValueType.Array:
                        throw new InvalidOperationException("Array does not support key.");
                    default:
                        throw new InvalidOperationException("Add method can only applies on Objects or Arrays.");
                }
            }

            /// <summary>
            /// Adds an element to the end of the <see cref="Array"/>.
            /// </summary>
            /// <param name="element">The element to be added to the end of the <see cref="Array"/>.</param>
            public void Add(Value element)
            {
                switch (Type)
                {
                    case ValueType.Object:
                        throw new InvalidOperationException("Object elements need provides a key.");
                    case ValueType.Array:
                        ((List<Value>)Data).Add(element);
                        break;
                    default:
                        throw new InvalidOperationException("Add method can only applies on Objects or Arrays.");
                }
            }

            /// <summary>
            /// Adds the elements of the specified collection to the end of the <see cref="Array"/>.
            /// </summary>
            /// <param name="collection">The collection whose elements should be added to the end of the <see cref="Array"/>.</param>
            public void AddRange(IEnumerable<Value> collection)
            {
                switch (Type)
                {
                    case ValueType.Array:
                        ((List<Value>)Data).AddRange(collection);
                        break;
                    default:
                        throw new InvalidOperationException("AddRange method can only applies on Arrays.");
                }
            }

            /// <summary>
            /// Inserts an element into the <see cref="Array"/> at specified index.
            /// </summary>
            /// <param name="index">The zero-based index at which element should be inserted.</param>
            /// <param name="element">The element to insert.</param>
            public void Insert(int index, Value element)
            {
                switch (Type)
                {
                    case ValueType.Array:
                        ((List<Value>)Data).Insert(index, element);
                        break;
                    default:
                        throw new InvalidOperationException("Insert method can only applies on Arrays.");
                }
            }

            /// <summary>
            /// Inserts the elements of a collection into the <see cref="Array"/> at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
            /// <param name="collection">The collection whose elements should be insterted into the <see cref="Array"/>.</param>
            public void InsertRange(int index, IEnumerable<Value> collection)
            {
                switch (Type)
                {
                    case ValueType.Array:
                        ((List<Value>)Data).InsertRange(index, collection);
                        break;
                    default:
                        throw new InvalidOperationException("InsertRange method can only applies on Arrays.");
                }
            }

            /// <summary>
            /// Removes the value with the specified key form the <see cref="Object"/>.
            /// </summary>
            /// <param name="key">The key of the element to remove.</param>
            public void Remove(string key)
            {
                switch (Type)
                {
                    case ValueType.Object:
                        ((Dictionary<string, Value>)Data).Remove(key);
                        break;
                    case ValueType.Array:
                        throw new InvalidOperationException("Array need provides an index.");
                    default:
                        throw new InvalidOperationException("Remove method can only applies on Objects or Arrays.");
                }
            }

            /// <summary>
            /// Removes the element at the specified index of the <see cref="Array"/>.
            /// </summary>
            /// <param name="index">The zero-based index of the element to remove.</param>
            public void Remove(int index)
            {
                switch (Type)
                {
                    case ValueType.Object:
                        throw new InvalidOperationException("Object need provides an key.");
                    case ValueType.Array:
                        ((List<Value>)Data).RemoveAt(index);
                        break;
                    default:
                        throw new InvalidOperationException("Remove method can only applies on Objects or Arrays.");
                }
            }

            /// <summary>
            /// Removes a range of elements form the <see cref="Array"/>
            /// </summary>
            /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
            /// <param name="count">The number of elements to remove.</param>
            public void RemoveRange(int index, int count)
            {
                switch (Type)
                {
                    case ValueType.Array:
                        ((List<Value>)Data).RemoveRange(index, count);
                        break;
                    default:
                        throw new InvalidOperationException("Remove method can only applies on Arrays.");
                }
            }

            /// <summary>
            /// Removes all keys and values or elements from the <see cref="Object"/> or <see cref="Array"/>.
            /// </summary>
            public void Clear()
            {
                switch (Type)
                {
                    case ValueType.Object:
                        ((Dictionary<string, Value>)Data).Clear();
                        break;
                    case ValueType.Array:
                        ((List<Value>)Data).Clear();
                        break;
                    default:
                        throw new InvalidOperationException("Remove method can only applies on Objects or Arrays.");
                }
            }

            /// <summary>
            /// Determines whether the <see cref="Object"/> contains the specified key.
            /// </summary>
            /// <param name="key">The key to locate in the <see cref="Object"/></param>
            /// <returns></returns>
            public bool Contains(string key)
            {
                switch (Type)
                {
                    case ValueType.Object:
                        return ((Dictionary<string, Value>)Data).ContainsKey(key);
                    default:
                        throw new InvalidOperationException("Contains method can only applies on Objects.");
                }
            }

            #endregion

            #region IEnumerable and Indexer

            public virtual IEnumerator GetEnumerator()
            {
                throw new InvalidOperationException();
            }

            public virtual Value this[int index]
            {
                get
                {
                    throw new InvalidOperationException();
                }
                set
                {
                    throw new InvalidOperationException();
                }
            }

            public virtual Value this[string index]
            {
                get
                {
                    throw new InvalidOperationException();
                }
                set
                {
                    throw new InvalidOperationException();
                }
            }

            #endregion

            #region Cast

            public static implicit operator bool(Value obj)
            {
                return (bool)obj.Data;
            }

            public static implicit operator byte(Value obj)
            {
                return (byte)(double)obj.Data;
            }

            public static implicit operator char(Value obj)
            {
                return (char)(double)obj.Data;
            }

            public static implicit operator decimal(Value obj)
            {
                return new decimal((double)obj.Data);
            }

            public static implicit operator double(Value obj)
            {
                return (double)obj.Data;
            }

            public static implicit operator float(Value obj)
            {
                return (float)(double)obj.Data;
            }

            public static implicit operator int(Value obj)
            {
                return (int)(double)obj.Data;
            }

            public static implicit operator long(Value obj)
            {
                return (long)(double)obj.Data;
            }

            public static implicit operator sbyte(Value obj)
            {
                return (sbyte)(double)obj.Data;
            }

            public static implicit operator short(Value obj)
            {
                return (short)(double)obj.Data;
            }

            public static implicit operator uint(Value obj)
            {
                return (uint)(double)obj.Data;
            }

            public static implicit operator ulong(Value obj)
            {
                return (ulong)(double)obj.Data;
            }

            public static implicit operator ushort(Value obj)
            {
                return (ushort)(double)obj.Data;
            }

            public static implicit operator string(Value obj)
            {
                return (string)obj.Data;
            }

            public static implicit operator Dictionary<string, Value>(Value obj)
            {
                return (Dictionary<string, Value>)obj.Data;
            }

            public static implicit operator List<Value>(Value obj)
            {
                return (List<Value>)obj.Data;
            }

            #endregion

            #region Operator =

            public static implicit operator Value(bool value)
            {
                return new Boolean(value);
            }

            public static implicit operator Value(byte value)
            {
                return new Number(value);
            }

            public static implicit operator Value(char value)
            {
                return new Number(value);
            }

            public static implicit operator Value(decimal value)
            {
                return new Number((double)value);
            }

            public static implicit operator Value(double value)
            {
                return new Number(value);
            }

            public static implicit operator Value(float value)
            {
                return new Number(value);
            }

            public static implicit operator Value(int value)
            {
                return new Number(value);
            }

            public static implicit operator Value(long value)
            {
                return new Number(value);
            }

            public static implicit operator Value(sbyte value)
            {
                return new Number(value);
            }

            public static implicit operator Value(short value)
            {
                return new Number(value);
            }

            public static implicit operator Value(uint value)
            {
                return new Number(value);
            }

            public static implicit operator Value(ulong value)
            {
                return new Number(value);
            }

            public static implicit operator Value(ushort value)
            {
                return new Number(value);
            }

            public static implicit operator Value(string value)
            {
                return new String(value);
            }

            public static implicit operator Value(Dictionary<string, Value> value)
            {
                return new Object(value);
            }

            public static implicit operator Value(List<Value> value)
            {
                return new Array(value);
            }

            #endregion

            #region Equals and HashCode

            public static bool Equals(Value objA, Value objB)
            {
                if (objA == null)
                    return Equals(objB, (object)objA);
                if (objB == null)
                    return Equals(objA, (object)objB);

                if (objA.Type == objB.Type)
                {
                    if (objA.Type == ValueType.Null)
                        return true;
                    if (objA.Type == ValueType.String)
                        return (string)objA == (string)objB;
                    if (objA.Type == ValueType.Boolean)
                        return (bool)objA == (bool)objB;
                    if (objA.Type == ValueType.Number)
                        return (double)objA == (double)objB;
                }
                return false;
            }

            public static bool Equals(Value objA, object objB)
            {
                if (objB == null)
                    return Equals((object)objA, objB);

                if (typeof(Value).IsAssignableFrom(objB.GetType()))
                    return Equals(objA, (Value)objB);

                if (objA.Type == ValueType.String && objB.GetType() == typeof(string))
                    return (string)objA == (string)objB;

                if (objA.Type == ValueType.Boolean && objB.GetType() == typeof(bool))
                    return (bool)objA == (bool)objB;

                if (objA.Type == ValueType.Number && (objB.GetType() == typeof(byte) ||
                                                      objB.GetType() == typeof(char) ||
                                                      objB.GetType() == typeof(double) ||
                                                      objB.GetType() == typeof(float) ||
                                                      objB.GetType() == typeof(int) ||
                                                      objB.GetType() == typeof(long) ||
                                                      objB.GetType() == typeof(sbyte) ||
                                                      objB.GetType() == typeof(short) ||
                                                      objB.GetType() == typeof(uint) ||
                                                      objB.GetType() == typeof(ulong) ||
                                                      objB.GetType() == typeof(ushort)))
                    return (double)objA == Convert.ToDouble(objB);

                if (objA.Type == ValueType.Number && objB.GetType() == typeof(decimal))
                    return (decimal)objA == (decimal)objB;
                return false;
            }

            public new static bool Equals(object objA, object objB)
            {
                if (objA == null && objB == null)
                    return true;
                if (objA == null)
                    return objB.GetType() == typeof(Null);
                if (objB == null)
                    return objA.GetType() == typeof(Null);

                if (typeof(Value).IsAssignableFrom(objA.GetType()) && typeof(Value).IsAssignableFrom(objB.GetType()))
                    return Equals((Value)objA, (Value)objB);
                if (typeof(Value).IsAssignableFrom(objA.GetType()))
                    return Equals((Value)objA, objB);
                if (typeof(Value).IsAssignableFrom(objB.GetType()))
                    return Equals((Value)objB, objA);
                return false;
            }

            public override bool Equals(object obj)
            {
                return Equals(this, obj);
            }

            public override int GetHashCode()
            {
                Type type = GetType();
                if (type == typeof(String))
                    return ((string)this).GetHashCode();
                if (type == typeof(Boolean))
                    return ((bool)this).GetHashCode();
                if (type == typeof(Number))
                    return ((double)this).GetHashCode();
                if (type == typeof(Null))
                    throw new NullReferenceException();
                return base.GetHashCode();
            }

            #endregion

            #region Operator ==

            public static bool operator ==(Value objA, object objB)
            {
                return Equals(objA, objB);
            }

            public static bool operator !=(Value objA, object objB)
            {
                return !Equals(objA, objB);
            }

            #endregion

            #region Object

            public class Object : Value
            {
                public Object()
                {
                    Type = ValueType.Object;
                    Data = new Dictionary<string, Value>();
                }

                public Object(Dictionary<string, Value> dic)
                {
                    Type = ValueType.Object;
                    Data = dic;
                }

                public override IEnumerator GetEnumerator()
                {
                    return ((Dictionary<string, Value>)Data).GetEnumerator();
                }

                public override Value this[string index]
                {
                    get
                    {
                        return ((Dictionary<string, Value>)Data)[index];
                    }
                    set
                    {
                        ((Dictionary<string, Value>)Data)[index] = value;
                    }
                }

                public override string ToString()
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append('{');
                    bool split = false;
                    foreach (KeyValuePair<string, Value> keyValuePair in (Dictionary<string, Value>)Data)
                    {
                        if (split)
                        {
                            stringBuilder.Append(string.Format(",\"{0}\":{1}", keyValuePair.Key, keyValuePair.Value));
                        }
                        else
                        {
                            split = true;
                            stringBuilder.Append(string.Format("\"{0}\":{1}", keyValuePair.Key, keyValuePair.Value));
                        }
                    }
                    stringBuilder.Append('}');
                    return stringBuilder.ToString();
                }
            }

            #endregion

            #region Array

            public class Array : Value
            {
                public Array()
                {
                    Type = ValueType.Array;
                    Data = new List<Value>();
                }

                public Array(List<Value> list)
                {
                    Type = ValueType.Array;
                    Data = list;
                }

                public override IEnumerator GetEnumerator()
                {
                    return ((List<Value>)Data).GetEnumerator();
                }

                public override Value this[int index]
                {
                    get
                    {
                        return ((List<Value>)Data)[index];
                    }
                    set
                    {
                        ((List<Value>)Data)[index] = value;
                    }
                }

                public override string ToString()
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append('[');
                    bool split = false;
                    foreach (Value jsonValue in (List<Value>)Data)
                    {
                        if (split)
                        {
                            stringBuilder.Append(string.Format(",{0}", jsonValue));
                        }
                        else
                        {
                            split = true;
                            stringBuilder.Append(string.Format("{0}", jsonValue));
                        }
                    }
                    stringBuilder.Append(']');
                    return stringBuilder.ToString();
                }
            }

            #endregion

            #region String

            public class String : Value
            {
                public String()
                {
                    Type = ValueType.String;
                    Data = null;
                }

                public String(string value)
                {
                    Type = ValueType.String;
                    Data = value;
                }

                public override string ToString()
                {
                    return string.Format("\"{0}\"", (string)Data);
                }
            }

            #endregion

            #region Boolean

            public class Boolean : Value
            {
                public Boolean()
                {
                    Type = ValueType.Boolean;
                    Data = false;
                }

                public Boolean(bool value)
                {
                    Type = ValueType.Boolean;
                    Data = value;
                }

                public override string ToString()
                {
                    return (bool)Data ? "true" : "false";
                }
            }

            #endregion

            #region Number

            public class Number : Value
            {
                public Number()
                {
                    Type = ValueType.Number;
                    Data = new double();
                }

                public Number(double value)
                {
                    Type = ValueType.Number;
                    Data = value;
                }

                public override string ToString()
                {
                    return ((double)Data).ToString();
                }
            }

            #endregion

            #region Null

            public class Null : Value
            {
                public Null()
                {
                    Type = ValueType.Null;
                    Data = null;
                }

                public override string ToString()
                {
                    return "null";
                }
            }

            #endregion
        }

        #endregion

        #region Parser

        public class Parser
        {
            #region Public methods

            /// <summary>
            /// Parse a json text to a json object or json array
            /// </summary>
            /// <param name="json">The json text need to been parse</param>
            /// <returns>A json object or json array</returns>
            public static Value Parse(string json)
            {
                if (json == null || string.IsNullOrWhiteSpace(json))
                    return null;
                using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    return Parse(stream);
                }
            }

            /// <summary>
            /// Parse a json stream to a json object or json array
            /// </summary>
            /// <param name="stream">The json stream need to been parse</param>
            /// <returns>A json object or json array</returns>
            public static Value Parse(Stream stream)
            {
                if (stream == null)
                    return null;
                stringBuilder = new StringBuilder();
                StreamReader streamReader = new StreamReader(stream);
                Value jsonValue = StartParse(streamReader);
                return jsonValue;
            }

            #endregion

            #region Exception

            public class JsonParseException : Exception
            {
                public long Position { get; set; }

                public JsonParseException(string message, long position) : base(message)
                {
                    Position = position;
                }
            }

            #endregion

            #region Private fileds

            private static StringBuilder stringBuilder;

            #endregion

            #region Private methods

            private static Value StartParse(StreamReader streamReader)
            {
                int buffer = ReadNextMark(streamReader);
                switch (buffer)
                {
                    case '{':
                        return ParseObject(streamReader, out _);
                    case '[':
                        return ParseArray(streamReader, out _);
                    case -1:
                    default:
                        return null;
                }
            }

            private static Value ParseObject(StreamReader streamReader, out int endMark)
            {
                Dictionary<string, Value> dic = new Dictionary<string, Value>();
                while (true)
                {
                    ParseKeyValuePaire(streamReader, out string key, out Value value, out int end);
                    if (key != null)
                        dic.Add(key, value);
                    if (end == '}')
                        break;
                    else if (end != ',')
                        throw new JsonParseException("Expected Object ending not found", streamReader.BaseStream.Position);
                }
                endMark = ReadNextMark(streamReader);
                return dic;
            }

            private static Value ParseArray(StreamReader streamReader, out int endMark)
            {
                List<Value> list = new List<Value>();
                while (true)
                {
                    Value jsonValue = ParseValue(streamReader, out int end, out bool success);
                    if (success)
                        list.Add(jsonValue);
                    if (end == ']' || !success)
                        break;
                    else if (end != ',')
                        throw new JsonParseException("Expected Array ending not found", streamReader.BaseStream.Position);
                }
                endMark = ReadNextMark(streamReader);
                return list;
            }

            private static void ParseKeyValuePaire(StreamReader streamReader, out string key, out Value value, out int endMark)
            {
                key = ParseKey(streamReader, out int split, out bool success);
                if (!success)
                {
                    value = null;
                    endMark = split;
                    return;
                }
                if (split != ':')
                    throw new JsonParseException("Expected key/value spliter not found", streamReader.BaseStream.Position);
                value = ParseValue(streamReader, out endMark, out _);
            }

            private static string ParseKey(StreamReader streamReader, out int splitMark, out bool success)
            {
                stringBuilder.Clear();
                int buffer = ReadNextMark(streamReader);
                switch (buffer)
                {
                    case -1:
                        throw new JsonParseException("Unexpacted key ending", streamReader.BaseStream.Position);
                    case '}':
                        splitMark = buffer;
                        success = false;
                        return null;
                    case '"':
                        while (true)
                        {
                            buffer = streamReader.Read();
                            switch (buffer)
                            {
                                case -1:
                                    throw new JsonParseException("Unexpacted key ending", streamReader.BaseStream.Position);
                                case '\\':
                                    stringBuilder.Append((char)buffer);
                                    buffer = streamReader.Read();
                                    if (buffer == -1)
                                        throw new JsonParseException("Unexpacted key escape ending", streamReader.BaseStream.Position);
                                    stringBuilder.Append((char)buffer);
                                    break;
                                case '"':
                                    splitMark = ReadNextMark(streamReader);
                                    success = true;
                                    return Regex.Unescape(stringBuilder.ToString());
                                default:
                                    stringBuilder.Append((char)buffer);
                                    break;
                            }
                        }
                    default:
                        throw new JsonParseException("Unexpacted key format", streamReader.BaseStream.Position);
                }
            }

            private static Value ParseValue(StreamReader streamReader, out int endMark, out bool success)
            {
                stringBuilder.Clear();
                int buffer = ReadNextMark(streamReader);
                switch (buffer)
                {
                    case -1:
                        throw new JsonParseException("Unexpacted value ending", streamReader.BaseStream.Position);
                    case ']':
                        endMark = buffer;
                        success = false;
                        return null;
                    case '{':
                        success = true;
                        return ParseObject(streamReader, out endMark);
                    case '[':
                        success = true;
                        return ParseArray(streamReader, out endMark);
                    case '"':
                        while (true)
                        {
                            buffer = streamReader.Read();
                            switch (buffer)
                            {
                                case -1:
                                    throw new JsonParseException("Unexpacted value ending", streamReader.BaseStream.Position);
                                case '\\':
                                    stringBuilder.Append((char)buffer);
                                    buffer = streamReader.Read();
                                    if (buffer == -1)
                                        throw new JsonParseException("Unexpacted value escape ending", streamReader.BaseStream.Position);
                                    stringBuilder.Append((char)buffer);
                                    break;
                                case '"':
                                    endMark = ReadNextMark(streamReader);
                                    success = true;
                                    return Regex.Unescape(stringBuilder.ToString());
                                default:
                                    stringBuilder.Append((char)buffer);
                                    break;
                            }
                        }
                    default:
                        stringBuilder.Append((char)buffer);
                        while (true)
                        {
                            buffer = streamReader.Read();
                            switch (buffer)
                            {
                                case -1:
                                    throw new JsonParseException("Unexpacted value ending", streamReader.BaseStream.Position);
                                case ',':
                                case '}':
                                case ']':
                                    endMark = buffer;
                                    success = true;
                                    string value = stringBuilder.ToString().Trim();
                                    switch (value.ToLower())
                                    {
                                        case "null":
                                            return new Value.Null();
                                        case "true":
                                            return new Value.Boolean(true);
                                        case "false":
                                            return new Value.Boolean(false);
                                        default:
                                            if (double.TryParse(value, out double result))
                                                return new Value.Number(result);
                                            else
                                                throw new JsonParseException("Unexpacted value type", streamReader.BaseStream.Position);
                                    }
                                default:
                                    stringBuilder.Append((char)buffer);
                                    break;
                            }
                        }
                }
            }

            private static int ReadNextMark(StreamReader streamReader)
            {
                int mark;
                do
                {
                    mark = streamReader.Read();
                }
                while (mark != -1 && char.IsWhiteSpace((char)mark));
                return mark;
            }

            #endregion
        }

        #endregion
    }
}
