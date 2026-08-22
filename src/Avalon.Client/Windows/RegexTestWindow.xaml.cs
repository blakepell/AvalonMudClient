/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mosaic.UI.Wpf.Themes;

namespace Avalon
{
    /// <summary>
    /// A simple Lua highlighted text editor for use with editing Lua scripts.
    /// </summary>
    public partial class RegexTestWindow
    {

        /// <summary>
        /// The pattern for the window.
        /// </summary>
        public string Pattern
        {
            get => TextBoxRegexPattern.Text;
            set => TextBoxRegexPattern.Text = value;
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
        /// A list of the text boxes on the window that can be iterated through in a for loop.
        /// </summary>
        private readonly List<RichTextBox> _textBoxList = new();


        /// <summary>
        /// Whether or not highlighting is occurring.
        /// </summary>
        private bool _highlighting = false;

        /// <summary>
        /// The Mosaic theme brush used to flag a pattern that failed to compile.
        /// </summary>
        private static Brush ErrorBrush => (Brush)Application.Current.TryFindResource(MosaicTheme.ErrorBrush) ?? Brushes.Red;

        /// <summary>
        /// Constructor.
        /// </summary>
        public RegexTestWindow()
        {
            InitializeComponent();
            _textBoxList.Add(TextBoxTest1);
            _textBoxList.Add(TextBoxTest2);
            _textBoxList.Add(TextBoxTest3);
            _textBoxList.Add(TextBoxTest4);
            _textBoxList.Add(TextBoxTest5);
            _textBoxList.Add(TextBoxTest6);
            _textBoxList.Add(TextBoxTest7);
        }

        public RegexTestWindow(string initialPattern) : this()
        {
            InitializeComponent();
            _textBoxList.Add(TextBoxTest1);
            _textBoxList.Add(TextBoxTest2);
            _textBoxList.Add(TextBoxTest3);
            _textBoxList.Add(TextBoxTest4);
            _textBoxList.Add(TextBoxTest5);
            _textBoxList.Add(TextBoxTest6);
            _textBoxList.Add(TextBoxTest7);

            TextBoxRegexPattern.Text = initialPattern;
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
            if (this.IsModal())
            {
                this.DialogResult = false;
            }

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

        public void RunTests()
        {
            try
            {
                foreach (var rtb in _textBoxList)
                {
                    if (string.IsNullOrEmpty(rtb.Text().TrimEnd()))
                    {
                        continue;
                    }

                    // Put a time out the regex match in case to stop run away regex patterns.
                    var match = Regex.Match(rtb.Text().TrimEnd(), TextBoxRegexPattern.Text.TrimEnd(), RegexOptions.None, TimeSpan.FromSeconds(2));

                    if (match.Success)
                    {
                        _highlighting = true;
                        rtb.HighlightWord(match.Value, Brushes.Green);
                        _highlighting = false;
                    }
                    else
                    {
                        rtb.ClearAllProperties();
                        rtb.SelectClear();
                    }

                    // Clearing the local values hands these back to the Mosaic theme so a live
                    // theme switch continues to flow through them.
                    this.ClearValue(BorderBrushProperty);
                    TextBlockStatus.ClearValue(BackgroundProperty);
                    StatusBarWindow.ClearValue(BackgroundProperty);
                    TextBlockStatus.Text = "";
                }
                
            }
            catch (Exception ex)
            {
                this.BorderBrush = ErrorBrush;
                TextBlockStatus.Background = ErrorBrush;
                StatusBarWindow.Background = ErrorBrush;
                TextBlockStatus.Text = ex.Message;

                foreach (var rtb in _textBoxList)
                {
                    rtb.ClearAllProperties();
                }
            }
        }

        /// <summary>
        /// Attempts to escape a RegEx Pattern.
        /// </summary>
        public void EscapePattern()
        {
            this.Pattern = Regex.Escape(this.Pattern);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_highlighting)
            {
                return;
            }

            RunTests();
        }
    }
}
