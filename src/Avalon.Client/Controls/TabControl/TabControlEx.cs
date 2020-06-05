using System.Windows;
using System.Windows.Controls;

namespace Avalon.Controls
{

    /// <summary>
    /// An subclass of the TabControl with support for additional buttons and styling.
    /// </summary>
    public class TabControlEx : TabControl
    {

        /// <summary>
        /// Whether or not the NetworkButton is showing the game as being connected.
        /// </summary>
        public bool IsConnected
        {
            get => (bool)GetValue(IsConnectedProperty);
            set => SetValue(IsConnectedProperty, value);
        }

        public static readonly DependencyProperty IsConnectedProperty =
            DependencyProperty.Register(nameof(IsConnected), typeof(bool), typeof(TabControlEx), new PropertyMetadata(false));


        public static readonly RoutedEvent NetworkButtonClickEvent =
            EventManager.RegisterRoutedEvent("NetworkButtonClickEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TabControlEx));

        /// <summary>
        /// The event that gets bubbled up when the network button is clicked so it can be handled by the caller.
        /// </summary>
        public event RoutedEventHandler NetworkButtonClick
        {
            add => AddHandler(SettingsButtonClickEvent, value);
            remove => RemoveHandler(SettingsButtonClickEvent, value);
        }

        /// <summary>
        /// Click event which will get bubbled up to the parent if they wired this event us.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(TabControlEx.SettingsButtonClickEvent));
        }

        public static readonly RoutedEvent SettingsButtonClickEvent =
            EventManager.RegisterRoutedEvent("SettingsButtonClickEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TabControlEx));

        /// <summary>
        /// The event that gets bubbled up when the settings button is clicked so it can be handled by the caller.
        /// </summary>
        public event RoutedEventHandler SettingsButtonClick
        {
            add => AddHandler(SettingsButtonClickEvent, value);
            remove => RemoveHandler(SettingsButtonClickEvent, value);
        }

        /// <summary>
        /// Click event which will get bubbled up to the parent if they wired this event us.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NetworkButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(TabControlEx.NetworkButtonClickEvent));
        }

        /// <summary>
        /// Event that gets fired when the template is applied.  This is used here to wire up any event handlers
        /// to items that were declared in the XAML template.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (this.GetTemplateChild("NetworkButton") is Button btnNetwork)
            {
                btnNetwork.Click += this.NetworkButton_Click;
            }

            if (this.GetTemplateChild("SettingsButton") is Button btnSettings)
            {
                btnSettings.Click += this.SettingsButton_Click;
            }

            base.OnApplyTemplate();
        }

    }
}
