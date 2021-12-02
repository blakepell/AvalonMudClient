/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using Avalon.HashCommands;

namespace Avalon.Plugins.DarkAndShatteredLands.HashCommands
{

    /// <summary>
    /// Queries the DSL wiki
    /// </summary>
    public class Wiki : HashCommand
    {
        public Wiki(IInterpreter interp) : base(interp)
        {
        }

        public Wiki()
        {
        }

        public override string Name { get; } = "#wiki";

        public override string Description { get; } = "Opens the DSL wiki and searches for the specified entry.";

        public override void Execute()
        {
            ShellLink($"https://dslmud.fandom.com/wiki/Special:Search?query={this.Parameters.HtmlEncode()}");
        }

        public override Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Shells a link via System.Diagnostics.Process.
        /// </summary>
        /// <param name="url"></param>
        public void ShellLink(string url)
        {
            var link = new Uri(url);

            var psi = new ProcessStartInfo
            {
                FileName = "cmd",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = $"/c start {link.AbsoluteUri}"
            };

            Process.Start(psi);
        }

    }

}
