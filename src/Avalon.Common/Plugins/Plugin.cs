/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

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

        public abstract void Tick();

        public abstract string IpAddress { get; set; }

        public ProfileSettings ProfileSettings { get; set; } = new();

        public IConveyor Conveyor { get; set; }

        public List<Trigger> Triggers { get; set; }

        public List<IHashCommand> HashCommands { get; set; }

        public List<object> MenuItems { get; set; }

        public bool Initialized { get; set; } = false;

        public Dictionary<string, Type> LuaCommands { get; set; }  = new();

    }
}
