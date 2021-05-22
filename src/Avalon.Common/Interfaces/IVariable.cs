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
    /// A variable that is persisted with the profile that can be used to track
    /// data as well as be coupled with aliases/triggers and the script engine.
    /// </summary>
    public interface IVariable
    {
        /// <summary>
        /// The name of the character the variable should be associated with.
        /// </summary>
        string Character { get; set; }

        /// <summary>
        /// The key used to reference the variable.
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// The value of the variable.
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// Whether or not the variable should be visible to controls that display variables.  This
        /// is considered a hint.  The use case is a generic control that displays variables that might
        /// be different between users and not every control is expected to implement this.
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// The order used for sorting when this is displayed on the UI.
        /// </summary>
        int DisplayOrder { get; set; }

        /// <summary>
        /// The label to display, if this is blank the key should be used.
        /// </summary>
        string Label { get; set; }

        /// <summary>
        /// The friendly name of the color to lookup that the variable should be displayed in.
        /// </summary>
        string ForegroundColor { get; set; }

        /// <summary>
        /// Lua code that will execute when the variable is changed.
        /// </summary>
        string OnChangeEvent { get; set; }

    }
}