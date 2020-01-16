using System.Collections;
using System.Collections.Generic;
using VRage;
using System;

namespace RichHudFramework
{
    public class PropWrapper<T>
    {
        public readonly Func<T> Getter;
        public readonly Action<T> Setter;

        public PropWrapper(Func<T> Getter, Action<T> Setter)
        {
            this.Getter = Getter;
            this.Setter = Setter;
        }

        public PropWrapper(MyTuple<Func<T>, Action<T>> tuple)
        {
            Getter = tuple.Item1;
            Setter = tuple.Item2;
        }
    }

    public class ReadOnlyCollectionData<T> : IReadOnlyCollection<T>
    {
        public T this[int index] => Getter(index);
        public int Count => CountFunc();

        private readonly Func<int, T> Getter;
        private readonly Func<int> CountFunc;
        private readonly IEnumerator<T> enumerator;

        public ReadOnlyCollectionData(Func<int, T> Getter, Func<int> CountFunc)
        {
            this.Getter = Getter;
            this.CountFunc = CountFunc;
            enumerator = new CollectionDataEnumerator<T>(Getter, CountFunc);
        }

        public ReadOnlyCollectionData(MyTuple<Func<int, T>, Func<int>> tuple)
        {
            Getter = tuple.Item1;
            CountFunc = tuple.Item2;
            enumerator = new CollectionDataEnumerator<T>(Getter, CountFunc);
        }

        public IEnumerator<T> GetEnumerator() =>
            enumerator;

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }

    public class CollectionDataEnumerator<T> : IEnumerator<T>
    {
        object IEnumerator.Current => Current;
        public T Current => Getter(index);

        private readonly Func<int, T> Getter;
        private readonly Func<int> CountFunc;
        private int index;

        public CollectionDataEnumerator(Func<int, T> Getter, Func<int> CountFunc)
        {
            this.Getter = Getter;
            this.CountFunc = CountFunc;
            index = -1;
        }

        public void Dispose()
        { }

        public bool MoveNext()
        {
            index++;
            return index < CountFunc();
        }

        public void Reset()
        {
            index = -1;
        }
    }
}