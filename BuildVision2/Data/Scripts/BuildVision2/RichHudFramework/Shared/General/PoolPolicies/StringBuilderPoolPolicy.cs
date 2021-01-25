using System.Collections.Generic;
using VRage;
using System.Text;

namespace RichHudFramework
{
    public class StringBuilderPoolPolicy : IPooledObjectPolicy<StringBuilder>
    {
        public StringBuilder GetNewObject()
        {
            return new StringBuilder();
        }

        public void ResetObject(StringBuilder obj)
        {
            obj.Clear();
        }

        public void ResetRange(IReadOnlyList<StringBuilder> objects, int index, int count)
        {
            for (int n = 0; (n < count && (index + n) < objects.Count); n++)
            {
                objects[index + n].Clear();
            }
        }

        public void ResetRange<T2>(IReadOnlyList<MyTuple<StringBuilder, T2>> objects, int index, int count)
        {
            for (int n = 0; (n < count && (index + n) < objects.Count); n++)
            {
                objects[index + n].Item1.Clear();
            }
        }
    }
}