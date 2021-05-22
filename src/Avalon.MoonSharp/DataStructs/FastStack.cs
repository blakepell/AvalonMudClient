#if !USE_DYNAMIC_STACKS

using System;
using System.Collections;
using System.Collections.Generic;

namespace MoonSharp.Interpreter.DataStructs
{
    /// <summary>
    /// A preallocated, non-resizable, stack
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class FastStack<T> : IList<T>
    {
        private T[] _storage;

        public FastStack(int maxCapacity)
        {
            _storage = new T[maxCapacity];
        }

        public T this[int index]
        {
            get => _storage[index];
            set => _storage[index] = value;
        }

        public int Count { get; private set; }

        public T Push(T item)
        {
            _storage[this.Count++] = item;
            return item;
        }

        public void Expand(int size)
        {
            this.Count += size;
        }

        private void Zero(int from, int to)
        {
            Array.Clear(_storage, from, to - from + 1);
        }

        private void Zero(int index)
        {
            _storage[index] = default;
        }

        public T Peek(int idxofs = 0)
        {
            var item = _storage[this.Count - 1 - idxofs];
            return item;
        }

        public void Set(int idxofs, T item)
        {
            _storage[this.Count - 1 - idxofs] = item;
        }

        public void CropAtCount(int p)
        {
            this.RemoveLast(this.Count - p);
        }

        public void RemoveLast(int cnt = 1)
        {
            if (cnt == 1)
            {
                --this.Count;
                _storage[this.Count] = default;
            }
            else
            {
                int oldhead = this.Count;
                this.Count -= cnt;
                this.Zero(this.Count, oldhead);
            }
        }

        public T Pop()
        {
            --this.Count;
            var retval = _storage[this.Count];
            _storage[this.Count] = default;
            return retval;
        }

        public void Clear()
        {
            Array.Clear(_storage, 0, _storage.Length);
            this.Count = 0;
        }


        #region IList<T> Impl.

        int IList<T>.IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        T IList<T>.this[int index]
        {
            get => this[index];
            set => this[index] = value;
        }

        void ICollection<T>.Add(T item)
        {
            this.Push(item);
        }

        void ICollection<T>.Clear()
        {
            this.Clear();
        }

        bool ICollection<T>.Contains(T item)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        int ICollection<T>.Count => this.Count;

        bool ICollection<T>.IsReadOnly => false;

        bool ICollection<T>.Remove(T item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

#endif