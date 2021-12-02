/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Avalon.Controls
{
    public class SearchBox : TextBox
    {
        private Border _searchBorder;

        public bool HasText
        {
            get => Convert.ToBoolean(GetValue(HasTextProperty));
            set => SetValue(HasTextProperty, value);
        }

        public static readonly DependencyProperty HasTextProperty =
            DependencyProperty.Register(nameof(HasText), typeof(bool), typeof(SearchBox), new PropertyMetadata(false));

        public bool HasFocus
        {
            get => Convert.ToBoolean(GetValue(HasFocusProperty));
            private set => SetValue(HasFocusProperty, value);
        }

        public static readonly DependencyProperty HasFocusProperty =
            DependencyProperty.Register(nameof(HasFocus), typeof(bool), typeof(SearchBox), new PropertyMetadata(false));

        public SolidColorBrush BorderSelectionColor 
        {
            get => (SolidColorBrush)GetValue(BorderSelectionColorProperty);
            set => SetValue(BorderSelectionColorProperty, value);
        }

        public static readonly DependencyProperty BorderSelectionColorProperty =
            DependencyProperty.Register(nameof(BorderSelectionColor), typeof(SolidColorBrush), typeof(TextBox), new PropertyMetadata((SolidColorBrush)(new BrushConverter().ConvertFrom("#007ACC"))));

        public new double Height
        {
            get => (double)GetValue(HeightProperty);
            set => SetValue(HeightProperty, value);
        }

        public new static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(SearchBox), new PropertyMetadata(28.0));

        static SearchBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBox), new FrameworkPropertyMetadata(typeof(SearchBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _searchBorder = (Border)GetTemplateChild("PART_SearchBorder");

            if (_searchBorder != null)
            {
                _searchBorder.MouseLeftButtonUp += SearchButton_MouseLeftButtonUpHandler;
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            this.HasText = this.Text.Length > 0;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            switch (e.Key)
            {
                case Key.Escape:
                    this.Text = string.Empty;
                    break;
                case Key.Enter:
                    SearchExecuted?.Invoke(this, new SearchEventArgs() { SearchText = this.Text });
                    this.Text = "";
                    break;
            }
        }

        /// <summary>
        /// A search event that can be subscribed to.
        /// </summary>
        public event EventHandler<SearchEventArgs> SearchExecuted;

        /// <summary>
        /// The event args for a search.
        /// </summary>
        public class SearchEventArgs
        {
            public string SearchText { get; set; }
        }

        private void SearchButton_MouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (this.HasText)
            {
                this.Text = string.Empty;
            }
        }

        public void SetKeyboardFocus()
        {
            this.Focus();
        }

    }
}