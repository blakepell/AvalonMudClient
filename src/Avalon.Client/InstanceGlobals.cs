using System;
using System.Collections.Generic;
using System.Text;

namespace Avalon
{
    /// <summary>
    /// Global variables that live in the app but are not persisted across boots of the
    /// mud client.
    /// </summary>
    public class InstanceGlobals
    {

        /// <summary>
        /// The guid of the last item editted.
        /// </summary>
        public string LastEdittedId { get; set; } = "";

        /// <summary>
        /// The last item type that was editted.
        /// </summary>
        public EditItem LastEditted { get; set; } = EditItem.None;

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
