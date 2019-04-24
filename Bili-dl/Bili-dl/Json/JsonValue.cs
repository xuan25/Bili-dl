using System.Collections;

namespace Json
{
    /// <summary>
    /// Class <c>JsonString</c> models a string value in json.
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public class JsonString : IJson
    {
        public string Value;
        public JsonString(string value)
        {
            Value = value;
        }

        public bool Contains(object index)
        {
            return false;
        }

        public double ToDouble()
        {
            return double.Parse(Value);
        }

        public long ToLong()
        {
            return long.Parse(Value);
        }

        public override string ToString()
        {
            return Value;
        }

        public bool ToBool()
        {
            return bool.Parse(Value);
        }

        public IJson GetValue(object index)
        {
            return null;
        }

        public bool SetValue(object index, object value)
        {
            return false;
        }

        public IEnumerator GetEnumerator()
        {
            return null;
        }
    }

    /// <summary>
    /// Class <c>JsonLong</c> models a long value in json.
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public class JsonLong : IJson
    {
        public long Value;
        public JsonLong(long value)
        {
            Value = value;
        }

        public bool Contains(object index)
        {
            return false;
        }

        public double ToDouble()
        {
            return Value;
        }

        public long ToLong()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public bool ToBool()
        {
            if (Value > 0)
                return true;
            else
                return false;
        }

        public IJson GetValue(object index)
        {
            return null;
        }

        public bool SetValue(object index, object value)
        {
            return false;
        }

        public IEnumerator GetEnumerator()
        {
            return null;
        }
    }

    /// <summary>
    /// Class <c>JsonDouble</c> models a double value in json.
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public class JsonDouble : IJson
    {
        public double Value;
        public JsonDouble(double value)
        {
            Value = value;
        }

        public bool Contains(object index)
        {
            return false;
        }

        public double ToDouble()
        {
            return Value;
        }

        public long ToLong()
        {
            return long.Parse(Value.ToString());
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public bool ToBool()
        {
            if (Value > 0)
                return true;
            else
                return false;
        }

        public IJson GetValue(object index)
        {
            return null;
        }

        public bool SetValue(object index, object value)
        {
            return false;
        }

        public IEnumerator GetEnumerator()
        {
            return null;
        }
    }

    /// <summary>
    /// Class <c>JsonBool</c> models a boolean value in json.
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public class JsonBool : IJson
    {
        public bool Value;
        public JsonBool(bool value)
        {
            Value = value;
        }

        public bool Contains(object index)
        {
            return false;
        }

        public double ToDouble()
        {
            if (Value)
                return 1;
            else
                return 0;
        }

        public long ToLong()
        {
            if (Value)
                return 1;
            else
                return 0;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public bool ToBool()
        {
            return Value;
        }

        public IJson GetValue(object index)
        {
            return null;
        }

        public bool SetValue(object index, object value)
        {
            return false;
        }

        public IEnumerator GetEnumerator()
        {
            return null;
        }
    }
}
