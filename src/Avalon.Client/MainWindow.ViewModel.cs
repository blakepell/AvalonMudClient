using System.Windows;

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
        /// Whether spell checking is currently enabled.
        /// </summary>
        public bool SpellCheckEnabled
        {
            get => (bool)GetValue(SpellCheckEnabledProperty);
            set => SetValue(SpellCheckEnabledProperty, value);
        }

        public static readonly DependencyProperty SpellCheckEnabledProperty =
            DependencyProperty.Register(nameof(SpellCheckEnabled), typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

    }
}