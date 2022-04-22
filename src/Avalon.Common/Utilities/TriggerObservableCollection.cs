/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Triggers;
using System.Buffers;

namespace Avalon.Common.Utilities
{
    /// <summary>
    /// A ObservableCollection of triggers that is thread safe and observable down to the
    /// property level.
    /// </summary>
    public class TriggerObservableCollection : FullyObservableCollection<Trigger>
    {
        /// <summary>
        /// Returns an IEnumerable to iterate over a snapshot of the Triggers.
        /// </summary>
        public IEnumerable<Trigger> Enumerable()
        {
            int found = 0;
            var pool = ArrayPool<Trigger>.Shared;
            Trigger[] snapshot;

            // We only need the lock while we're creating the temporary snapshot, once
            // that's done we can release and then allow the enumeration to continue.
            try
            {
                Lock.EnterReadLock();

                int count = this.Count;
                snapshot = pool.Rent(count);

                for (int i = 0; i < count; i++)
                {
                    snapshot[found] = this[i];
                    found++;
                }
            }
            finally
            {
                Lock.ExitReadLock();
            }

            // Since the array returned from the pool could be larger than we requested
            // we will use the saved count to only iterate over the items we know to be
            // in the range of the ones we requested.
            for (int i = 0; i < found; i++)
            {
                yield return snapshot[i];
            }

            pool.Return(snapshot, true);
        }

        /// <summary>
        /// Returns an IEnumerable to iterate over currently enabled gag triggers.
        /// </summary>
        public IEnumerable<Trigger> GagEnumerable()
        {
            int found = 0;
            var pool = ArrayPool<Trigger>.Shared;
            Trigger[] snapshot;

            // We only need the lock while we're creating the temporary snapshot, once
            // that's done we can release and then allow the enumeration to continue.  We
            // will get the count after the lock and then use it.
            try
            {
                Lock.EnterReadLock();

                int count = this.Count;
                snapshot = pool.Rent(count);

                for (int i = 0; i < count; i++)
                {
                    // Make sure the trigger is a gag and that it's enabled.
                    if (this[i].Gag && this[i].Enabled)
                    {
                        snapshot[found] = this[i];
                        found++;
                    }
                }
            }
            finally
            {
                Lock.ExitReadLock();
            }

            // Since the array returned from the pool could be larger than we requested
            // we will use the saved count to only iterate over the items we know to be
            // in the range of the ones we requested.
            for (int i = 0; i < found; i++)
            {
                yield return snapshot[i];
            }

            pool.Return(snapshot, true);
        }

        /// <summary>
        /// Returns an IEnumerable to iterate over only enabled triggers.
        /// </summary>
        public IEnumerable<Trigger> EnabledEnumerable()
        {
            int found = 0;
            var pool = ArrayPool<Trigger>.Shared;
            Trigger[] snapshot;

            // We only need the lock while we're creating the temporary snapshot, once
            // that's done we can release and then allow the enumeration to continue.  We
            // will get the count after the lock and then use it.  This will also get gags
            // because even those the gag is processed from the terminal rendering it also
            // needs to be processed here in order to redirect the content to other terminals.
            // When the gag is called from the render process it can be called a number of times
            // and therefore it doesn't do extra processing there (AvalonEdit makes the call on
            // which lines get drawn).
            try
            {
                Lock.EnterReadLock();

                int count = this.Count;
                snapshot = pool.Rent(count);

                for (int i = 0; i < count; i++)
                {
                    // Make sure the trigger is a gag and that it's enabled and that
                    // it actually has a pattern.  Also, this isn't for LineTransformers.
                    if (this[i].Enabled && !this[i].LineTransformer && !string.IsNullOrWhiteSpace(this[i].Pattern))
                    {
                        snapshot[found] = this[i];
                        found++;
                    }
                }
            }
            finally
            {
                Lock.ExitReadLock();
            }

            // Since the array returned from the pool could be larger than we requested
            // we will use the saved count to only iterate over the items we know to be
            // in the range of the ones we requested.
            for (int i = 0; i < found; i++)
            {
                yield return snapshot[i];
            }

            pool.Return(snapshot, true);
        }

        /// <summary>
        /// Returns an IEnumerable to iterate over only enabled line transformer triggers.
        /// </summary>
        public IEnumerable<Trigger> EnabledLineTransformersEnumerable()
        {
            int found = 0;
            var pool = ArrayPool<Trigger>.Shared;
            Trigger[] snapshot;

            try
            {
                Lock.EnterReadLock();

                int count = this.Count;
                snapshot = pool.Rent(count);

                for (int i = 0; i < count; i++)
                {
                    // Make sure the trigger is a gag and that it's enabled and that
                    // it actually has a pattern.
                    if (this[i].Enabled && this[i].LineTransformer && !string.IsNullOrWhiteSpace(this[i].Pattern))
                    {
                        snapshot[found] = this[i];
                        found++;
                    }
                }
            }
            finally
            {
                Lock.ExitReadLock();
            }

            // Since the array returned from the pool could be larger than we requested
            // we will use the saved count to only iterate over the items we know to be
            // in the range of the ones we requested.
            for (int i = 0; i < found; i++)
            {
                yield return snapshot[i];
            }

            pool.Return(snapshot, true);
        }
    }
}