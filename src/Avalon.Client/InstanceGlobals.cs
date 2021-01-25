namespace Avalon
{
    /// <summary>
    /// Global variables that live in the app but are not persisted across boots of the
    /// mud client.
    /// </summary>
    public class InstanceGlobals
    {
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
