using Avalon.Common.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using MoonSharp.Interpreter.Loaders;
using Avalon.Common.Interfaces;

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
        DispatcherTimer _typingTimer;

        /// <summary>
        /// Whether it's the first time the control has been shown.
        /// </summary>
        public bool FirstLoad { get; set; } = false;

        public DirectionList()
        {
            InitializeComponent();
            _typingTimer = new DispatcherTimer();
            _typingTimer.Tick += this._typingTimer_Tick;
            DataContext = this;
            this.FirstLoad = true;
        }

        /// <summary>
        /// Sets the focus onto the filter text box.
        /// </summary>
        public void FocusFilter()
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(delegate ()
                {
                    TextFilter.Focus();
                }));
        }

        /// <summary>
        /// This will effectively load the list data into the DataGrid the first time it's shown to the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                this.FocusFilter();
            }

            // Load the direction list the first time that it's requested.
            if (DataList.ItemsSource == null)
            {
                DataList.ItemsSource = new ListCollectionView(App.Settings.ProfileSettings.DirectionList)
                {
                    Filter = Filter
                };
            }

            // Nothing should be selected at the start.
            if (this.FirstLoad)
            {
                DataList.SelectedItem = null;
            }

            this.FirstLoad = false;
        }

        /// <summary>
        /// Reloads the DataList's ItemSource if it's changed.
        /// </summary>
        public void Reload()
        {
            var lcv = new ListCollectionView(App.Settings.ProfileSettings.DirectionList)
            {
                Filter = Filter
            };

            DataList.ItemsSource = null;
            DataList.ItemsSource = lcv;
        }

        /// <summary>
        /// The number of items currently selected.
        /// </summary>
        public int SelectedCount()
        {
            return DataList?.SelectedItems?.Count ?? 0;
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

            var direction = (Direction)item;

            // Ugly, but must be done in case nulls slip in (say if someone nulls out a value in the JSON
            // and it gets loaded that way).
            return (direction?.Name?.Contains(TextFilter.Text) ?? false)
                   || (direction?.StartingRoom?.Contains(TextFilter.Text) ?? false);
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
                Text = direction.Speedwalk
            };

            // Set this to be a text editor.
            win.EditorMode = StringEditor.EditorType.Text;

            // Show what direction is being edited in the status bar of the string editor window.
            win.StatusText = $"Direction: {direction.Name}";

            // Startup position of the dialog should be in the center of the parent window.  The
            // owner has to be set for this to work.
            win.Owner = App.MainWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;

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

            var dir = new Direction();
            dir.StartingRoom = direction.EndingRoom;
            dir.EndingRoom = direction.StartingRoom;
            dir.Name = direction.StartingRoom;
            dir.Speedwalk = rev;

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
