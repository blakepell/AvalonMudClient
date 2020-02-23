using Avalon.Common.Interfaces;
using Avalon.Common.Triggers;
using System.Collections.Generic;

namespace Avalon.Common.Models
{
    public class Package : IPackage
    {
        public List<Alias> AliasList { get; set; } = new List<Alias>();

        public List<Trigger> TriggerList { get; set; } = new List<Trigger>();

        public List<Direction> DirectionList { get; set; } = new List<Direction>();

    }
}
