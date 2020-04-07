using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalon.Controls.AutoCompleteTextBox.Editors;

namespace Avalon.Controls.AutoCompleteTextBox
{
    /// <summary>
    /// The provider for the auto complete box that surfaces the data
    /// </summary>
    public class AutoCompleteCommandProvider : ISuggestionProvider
    {
        public List<AutoCompleteCommand> Items { get; set; } = new List<AutoCompleteCommand>();

        public AutoCompleteCommandProvider()
        {

        }

        /// <summary>
        /// Updates the list of auto complete entries.
        /// </summary>
        public void RefreshAutoCompleteEntries()
        {
            Items.Clear();

            var directions = App.Settings.ProfileSettings.DirectionList.Select(x => x.Name).Distinct();
            var aliases = App.Settings.ProfileSettings.AliasList.Select(x => x.AliasExpression).Distinct();

            // TODO - Directions based soley off of the room your in
            //string room = Utilities.GetVariable("Room");
            //var directions = AppSettings.DirectionList.Where(x => x.StartingRoom == room).Select(x => x.Name).Distinct();

            foreach (var item in directions)
            {
                var cmd = new AutoCompleteCommand();
                cmd.Command = $"#go {item}";
                Items.Add(cmd);
            }

            foreach (var item in aliases)
            {
                var cmd = new AutoCompleteCommand();
                cmd.Command = $"#a {item}";
                Items.Add(cmd);
            }

            // Add the available hash commands into the auto complete box.
            foreach (var hashCmd in App.MainWindow.Interp.HashCommands)
            {
                var cmd = new AutoCompleteCommand();
                cmd.Command = hashCmd.Name;
                Items.Add(cmd);
            }

        }

        public IEnumerable GetSuggestions(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return null;

            return Items.Where(x => x.Command.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }
    }
}