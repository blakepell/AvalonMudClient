using Avalon.Common.Models;
using System;
using System.ComponentModel;
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
        ICollectionView _view = CollectionViewSource.GetDefaultView(App.Settings.ProfileSettings.DirectionList);

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
            _view.Filter = CustomerFilter;
            ListBoxDirections.ItemsSource = _view;
            TextBoxSearch.Focus();
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

            string room = App.Conveyor.GetVariable("Room");

            // Filter by the current room AND by the text the user has typed.
            if (dir.StartingRoom.Equals(room, StringComparison.OrdinalIgnoreCase) && dir.Name.Contains(TextBoxSearch.Text, StringComparison.OrdinalIgnoreCase))
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

                string direction = ((Direction)ListBoxDirections?.SelectedItem)?.Name;

                if (direction != null)
                {
                    App.MainWindow.Interp.Send($"#go {direction}");
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
