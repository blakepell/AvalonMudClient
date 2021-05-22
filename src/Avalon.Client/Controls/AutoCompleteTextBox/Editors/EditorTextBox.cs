/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System.Windows.Controls;

namespace Avalon.Controls.AutoCompleteTextBox.Editors
{
    /// <summary>
    /// Inherited TextBox.
    /// </summary>
    /// <remarks>
    /// Because this TextBox needs to override behaviors from the parent theme I've created this so
    /// we can target all instances of this specific TextBox.
    /// </remarks>
    public class EditorTextBox : TextBox
    {
    }
}
