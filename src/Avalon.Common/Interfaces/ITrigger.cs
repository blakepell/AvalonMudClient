﻿using System;
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
        /// allows for this trigger to be manipulated via hash commands, alises, lua scripts, etc.        
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
        /// Matches a trigger.  Also allows for the variable replacement triggers to be explicitly ignored by the caller
        /// regardless of how they're setup by the user.  This is important because the screen rendering code from AvalonEdit
        /// will hit those triggers over and over as each line comes in.  Variable replace should be ignored in those cases
        /// because they've already been processed (and in some cases it will cause them to re-process out of order).  If you're
        /// reading variables in from a prompt and getting say, a room name, you want that to process once, when you're there
        /// and not out of order.  To be clear, this isn't replace @ variables in the pattern, it's the part that sets the
        /// the variable later down the line.  The first "variable replacement" has to happen in both cases.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="skipVariableSet">Default false: Whether to explicitly skip variable setting (not replacing).</param>
        bool IsMatch(string line, bool skipVariableSet = false);

        /// <summary>
        /// Whether the triggers output should be silent (not echo to the main terminal).
        /// </summary>
        bool IsSilent { get; set; }

        /// <summary>
        /// Whether or not variables should be replaced in the pattern.  This is offered as
        /// a performance tweak so the player has to opt into it.
        /// </summary>
        bool VariableReplacement { get; set; }

        /// <summary>
        /// Whether or not the matching line should be gagged from terminal.  A gagged line is hidden from view
        /// as if it does not exist but does in fact still exist in the terminal.  If triggers are disabled you
        /// will see gagged lines re-appear.  Further, gagged lines will appear in clipboard actions such as copy.
        /// </summary>
        bool Gag { get; set; }

        /// <summary>
        /// Indicates whether a trigger was loaded from a plugin or not.
        /// </summary>
        bool Plugin { get; set; }

        /// <summary>
        /// If the current trigger is enabled.  A trigger that is not enabled will not be processed.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Whether the command should be executed as a Lua script.
        /// </summary>
        bool IsLua { get; set; }

        /// <summary>
        /// What terminal window to move the triggered line to.
        /// </summary>
        TerminalTarget MoveTo { get; set; }

        /// <summary>
        /// Whether or not the matching line should be highlighted.
        /// </summary>
        bool HighlightLine { get; set; }

        /// <summary>
        /// Whether the trigger is locked.  This will stop a trigger from being auto-updated in a package.  It should
        /// be noted that a lock does not however stop a user from editting the trigger.
        /// </summary>
        bool Lock { get; set; }

        /// <summary>
        /// If set to true will disable the trigger after it fires.
        /// </summary>
        bool DisableAfterTriggered { get; set; }

        /// <summary>
        /// The number of times a trigger has fired.
        /// </summary>
        int Count { get; set; }

        /// <summary>
        /// Execute command that is overridable.
        /// </summary>
        void Execute();

        /// <summary>
        /// A reference to the game's Conveyor so that the trigger can interact with the UI if it's
        /// a CLR trigger.
        /// </summary>
        IConveyor Conveyor { get; set; }

        /// <summary>
        /// The date and time that the trigger was last matched.
        /// </summary>
        /// <remarks>
        /// This will generally be used to debug triggers and try to understand what triggers are firing and in what
        /// order they are firing in.  This can be toggled not to set via the TrackTriggerLastMatched profile setting.
        /// </remarks>
        DateTime LastMatched { get; set; }

    }
}
