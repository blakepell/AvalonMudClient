using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Avalon.Controls
{
    public class SearchBox : TextBox
    {
        private Border _searchBorder;

        public bool HasText
        {
            get { return Convert.ToBoolean(GetValue(HasTextProperty)); }

            set { SetValue(HasTextProperty, value); }
        }

        public static readonly DependencyProperty HasTextProperty =
            DependencyProperty.Register(nameof(HasText), typeof(bool), typeof(SearchBox), new PropertyMetadata(false));

        public bool HasFocus
        {
            get { return Convert.ToBoolean(GetValue(HasFocusProperty)); }

            set { SetValue(HasFocusProperty, value); }
        }

        public static readonly DependencyProperty HasFocusProperty =
            DependencyProperty.Register(nameof(HasFocus), typeof(bool), typeof(SearchBox), new PropertyMetadata(false));

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
                _searchBorder.MouseLeftButtonUp += SearchButton_MouseLeftButtonUphandler;
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
            }
        }
        private void SearchButton_MouseLeftButtonUphandler(object sender, MouseButtonEventArgs e)
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