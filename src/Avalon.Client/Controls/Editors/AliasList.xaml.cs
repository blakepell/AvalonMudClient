using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using ModernWpf;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for the AliasList editor.
    /// </summary>
    public partial class AliasList : UserControl, IShellControl
    {
        /// <summary>
        /// Provided because of binding.
        /// </summary>
        public bool AliasesEnabled
        {
            get => App.Settings.ProfileSettings.AliasesEnabled;
            set => App.Settings.ProfileSettings.AliasesEnabled = value;
        }

        /// <summary>
        /// Timer that sets the delay on your filtering TextBox.
        /// </summary>
        private readonly DispatcherTimer _typingTimer;

        public AliasList()
        {
            InitializeComponent();
            _typingTimer = new DispatcherTimer();
            _typingTimer.Tick += _typingTimer_Tick;
            DataContext = this;
        }

        private void AliasList_OnLoaded(object sender, RoutedEventArgs e)
        {
            FocusFilter();

            // Load the alias list the first time that it's requested.
            DataList.ItemsSource = new ListCollectionView(App.Settings.ProfileSettings.AliasList)
            {
                Filter = Filter
            };

            DataList.SelectedItem = null;

            // Manually setup the bindings.  I couldn't get it to work in the Xaml because the AppSettings gets replaced
            // after this control is loaded.
            var binding = new Binding
            {
                Source = App.Settings.ProfileSettings,
                Path = new PropertyPath("AliasesEnabled"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            BindingOperations.SetBinding(CheckBoxAliasesEnabled, CheckBox.IsCheckedProperty, binding);

            var win = this.FindAscendant<Shell>();

            if (win != null)
            {
                win.StatusBarRightText = $"{App.Settings.ProfileSettings.AliasList.Count} Aliases";
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
        private void AliasList_OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataList.ItemsSource is ListCollectionView lcv)
            {
                lcv.DetachFromSourceCollection();
                DataList.ItemsSource = null;
            }

            // Unsubscribe to the tick event so it doesn't leak.
            _typingTimer.Tick -= _typingTimer_Tick;
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

            var lcv = new ListCollectionView(App.Settings.ProfileSettings.AliasList)
            {
                Filter = Filter
            };

            DataList.ItemsSource = lcv;
            DataList.Items.Refresh();

            // Manually setup the bindings.  I couldn't get it to work in the Xaml because the AppSettings gets replaced
            // after this control is loaded.
            var binding = new Binding
            {
                Source = App.Settings.ProfileSettings,
                Path = new PropertyPath("AliasesEnabled"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            BindingOperations.ClearAllBindings(CheckBoxAliasesEnabled);
            BindingOperations.SetBinding(CheckBoxAliasesEnabled, CheckBox.IsCheckedProperty, binding);
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

            var alias = (Alias)item;

            // Ugly, but must be done in case nulls slip in (say if someone nulls out a value in the JSON
            // and it gets loaded that way).
            return (alias?.AliasExpression?.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (alias?.Command?.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (alias?.Character?.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (alias?.Group?.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase) ?? false);
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
            // Get the alias from the current line.
            var alias = ((FrameworkElement)sender).DataContext as Alias;

            // Hmm, no alias.. gracefully exit.
            if (alias == null)
            {
                return;
            }

            // Set the initial text for the editor.
            var win = new StringEditor
            {
                Text = alias.Command
            };

            // Set the initial type for highlighting.
            if (alias.IsLua)
            {
                win.EditorMode = StringEditor.EditorType.Lua;
            }

            // Save the last item and type so the Control+Alt+L alias can re-open it.
            App.InstanceGlobals.LastEditedId = alias.AliasExpression;
            App.InstanceGlobals.LastEdited = InstanceGlobals.EditItem.Alias;

            // Show what alias is being edited in the status bar of the string editor window.
            win.StatusText = $"Alias: {alias.AliasExpression}";

            // Startup position of the dialog should be in the center of the parent window.  The
            // owner has to be set for this to work.
            win.Owner = App.MainWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Show the Lua dialog.
            var result = win.ShowDialog();

            // If the result
            if (result != null && result.Value)
            {
                alias.Command = win.Text;
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
