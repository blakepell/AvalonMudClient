/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
        private readonly List<RichTextBox> _textBoxList = new List<RichTextBox>();


        /// <summary>
        /// Whether or not highlighting is occurring.
        /// </summary>
        private bool _highlighting = false;

        /// <summary>
        /// The default status bar color.
        /// </summary>
        private readonly SolidColorBrush _defaultStatusColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#007ACC"));

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

                    var match = Regex.Match(rtb.Text().TrimEnd(), TextBoxRegexPattern.Text.TrimEnd());

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

                    this.BorderBrush = _defaultStatusColor;
                    TextBlockStatus.Background = _defaultStatusColor;
                    StatusBarWindow.Background = _defaultStatusColor;
                    TextBlockStatus.Text = "";
                }
                
            }
            catch (Exception ex)
            {
                this.BorderBrush = Brushes.Red;
                TextBlockStatus.Background = Brushes.Red;
                StatusBarWindow.Background = Brushes.Red;
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
