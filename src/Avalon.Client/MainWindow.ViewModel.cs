using System.Windows;
using MahApps.Metro.IconPacks;

namespace Avalon
{
    /// <summary>
    /// ViewModel for the main game window.
    /// </summary>
    public class MainWindowViewModel : DependencyObject
    {
        public MainWindowViewModel()
        {
            this.ScheduledTasksActive = 0;
            this.LuaScriptsActive = 0;
            this.PendingSqlStatements = 0;
        }

        /// <summary>
        /// The current number of tasks that are currently scheduled.
        /// </summary>
        public int ScheduledTasksActive
        {
            get => (int)GetValue(ScheduledTasksActiveProperty);
            set => SetValue(ScheduledTasksActiveProperty, value);
        }

        public static readonly DependencyProperty ScheduledTasksActiveProperty =
            DependencyProperty.Register(nameof(ScheduledTasksActive), typeof(int), typeof(MainWindowViewModel), new PropertyMetadata(0));


        /// <summary>
        /// The current number of Lua scripts that are active.
        /// </summary>
        public int LuaScriptsActive
        {
            get => (int)GetValue(LuaScriptsActiveProperty);
            set => SetValue(LuaScriptsActiveProperty, value);
        }

        public static readonly DependencyProperty LuaScriptsActiveProperty =
            DependencyProperty.Register(nameof(LuaScriptsActive), typeof(int), typeof(MainWindowViewModel), new PropertyMetadata(0));

        /// <summary>
        /// The current number of pending SQL statements.
        /// </summary>
        public int PendingSqlStatements
        {
            get => (int)GetValue(PendingSqlStatementsProperty);
            set => SetValue(PendingSqlStatementsProperty, value);
        }

        public static readonly DependencyProperty PendingSqlStatementsProperty =
            DependencyProperty.Register(nameof(PendingSqlStatements), typeof(int), typeof(MainWindowViewModel), new PropertyMetadata(0));

        /// <summary>
        /// Whether spell checking is currently enabled.
        /// </summary>
        public bool SpellCheckEnabled
        {
            get => (bool)GetValue(SpellCheckEnabledProperty);
            set => SetValue(SpellCheckEnabledProperty, value);
        }

        public static readonly DependencyProperty SpellCheckEnabledProperty =
            DependencyProperty.Register(nameof(SpellCheckEnabled), typeof(bool), typeof(MainWindow), new PropertyMetadata(true));


        /// <summary>
        /// User generated status bar text.
        /// </summary>
        public string StatusBarText
        {
            get => (string) GetValue(StatusBarTextProperty);
            set
            {
                SetValue(StatusBarTextProperty, value);
                this.StatusBarTextIconVisibility = string.IsNullOrWhiteSpace(value) ? Visibility.Hidden : Visibility.Visible;
                this.StatusBarSeparatorVisibility = string.IsNullOrWhiteSpace(value) ? Visibility.Hidden : Visibility.Visible;
            }
        }

        public static readonly DependencyProperty StatusBarTextProperty = DependencyProperty.Register(
            nameof(StatusBarText), typeof(string), typeof(MainWindowViewModel), new PropertyMetadata(""));


        /// <summary>
        /// The status bar separator for the main status bar TextBlock.
        /// </summary>
        public Visibility StatusBarSeparatorVisibility
        {
            get => (Visibility) GetValue(StatusBarSeparatorVisibilityProperty);
            set => SetValue(StatusBarSeparatorVisibilityProperty, value);
        }

        public static readonly DependencyProperty StatusBarSeparatorVisibilityProperty = DependencyProperty.Register(
            nameof(StatusBarSeparatorVisibility), typeof(Visibility), typeof(MainWindowViewModel), new PropertyMetadata(Visibility.Collapsed));

        /// <summary>
        /// Sets the visibility of the icon that is paired with the StatusBarText property.
        /// </summary>
        public Visibility StatusBarTextIconVisibility
        {
            get => (Visibility) GetValue(StatusBarTextIconVisibilityProperty);
            set => SetValue(StatusBarTextIconVisibilityProperty, value);
        }

        public static readonly DependencyProperty StatusBarTextIconVisibilityProperty = DependencyProperty.Register(
            nameof(StatusBarTextIconVisibility), typeof(Visibility), typeof(MainWindowViewModel), new PropertyMetadata(Visibility.Collapsed));


        /// <summary>
        /// Sets the icon associated with the main status bar text box.
        /// </summary>
        public PackIconMaterialKind StatusBarTextIconKind
        {
            get => (PackIconMaterialKind) GetValue(StatusBarTextIconKindProperty);
            set => SetValue(StatusBarTextIconKindProperty, value);
        }

        public static readonly DependencyProperty StatusBarTextIconKindProperty = DependencyProperty.Register(
            nameof(StatusBarTextIconKind), typeof(PackIconMaterialKind), typeof(MainWindowViewModel), new PropertyMetadata(PackIconMaterialKind.None));

    }
}