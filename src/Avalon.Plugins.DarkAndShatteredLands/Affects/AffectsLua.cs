using Avalon.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Avalon.Plugins.DarkAndShatteredLands.Affects
{

    /// <summary>
    /// C# methods that are exposed to LUA.
    /// </summary>
    public class AffectsLuaCommands
    {
        private IInterpreter _interpreter;

        /// <summary>
        /// A reference to the affects trigger.
        /// </summary>
        public AffectsTrigger AffectsTrigger { get; set; }

        public AffectsLuaCommands(IInterpreter interp)
        {
            _interpreter = interp;
        }

        /// <summary>
        /// If the character is affected by the affect name.
        /// </summary>
        /// <param name="affectName"></param>
        public bool IsAffected(string affectName)
        {
            return this.AffectsTrigger.Affects.Any(x => x.Name.Equals(affectName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns the duration in ticks of the affect.  -1 indicates permanent and -2 indicates the affect does not exist on the player.
        /// </summary>
        /// <param name="affectName"></param>
        public int AffectDuration(string affectName)
        {
            return this.AffectsTrigger.Affects.FirstOrDefault(x => x.Name.Equals(affectName, StringComparison.OrdinalIgnoreCase))?.Duration ?? -2;
        }

    }

}
