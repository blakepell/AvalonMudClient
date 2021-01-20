using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using ModernWpf;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Argus.Extensions;

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

        public VariableList()
        {
            InitializeComponent();
            _typingTimer = new DispatcherTimer();
            _typingTimer.Tick += this._typingTimer_Tick;
            DataContext = null;
        }

        private void VariableList_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.FocusFilter();

            // Load the variable list the first time that it's requested.
            DataList.ItemsSource = new ListCollectionView(App.Settings.ProfileSettings.Variables)
            {
                Filter = Filter
            };

            DataList.SelectedItem = null;

            var win = this.FindAscendant<Shell>();

            if (win != null)
            {
                win.StatusBarRightText = $"{App.Settings.ProfileSettings.Variables.Count} Variables";
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
        private void VariableList_OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataList.ItemsSource is ListCollectionView lcv)
            {
                lcv.DetachFromSourceCollection();
                DataList.ItemsSource = null;
            }

            // Unsubscribe to the tick event so it doesn't leak.
            _typingTimer.Tick -= this._typingTimer_Tick;

            // Remove anything with a null or blank key.
            for (int i = App.Settings.ProfileSettings.Variables.Count - 1; i >= 0; i--)
            {
                if (App.Settings.ProfileSettings.Variables[i].Key.IsNullOrEmptyOrWhiteSpace())
                {
                    App.Settings.ProfileSettings.Variables.RemoveAt(i);
                }
            }
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

            DataList.ItemsSource = new ListCollectionView(App.Settings.ProfileSettings.Variables)
            {
                Filter = Filter
            };

            DataList.Items.Refresh();
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

        /// <summary>
        /// Close the window with an Ok or success.
        /// </summary>
        public void PrimaryButtonClick()
        {
            App.MainWindow.VariableRepeater.Bind();
        }

        /// <summary>
        /// Close the window with a cancel or close.
        /// </summary>
        public void SecondaryButtonClick()
        {
            App.MainWindow.VariableRepeater.Bind();
        }

        /// <summary>
        /// Event that fires when the selected variable changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var v = e.AddedItems[0] as Variable;

                if (v == null)
                {
                    App.Settings.ProfileSettings.Variables.Add(new Variable("", ""));
                    return;
                }

                this.DataContext = v;
                LuaEditor.SaveObject = v;
                LuaEditor.SaveProperty = "OnChangeEvent";
                LuaEditor.Editor.Text = !string.IsNullOrWhiteSpace(v.OnChangeEvent) ? v.OnChangeEvent : string.Empty;

                App.MainWindow.VariableRepeater.Bind();
            }
        }

        /// <summary>
        /// Limits the number box to only digits.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextPriority_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (e.Text.Any(c => !char.IsDigit(c)))
            {
                e.Handled = true;
            }
        }
    }
}