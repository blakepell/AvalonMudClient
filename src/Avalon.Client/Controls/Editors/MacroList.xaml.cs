using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for the MacroList editor.
    /// </summary>
    public partial class MacroList : UserControl, IShellControl
    {

        /// <summary>
        /// Timer that sets the delay on your filtering TextBox.
        /// </summary>
        private readonly DispatcherTimer _typingTimer;

        public MacroList()
        {
            InitializeComponent();
            _typingTimer = new DispatcherTimer();
            _typingTimer.Tick += this._typingTimer_Tick;
            DataContext = this;
        }

        private void MacroList_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.FocusFilter();

            // Fix the descriptions on any macros in case they get borked up.
            this.FixMacros();

            DataList.ItemsSource = new ListCollectionView(App.Settings.ProfileSettings.MacroList)
            {
                Filter = Filter
            };

            DataList.SelectedItem = null;
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
        /// When the control is unloaded: Release any bindings or objects that need to be freed or
        /// detached from this control so it can be GC'd properly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MacroList_OnUnloaded(object sender, RoutedEventArgs e)
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

            var lcv = new ListCollectionView(App.Settings.ProfileSettings.MacroList)
            {
                Filter = Filter
            };

            DataList.ItemsSource = lcv;
            DataList.Items.Refresh();
        }

        /// <summary>
        /// Attempt to fix any macros with broken descriptions.
        /// </summary>
        public void FixMacros()
        {
            // Fix any macros if they don't have descriptions
            try
            {
                var convert = new System.Windows.Input.KeyConverter();

                foreach (var macro in App.Settings.ProfileSettings.MacroList)
                {
                    if (string.IsNullOrWhiteSpace(macro.KeyDescription))
                    {
                        macro.KeyDescription = convert.ConvertToString(macro.Key);
                    }
                }
            }
            catch
            {
                // TODO - Error Logging
            }
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

            var macro = (Macro)item;

            return macro?.Command?.Contains(TextFilter.Text) ?? false;
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
            // Get the macro from the current line.
            var macro = ((FrameworkElement)sender).DataContext as Macro;

            // Hmm, no macro.. gracefully exit.
            if (macro == null)
            {
                return;
            }

            // Set the initial text for the editor.
            var win = new MacroEditWindow
            {
                Macro = macro
            };

            // Show what macro is being edited in the status bar of the string editor window.
            win.SetStatus(macro);

            // Startup position of the dialog should be in the center of the parent window.  The
            // owner has to be set for this to work.
            win.Owner = App.MainWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.EditType = MacroEditWindow.EditTypeCode.Edit;

            // Show the string dialog
            var result = win.ShowDialog();

            //// If the result
            //if (result != null && result.Value)
            //{
            //    macro. = win.Command;
            //}
        }

        /// <summary>
        /// Creates a new macro.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonNewMacro_Click(object sender, RoutedEventArgs e)
        {
            var macro = new Macro();
            App.Settings.ProfileSettings.MacroList.Add(macro);

            // Set the initial text for the editor.
            var win = new MacroEditWindow
            {
                Macro = macro
            };

            // Show what macro is being edited in the status bar of the string editor window.
            win.SetStatus("Put the focus in the 'Macro Key' text box and press the key you want to create a macro for.");

            // Startup position of the dialog should be in the center of the parent window.  The
            // owner has to be set for this to work.
            win.Owner = App.MainWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.EditType = MacroEditWindow.EditTypeCode.Add;

            // Show the string dialog
            var result = win.ShowDialog();
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
