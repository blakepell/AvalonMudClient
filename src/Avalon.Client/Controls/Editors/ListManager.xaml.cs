/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using System.Windows;

namespace Avalon.Controls
{
    /// <summary>
    /// This control simply allows for adding and deleting from a string list.  This control copies
    /// the contents of a list in and the caller is expected to retrieve it when it's done being used.
    /// The reason for this is that we want a cancel scenario to be supported without writing too much
    /// plumbing.  The LoadList method must be called to load a list otherwise the blank list will be
    /// the source in SourceList.
    /// </summary>
    public partial class ListManager
    {
        public List<string> SourceList { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ListManager()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Code to execute when the control has been loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListManagerControl_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxAdd.Focus();
        }

        /// <summary>
        /// Loads a list in via copying it.  The source list is not updated.
        /// </summary>
        /// <param name="list"></param>
        public void LoadList(List<string> list)
        {
            if (this.SourceList == null)
            {
                this.SourceList = new List<string>();
            }

            this.SourceList.Clear();
            this.SourceList.AddRange(list);

            foreach (string item in list)
            {
                ListItems.Items.Add(item);
            }

        }

        /// <summary>
        /// Adds a string to the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            SourceList.Add(TextBoxAdd.Text.Trim());
            ListItems.Items.Add(TextBoxAdd.Text.Trim());
            TextBoxAdd.Text = "";
        }

        /// <summary>
        /// Handles key interactions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListItems_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
            {
                if (this.ListItems.SelectedItem != null)
                {
                    int position = this.ListItems.SelectedIndex;

                    this.ListItems.Items.Remove(this.ListItems.SelectedItem);

                    if (this.ListItems.Items.Count >= position)
                    {
                        this.ListItems.SelectedIndex = position;
                    }

                }
            }
        }

        /// <summary>
        /// Handles the key down events for the entry text box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxAdd_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(TextBoxAdd.Text))
                {
                    return;
                }

                SourceList.Add(TextBoxAdd.Text.Trim());
                ListItems.Items.Add(TextBoxAdd.Text.Trim());
                TextBoxAdd.Text = "";

            }
        }

    }
}