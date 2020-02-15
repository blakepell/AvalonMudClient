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
        /// IP address so system triggers only load for a mud you're connecting to in case you have mulitple
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
        /// The list of triggers that should be imported into (or run) from the mud.  This can be standard
        /// triggers or CLR triggers which are very flexible.  These should be considered core system triggers
        /// that are stable as they are only loaded from the plugin and not changeable by the user at this
        /// point.
        /// </summary>
        List<Trigger> Triggers { get; set; }

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
        /// The Conveyor so that the plugin can interact with the UI.
        /// </summary>
        IConveyor Conveyor { get; set; }

    }
}
