/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Extensions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Avalon.Controls.AutoCompleteTextBox.Editors
{
    [TemplatePart(Name = PartEditor, Type = typeof(EditorTextBox))]
    [TemplatePart(Name = PartPopup, Type = typeof(Popup))]
    [TemplatePart(Name = PartSelector, Type = typeof(Selector))]
    public class AutoCompleteTextBox : Control
    {
        #region "Fields"

        public const string PartEditor = "PART_Editor";
        public const string PartPopup = "PART_Popup";
        public const string PartSelector = "PART_Selector";

        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register(nameof(Delay), typeof(int), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(200));
        public static readonly DependencyProperty DisplayMemberProperty = DependencyProperty.Register(nameof(DisplayMember), typeof(string), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty IconPlacementProperty = DependencyProperty.Register(nameof(IconPlacement), typeof(IconPlacement), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(IconPlacement.Left));
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(object), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register(nameof(IconVisibility), typeof(Visibility), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register(nameof(ItemTemplateSelector), typeof(DataTemplateSelector), typeof(AutoCompleteTextBox));
        public static readonly DependencyProperty LoadingContentProperty = DependencyProperty.Register(nameof(LoadingContent), typeof(object), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ProviderProperty = DependencyProperty.Register(nameof(Provider), typeof(ISuggestionProvider), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null, OnSelectedItemChanged));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register(nameof(MaxLength), typeof(int), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(0));
        public static readonly DependencyProperty CharacterCasingProperty = DependencyProperty.Register(nameof(CharacterCasing), typeof(CharacterCasing), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(CharacterCasing.Normal));
        public static readonly DependencyProperty MaxPopUpHeightProperty = DependencyProperty.Register(nameof(MaxPopupHeight), typeof(int), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(600));
        public static readonly DependencyProperty SuggestionBackgroundProperty = DependencyProperty.Register(nameof(SuggestionBackground), typeof(Brush), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(Brushes.White));
        public static readonly DependencyProperty DirectionalKeysOpenProperty = DependencyProperty.Register(nameof(DirectionalKeysOpen), typeof(bool), typeof(AutoCompleteTextBox), new PropertyMetadata(false));
        public static readonly DependencyProperty SpellCheckEnabledProperty = DependencyProperty.Register(nameof(SpellCheckEnabled), typeof(bool), typeof(AutoCompleteTextBox), new PropertyMetadata(false));
        public static readonly DependencyProperty AcceptsTabProperty = DependencyProperty.Register(nameof(AcceptsTab), typeof(bool), typeof(AutoCompleteTextBox), new PropertyMetadata(false));

        private bool _isUpdatingText;
        private bool _selectionCancelled;
        private SuggestionsAdapter _suggestionsAdapter;

        #endregion

        #region "Constructors"

        static AutoCompleteTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(typeof(AutoCompleteTextBox)));
        }

        #endregion

        #region "Properties"

        public int MaxPopupHeight
        {
            get => (int)GetValue(MaxPopUpHeightProperty);
            set => SetValue(MaxPopUpHeightProperty, value);
        }

        public BindingEvaluator BindingEvaluator { get; set; }

        public CharacterCasing CharacterCasing
        {
            get => (CharacterCasing)GetValue(CharacterCasingProperty);
            set => SetValue(CharacterCasingProperty, value);
        }

        /// <summary>
        /// Whether or not the directional keys will open the auto complete list.
        /// </summary>
        public bool DirectionalKeysOpen
        {
            get => (bool)GetValue(DirectionalKeysOpenProperty);
            set => SetValue(DirectionalKeysOpenProperty, value);
        }

        /// <summary>
        /// Whether spell checking is enabled on the input box.
        /// </summary>
        public bool SpellCheckEnabled
        {
            get => (bool)GetValue(SpellCheckEnabledProperty);
            set => SetValue(SpellCheckEnabledProperty, value);
        }

        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        public int Delay
        {
            get => (int)GetValue(DelayProperty);
            set => SetValue(DelayProperty, value);
        }

        public string DisplayMember
        {
            get => (string)GetValue(DisplayMemberProperty);
            set => SetValue(DisplayMemberProperty, value);
        }

        public EditorTextBox Editor { get; set; }

        public DispatcherTimer FetchTimer { get; set; }

        public string Filter { get; set; }

        public object Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public IconPlacement IconPlacement
        {
            get => (IconPlacement)GetValue(IconPlacementProperty);
            set => SetValue(IconPlacementProperty, value);
        }

        public Visibility IconVisibility
        {
            get => (Visibility)GetValue(IconVisibilityProperty);
            set => SetValue(IconVisibilityProperty, value);
        }

        public bool IsDropDownOpen
        {
            get => (bool)GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public bool AcceptsTab
        {
            get => (bool)GetValue(AcceptsTabProperty);
            set => SetValue(AcceptsTabProperty, value);
        }

        public Selector ItemsSelector { get; set; }

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public DataTemplateSelector ItemTemplateSelector
        {
            get => (DataTemplateSelector)(GetValue(ItemTemplateSelectorProperty));
            set => SetValue(ItemTemplateSelectorProperty, value);
        }

        public object LoadingContent
        {
            get => GetValue(LoadingContentProperty);
            set => SetValue(LoadingContentProperty, value);
        }

        public Popup Popup { get; set; }

        public ISuggestionProvider Provider
        {
            get => (ISuggestionProvider)GetValue(ProviderProperty);
            set => SetValue(ProviderProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public SelectionAdapter SelectionAdapter { get; set; }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public Brush SuggestionBackground
        {
            get => (Brush)GetValue(SuggestionBackgroundProperty);
            set => SetValue(SuggestionBackgroundProperty, value);
        }

        #endregion

        #region "Methods"

        /// <summary>
        /// Pulses the input text box a color.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="durationMilliseconds"></param>
        public async void Pulse(Color color, int durationMilliseconds = 1000)
        {
            await Editor.Pulse(color, durationMilliseconds);
        }

        public static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoCompleteTextBox act && act.Editor != null & !act._isUpdatingText)
            {
                act._isUpdatingText = true;
                act.Editor.Text = act.BindingEvaluator.Evaluate(e.NewValue);
                act._isUpdatingText = false;
            }
        }

        private void ScrollToSelectedItem()
        {
            if (ItemsSelector is ListBox listBox && listBox.SelectedItem != null)
            {
                listBox.ScrollIntoView(listBox.SelectedItem);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Editor = Template.FindName(PartEditor, this) as EditorTextBox;
            Popup = Template.FindName(PartPopup, this) as Popup;
            ItemsSelector = Template.FindName(PartSelector, this) as Selector;
            BindingEvaluator = new BindingEvaluator(new Binding(DisplayMember));

            if (Editor != null)
            {
                Editor.TextChanged += OnEditorTextChanged;
                Editor.PreviewKeyDown += OnEditorKeyDown;
                Editor.LostFocus += OnEditorLostFocus;

                if (SelectedItem != null)
                {
                    _isUpdatingText = true;
                    Editor.Text = BindingEvaluator.Evaluate(SelectedItem);
                    _isUpdatingText = false;
                }
            }

            GotFocus += AutoCompleteTextBox_GotFocus;

            if (Popup != null)
            {
                Popup.StaysOpen = false;
                Popup.Opened += OnPopupOpened;
                Popup.Closed += OnPopupClosed;
            }

            if (ItemsSelector != null)
            {
                SelectionAdapter = new SelectionAdapter(ItemsSelector);
                SelectionAdapter.Commit += OnSelectionAdapterCommit;
                SelectionAdapter.Cancel += OnSelectionAdapterCancel;
                SelectionAdapter.SelectionChanged += OnSelectionAdapterSelectionChanged;
                ItemsSelector.PreviewMouseDown += ItemsSelector_PreviewMouseDown;
            }
        }
        private void ItemsSelector_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.OriginalSource as FrameworkElement)?.DataContext == null)
            {
                return;
            }

            if (!ItemsSelector.Items.Contains(((FrameworkElement) e.OriginalSource)?.DataContext))
            {
                return;
            }

            ItemsSelector.SelectedItem = ((FrameworkElement)e.OriginalSource)?.DataContext;
            OnSelectionAdapterCommit();
        }

        private void AutoCompleteTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Editor?.Focus();
        }

        private string GetDisplayText(object dataItem)
        {
            if (BindingEvaluator == null)
            {
                BindingEvaluator = new BindingEvaluator(new Binding(DisplayMember));
            }

            if (dataItem == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(DisplayMember))
            {
                return dataItem.ToString();
            }

            return BindingEvaluator.Evaluate(dataItem);
        }

        private void OnEditorKeyDown(object sender, KeyEventArgs e)
        {
            if (SelectionAdapter == null)
            {
                return;
            }

            if (IsDropDownOpen)
            {
                SelectionAdapter.HandleKeyDown(e);
            }
            else
            {
                // If the DirectionalKeysOpen property is set then the up and down will open the list box and start
                // cycling through it, otherwise, it will open only when the pattern is found THEN you can use the
                // up and own.
                if (this.DirectionalKeysOpen)
                {
                    IsDropDownOpen = e.Key == Key.Down || e.Key == Key.Up;
                }
            }
        }

        private void OnEditorLostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsKeyboardFocusWithin)
            {
                IsDropDownOpen = false;
            }
        }

        private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingText)
            {
                return;
            }

            if (FetchTimer == null)
            {
                FetchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Delay) };
                FetchTimer.Tick += OnFetchTimerTick;
            }

            FetchTimer.IsEnabled = false;
            FetchTimer.Stop();
            SetSelectedItem(null);

            if (Editor.Text.Length > 0)
            {
                FetchTimer.IsEnabled = true;
                FetchTimer.Start();
            }
            else
            {
                IsDropDownOpen = false;
            }
        }

        private void OnFetchTimerTick(object sender, EventArgs e)
        {
            FetchTimer.IsEnabled = false;
            FetchTimer.Stop();

            if (Provider == null || ItemsSelector == null)
            {
                return;
            }

            Filter = Editor.Text;

            if (_suggestionsAdapter == null)
            {
                _suggestionsAdapter = new SuggestionsAdapter(this);
            }

            _suggestionsAdapter.GetSuggestions(Filter);
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            if (!_selectionCancelled)
            {
                OnSelectionAdapterCommit();
            }
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            _selectionCancelled = false;
            ItemsSelector.SelectedItem = SelectedItem;
        }

        private void OnSelectionAdapterCancel()
        {
            _isUpdatingText = true;
            Editor.Text = SelectedItem == null ? Filter : GetDisplayText(SelectedItem);
            Editor.SelectionStart = Editor.Text.Length;
            Editor.SelectionLength = 0;
            _isUpdatingText = false;
            IsDropDownOpen = false;
            _selectionCancelled = true;
        }

        private void OnSelectionAdapterCommit()
        {
            if (ItemsSelector.SelectedItem != null)
            {
                SelectedItem = ItemsSelector.SelectedItem;
                _isUpdatingText = true;
                Editor.Text = GetDisplayText(ItemsSelector.SelectedItem);
                SetSelectedItem(ItemsSelector.SelectedItem);
                _isUpdatingText = false;
                IsDropDownOpen = false;
            }
        }

        private void OnSelectionAdapterSelectionChanged()
        {
            _isUpdatingText = true;
            Editor.Text = ItemsSelector.SelectedItem == null ? Filter : GetDisplayText(ItemsSelector.SelectedItem);
            Editor.SelectionStart = Editor.Text.Length;
            Editor.SelectionLength = 0;
            ScrollToSelectedItem();
            _isUpdatingText = false;
        }

        private void SetSelectedItem(object item)
        {
            _isUpdatingText = true;
            SelectedItem = item;
            _isUpdatingText = false;
        }
        #endregion

        private class SuggestionsAdapter
        {
            private readonly AutoCompleteTextBox _actb;
            private string _filter;

            public SuggestionsAdapter(AutoCompleteTextBox actb)
            {
                _actb = actb;
            }

            public void GetSuggestions(string searchText)
            {
                _filter = searchText;
                _actb.IsLoading = true;
                _actb.ItemsSelector.ItemsSource = null;

                ParameterizedThreadStart thInfo = GetSuggestionsAsync;
                var th = new Thread(thInfo);

                th.Start(new object[] {
                searchText,
                _actb.Provider
            });
            }

            private void DisplaySuggestions(IEnumerable suggestions, string filter)
            {
                if (_filter != filter)
                {
                    return;
                }

                _actb.IsLoading = false;
                _actb.ItemsSelector.ItemsSource = suggestions;
                _actb.IsDropDownOpen = _actb.ItemsSelector.HasItems;
            }

            private void GetSuggestionsAsync(object param)
            {
                if (param is object[] args)
                {
                    string searchText = Convert.ToString(args[0]);
                    if (args[1] is ISuggestionProvider provider)
                    {
                        var list = provider.GetSuggestions(searchText);
                        _actb.Dispatcher.BeginInvoke(new Action<IEnumerable, string>(DisplaySuggestions), DispatcherPriority.Background, list, searchText);
                    }
                }
            }

        }
    }
}