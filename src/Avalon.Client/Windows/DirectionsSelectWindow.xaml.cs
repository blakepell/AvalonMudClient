/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Avalon.Windows
{
    /// <summary>
    /// Custom Directions dialog.
    /// </summary>
    public partial class DirectionsSelectWindow
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
            // Build the directions that are available from this room.
            this.DirectionList = new List<Direction>();
            this.BuildDirections();

            // Setup the view that will allow us to filter this list.
            _view = CollectionViewSource.GetDefaultView(this.DirectionList);
            _view.Filter = DirectionsFilter;
            ListBoxDirections.ItemsSource = _view;

            // Wire up what we're going to do for the search box.
            TextBoxSearch.SearchExecuted += this.TextBoxSearch_SearchExecuted;

            // Focus the text box so the user can start typing immediately.
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
                        if (path.EndingRoom.Equals(room, StringComparison.OrdinalIgnoreCase)
                            || string.IsNullOrWhiteSpace(path.EndingRoom)
                            || string.IsNullOrWhiteSpace(path.StartingRoom)
                            || this.DirectionList.Exists(x => path.EndingRoom.Equals(x.EndingRoom)))
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
            // Cleanup any events that need cleaning up.
            TextBoxSearch.SearchExecuted -= this.TextBoxSearch_SearchExecuted;

            // Don't call close here if Close has already been called.
            if (!_forceClose)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Filters the list of directions based off of the Room name and then text that is entered into the text box.
        /// </summary>
        /// <param name="item"></param>
        private bool DirectionsFilter(object item)
        {
            if (!(item is Direction dir))
            {
                return false;
            }

            // Filter by the current room AND by the text the user has typed.
            if (dir.Name.Contains($"{TextBoxSearch.Text}", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles when a key is pressed in the <see cref="TextBox"/>.  Used for special keys, Up, Down
        /// and Escape.
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

        }

        private void TextBoxSearch_SearchExecuted(object sender, Controls.SearchBox.SearchEventArgs e)
        {
            // In case the deactivate stops working, if the text box is empty and enter is hit close the window.
            if (_view.CurrentItem == null)
            {
                _forceClose = true;
                this.Close();
                return;
            }

            // Cast the direction from the CurrentItem of the view.
            var direction = _view?.CurrentItem as Direction;
            
            // Must have the speedwalk available to send.
            if (direction == null || string.IsNullOrWhiteSpace(direction.Speedwalk))
            {
                _forceClose = true;
                this.Close();
                return;
            }


            App.MainWindow.Interp.Send($"#walk {direction.Speedwalk}");

            _forceClose = true;
            this.Close();
        }

        /// <summary>
        /// Handles when the search box text changes.  This will refresh the view that filters the
        /// list box as well as move the currently selected item to the top every time the refresh
        /// takes place.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Move enter portion here, get preview key down
            _view.Refresh();

            // If a return wasn't added to the search box then it was filtered, move the
            // selected item back to the top.
            if (!TextBoxSearch.Text.EndsWith("\r\n", StringComparison.Ordinal))
            {
                _view.MoveCurrentToFirst();
            }
        }

        /// <summary>
        /// Handle if an item is clicked on with the mouse.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxDirections_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

        /// <summary>
        /// Decrements the position in the list box.
        /// </summary>
        private void DecrementSelection()
        {
            if (ListBoxDirections.SelectedIndex == -1)
            {
                _view.MoveCurrentToLast();
            }
            else
            {
                _view.MoveCurrentToPrevious();
            }

            // Since the selected item was changed programmatically, scroll it into view.
            if (ListBoxDirections.SelectedItem != null)
            {
                ListBoxDirections.ScrollIntoView(ListBoxDirections.SelectedItem);
            }
        }

        /// <summary>
        /// Increments the position in the list box.
        /// </summary>
        private void IncrementSelection()
        {
            if (ListBoxDirections.SelectedIndex == ListBoxDirections.Items.Count - 1)
            {
                _view.MoveCurrentToFirst();
            }
            else
            {
                _view.MoveCurrentToNext();
            }

            // Since the selected item was changed programmatically, scroll it into view.
            if (ListBoxDirections.SelectedItem != null)
            {
                ListBoxDirections.ScrollIntoView(ListBoxDirections.SelectedItem);
            }
        }

    }
}
