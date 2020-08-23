using Argus.ComponentModel;
using Avalon.Common.Models;
using Avalon.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Avalon.Windows
{
    /// <summary>
    /// Custom Directions dialog.
    /// </summary>
    public partial class DirectionsSelectWindow : Window
    {

        /// <summary>
        /// The internal filter for the directions.
        /// </summary>
        ICollectionView _view;

        /// <summary>
        /// The list of directions that we build from the current location.
        /// </summary>
        private List<Direction> DirectionList { get; set; }

        private bool _forceClose = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public DirectionsSelectWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loaded event:  Sets up the custom filter for our current set of directions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DirectionList = new List<Direction>();

            this.BuildDirections();

            _view = CollectionViewSource.GetDefaultView(this.DirectionList);
            _view.Filter = CustomerFilter;
            ListBoxDirections.ItemsSource = _view;
            TextBoxSearch.Focus();
        }

        /// <summary>
        /// Builds a list of directions attempting to link together known directions to find more paths.
        /// </summary>
        public void BuildDirections()
        {
            string room = App.Conveyor.GetVariable("Room");

            this.DirectionList.Clear();

            // Exact matches
            foreach (var item in App.Settings.ProfileSettings.DirectionList.Where(x => x.StartingRoom.Equals(room, StringComparison.OrdinalIgnoreCase)))
            {
                var dir = (Direction)item.Clone();
                dir.DegreeOfSeparation = 1;
                this.DirectionList.Add(dir);
            }

            // TODO: Make this a setting.
            // Now, go through multiple depths trying to build new sets of directions.  If no paths are found at
            // a given depth we can exit ahead of schedule.
            int depth = 5;

            for (int i = 2; i <= depth; i++)
            {
                var tempList = new List<Direction>();
                bool found = false;

                foreach (var item in this.DirectionList)
                {
                    foreach (var path in App.Settings.ProfileSettings.DirectionList.Where(x => x.StartingRoom.Equals(item.EndingRoom)))
                    {
                        if (path.EndingRoom.Equals(room, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (this.DirectionList.Exists(x => path.EndingRoom.Equals(x.EndingRoom)))
                        {
                            continue;
                        }

                        found = true;

                        var newPath = (Direction)path.Clone();
                        newPath.StartingRoom = item.StartingRoom;
                        newPath.Speedwalk = $"{item.Speedwalk} {newPath.Speedwalk}";
                        newPath.DegreeOfSeparation = i;
                        tempList.Add(newPath);
                    }
                }

                if (!found)
                {
                    break;
                }

                this.DirectionList.AddRange(tempList);
            }

            //var sb = new StringBuilder();
            //App.Conveyor.EchoText($"{this.DirectionList.Count} paths found.\r\n");
            //this.DirectionList.ForEach(x => sb.AppendFormat("Tier {0} => {1}: {2}\r\n", x.DegreeOfSeparation, x.Name, x.Speedwalk));
            //App.Conveyor.EchoText(sb.ToString());
        }

        /// <summary>
        /// When the window loses focus is closes itself.  If something else forced the window to close
        /// they should have set _forceClose at what point this function will skip the Close as it would
        /// cause an exception.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Deactivated(object sender, EventArgs e)
        {
            // Don't call close here if Close has already been called.
            if (!_forceClose)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Updates the ICollectionView which filters when the TextBoxSearch's text changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _view.Refresh();
        }

        /// <summary>
        /// Filters the list of directions based off of the Room name and then text that is entered into the text box.
        /// </summary>
        /// <param name="item"></param>
        private bool CustomerFilter(object item)
        {
            var dir = item as Direction;

            if (dir == null)
            {
                return false;
            }

            // Filter by the current room AND by the text the user has typed.
            if (dir.Name.Contains(TextBoxSearch.Text, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles when a key is pressed in the textbox.  Escape exits, enter sends the listbox item
        /// if one is selected, in this case to the #go hash command which will do the actual processing
        /// for sending the direction.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // If Up or Down is pressed then cycle through the ListBox
            if (e.Key == Key.Down || e.Key == Key.Up)
            {
                e.Handled = true;

                if (e.Key == Key.Down)
                {
                    this.IncrementSelection();
                }
                else if (e.Key == Key.Up)
                {
                    this.DecrementSelection();
                }
            }
            else if (e.Key == Key.Escape)
            {
                _forceClose = true;
                this.Close();
            }
            else if (e.Key == Key.Enter)
            {
                if (ListBoxDirections?.SelectedItem == null)
                {
                    return;
                }

                var direction = ((Direction)ListBoxDirections?.SelectedItem)?.Speedwalk;

                if (!string.IsNullOrWhiteSpace(direction))
                {
                    App.MainWindow.Interp.Send($"#walk {direction}");
                }

                _forceClose = true;
                this.Close();
            }
        }

        /// <summary>
        /// Decrements the position in the list box.
        /// </summary>
        private void DecrementSelection()
        {
            if (ListBoxDirections.SelectedIndex == -1)
            {
                ListBoxDirections.SelectedIndex = ListBoxDirections.Items.Count - 1;
            }
            else
            {
                ListBoxDirections.SelectedIndex -= 1;
            }
        }

        /// <summary>
        /// Increments the position in the list box.
        /// </summary>
        private void IncrementSelection()
        {
            if (ListBoxDirections.SelectedIndex == ListBoxDirections.Items.Count - 1)
            {
                ListBoxDirections.SelectedIndex = -1;
            }
            else
            {
                ListBoxDirections.SelectedIndex += 1;
            }
        }

    }
}
