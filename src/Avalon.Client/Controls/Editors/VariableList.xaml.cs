using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using ModernWpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for the VariableList editor.
    /// </summary>
    public partial class VariableList : UserControl, IShellControl
    {

        /// <summary>
        /// Timer that sets the delay on your filtering TextBox.
        /// </summary>
        private readonly DispatcherTimer _typingTimer;

        /// <summary>
        /// Whether it's the first time the control has been shown.
        /// </summary>
        public bool FirstLoad { get; set; } = false;

        public VariableList()
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

            // Load the variable list the first time that it's requested.
            if (DataList.ItemsSource == null)
            {
                var lcv = new ListCollectionView(App.Settings.ProfileSettings.Variables)
                {
                    Filter = Filter
                };

                DataList.ItemsSource = lcv;
            }

            // Nothing should be selected at the start.
            if (this.FirstLoad)
            {
                DataList.SelectedItem = null;
            }

            var win = this.FindAscendant<Shell>();
            win.StatusBarRightText = $"{App.Settings.ProfileSettings.Variables.Count} Variables";

            this.FirstLoad = false;
        }

        /// <summary>
        /// When the control is unloaded: Release any bindings or objects that need to be freed or
        /// detached from this control so it can be GC'd properly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VariableList_OnUnloaded(object sender, RoutedEventArgs e)
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

            var lcv = new ListCollectionView(App.Settings.ProfileSettings.Variables)
            {
                Filter = Filter
            };

            DataList.ItemsSource = lcv;
            DataList.Items.Refresh();
        }

        /// <summary>
        /// The number of items currently selected.
        /// </summary>
        public int SelectedCount()
        {
            return DataList?.SelectedItems?.Count ?? 0;
        }

        /// <summary>
        /// A element has been updated.  Update the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataList_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            App.MainWindow.VariableRepeater.Bind();
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

            var variable = (Variable)item;

            return (variable?.Key?.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (variable?.Value?.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (variable?.Character?.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase) ?? false);
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
            // Get the variable from the current line.
            var variable = ((FrameworkElement)sender).DataContext as Variable;

            // Hmm, no variable.. gracefully exit.
            if (variable == null)
            {
                return;
            }

            // Set the initial text for the editor.
            var win = new StringEditor
            {
                Text = variable.Value
            };

            // Set this to be a text editor.
            win.EditorMode = StringEditor.EditorType.Text;

            // Show what alias is being edited in the status bar of the string editor window.
            win.StatusText = $"Variable: {variable.Key}";

            // Startup position of the dialog should be in the center of the parent window.  The
            // owner has to be set for this to work.
            win.Owner = App.MainWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Show the string dialog
            var result = win.ShowDialog();

            // If the result
            if (result != null && result.Value)
            {
                variable.Value = win.Text;
            }
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
