using System.Collections.Generic;
using Avalon.Common.Interfaces;
using Avalon.Common.Triggers;

namespace Avalon.Common.Plugins
{
    /// <summary>
    /// A base class that can be used with any IPlugin implementations that will instantiate
    /// any properties and keep from having redundant code.
    /// </summary>
    public abstract class Plugin : IPlugin
    {
        protected Plugin()
        {
            this.Triggers = new List<Trigger>();
        }

        public abstract void Initialize();

        public abstract string IpAddress { get; set; }

        public List<Trigger> Triggers { get; set; }
    }
}
