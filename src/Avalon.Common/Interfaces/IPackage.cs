using Avalon.Common.Models;
using Avalon.Common.Triggers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalon.Common.Interfaces
{
    public interface IPackage
    {
        List<Alias> AliasList { get; set; }

        List<Direction> DirectionList { get; set; }

        List<Trigger> TriggerList { get; set; }
    }
}
