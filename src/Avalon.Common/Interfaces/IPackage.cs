/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Models;
using Avalon.Common.Triggers;
using System.Collections.Generic;

namespace Avalon.Common.Interfaces
{
    /// <summary>
    /// A package which holds all of the metadata to import a set of game functionality.
    /// </summary>
    public interface IPackage
    {
        /// <summary>
        /// The unique identifier of the package.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the package.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// A description for the package.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// An optional category that the package can be grouped under.
        /// </summary>
        string Category { get; set; }

        /// <summary>
        /// The author of the package.
        /// </summary>
        string Author { get; set; }

        /// <summary>
        /// The game address that the package is intended for (e.g. dsl-mud.org)
        /// </summary>
        string GameAddress { get; set; }

        /// <summary>
        /// The version of the import package.
        /// </summary>
        int Version { get; set; }

        /// <summary>
        /// The minimum version of the mud client that is required for this plugin to work.  Only
        /// set if is needed for commands or features that might not exist in earlier versions.
        /// </summary>
        string MinimumClientVersion { get; set; }

        /// <summary>
        /// A command or commands that will be run when the package is installed.
        /// </summary>
        string SetupCommand { get; set; }

        /// <summary>
        /// A Lua script that if populated will be run when the package is installed.
        /// </summary>
        string SetupLuaScript { get; set; }

        /// <summary>
        /// A command or commands that will be run when the package is uninstalled.
        /// </summary>
        string UninstallCommand { get; set; }

        /// <summary>
        /// A Lua script that if populated will be run when the package is installed.
        /// </summary>
        string UninstallLuaScript { get; set; }

        /// <summary>
        /// The list of packaged aliases.
        /// </summary>
        List<Alias> AliasList { get; set; }

        /// <summary>
        /// The list of packaged directions.
        /// </summary>
        List<Direction> DirectionList { get; set; }

        /// <summary>
        /// The list of packaged triggers.
        /// </summary>
        List<Trigger> TriggerList { get; set; }
    }
}
