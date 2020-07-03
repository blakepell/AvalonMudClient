using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Argus.Extensions;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for the TriggerList editor.
    /// </summary>
    public partial class TriggerList : UserControl
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
        /// Whether it's the first time the control has been shown.
        /// </summary>
        public bool FirstLoad { get; set; } = false;

        /// <summary>
        /// Timer that sets the delay on your filtering TextBox.
        /// </summary>
        DispatcherTimer _typingTimer;

        public TriggerList()
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

            // Load the Trigger list the first time that it's requested.
            if (DataList.ItemsSource == null)
            {
                var lcv = new ListCollectionView(App.Settings.ProfileSettings.TriggerList)
                {
                    Filter = Filter
                };

                DataList.ItemsSource = lcv;
            }

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
            var lcv = new ListCollectionView(App.Settings.ProfileSettings.TriggerList)
            {
                Filter = Filter
            };

            DataList.ItemsSource = null;
            DataList.ItemsSource = lcv;

            TriggerConveyorSetup();

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
            return DataList?.SelectedItems?.Count ?? 0;
        }

        /// <summary>
        /// Sets all triggers up with the Conveyor from the MainWindow if they haven't been wired up already.
        /// </summary>
        public void TriggerConveyorSetup()
        {
            if (App.Settings?.ProfileSettings?.TriggerList == null)
            {
                return;
            }

            foreach (var trigger in App.Settings.ProfileSettings.TriggerList)
            {
                if (trigger.Conveyor == null && App.Conveyor != null)
                {
                    trigger.Conveyor = App.Conveyor;
                }
            }
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

            return (trigger?.Pattern?.Contains(TextFilter.Text) ?? false)
                   || (trigger?.Command?.Contains(TextFilter.Text) ?? false)
                   || (trigger?.Character?.Contains(TextFilter.Text) ?? false)
                   || (trigger?.Group?.Contains(TextFilter.Text) ?? false);
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
            var win = new TriggerEditorWindow(trigger);

            // Show what trigger is being edited in the status bar of the string editor window.
            win.StatusText = $"This trigger has fired {trigger.Count.ToString().FormatIfNumber(0)} times.";

            // Save the last item and type so the Control+Alt+L alias can re-open it.
            App.InstanceGlobals.LastEdittedId = trigger.Identifier;
            App.InstanceGlobals.LastEditted = InstanceGlobals.EditItem.Trigger;

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
            TriggerConveyorSetup();
        }

        private void DataList_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            // Get the Trigger from the current line.
            var trigger = e?.NewItem as Common.Triggers.Trigger;

            if (trigger != null & App.Conveyor != null)
            {
                trigger.Conveyor = App.Conveyor;
            }
        }
    }
}
