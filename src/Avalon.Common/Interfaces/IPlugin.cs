using System;
using System.Collections.Generic;
using Avalon.Common.Settings;
using Avalon.Common.Triggers;

namespace Avalon.Common.Interfaces
{

    /// <summary>
    /// A plugin that can be instantiated used to add triggers, hash commands and aliases into the
    /// mud client that have been developed for advanced scenarios or ease of use.  The client will
    /// instantiate this class and pass the Conveyor off but this class will be responsible for
    /// loading up any items and preparing them.  IPlugin will be the only class searched for to load.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// The IP Address for the mud the plugin should be active for.  This will match with the profile
        /// IP address so system triggers only load for a mud you're connecting to in case you have multiple
        /// sets of plugins.
        /// </summary>
        /// <returns></returns>
        string IpAddress { get; set; }

        /// <summary>
        /// Will be called first by the mud client.  This method is where all items should be initialized
        /// and put into their respective collections.  Any setup should be called from here (such as creating
        /// database tables if they don't exist, etc.).
        /// </summary>
        void Initialize();

        /// <summary>
        /// A procedure that fires off when a tick occurs.  The client if it knows about this will fire off
        /// the Tick of all plugins that are loaded so that they can do specially handling for anything they
        /// keep track of that has to be updated on tick.
        /// </summary>
        void Tick();

        /// <summary>
        /// The list of triggers that should be imported into (or run) from the mud.  This can be standard
        /// triggers or CLR triggers which are very flexible.  These should be considered core system triggers
        /// that are stable as they are only loaded from the plugin and not changeable by the user at this
        /// point.
        /// </summary>
        List<Trigger> Triggers { get; set; }

        /// <summary>
        /// A list of custom hash commands for the mud the plugin is for.
        /// </summary>
        List<IHashCommand> HashCommands { get; set; }

        /// <summary>
        /// The profile settings of the currently loaded profile.
        /// </summary>
        /// <remarks>
        /// The profile is important because it will allow access to things like the location of the current
        /// database so that any CLR triggers or menu options might be able to use that to establish a database
        /// connection.
        /// </remarks>
        ProfileSettings ProfileSettings { get; set; }

        /// <summary>
        /// A list of top level menu items to add to the UI that represent the platform specific menu.
        /// </summary>
        List<object> MenuItems { get; set; }

        /// <summary>
        /// The Conveyor so that the plugin can interact with the UI.
        /// </summary>
        IConveyor Conveyor { get; set; }
        
        /// <summary>
        /// The prefix and Type for commands that should be exposed to Lua.  Each class will need it's own
        /// prefix/namespace when registered with Lua.
        /// </summary>
        Dictionary<string, Type> LuaCommands { get; set; }

        /// <summary>
        /// Whether or not the plugin is currently initialized.
        /// </summary>
        bool Initialized { get; set; }

    }
}
