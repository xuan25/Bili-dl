using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace Json
{
    /// <summary>
    /// Class <c>JsonObject</c> models an Object in json.
    /// Author: Xuan525
    /// Date: 12/04/2019
    /// </summary>
    public class JsonObject : DynamicObject, IEnumerable, IJson
    {
        private Dictionary<string, object> dictionary = new Dictionary<string, object>();

        /// <summary>
        /// The number of items in the Object
        /// </summary>
        public int Count
        {
            get
            {
                return dictionary.Count;
            }
        }

        /// <summary>
        /// Add a key-value pair to the Object
        /// </summary>
        /// <param name="key">The Key of the key-value pair</param>
        /// <param name="value">The Value of the key-value pair</param>
        public void Add(string key, object value)
        {
            dictionary.Add(key.ToLower(), value);
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(dictionary);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            HashSet<string> set = new HashSet<string>();
            foreach (KeyValuePair<string, object> p in dictionary)
                set.Add(p.Key);
            return set;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name.ToLower();
            return dictionary.TryGetValue(name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            dictionary[binder.Name.ToLower()] = value;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (dictionary.ContainsKey(((string)indexes[0]).ToLower()))
                result = dictionary[((string)indexes[0]).ToLower()];
            else
                throw new System.NullReferenceException();
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (dictionary.ContainsKey(((string)indexes[0]).ToLower()))
                dictionary[((string)indexes[0]).ToLower()] = value;
            else
                dictionary.Add(((string)indexes[0]).ToLower(), value);
            return true;
        }

        public IJson GetValue(object name)
        {
            if (dictionary.ContainsKey(((string)name).ToLower()))
                return (IJson)dictionary[((string)name).ToLower()];
            else
                throw new System.NullReferenceException();
        }

        public bool SetValue(object name, object value)
        {
            if (dictionary.ContainsKey(((string)name).ToLower()))
                dictionary[((string)name).ToLower()] = value;
            else
                dictionary.Add(((string)name).ToLower(), value);
            return true;
        }

        public bool Contains(object name)
        {
            return dictionary.ContainsKey((string)name);
        }

        public override string ToString()
        {
            throw new System.NotImplementedException();
        }

        public long ToLong()
        {
            throw new System.NotImplementedException();
        }

        public double ToDouble()
        {
            throw new System.NotImplementedException();
        }

        public bool ToBool()
        {
            throw new System.NotImplementedException();
        }

        public class Enumerator : IEnumerator<KeyValuePair<string, object>>
        {
            private Dictionary<string, object> dictionary;
            private Dictionary<string, object>.Enumerator enumerator;

            public Enumerator(Dictionary<string, object> dictionary)
            {
                this.dictionary = dictionary;
                enumerator = dictionary.GetEnumerator();
            }

            public KeyValuePair<string, object> Current => enumerator.Current;

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Dispose()
            {
                dictionary = null;
                enumerator.Dispose();
            }

            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            public void Reset()
            {
                enumerator.Dispose();
                enumerator = dictionary.GetEnumerator();
            }
        }

    }
}
