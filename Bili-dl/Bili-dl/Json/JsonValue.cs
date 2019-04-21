using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Json
{
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
