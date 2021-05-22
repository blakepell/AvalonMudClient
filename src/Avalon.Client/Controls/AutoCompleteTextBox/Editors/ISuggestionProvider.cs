/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System.Collections;

namespace Avalon.Controls.AutoCompleteTextBox.Editors
{
    public interface ISuggestionProvider
    {
        IEnumerable GetSuggestions(string filter);
    }
}