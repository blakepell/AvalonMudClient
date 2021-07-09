/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Colors;
using Avalon.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Trigger = Avalon.Common.Triggers.Trigger;

namespace Avalon
{
    /// <summary>
    /// Trigger editor.  We are not binding here because we don't want the Trigger to be updated as
    /// the trigger is being edited which can cause RegEx errors in the client and we don't want
    /// it partially updated.  Save must be clicked for this Trigger to go into effect.
    /// </summary>
    public partial class TriggerEditorWindow
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
            TextPriority.Value = trigger.Priority;
            CheckBoxEnabled.IsChecked = trigger.Enabled;
            CheckBoxGag.IsChecked = trigger.Gag;
            CheckBoxVariableReplace.IsChecked = trigger.VariableReplacement;
            CheckBoxLock.IsChecked = trigger.Lock;
            CheckBoxHighlight.IsChecked = trigger.HighlightLine;
            CheckBoxSilent.IsChecked = trigger.IsSilent;
            CheckBoxDisableAfterTriggered.IsChecked = trigger.DisableAfterTriggered;
            CheckBoxStopProcessing.IsChecked = trigger.StopProcessing;
            CheckBoxLineTransformer.IsChecked = trigger.LineTransformer;
            CheckBoxTemp.IsChecked = trigger.Temp;

            var dict = new Dictionary<int, string>
            {
                { 0, "None" },
                { 2, "Terminal 1" },
                { 3, "Terminal 2" },
                { 5, "Terminal 3" }
            };

            ComboBoxRedirectTo.ItemsSource = dict;
            ComboBoxRedirectTo.SelectedValue = (int)trigger.MoveTo;

            // Not using the enum here pains me.
            var dictExecuteAs = new Dictionary<int, string>
            {
                { 0, "Command" },
                { 1, "Lua: MoonSharp" },
            };

            ComboBoxExecuteAs.ItemsSource = dictExecuteAs;
            ComboBoxExecuteAs.SelectedValue = (int)trigger.ExecuteAs;
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
                this.Trigger.Gag = (bool)CheckBoxGag.IsChecked;
                this.Trigger.VariableReplacement = (bool)CheckBoxVariableReplace.IsChecked;
                this.Trigger.Lock = (bool)CheckBoxLock.IsChecked;
                this.Trigger.HighlightLine = (bool)CheckBoxHighlight.IsChecked;
                this.Trigger.IsSilent = (bool)CheckBoxSilent.IsChecked;
                this.Trigger.DisableAfterTriggered = (bool)CheckBoxDisableAfterTriggered.IsChecked;
                this.Trigger.StopProcessing = (bool)CheckBoxStopProcessing.IsChecked;
                this.Trigger.LineTransformer = (bool)CheckBoxLineTransformer.IsChecked;
                this.Trigger.Temp = (bool)CheckBoxTemp.IsChecked;

                // Set it to the default trigger priority if it is NaN.
                if (double.IsNaN(TextPriority.Value))
                {
                    this.Trigger.Priority = 10000;
                }
                else
                {

                    this.Trigger.Priority = (int)TextPriority.Value;
                }

                if (ComboBoxRedirectTo.SelectedValue != null)
                {
                    this.Trigger.MoveTo = (TerminalTarget)ComboBoxRedirectTo.SelectedValue;
                }

                if (ComboBoxExecuteAs.SelectedValue != null)
                {
                    this.Trigger.ExecuteAs = (ExecuteType) ComboBoxExecuteAs.SelectedValue;
                }

                // Just in case this will make sure the Conveyor is setup on this trigger.
                Utilities.Utilities.SetupTriggers();

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

            if (string.IsNullOrWhiteSpace(TextCommand.Text) 
                && this.Trigger.ExecuteAs == ExecuteType.LuaMoonsharp
                && (this.Trigger.LineTransformer || CheckBoxLineTransformer.IsChecked.GetValueOrDefault(false)))
            {
                win.Text = "-- Example of a Lua line transformer\r\n --local argOne = getarg(2, ...)\r\n\r\nreturn \"\"";
            }

            if (this.Trigger.IsLua 
                || this.Trigger.ExecuteAs == ExecuteType.LuaMoonsharp)
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
                var win = new RegexTestWindow
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    CancelButtonText = "Close",
                    SaveButtonVisible = true
                };

                // Remove any ANSI codes from the selected text, then escape the pattern.
                var sb = new StringBuilder(TextPattern.Text);
                Colorizer.RemoveAllAnsiCodes(sb);
                win.Pattern = sb.ToString();
                win.EscapePattern();

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