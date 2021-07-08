/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;
using System.Text.RegularExpressions;
using Avalon.Common.Models;

namespace Avalon.Common.Interfaces
{
    /// <summary>
    /// A trigger is an action that is executed based off of a pattern that is sent from the game.
    /// </summary>
    public interface ITrigger
    {
        /// <summary>
        /// The regular expression pattern to match the trigger on.
        /// </summary>
        string Pattern { get; set; }

        /// <summary>
        /// The command as entered by the user before it is processed in anyway.
        /// </summary>
        string Command { get; set; }

        /// <summary>
        /// The character who the trigger should be isolated to (if any).
        /// </summary>
        string Character { get; set; }

        /// <summary>
        /// A unique identifier, typically a <see cref="Guid"/> that identifies this specific trigger.  The identifier
        /// allows for this trigger to be manipulated via hash commands, aliases, lua scripts, etc.        
        /// </summary>
        /// <remarks>
        /// The identifier is useful for crafting efficient sets of triggers and manipulating them in real time.  For instance, 
        /// if you have 50 triggers that allow a character to attack creatures for leveling in a game you can instead have one 
        /// trigger that's pattern is updated based on what area or zone you are in.  This means, 1 triggers processes instead of 
        /// 50 and that trigger is dynamic.
        /// </remarks>
        string Identifier { get; set; }

        /// <summary>
        /// The group the trigger is in.  This can be used to toggle all triggers on or off.
        /// </summary>
        string Group { get; set; }

        /// <summary>
        /// Matches a trigger against a provided line of text.
        /// </summary>
        /// <param name="line"></param>
        bool IsMatch(string line);

        /// <summary>
        /// Indicates whether a trigger was loaded from a plugin or not.
        /// </summary>
        bool Plugin { get; set; }

        /// <summary>
        /// If the current trigger is enabled.  A trigger that is not enabled will not be processed.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Whether the trigger is locked.  This will stop a trigger from being auto-updated in a package.  It should
        /// be noted that a lock does not however stop a user from editing the trigger.
        /// </summary>
        bool Lock { get; set; }

        /// <summary>
        /// The number of times a trigger has fired.
        /// </summary>
        int Count { get; set; }

        /// <summary>
        /// The priority or order the trigger should be executed in comparison to all of the other Triggers
        /// that exist.  A default priority value should be specified by the implementing class that is well
        /// above zero that all triggers without a priority set start at.
        /// </summary>
        int Priority { get; set; }

        /// <summary>
        /// Execute command that is overridable.
        /// </summary>
        void Execute();

        /// <summary>
        /// Whether or not this trigger is a system trigger.  In the plugin system, a system trigger gets loaded
        /// from the Plugin into the system triggers list.  These triggers fire before regular triggers and also
        /// do not save, they only for the boot in which they were loaded.  Non-CLR triggers from a plugin will
        /// be loaded into the main trigger list (overwriting previous copies) if the trigger it's replacing does
        /// not have the <see cref="Lock"/> flag set.
        /// </summary>
        bool SystemTrigger { get; set; }

        /// <summary>
        /// The underlying regular expression provided in case an outside caller wants to call it directly such
        /// as in gagging operations where a command won't be executed.
        /// </summary>
        Regex Regex { get; set; }

        /// <summary>
        /// The date and time that the trigger was last matched.
        /// </summary>
        /// <remarks>
        /// This will generally be used to debug triggers and try to understand what triggers are firing and in what
        /// order they are firing in.  This can be toggled not to set via the TrackTriggerLastMatched profile setting.
        /// </remarks>
        DateTime LastMatched { get; set; }

        /// <summary>
        /// The package that imported this trigger.
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        /// How the command should be executed (as a command or sent to a different output like
        /// a script engine or even to a file).
        /// </summary>
        public ExecuteType ExecuteAs { get; set; }
    }
}
