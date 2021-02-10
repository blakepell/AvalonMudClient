namespace Avalon.Common.Interfaces
{
    /// <summary>
    /// Interface that helps to expose common Model functionality to the UI when it
    /// exists.  A primary example of this is editor controls being able to see if a
    /// variable IsEmpty when the form closes and remove empty items from the collection.
    /// </summary>
    public interface IModelInfo
    {
        /// <summary>
        /// If the object is considered empty.  This should return false if the model is
        /// always considered to be not empty.
        /// </summary>
        bool IsEmpty() => true;
    }
}
