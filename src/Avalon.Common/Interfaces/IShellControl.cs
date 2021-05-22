/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

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
