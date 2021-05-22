/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Triggers;
using Avalon.Utilities;
using System.Collections.Generic;

namespace Avalon
{
    /// <summary>
    /// Global variables that live in the app but are not persisted across boots of the
    /// mud client.  Some of these data structures will be however in sync with the saved
    /// Profile structures.
    /// </summary>
    /// <remarks>
    /// As an example, it became problematic that the Triggers were run off of the data structures
    /// in the profile.  In particular if one was added while they were being enumerated an error
    /// would occur.  If one was being edited while they were being enumerated a half edited structure
    /// might get processed.  Further, for triggers like the gag triggers the entire list has to filter
    /// when in reality a synced copy could only hold the few gag triggers required.  These can then be
    /// updated when the list is updated/saved.
    /// </remarks>
    public class InstanceGlobals
    {
        /// <summary>
        /// These are immutable triggers that are set by the mud client or loaded from a plugin that will be
        /// isolated from the user defined triggers.  These run before triggers but after line transformer triggers.
        /// </summary>
        public List<Trigger> SystemTriggers { get; set; } = new List<Trigger>();
        
        /// <summary>
        /// An override to force skipping a save on exit in case someone borked their settings up.
        /// </summary>
        public bool SkipSaveOnExit { get; set; } = false;

        /// <summary>
        /// The guid of the last item edited.
        /// </summary>
        public string LastEditedId { get; set; } = "";

        /// <summary>
        /// The last item type that was edited.
        /// </summary>
        public EditItem LastEdited { get; set; } = EditItem.None;

        /// <summary>
        /// Editor types.
        /// </summary>
        public enum EditItem
        {
            None,
            Alias,
            Trigger
        }
    }
}