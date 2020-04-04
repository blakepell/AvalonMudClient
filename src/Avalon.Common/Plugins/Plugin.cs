using System.Collections.Generic;
using Avalon.Common.Interfaces;
using Avalon.Common.Settings;
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
            this.MenuItems = new List<object>();
            this.HashCommands = new List<IHashCommand>();
        }

        public abstract void Initialize();

        public abstract string IpAddress { get; set; }

        public ProfileSettings ProfileSettings { get; set; } = new ProfileSettings();

        public IConveyor Conveyor { get; set; }

        public List<Trigger> Triggers { get; set; }

        public List<IHashCommand> HashCommands { get; set; }

        public List<object> MenuItems { get; set; }

    }
}
