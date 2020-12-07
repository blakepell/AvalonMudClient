namespace Avalon.Common.Interfaces
{
    /// <summary>
    /// Interface that UserControl's should inherit if they are used in the Shell window.
    /// </summary>
    public interface IShellControl
    {

        /// <summary>
        /// Code to execute if the primary button is clicked.
        /// </summary>
        void PrimaryButtonClick();

        /// <summary>
        /// Code to execute if the secondary button is clicked.
        /// </summary>
        void SecondaryButtonClick();

    }
}
