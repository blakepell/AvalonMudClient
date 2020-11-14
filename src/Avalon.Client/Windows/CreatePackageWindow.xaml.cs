using Avalon.Common.Models;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Linq;
using System.Linq.Expressions;

namespace Avalon
{
    /// <summary>
    /// Interaction logic for ExportPackage.xaml
    /// </summary>
    public partial class CreatePackageWindow : Window
    {
        public CreatePackageWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Button event to create a package.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            int count = AliasList.SelectedCount() + TriggerList.SelectedCount() + DirectionList.SelectedCount();

            if (count == 0)
            {
                var msgbox = new MessageBoxDialog()
                {
                    Title = "Info",
                    Content = "No items were selected to create a package from.",
                };

                await msgbox.ShowAsync();

                return;
            }

            var win = new InputBoxDialog
            {
                Title = "Author",
                Caption = "Who should be listed as the author of this package?"
            };

            _ = await win.ShowAsync();

            var package = new Package
            {
                Id = Guid.NewGuid().ToString(),
                GameAddress = App.Settings.ProfileSettings.IpAddress,
                Author = win.Text ?? ""
            };

            foreach (object obj in AliasList.DataList.SelectedItems)
            {
                // Make sure the item is an Alias.  The last item in the list which is a new record is sometimes
                // an different object type.
                if (obj is Alias item)
                {
                    var alias = (Alias)item.Clone();
                    alias.Count = 0;
                    alias.Character = "";
                    package.AliasList.Add(alias);
                }
            }

            foreach (object obj in TriggerList.DataList.SelectedItems)
            {
                // Make sure the item is an Alias.  The last item in the list which is a new record is sometimes
                // an different object type.
                if (obj is Common.Triggers.Trigger item)
                {
                    var trigger = (Common.Triggers.Trigger)item.Clone();
                    trigger.Character = "";
                    trigger.LastMatched = DateTime.MinValue;
                    trigger.Count = 0;
                    package.TriggerList.Add(trigger);
                }

            }

            foreach (object obj in DirectionList.DataList.SelectedItems)
            {
                // Make sure the item is an Alias.  The last item in the list which is a new record is sometimes
                // an different object type.
                if (obj is Direction item)
                {
                    var direction = (Direction)item.Clone();
                    package.DirectionList.Add(direction);
                }
            }

            var dialog = new SaveFileDialog
            {
                InitialDirectory = App.Settings.AvalonSettings.SaveDirectory,
                Filter = "JSON files (*.json)|*.json",
                Title = "Save Package"
            };

            try
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllText(dialog.FileName, Newtonsoft.Json.JsonConvert.SerializeObject(package, Newtonsoft.Json.Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                var msgbox = new MessageBoxDialog()
                {
                    Title = "Error",
                    Content = ex.Message,
                };

                return;
            }

            this.Close();
        }

        /// <summary>
        /// Button event to cancel out of the package creation dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
