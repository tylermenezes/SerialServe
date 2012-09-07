using System;
using System.Collections;
using System.Collections.Generic;

namespace SerialServe
{
    internal class AssociativeArray<T, U> : IEnumerable
    {
        private Dictionary<T, U> _int = new Dictionary<T, U>();

        public U this[T i]
        {
            get
            {
                if (_int.ContainsKey(i))
                {
                    return _int[i];
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }

            set
            {
                if (_int.ContainsKey(i))
                {
                    _int[i] = value;
                }
                else
                {
                    _int.Add(i, value);
                }
            }
        }

        public bool ContainsKey(T toCheck)
        {
            return _int.ContainsKey(toCheck);
        }

        public IEnumerator GetEnumerator()
        {
            foreach (KeyValuePair<T, U> kvp in _int)
            {
                yield return kvp.Value;
            }
        }
    }
}
