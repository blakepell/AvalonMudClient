/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using ModernWpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for the DirectionList editor.
    /// </summary>
    public partial class DirectionList : UserControl, IShellControl
    {

        /// <summary>
        /// Timer that sets the delay on your filtering TextBox.
        /// </summary>
        readonly DispatcherTimer _typingTimer;

        public DirectionList()
        {
            InitializeComponent();
            _typingTimer = new DispatcherTimer();
            _typingTimer.Tick += this._typingTimer_Tick;
            DataContext = this;
        }

        private void DirectionList_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.FocusFilter();

            // Load the direction list the first time that it's requested.
            DataList.ItemsSource = new ListCollectionView(App.Settings.ProfileSettings.DirectionList)
            {
                Filter = Filter
            };

            DataList.SelectedItem = null;

            var win = this.FindAscendant<Shell>();

            if (win != null)
            {
                win.StatusBarRightText = $"{App.Settings.ProfileSettings.AliasList.Count} Directions";
            }
        }

        /// <summary>
        /// Sets the focus onto the filter text box.
        /// </summary>
        public void FocusFilter()
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(() => TextFilter.Focus()));
        }

        /// <summary>
        /// When the control is unloaded: Release any bindings or objects that need to be freed or
        /// detached from this control so it can be GC'd properly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DirectionList_OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataList.ItemsSource is ListCollectionView lcv)
            {
                lcv.DetachFromSourceCollection();
                DataList.ItemsSource = null;
            }

            // Unsubscribe to the tick event so it doesn't leak.
            _typingTimer.Tick -= this._typingTimer_Tick;
        }

        /// <summary>
        /// Reloads the DataList's ItemSource if it's changed.
        /// </summary>
        public void Reload()
        {
            if (DataList.ItemsSource is ListCollectionView lcvOld)
            {
                lcvOld.DetachFromSourceCollection();
                DataList.ItemsSource = null;
            }

            var lcv = new ListCollectionView(App.Settings.ProfileSettings.DirectionList)
            {
                Filter = Filter
            };

            DataList.ItemsSource = lcv;
        }

        /// <summary>
        /// The number of items currently selected.
        /// </summary>
        public int SelectedCount()
        {
            return DataList?.SelectedItems.Count ?? 0;
        }

        /// <summary>
        /// The typing delay timer's tick that will refresh the filter after 300ms.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// It's important to stop this timer when this fires so that it doesn't continue
        /// to fire until it's needed again.
        /// </remarks>
        private void _typingTimer_Tick(object sender, EventArgs e)
        {
            _typingTimer.Stop();
            ((ListCollectionView)DataList?.ItemsSource)?.Refresh();
        }

        /// <summary>
        /// The actual filter that's used to filter down the DataGrid.
        /// </summary>
        /// <param name="item"></param>
        private bool Filter(object item)
        {
            if (string.IsNullOrWhiteSpace(TextFilter.Text))
            {
                return true;
            }

            var direction = item as Direction;

            // Ugly, but must be done in case nulls slip in (say if someone nulls out a value in the JSON
            // and it gets loaded that way).
            return (direction?.Name?.Contains(TextFilter.Text, StringComparison.CurrentCultureIgnoreCase) ?? false)
                   || (direction?.StartingRoom?.Contains(TextFilter.Text, StringComparison.CurrentCultureIgnoreCase) ?? false)
                   || (direction?.EndingRoom?.Contains(TextFilter.Text, StringComparison.CurrentCultureIgnoreCase) ?? false);
        }

        /// <summary>
        /// The filter's text changed event that will setup the delay timer and effective callback.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            _typingTimer.Stop();
            _typingTimer.Interval = TimeSpan.FromMilliseconds(300);
            _typingTimer.Start();
        }

        private void ButtonEdit_OnClick(object sender, RoutedEventArgs e)
        {
            // Get the direction from the current line.
            var direction = ((FrameworkElement)sender).DataContext as Direction;

            // Hmm, no direction.. gracefully exit.
            if (direction == null)
            {
                return;
            }

            // Set the initial text for the editor.
            var win = new StringEditor
            {
                Text = direction.Speedwalk,
                EditorMode = StringEditor.EditorType.Text,
                StatusText = $"Direction: {direction.Name}",
                Owner = App.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            // Show the string dialog
            var result = win.ShowDialog();

            // If the result
            if (result != null && result.Value)
            {
                direction.Speedwalk = win.Text;
            }
        }

        /// <summary>
        /// A cell has been edited.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataList_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // A direction has been edited possibly, go ahead and refresh the auto complete entries.
            App.MainWindow.RefreshAutoCompleteEntries();
        }

        /// <summary>
        /// Creates a new direction with the reverse direction of the one selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateReverseMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            // Get the direction from the current line.
            var direction = ((FrameworkElement)sender).DataContext as Direction;

            // Hmm, no direction.. gracefully exit.
            if (direction == null)
            {
                return;
            }

            string rev = Utilities.Utilities.SpeedwalkReverse(direction.Speedwalk, true);

            var dir = new Direction
            {
                StartingRoom = direction.EndingRoom,
                EndingRoom = direction.StartingRoom,
                Name = direction.StartingRoom,
                Speedwalk = rev
            };

            App.Settings.ProfileSettings.DirectionList.Add(dir);
        }

        public void PrimaryButtonClick()
        {
            // Do nothing.
        }

        public void SecondaryButtonClick()
        {
            // Do nothing.
        }
    }
}