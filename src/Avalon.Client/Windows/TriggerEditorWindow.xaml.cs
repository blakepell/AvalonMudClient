﻿using Avalon.Colors;
using Avalon.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Trigger = Avalon.Common.Triggers.Trigger;

namespace Avalon
{
    /// <summary>
    /// Trigger editor.  We are not binding here because we don't want the Trigger to be updated as
    /// the trigger is being editted which can cause RegEx errors in the client and we don't want
    /// it partially updated.  Save must be clicked for this Trigger to go into effect.
    /// </summary>
    public partial class TriggerEditorWindow : Window
    {
        public Trigger Trigger { get; set; }

        /// <summary>
        /// The text for the status bar.
        /// </summary>
        public string StatusText
        {
            get => TextBlockStatus.Text;
            set => TextBlockStatus.Text = value;
        }

        /// <summary>
        /// Constructor: Must be provided an existing or a new trigger by the caller.
        /// </summary>
        /// <param name="trigger"></param>
        public TriggerEditorWindow(Trigger trigger)
        {
            InitializeComponent();

            // This is hacky but it was showing the text box and expanding the height even if the height was
            // 32 in XAML if the text had multiple lines, but setting it before the property is set here forced
            // it to stay at 32.
            TextCommand.Height = 32;

            this.Trigger = trigger;
            TextPattern.Text = trigger.Pattern;
            TextCommand.Text = trigger.Command;
            TextCharacter.Text = trigger.Character;
            TextGroup.Text = trigger.Group;
            TextIdentifier.Text = trigger.Identifier;
            CheckBoxEnabled.IsChecked = trigger.Enabled;
            CheckBoxLua.IsChecked = trigger.IsLua;
            CheckBoxGag.IsChecked = trigger.Gag;
            CheckBoxVariableReplace.IsChecked = trigger.VariableReplacement;
            CheckBoxLock.IsChecked = trigger.Lock;
            CheckBoxHighlight.IsChecked = trigger.HighlightLine;
            CheckBoxSilent.IsChecked = trigger.IsSilent;
            CheckBoxDisableAfterTriggered.IsChecked = trigger.DisableAfterTriggered;

            var dict = new Dictionary<int, string>
            {
                { 0, "None" },
                { 2, "Terminal 1" },
                { 3, "Terminal 2" },
                { 5, "Terminal 3" }
            };

            ComboBoxRedirectTo.ItemsSource = dict;
            ComboBoxRedirectTo.SelectedValue = (int)trigger.MoveTo;
        }

        /// <summary>
        /// Fires when the Window is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TriggerEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Closes the window and does not save changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            // Don't update the base trigger and get out.
            this.Trigger = null;
            this.Close();
        }

        /// <summary>
        /// Closes the window and saves changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Trigger.Pattern = TextPattern.Text;
                this.Trigger.Command = TextCommand.Text;
                this.Trigger.Character = TextCharacter.Text;
                this.Trigger.Group = TextGroup.Text;
                this.Trigger.Identifier = TextIdentifier.Text;
                this.Trigger.Enabled = (bool)CheckBoxEnabled.IsChecked;
                this.Trigger.IsLua = (bool)CheckBoxLua.IsChecked;
                this.Trigger.Gag = (bool)CheckBoxGag.IsChecked;
                this.Trigger.VariableReplacement = (bool)CheckBoxVariableReplace.IsChecked;
                this.Trigger.Lock = (bool)CheckBoxLock.IsChecked;
                this.Trigger.HighlightLine = (bool)CheckBoxHighlight.IsChecked;
                this.Trigger.IsSilent = (bool)CheckBoxSilent.IsChecked;
                this.Trigger.DisableAfterTriggered = (bool)CheckBoxDisableAfterTriggered.IsChecked;

                if (ComboBoxRedirectTo.SelectedValue != null)
                {
                    this.Trigger.MoveTo = (TerminalTarget)ComboBoxRedirectTo.SelectedValue;
                }

                // Just in case this will make sure the Conveyor is setup on this trigger.
                App.MainWindow.TriggersList.TriggerConveyorSetup();

                this.Close();
            }
            catch (Exception ex)
            {
                this.StatusText = $"Failed to save trigger: {ex.Message}";
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            // Set the initial text for the editor.
            var win = new StringEditor
            {
                Text = this.TextCommand.Text
            };

            if (this.Trigger.IsLua)
            {
                win.EditorMode = StringEditor.EditorType.Lua;
            }

            // Show what trigger is being edited in the status bar of the string editor window.
            win.StatusText = $"Trigger: {this.Trigger.Pattern}";

            // Startup position of the dialog should be in the center of the parent window.  The
            // owner has to be set for this to work.
            win.Owner = App.MainWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Show the Lua dialog.
            var result = win.ShowDialog();

            // If the result
            if (result != null && result.Value)
            {
                TextCommand.Text = win.Text;
            }
        }

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set the initial text for the editor.
                var win = new RegexTestWindow();

                // Startup position of the dialog should be in the center of the parent window.  The
                // owner has to be set for this to work.
                win.Owner = this;
                win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                win.CancelButtonText = "Close";
                win.SaveButtonVisible = true;

                // Remove any ANSI codes from the selected text.
                var sb = new StringBuilder(TextPattern.Text);
                Colorizer.RemoveAllAnsiCodes(sb);
                win.Pattern = sb.ToString();

                // Show the dialog.
                var result = win.ShowDialog() ?? false;

                if (result)
                {
                    this.TextPattern.Text = win.TextBoxRegexPattern.Text;
                }
            }
            catch (Exception ex)
            {
                this.StatusText = $"Error: {ex.Message}";
            }
        }
    }
}