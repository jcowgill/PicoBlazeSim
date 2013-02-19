using System;
using System.Collections;
using System.Collections.Generic;

namespace JCowgill.PicoBlazeSim
{
    /// <summary>
    /// Read-only dictionary wrapper
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in this dictionary</typeparam>
    /// <typeparam name="TValue">The type of the values in this dictionary</typeparam>
    public sealed class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private IDictionary<TKey, TValue> dict;

        #region Constructors

        /// <summary>
        /// Creates a new empty ReadOnlyDictionary
        /// </summary>
        public ReadOnlyDictionary()
        {
        }

        /// <summary>
        /// Creates a new ReadOnlyDictionary wrapping another
        /// </summary>
        /// <param name="dict">dictionary to wrap</param>
        public ReadOnlyDictionary(IDictionary<TKey, TValue> dict)
        {
            this.dict = dict;
        }

        #endregion

        #region IDictionary<TKey,TValue> Members

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            throw ReadOnlyException();
        }

        public bool ContainsKey(TKey key)
        {
            return dict.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get { return dict.Keys; }
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            throw ReadOnlyException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dict.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values
        {
            get { return dict.Values; }
        }

        public TValue this[TKey key]
        {
            get
            {
                return dict[key];
            }
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get
            {
                return this[key];
            }
            set
            {
                throw ReadOnlyException();
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw ReadOnlyException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            throw ReadOnlyException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return dict.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            dict.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return dict.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw ReadOnlyException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private static Exception ReadOnlyException()
        {
            return new NotSupportedException("This dictionary is read-only");
        }
    }
}
