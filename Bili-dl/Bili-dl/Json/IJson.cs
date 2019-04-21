using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Json
{
    public interface IJson : IEnumerable
    {
        IJson GetValue(object index);
        bool SetValue(object index, object value);

        bool Contains(object index);

        string ToString();
        long ToLong();
        double ToDouble();
        bool ToBool();
    }
}
