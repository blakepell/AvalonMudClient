using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter.DataStructs;

namespace MoonSharp.Interpreter
{
    /// <summary>
    /// A class representing a Lua table.
    /// </summary>
    public class Table : RefIdObject, IScriptPrivateResource
    {
        private readonly LinkedListIndex<int, TablePair> _arrayMap;
        private readonly LinkedListIndex<string, TablePair> _stringMap;
        private readonly LinkedListIndex<DynValue, TablePair> _valueMap;
        private readonly LinkedList<TablePair> _values;
        private int _cachedLength = -1;
        private bool _containsNilEntries;

        private int _initArray;
        private Table _metaTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        /// <param name="owner">The owner script.</param>
        public Table(Script owner)
        {
            _values = new LinkedList<TablePair>();
            _stringMap = new LinkedListIndex<string, TablePair>(_values);
            _arrayMap = new LinkedListIndex<int, TablePair>(_values);
            _valueMap = new LinkedListIndex<DynValue, TablePair>(_values);
            this.OwnerScript = owner;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="arrayValues">The values for the "array-like" part of the table.</param>
        public Table(Script owner, params DynValue[] arrayValues)
            : this(owner)
        {
            for (int i = 0; i < arrayValues.Length; i++)
            {
                this.Set(DynValue.NewNumber(i + 1), arrayValues[i]);
            }
        }

        /// <summary>
        /// Gets or sets the 
        /// <see cref="System.Object" /> with the specified key(s).
        /// This will marshall CLR and MoonSharp objects in the best possible way.
        /// Multiple keys can be used to access subtables.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object" />.
        /// </value>
        /// <param name="keys">The keys to access the table and subtables</param>
        public object this[params object[] keys]
        {
            get => this.Get(keys).ToObject();
            set => this.Set(keys, DynValue.FromObject(this.OwnerScript, value));
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> with the specified key(s).
        /// This will marshall CLR and MoonSharp objects in the best possible way.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        public object this[object key]
        {
            get => this.Get(key).ToObject();
            set => this.Set(key, DynValue.FromObject(this.OwnerScript, value));
        }


        /// <summary>
        /// Gets the length of the "array part".
        /// </summary>
        public int Length
        {
            get
            {
                if (_cachedLength < 0)
                {
                    _cachedLength = 0;

                    for (int i = 1; _arrayMap.ContainsKey(i) && !_arrayMap.Find(i).Value.Value.IsNil(); i++)
                    {
                        _cachedLength = i;
                    }
                }

                return _cachedLength;
            }
        }

        /// <summary>
        /// Gets the meta-table associated with this instance.
        /// </summary>
        public Table MetaTable
        {
            get => _metaTable;
            set
            {
                this.CheckScriptOwnership(_metaTable);
                _metaTable = value;
            }
        }


        /// <summary>
        /// Enumerates the key/value pairs.
        /// </summary>
        public IEnumerable<TablePair> Pairs
        {
            get { return _values.Select(n => new TablePair(n.Key, n.Value)); }
        }

        /// <summary>
        /// Enumerates the keys.
        /// </summary>
        public IEnumerable<DynValue> Keys
        {
            get { return _values.Select(n => n.Key); }
        }

        /// <summary>
        /// Enumerates the values
        /// </summary>
        public IEnumerable<DynValue> Values
        {
            get { return _values.Select(n => n.Value); }
        }

        /// <summary>
        /// Gets the script owning this resource.
        /// </summary>
        public Script OwnerScript { get; }

        /// <summary>
        /// Removes all items from the Table.
        /// </summary>
        public void Clear()
        {
            _values.Clear();
            _stringMap.Clear();
            _arrayMap.Clear();
            _valueMap.Clear();
            _cachedLength = -1;
        }

        /// <summary>
        /// Gets the integral key from a double.
        /// </summary>
        private int GetIntegralKey(double d)
        {
            int v = ((int) d);

            if (d >= 1.0 && d == v)
            {
                return v;
            }

            return -1;
        }

        private Table ResolveMultipleKeys(object[] keys, out object key)
        {
            var t = this;
            key = (keys.Length > 0) ? keys[0] : null;

            for (int i = 1; i < keys.Length; ++i)
            {
                var vt = t.RawGet(key);

                if (vt == null)
                {
                    throw new ScriptRuntimeException("Key '{0}' did not point to anything");
                }

                if (vt.Type != DataType.Table)
                {
                    throw new ScriptRuntimeException("Key '{0}' did not point to a table");
                }

                t = vt.Table;
                key = keys[i];
            }

            return t;
        }

        /// <summary>
        /// Append the value to the table using the next available integer index.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Append(DynValue value)
        {
            this.CheckScriptOwnership(value);
            this.PerformTableSet(_arrayMap, this.Length + 1, DynValue.NewNumber(this.Length + 1), value, true,
                this.Length + 1);
        }

        /// <summary>
        /// Collects the dead keys. This frees up memory but invalidates pending iterators.
        /// It's called automatically internally when the semantics of Lua tables allow, but can be forced
        /// externally if it's known that no iterators are pending.
        /// </summary>
        public void CollectDeadKeys()
        {
            for (var node = _values.First; node != null; node = node.Next)
            {
                if (node.Value.Value.IsNil())
                {
                    this.Remove(node.Value.Key);
                }
            }

            _containsNilEntries = false;
            _cachedLength = -1;
        }


        /// <summary>
        /// Returns the next pair from a value
        /// </summary>
        public TablePair? NextKey(DynValue v)
        {
            if (v.IsNil())
            {
                var node = _values.First;

                if (node == null)
                {
                    return TablePair.Nil;
                }

                if (node.Value.Value.IsNil())
                {
                    return this.NextKey(node.Value.Key);
                }

                return node.Value;
            }

            if (v.Type == DataType.String)
            {
                return this.GetNextOf(_stringMap.Find(v.String));
            }

            if (v.Type == DataType.Number)
            {
                int idx = this.GetIntegralKey(v.Number);

                if (idx > 0)
                {
                    return this.GetNextOf(_arrayMap.Find(idx));
                }
            }

            return this.GetNextOf(_valueMap.Find(v));
        }

        private TablePair? GetNextOf(LinkedListNode<TablePair> linkedListNode)
        {
            while (true)
            {
                if (linkedListNode == null)
                {
                    return null;
                }

                if (linkedListNode.Next == null)
                {
                    return TablePair.Nil;
                }

                linkedListNode = linkedListNode.Next;

                if (!linkedListNode.Value.Value.IsNil())
                {
                    return linkedListNode.Value;
                }
            }
        }

        internal void InitNextArrayKeys(DynValue val, bool lastpos)
        {
            if (val.Type == DataType.Tuple && lastpos)
            {
                foreach (var v in val.Tuple)
                {
                    this.InitNextArrayKeys(v, true);
                }
            }
            else
            {
                this.Set(++_initArray, val.ToScalar());
            }
        }

        #region Set

        private void PerformTableSet<T>(LinkedListIndex<T, TablePair> listIndex, T key, DynValue keyDynValue,
            DynValue value, bool isNumber, int appendKey)
        {
            var prev = listIndex.Set(key, new TablePair(keyDynValue, value));

            // If this is an insert, we can invalidate all iterators and collect dead keys
            if (_containsNilEntries && value.IsNotNil() && (prev.Value == null || prev.Value.IsNil()))
            {
                this.CollectDeadKeys();
            }
            // If this value is nil (and we didn't collect), set that there are nil entries, and invalidate array len cache
            else if (value.IsNil())
            {
                _containsNilEntries = true;

                if (isNumber)
                {
                    _cachedLength = -1;
                }
            }
            else if (isNumber)
            {
                // If this is an array insert, we might have to invalidate the array length
                if (prev.Value == null || prev.Value.IsNilOrNan())
                {
                    // If this is an array append, let's check the next element before blindly invalidating
                    if (appendKey >= 0)
                    {
                        var next = _arrayMap.Find(appendKey + 1);
                        if (next == null || next.Value.Value == null || next.Value.Value.IsNil())
                        {
                            _cachedLength += 1;
                        }
                        else
                        {
                            _cachedLength = -1;
                        }
                    }
                    else
                    {
                        _cachedLength = -1;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the value associated to the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set(string key, DynValue value)
        {
            if (key == null)
            {
                throw ScriptRuntimeException.TableIndexIsNil();
            }

            this.CheckScriptOwnership(value);
            this.PerformTableSet(_stringMap, key, DynValue.NewString(key), value, false, -1);
        }

        /// <summary>
        /// Sets the value associated to the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set(int key, DynValue value)
        {
            this.CheckScriptOwnership(value);
            this.PerformTableSet(_arrayMap, key, DynValue.NewNumber(key), value, true, -1);
        }

        /// <summary>
        /// Sets the value associated to the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set(DynValue key, DynValue value)
        {
            if (key.IsNilOrNan())
            {
                if (key.IsNil())
                {
                    throw ScriptRuntimeException.TableIndexIsNil();
                }

                throw ScriptRuntimeException.TableIndexIsNaN();
            }

            if (key.Type == DataType.String)
            {
                this.Set(key.String, value);
                return;
            }

            if (key.Type == DataType.Number)
            {
                int idx = this.GetIntegralKey(key.Number);

                if (idx > 0)
                {
                    this.Set(idx, value);
                    return;
                }
            }

            this.CheckScriptOwnership(key);
            this.CheckScriptOwnership(value);

            this.PerformTableSet(_valueMap, key, key, value, false, -1);
        }

        /// <summary>
        /// Sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set(object key, DynValue value)
        {
            if (key == null)
            {
                throw ScriptRuntimeException.TableIndexIsNil();
            }

            if (key is string s)
            {
                this.Set(s, value);
            }
            else if (key is int i)
            {
                this.Set(i, value);
            }
            else
            {
                this.Set(DynValue.FromObject(this.OwnerScript, key), value);
            }
        }

        /// <summary>
        /// Sets the value associated with the specified keys.
        /// Multiple keys can be used to access subtables.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <param name="value">The value.</param>
        public void Set(object[] keys, DynValue value)
        {
            if (keys == null || keys.Length <= 0)
            {
                throw ScriptRuntimeException.TableIndexIsNil();
            }

            this.ResolveMultipleKeys(keys, out var key).Set(key, value);
        }

        #endregion

        #region Get

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public DynValue Get(string key)
        {
            return this.RawGet(key) ?? DynValue.Nil;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public DynValue Get(int key)
        {
            return this.RawGet(key) ?? DynValue.Nil;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public DynValue Get(DynValue key)
        {
            return this.RawGet(key) ?? DynValue.Nil;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// (expressed as a <see cref="System.Object"/>).
        /// </summary>
        /// <param name="key">The key.</param>
        public DynValue Get(object key)
        {
            return this.RawGet(key) ?? DynValue.Nil;
        }

        /// <summary>
        /// Gets the value associated with the specified keys (expressed as an 
        /// array of <see cref="System.Object"/>).
        /// This will marshall CLR and MoonSharp objects in the best possible way.
        /// Multiple keys can be used to access subtables.
        /// </summary>
        /// <param name="keys">The keys to access the table and subtables</param>
        public DynValue Get(params object[] keys)
        {
            return this.RawGet(keys) ?? DynValue.Nil;
        }

        #endregion

        #region RawGet

        private static DynValue RawGetValue(LinkedListNode<TablePair> linkedListNode)
        {
            return linkedListNode?.Value.Value;
        }

        /// <summary>
        /// Gets the value associated with the specified key,
        /// without bringing to Nil the non-existant values.
        /// </summary>
        /// <param name="key">The key.</param>
        public DynValue RawGet(string key)
        {
            return RawGetValue(_stringMap.Find(key));
        }

        /// <summary>
        /// Gets the value associated with the specified key,
        /// without bringing to Nil the non-existant values.
        /// </summary>
        /// <param name="key">The key.</param>
        public DynValue RawGet(int key)
        {
            return RawGetValue(_arrayMap.Find(key));
        }

        /// <summary>
        /// Gets the value associated with the specified key,
        /// without bringing to Nil the non-existant values.
        /// </summary>
        /// <param name="key">The key.</param>
        public DynValue RawGet(DynValue key)
        {
            if (key.Type == DataType.String)
            {
                return this.RawGet(key.String);
            }

            if (key.Type == DataType.Number)
            {
                int idx = this.GetIntegralKey(key.Number);
                if (idx > 0)
                {
                    return this.RawGet(idx);
                }
            }

            return RawGetValue(_valueMap.Find(key));
        }

        /// <summary>
        /// Gets the value associated with the specified key,
        /// without bringing to Nil the non-existant values.
        /// </summary>
        /// <param name="key">The key.</param>
        public DynValue RawGet(object key)
        {
            if (key == null)
            {
                return null;
            }

            if (key is string s)
            {
                return this.RawGet(s);
            }

            if (key is int i)
            {
                return this.RawGet(i);
            }

            return this.RawGet(DynValue.FromObject(this.OwnerScript, key));
        }

        /// <summary>
        /// Gets the value associated with the specified keys (expressed as an
        /// array of <see cref="System.Object"/>).
        /// This will marshall CLR and MoonSharp objects in the best possible way.
        /// Multiple keys can be used to access subtables.
        /// </summary>
        /// <param name="keys">The keys to access the table and subtables</param>
        public DynValue RawGet(params object[] keys)
        {
            if (keys == null || keys.Length <= 0)
            {
                return null;
            }

            return this.ResolveMultipleKeys(keys, out var key).RawGet(key);
        }

        #endregion

        #region Remove

        private bool PerformTableRemove<T>(LinkedListIndex<T, TablePair> listIndex, T key, bool isNumber)
        {
            bool removed = listIndex.Remove(key);

            if (removed && isNumber)
            {
                _cachedLength = -1;
            }

            return removed;
        }

        /// <summary>
        /// Remove the value associated with the specified key from the table.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if values was successfully removed; otherwise, <c>false</c>.</returns>
        public bool Remove(string key)
        {
            return this.PerformTableRemove(_stringMap, key, false);
        }

        /// <summary>
        /// Remove the value associated with the specified key from the table.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if values was successfully removed; otherwise, <c>false</c>.</returns>
        public bool Remove(int key)
        {
            return this.PerformTableRemove(_arrayMap, key, true);
        }

        /// <summary>
        /// Remove the value associated with the specified key from the table.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if values was successfully removed; otherwise, <c>false</c>.</returns>
        public bool Remove(DynValue key)
        {
            if (key.Type == DataType.String)
            {
                return this.Remove(key.String);
            }

            if (key.Type == DataType.Number)
            {
                int idx = this.GetIntegralKey(key.Number);
                if (idx > 0)
                {
                    return this.Remove(idx);
                }
            }

            return this.PerformTableRemove(_valueMap, key, false);
        }

        /// <summary>
        /// Remove the value associated with the specified key from the table.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if values was successfully removed; otherwise, <c>false</c>.</returns>
        public bool Remove(object key)
        {
            if (key is string s)
            {
                return this.Remove(s);
            }

            if (key is int i)
            {
                return this.Remove(i);
            }

            return this.Remove(DynValue.FromObject(this.OwnerScript, key));
        }

        /// <summary>
        /// Remove the value associated with the specified keys from the table.
        /// Multiple keys can be used to access subtables.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns><c>true</c> if values was successfully removed; otherwise, <c>false</c>.</returns>
        public bool Remove(params object[] keys)
        {
            if (keys == null || keys.Length <= 0)
            {
                return false;
            }

            return this.ResolveMultipleKeys(keys, out var key).Remove(key);
        }

        #endregion
    }
}