/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Settings;
using ModernWpf.Controls;
using Newtonsoft.Json;
using System.IO;

namespace Avalon
{
    public partial class ConnectionManagerNewProfileDialog
    {
        public ConnectionManagerNewProfileDialog()
        {
            InitializeComponent();
        }

        private void ButtonCreate_OnClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string fileName = Path.Combine($"{App.Settings.AvalonSettings.SaveDirectory}", $"{IpAddress.Text}-{Port.Text}.json");

            // TODO: Make the file name unique
            if (File.Exists(fileName))
            {
                return;
            }

            var profile = new ProfileSettings
            {
                FileName = $"{IpAddress.Text}-{Port.Text}.json",
                WindowTitle = this.GameDescription.Text,
                IpAddress = IpAddress.Text,
                Port = (int)Port.Value
            };

            // Write the profile settings file.
            File.WriteAllText(fileName, JsonConvert.SerializeObject(profile, Formatting.Indented));
            this.ProfileFileName = fileName;
        }

        /// <summary>
        /// The path to the profile that was created if one was created.
        /// </summary>
        public string ProfileFileName { get; set; }
    }
}
