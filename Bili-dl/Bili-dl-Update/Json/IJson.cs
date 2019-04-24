using System.Collections;

namespace Json
{
    /// <summary>
    /// Interface <c>IJson</c> provide interfaces for Json-object, Json-array and a number types of json values.
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
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
