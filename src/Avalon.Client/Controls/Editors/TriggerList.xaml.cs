/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using ModernWpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for the TriggerList editor.
    /// </summary>
    public partial class TriggerList : IShellControl
    {
        /// <summary>
        /// Provided because of binding.
        /// </summary>
        public bool TriggersEnabled
        {
            get => App.Settings.ProfileSettings.TriggersEnabled;
            set => App.Settings.ProfileSettings.TriggersEnabled = value;
        }

        /// <summary>
        /// Timer that sets the delay on your filtering TextBox.
        /// </summary>
        private readonly DispatcherTimer _typingTimer;

        public TriggerList()
        {
            InitializeComponent();
            _typingTimer = new DispatcherTimer();
            _typingTimer.Tick += this._typingTimer_Tick;
            DataContext = this;
        }

        private void TriggerList_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.FocusFilter();

            // Load the Trigger list the first time that it's requested.
            DataList.ItemsSource = new ListCollectionView(App.Settings.ProfileSettings.TriggerList)
            {
                Filter = Filter
            };

            DataList.SelectedItem = null;

            // Manually setup the bindings.  I couldn't get it to work in the Xaml because the AppSettings gets replaced
            // after this control is loaded.
            var binding = new Binding
            {
                Source = App.Settings.ProfileSettings,
                Path = new PropertyPath("TriggersEnabled"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            BindingOperations.SetBinding(CheckBoxTriggersEnabled, CheckBox.IsCheckedProperty, binding);

            var win = this.FindAscendant<Shell>();

            if (win != null)
            {
                win.StatusBarRightText = $"{App.Settings.ProfileSettings.TriggerList.Count.ToString()} Triggers";
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
        private void TriggerList_OnUnloaded(object sender, RoutedEventArgs e)
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

            DataList.ItemsSource = new ListCollectionView(App.Settings.ProfileSettings.TriggerList)
            {
                Filter = Filter
            };

            Utilities.Utilities.SetupTriggers();

            DataList.Items.Refresh();

            // Manually setup the bindings.  I couldn't get it to work in the Xaml because the AppSettings gets replaced
            // after this control is loaded.
            var binding = new Binding
            {
                Source = App.Settings.ProfileSettings,
                Path = new PropertyPath("TriggersEnabled"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            BindingOperations.ClearAllBindings(CheckBoxTriggersEnabled);
            BindingOperations.SetBinding(CheckBoxTriggersEnabled, CheckBox.IsCheckedProperty, binding);
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

            var trigger = (Common.Triggers.Trigger)item;

            // Search by function name.
            if (TextFilter.Text.StartsWith("function:", StringComparison.Ordinal))
            {
                return trigger?.FunctionName?.Contains(TextFilter.Text.Replace("function:", ""), StringComparison.OrdinalIgnoreCase) ?? false;
            }

            // Search by function name.
            if (TextFilter.Text.StartsWith("id:", StringComparison.Ordinal))
            {
                return trigger?.Identifier?.Contains(TextFilter.Text.Replace("id:", ""), StringComparison.OrdinalIgnoreCase) ?? false;
            }

            // Search by package name.
            if (TextFilter.Text.StartsWith("package:", StringComparison.Ordinal))
            {
                return trigger?.PackageId?.Contains(TextFilter.Text.Replace("package:", ""), StringComparison.OrdinalIgnoreCase) ?? false;
            }

            return (trigger?.Pattern?.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (trigger?.Command?.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (trigger?.Character?.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (trigger?.Group?.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase) ?? false);
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
            // Get the Trigger from the current line.
            var trigger = ((FrameworkElement)sender).DataContext as Common.Triggers.Trigger;

            // Hmm, no Trigger.. gracefully exit.
            if (trigger == null)
            {
                return;
            }

            // Set the initial trigger for the editor.
            var win = new TriggerEditorWindow(trigger)
            {
                StatusText = $"This trigger has fired {trigger.Count.ToString().FormatIfNumber(0)} times."
            };

            // Save the last item and type so the Control+Alt+L alias can re-open it.
            App.InstanceGlobals.LastEditedId = trigger.Identifier;
            App.InstanceGlobals.LastEdited = InstanceGlobals.EditItem.Trigger;

            // Startup position of the dialog should be in the center of the parent window.  The
            // owner has to be set for this to work.
            win.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Show the Trigger editor window.
            win.Show();
        }

        /// <summary>
        /// When a cell is done being edited ensure that all of the Triggers have Conveyors setup.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataList_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Utilities.Utilities.SetupTriggers();
        }

        /// <summary>
        /// Creates a new trigger and shows the trigger editor dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonNewTrigger_Click(object sender, RoutedEventArgs e)
        {
            var trigger = new Common.Triggers.Trigger();

            App.Settings.ProfileSettings.TriggerList.Add(trigger);

            // Set the initial trigger for the editor.
            var win = new TriggerEditorWindow(trigger);

            // Save the last item and type so the Control+Alt+L alias can re-open it.
            App.InstanceGlobals.LastEditedId = trigger.Identifier;
            App.InstanceGlobals.LastEdited = InstanceGlobals.EditItem.Trigger;

            // Startup position of the dialog should be in the center of the parent window.  The
            // owner has to be set for this to work.
            win.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Show the Trigger editor window.
            win.Show();
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