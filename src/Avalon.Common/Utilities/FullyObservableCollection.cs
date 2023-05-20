/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using System.Buffers;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;

namespace Avalon.Common.Utilities
{
    /// <summary>
    /// Represents a thread safe ObservableCollection that sends notify messages down
    /// to the property level.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FullyObservableCollection<T> : ObservableCollection<T>, IDisposable, IEnumerable
    {
        /// <summary>
        /// The lock mechanism with support for recursion which allows <see cref="GetEnumerator"/> to be called without
        /// a <see cref="LockRecursionException"/> being thrown.
        /// </summary>
        public ReaderWriterLockSlim Lock = new(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Delegate for when a list item changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void ListItemChangedEventHandler(object sender, PropertyChangedEventArgs e);

        /// <summary>
        /// Event that is raised when a list item changes, including properties.
        /// </summary>
        public event ListItemChangedEventHandler ListItemChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        public FullyObservableCollection()
        {
            this.CollectionChanged += this.OnCollectionChanged;
        }

        public new T this[int index]
        {
            get
            {
                Lock.EnterReadLock();

                try
                {
                    return base[index];
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
            set => this.Add(value);
        }

        public new IEnumerator<T> GetEnumerator()
        {
            int count;
            var pool = ArrayPool<T>.Shared;
            T[] snapshot;

            // We only need the lock while we're creating the temporary snapshot, once
            // that's done we can release and then allow the enumeration to continue.  We
            // will get the count after the lock and then use it.
            try
            {
                Lock.EnterReadLock();

                count = this.Count;
                snapshot = pool.Rent(count);

                for (int i = 0; i < count; i++)
                {
                    snapshot[i] = this[i];
                }
            }
            finally
            {
                Lock.ExitReadLock();
            }

            // Since the array returned from the pool could be larger than we requested
            // we will use the saved count to only iterate over the items we know to be
            // in the range of the ones we requested.
            for (int i = 0; i < count; i++)
            {
                yield return snapshot[i];
            }

            pool.Return(snapshot, true);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item"></param>
        public new void Add(T item)
        {
            Lock.EnterWriteLock();

            try
            {
                base.Add(item);
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public new void Clear()
        {
            Lock.EnterWriteLock();

            try
            {
                base.Clear();
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Returns the number of elements in the collection.
        /// </summary>
        public new int Count
        {
            get
            {
                Lock.EnterReadLock();

                try
                {
                    return base.Count;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Whether an item currently exists in the collection.
        /// </summary>
        /// <param name="item"></param>
        public new bool Contains(T item)
        {
            Lock.EnterReadLock();

            try
            {
                return base.Contains(item);
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Copies all of the items from the index on into the array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public new void CopyTo(T[] array, int arrayIndex)
        {
            Lock.EnterWriteLock();

            try
            {
                base.CopyTo(array, arrayIndex);
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Event that fires when the collection changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.AddOrRemoveListToPropertyChanged(e.NewItems, true);
            this.AddOrRemoveListToPropertyChanged(e.OldItems, false);
        }

        /// <summary>
        /// Adds or removes from the property changed list.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="add"></param>
        private void AddOrRemoveListToPropertyChanged(IEnumerable list, bool add)
        {
            if (list == null)
            {
                return;
            }

            foreach (var item in list)
            {
                if (item is INotifyPropertyChanged o)
                {
                    if (add)
                    {
                        o.PropertyChanged += this.ListItemPropertyChanged;
                    }

                    if (!add)
                    {
                        o.PropertyChanged -= this.ListItemPropertyChanged;
                    }
                }
                else
                {
                    throw new Exception("INotifyPropertyChanged is required");
                }
            }
        }

        /// <summary>
        /// Implements a comparable version of the Linq Find which searches via index and not IEnumerable and can
        /// offer performance improvements over FirstOrDefault when searching in memory objects.
        /// </summary>
        /// <param name="match"></param>
        public T Find(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("Predicate cannot be null.");
            }

            for (int i = 0; i < this.Count; i++)
            {
                if (match(this[i]))
                {
                    return this[i];
                }
            }

            return default;
        }

        /// <summary>
        /// Raised when a property on one of the items in the collection changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.ListItemChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Disposes of resources for the SpecialObservableCollection including the removal of
        /// event handlers.
        /// </summary>
        public void Dispose()
        {
            this.CollectionChanged -= this.OnCollectionChanged;

            foreach (var item in this)
            {
                if (item is INotifyPropertyChanged o)
                {
                    o.PropertyChanged -= this.ListItemPropertyChanged;
                }
            }
        }
    }
}