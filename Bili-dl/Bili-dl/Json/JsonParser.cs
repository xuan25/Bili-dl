using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Json
{
    /// <summary>
    /// Class <c>JsonParser</c> provides parsing methods for converting json strings to C# objects.
    /// Author: Xuan525
    /// Date: 08/04/2019
    /// </summary>
    class JsonParser
    {
        private static IJson ToValue(string str)
        {
            if (str.Trim().ToLower() == "null")
                return null;
            if (str.Trim().ToLower() == "true" || str.Trim().ToLower() == "false")
            {
                bool.TryParse(str, out bool result);
                return new JsonBool(result);
            }
            else if (str.Contains("."))
            {
                double.TryParse(str, out double result);
                return new JsonDouble(result);
            }
            else
            {
                long.TryParse(str, out long result);
                return new JsonLong(result);
            }
        }

        private static IJson ParseValue(StringReader stringReader)
        {
            while (stringReader.Peek() == ' ' || stringReader.Peek() == '\r' || stringReader.Peek() == '\n')
                stringReader.Read();
            if (stringReader.Peek() == '\"')
            {
                stringReader.Read();
                StringBuilder stringBuilder = new StringBuilder();
                while (stringReader.Peek() != -1)
                {
                    if (stringReader.Peek() == '\\')
                    {
                        stringBuilder.Append((char)stringReader.Read());
                        stringBuilder.Append((char)stringReader.Read());
                    }
                    else if (stringReader.Peek() == '\"')
                    {
                        string value = stringBuilder.ToString();
                        while (stringReader.Peek() != ',' && stringReader.Peek() != '}' && stringReader.Peek() != ']')
                            stringReader.Read();
                        if (stringReader.Peek() == ',')
                            stringReader.Read();
                        return new JsonString(value);
                    }
                    else
                        stringBuilder.Append((char)stringReader.Read());
                }
                return new JsonString(stringBuilder.ToString());
            }
            else if (stringReader.Peek() == '{')
            {
                JsonObject jsonObject = ParseObject(stringReader);
                while (stringReader.Peek() != -1 && (stringReader.Peek() == ',' ||  stringReader.Peek() == ' ' || stringReader.Peek() == '\r' || stringReader.Peek() == '\n'))
                    stringReader.Read();
                return jsonObject;
            }
            else if (stringReader.Peek() == '[')
            {
                JsonArray jsonArray = ParseArray(stringReader);
                while (stringReader.Peek() != -1 && (stringReader.Peek() == ',' || stringReader.Peek() == ' ' || stringReader.Peek() == '\r' || stringReader.Peek() == '\n'))
                    stringReader.Read();
                return jsonArray;
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                while (stringReader.Peek() != -1)
                {
                    if (stringReader.Peek() == '\\')
                    {
                        stringBuilder.Append((char)stringReader.Read());
                        stringBuilder.Append((char)stringReader.Read());
                    }
                    else if (stringReader.Peek() == ',')
                    {
                        string value = stringBuilder.ToString();
                        stringReader.Read();
                        return ToValue(value);
                    }
                    else if (stringReader.Peek() == '}' || stringReader.Peek() == ']')
                        return ToValue(stringBuilder.ToString());
                    else
                        stringBuilder.Append((char)stringReader.Read());
                }
                return new JsonString(stringBuilder.ToString());
            }
        }

        private static KeyValuePair<string, object> ParseKeyValuePaire(StringReader stringReader)
        {
            StringBuilder stringBuilder = new StringBuilder();
            while (stringReader.Peek() != -1 && stringReader.Read() != '\"') ;
            while (stringReader.Peek() > -1)
            {
                if (stringReader.Peek() == '\\')
                {
                    stringBuilder.Append((char)stringReader.Read());
                    stringBuilder.Append((char)stringReader.Read());
                }
                else if (stringReader.Peek() == '\"')
                {
                    stringReader.Read();
                    while (stringReader.Peek() != -1 && stringReader.Read() != ':') ;
                    string key = stringBuilder.ToString();
                    object value = ParseValue(stringReader);
                    return new KeyValuePair<string, object>(key, value);
                }
                else
                {
                    stringBuilder.Append((char)stringReader.Read());
                }
            }
            return new KeyValuePair<string, object>("UNKNOW", null);
        }

        private static JsonObject ParseObject(StringReader stringReader)
        {
            stringReader.Read();
            JsonObject jsonObject = new JsonObject();
            while (stringReader.Peek() > -1)
            {
                if (stringReader.Peek() == '{')
                    ParseObject(stringReader);
                else if (stringReader.Peek() == '[')
                    ParseArray(stringReader);
                else if (stringReader.Peek() == '\"')
                {
                    KeyValuePair<string, object> keyValuePair = ParseKeyValuePaire(stringReader);
                    jsonObject.Add(keyValuePair.Key, keyValuePair.Value);
                }
                else if (stringReader.Peek() == '}')
                {
                    stringReader.Read();
                    return jsonObject;
                }
                else
                    stringReader.Read();
            }
            return jsonObject;
        }

        private static JsonArray ParseArray(StringReader stringReader)
        {
            stringReader.Read();
            JsonArray jsonArray = new JsonArray();
            while (stringReader.Peek() > -1)
            {
                if (stringReader.Peek() == ']')
                {
                    stringReader.Read();
                    return jsonArray;
                }
                else
                    jsonArray.Add(ParseValue(stringReader));
            }
            return jsonArray;
        }

        /// <summary>
        /// Parse a json string to an object
        /// <example>For example:
        /// <code>
        ///    dynamic json = JsonParser.Parse(jsonStr);
        ///    string keyData = json.data;
        ///    string keyData1 = json["data"];
        ///    string arrayItem = json.arrayExample[0];
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="json">Json string</param>
        /// <returns>Json object</returns>
        public static IJson Parse(string json)
        {
            if (json == null)
                return null;
            StringReader stringReader = new StringReader(json.Trim());
            if (stringReader.Peek() == -1)
                return null;
            else if (stringReader.Peek() == '{')
                return ParseObject(stringReader);
            else if (stringReader.Peek() == '[')
                return ParseArray(stringReader);
            else
                return null;
        }
    }
}
