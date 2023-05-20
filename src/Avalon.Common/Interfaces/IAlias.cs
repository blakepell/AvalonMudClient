/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Models;

namespace Avalon.Common.Interfaces
{
    /// <summary>
    /// An alias that invokes another command, a series of commands or a script by a
    /// provided scripting engine.
    /// </summary>
    public interface IAlias
    {
        /// <summary>
        /// A unique identifier that does not change even when the alias <see cref="AliasExpression"/> changes.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// The expression which fires off this <see cref="Alias"/>.
        /// </summary>
        string AliasExpression { get; set; }

        /// <summary>
        /// The character who the alias should be isolated to (if any).  If set, the Character
        /// variable must be set with the current character who is playing.  In that scenario
        /// this trigger will only ever be processed if that character is logged in (e.g. if that
        /// variable is set).
        /// </summary>
        string Character { get; set; }

        /// <summary>
        /// The command or script that should be executed when this alias is executed.
        /// </summary>
        string Command { get; set; }

        /// <summary>
        /// The number of times the alias has been processed.
        /// </summary>
        int Count { get; set; }

        /// <summary>
        /// If the alias is currently enabled to be processed.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// The name of the group a package belongs to.  This metadata will allow the user to sort
        /// in the UI or enable/disable entire groups if needed.
        /// </summary>
        string Group { get; set; }

        /// <summary>
        /// If the alias should execute it's command in the Lua scripting engine.
        /// </summary>
        bool IsLua { get; set; }

        /// <summary>
        /// How the command should be executed (as a command or sent to a different output like
        /// a script engine or even to a file).
        /// </summary>
        public ExecuteType ExecuteAs { get; set; }

        /// <summary>
        /// If the alias is locked or not.  A locked alias will not be updated by the package
        /// engine.  This allows the user to make alterations that won't be overwritten by
        /// package updates.
        /// </summary>
        bool Lock { get; set; }

        /// <summary>
        /// The unique identifier of the Package if the <see cref="IAlias"/> was imported.
        /// </summary>
        string PackageId { get; set; }
    }
}