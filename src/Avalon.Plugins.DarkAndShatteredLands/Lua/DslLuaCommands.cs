/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using Avalon.Plugins.DarkAndShatteredLands.Affects;
using System;
using System.Linq;

namespace Avalon.Plugins.DarkAndShatteredLands.Lua
{

    /// <summary>
    /// General DSL Lua commands.
    /// </summary>
    public class DslLuaCommands : ILuaCommand
    {
        public IInterpreter Interpreter { get; set; }

        public string Namespace { get; set; } = "dsl";

        /// <summary>
        /// If the character is affected by the affect name.
        /// </summary>
        /// <param name="affectName">The name of the affect as it is specified in game.</param>
        public bool IsAffected(string affectName)
        {
            var affectsTrigger = (AffectsTrigger)this.Interpreter.Conveyor.FindTrigger("c40f9237-7753-4357-84a5-8e7d789853ed");

            if (affectsTrigger == null)
            {
               throw new Exception("Affects trigger was null.");
            }

            return affectsTrigger.Affects.Any(x => x.Name.Equals(affectName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns the duration in ticks of the affect.  -1 indicates permanent and -2 indicates the affect does not exist on the player.
        /// </summary>
        /// <param name="affectName">The name of the affect as it is specified in game.</param>
        public int AffectDuration(string affectName)
        {
            var affectsTrigger = (AffectsTrigger)this.Interpreter.Conveyor.FindTrigger("c40f9237-7753-4357-84a5-8e7d789853ed");

            if (affectsTrigger == null)
            {
                throw new Exception("Affects trigger was null.");
            }

            return affectsTrigger.Affects.Find(x => x.Name.Equals(affectName, StringComparison.OrdinalIgnoreCase))?.Duration ?? -2;
        }

    }

}
