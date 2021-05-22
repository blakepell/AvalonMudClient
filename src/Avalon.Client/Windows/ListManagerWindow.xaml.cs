/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System.Collections.Generic;
using System.Windows;

namespace Avalon
{
    /// <summary>
    /// A simple Lua highlighted text editor for use with editing Lua scripts.
    /// </summary>
    public partial class ListManagerWindow
    {
        public List<string> SourceList => ListMan.SourceList;

        public void LoadList(List<string> list)
        {
            ListMan.LoadList(list);
        }

        /// <summary>
        /// The text for the status bar.
        /// </summary>
        public string StatusText
        {
            get => TextBlockStatus.Text;
            set => TextBlockStatus.Text = value;
        }

        /// <summary>
        /// Ability to set the text on the cancel button.
        /// </summary>
        public string CancelButtonText
        {
            get => ButtonCancel.Content.ToString();
            set => ButtonCancel.Content = value;
        }

        private bool _saveButtonVisible = true;

        /// <summary>
        /// Whether the save button is visible or not.
        /// </summary>
        public bool SaveButtonVisible
        {
            get => _saveButtonVisible;
            set
            {
                _saveButtonVisible = value;

                if (value)
                {
                    ButtonSave.Visibility = Visibility.Visible;
                }
                else
                {
                    ButtonSave.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ListManagerWindow()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Gets or sets the text of the action button.
        /// </summary>
        public string ActionButtonText
        {
            get => ButtonSave.Content.ToString();
            set => ButtonSave.Content = value;
        }

        /// <summary>
        /// Code that is executed for the Cancel button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Code that is executed for the Save button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

    }
}
