using Avalon.Common.Models;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;

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

            var package = new Package();

            foreach (Alias item in AliasList.DataList.SelectedItems)
            {
                var alias = (Alias)item.Clone();
                alias.Count = 0;
                alias.Character = "";
                package.AliasList.Add(alias);
            }

            foreach (Common.Triggers.Trigger item in TriggerList.DataList.SelectedItems)
            {
                var trigger = (Common.Triggers.Trigger)item.Clone();
                trigger.Character = "";
                trigger.LastMatched = DateTime.MinValue;
                trigger.Count = 0;
                package.TriggerList.Add(trigger);
            }

            foreach (Direction item in DirectionList.DataList.SelectedItems)
            {
                var direction = (Direction)item.Clone();
                package.DirectionList.Add(direction);
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
