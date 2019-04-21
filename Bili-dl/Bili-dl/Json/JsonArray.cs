using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace Json
{
    /// <summary>
    /// Class <c>JsonArray</c> models an Array in json.
    /// Author: Xuan525
    /// Date: 08/04/2019
    /// </summary>
    public class JsonArray : DynamicObject, IEnumerable, IJson
    {
        private List<object> list = new List<object>();

        /// <summary>
        /// The number of items in the Array
        /// </summary>
        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        /// <summary>
        /// Add a value to the Array
        /// </summary>
        /// <param name="value">The Value</param>
        public void Add(object value)
        {
            list.Add(value);
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(list);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            return false;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (list.Count > (int)indexes[0])
                result = list[(int)indexes[0]];
            else
                throw new System.NullReferenceException();
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (list.Count > (int)indexes[0])
                list[(int)indexes[0]] = value;
            else
            {
                while (list.Count < (int)indexes[0])
                    list.Add(null);
                list.Add(value);
            }
            return true;
        }

        public IJson GetValue(object index)
        {
            if (list.Count > (int)index)
                return (IJson)list[(int)index];
            else
                throw new System.NullReferenceException();
        }

        public bool SetValue(object index, object value)
        {
            if (list.Count > (int)index)
                list[(int)index] = value;
            else
            {
                while (list.Count < (int)index)
                    list.Add(null);
                list.Add(value);
            }
            return true;
        }

        public bool Contains(object index)
        {
            if ((int)index < list.Count)
                return true;
            return false;
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

        public class Enumerator : IEnumerator<object>
        {
            private List<object> list;
            private List<object>.Enumerator enumerator;

            public Enumerator(List<object> list)
            {
                this.list = list;
                this.enumerator = list.GetEnumerator();
            }

            public object Current => enumerator.Current;

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Dispose()
            {
                list = null;
                enumerator.Dispose();
            }

            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            public void Reset()
            {
                enumerator.Dispose();
                this.enumerator = list.GetEnumerator();
            }
        }

    }
}
