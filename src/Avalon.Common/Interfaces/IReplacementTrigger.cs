/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Avalon.Common.Interfaces
{
    /// <summary>
    /// A trigger that is solely focused on replacing text that's come from the server
    /// to change how it's seen in the terminal.
    /// </summary>
    public interface IReplacementTrigger
    {
        /// <summary>
        /// A unique ID for the replacement.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// The package that installed the replacement if one did so.
        /// </summary>
        string PackageId { get; set; }

        /// <summary>
        /// The RegEx pattern that triggers if the replace should occur.
        /// </summary>
        string Pattern { get; set; }

        /// <summary>
        /// What the pattern should be replaced with.
        /// </summary>
        string Replacement { get; set; }

        /// <summary>
        /// The final replacement after any variables have been swapped in.  Ideally if no
        /// changes are made this would return a reference to <see cref="Replacement"/>.
        /// </summary>
        string ProcessedReplacement { get; set; }

        /// <summary>
        /// Lua that will be executed as a function call when a match is found.  
        /// </summary>
        string OnMatchEvent { get; set; }

        /// <summary>
        /// If the replacement trigger is enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// If the variable is temporary and should not be saved with the profile.
        /// </summary>
        bool Temp { get; set; }

        /// <summary>
        /// The group the replacement belongs to.
        /// </summary>
        string Group { get; set; }

        /// <summary>
        /// If the replacement trigger matches the provided text.
        /// </summary>
        /// <param name="line"></param>
        Match IsMatch(string line);

    }
}
