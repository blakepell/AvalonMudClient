using System.Collections.Generic;

namespace MoonSharp.Interpreter.DataStructs
{
    /// <summary>
    /// A Dictionary where multiple values can be associated to the same key
    /// </summary>
    /// <typeparam name="K">The key type</typeparam>
    /// <typeparam name="V">The value type</typeparam>
    internal class MultiDictionary<K, V>
    {
        private V[] _defaultRet = new V[0];
        private Dictionary<K, List<V>> _map;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiDictionary{K, V}"/> class.
        /// </summary>
        public MultiDictionary()
        {
            _map = new Dictionary<K, List<V>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiDictionary{K, V}"/> class.
        /// </summary>
        /// <param name="eqComparer">The equality comparer to use in the underlying dictionary.</param>
        public MultiDictionary(IEqualityComparer<K> eqComparer)
        {
            _map = new Dictionary<K, List<V>>(eqComparer);
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        public IEnumerable<K> Keys => _map.Keys;

        /// <summary>
        /// Adds the specified key. Returns true if this is the first value for a given key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public bool Add(K key, V value)
        {
            List<V> list;

            if (_map.TryGetValue(key, out list))
            {
                list.Add(value);
                return false;
            }

            list = new List<V>();
            list.Add(value);
            _map.Add(key, list);
            return true;
        }

        /// <summary>
        /// Finds all the values associated with the specified key. 
        /// An empty collection is returned if not found.
        /// </summary>
        /// <param name="key">The key.</param>
        public IEnumerable<V> Find(K key)
        {
            List<V> list;

            if (_map.TryGetValue(key, out list))
            {
                return list;
            }

            return _defaultRet;
        }

        /// <summary>
        /// Determines whether this contains the specified key 
        /// </summary>
        /// <param name="key">The key.</param>
        public bool ContainsKey(K key)
        {
            return _map.ContainsKey(key);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _map.Clear();
        }

        /// <summary>
        /// Removes the specified key and all its associated values from the multidictionary
        /// </summary>
        /// <param name="key">The key.</param>
        public void Remove(K key)
        {
            _map.Remove(key);
        }

        /// <summary>
        /// Removes the value. Returns true if the removed value was the last of a given key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public bool RemoveValue(K key, V value)
        {
            List<V> list;

            if (_map.TryGetValue(key, out list))
            {
                list.Remove(value);

                if (list.Count == 0)
                {
                    this.Remove(key);
                    return true;
                }
            }

            return false;
        }
    }
}