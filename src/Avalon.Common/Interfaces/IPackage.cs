using Avalon.Common.Models;
using Avalon.Common.Triggers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalon.Common.Interfaces
{
    /// <summary>
    /// A package which holds all of the metadata to import a set of game functionality.
    /// </summary>
    public interface IPackage
    {
        /// <summary>
        /// The name of the package.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// A description for the package.
        /// </summary>
        string Description { get; set; }

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
