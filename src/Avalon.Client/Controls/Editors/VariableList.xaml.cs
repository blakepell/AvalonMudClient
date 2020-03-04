using Avalon.Common.Models;
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
    public partial class VariableList : UserControl
    {

        /// <summary>
        /// Timer that sets the delay on your filtering TextBox.
        /// </summary>
        DispatcherTimer _typingTimer;

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
        /// This will effectively load the list data into the DataGrid the first time it's shown to the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
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

            this.FirstLoad = false;
        }

        /// <summary>
        /// Reloads the DataList's ItemSource if it's changed.
        /// </summary>
        public void Reload()
        {
            var lcv = new ListCollectionView(App.Settings.ProfileSettings.Variables)
            {
                Filter = Filter
            };

            DataList.ItemsSource = null;
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

            return (variable?.Key?.Contains(TextFilter.Text) ?? false)
                   || (variable?.Value?.Contains(TextFilter.Text) ?? false)
                   || (variable?.Character?.Contains(TextFilter.Text) ?? false);
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

    }
}
