/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Plugins.DarkAndShatteredLands.Affects
{
    /// <summary>
    /// A class that represents mapping data for how a player can invoke an affect via a command.
    /// </summary>
    public class AffectCommand
    {
        public AffectCommand()
        {

        }

        public AffectCommand(string affectName, string command, bool ignoreCommand)
        {
            this.AffectName = affectName;
            this.Command = command;
            this.IgnoreCommand = ignoreCommand;
        }

        /// <summary>
        /// The exact name of the affect.
        /// </summary>
        public string AffectName { get; set; } = "";

        /// <summary>
        /// The command that should be executed to re-apply this affect.
        /// </summary>
        public string Command { get; set; } = "";

        /// <summary>
        /// A boolean (for speed) on whether the command should be ignored for this affect.
        /// </summary>
        public bool IgnoreCommand { get; set; } = false;

    }
}
