using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for InfoBar.xaml
    /// </summary>
    public partial class InfoBar : UserControl
    {
        public InfoBar()
        {
            InitializeComponent();

            // Important, this sets the data context of this control so that properties can be bound to dependency properties on this control.
            DataContext = this;
        }

        public void Update()
        {

        }

        public static readonly DependencyProperty HealthProperty = DependencyProperty.Register(
            "Health", typeof(int), typeof(InfoBar), new PropertyMetadata(default(int)));

        public int Health
        {
            get => (int)GetValue(HealthProperty);
            set
            {
                this.SetValue(HealthProperty, value);
                this.SetValue(HealthColorBrushProperty, Utilities.Utilities.ColorPercent(value, MaxHealth));
            }
        }

        public static readonly DependencyProperty MaxHealthProperty = DependencyProperty.Register(
            "MaxHealth", typeof(int), typeof(InfoBar), new PropertyMetadata(default(int)));

        public int MaxHealth
        {
            get => (int)GetValue(MaxHealthProperty);
            set => SetValue(MaxHealthProperty, value);
        }

        public static readonly DependencyProperty HealthColorBrushProperty = DependencyProperty.Register(
            "HealthColorBrush", typeof(SolidColorBrush), typeof(InfoBar), new PropertyMetadata(Brushes.White));

        public SolidColorBrush HealthColorBrush
        {
            get => (SolidColorBrush)GetValue(HealthColorBrushProperty);
            set => SetValue(HealthColorBrushProperty, value);
        }

        public static readonly DependencyProperty ManaProperty = DependencyProperty.Register(
            "Mana", typeof(int), typeof(InfoBar), new PropertyMetadata(default(int)));

        public int Mana
        {
            get => (int) GetValue(ManaProperty);
            set
            {
                this.SetValue(ManaProperty, value);
                this.SetValue(ManaColorBrushProperty, Utilities.Utilities.ColorPercent(value, MaxMana));
            }
        }


        public static readonly DependencyProperty MaxManaProperty = DependencyProperty.Register(
            "MaxMana", typeof(int), typeof(InfoBar), new PropertyMetadata(default(int)));

        public int MaxMana
        {
            get => (int)GetValue(MaxManaProperty);
            set => this.SetValue(MaxManaProperty, value);
        }

        public static readonly DependencyProperty ManaColorBrushProperty = DependencyProperty.Register(
            "ManaColorBrush", typeof(SolidColorBrush), typeof(InfoBar), new PropertyMetadata(Brushes.White));

        public SolidColorBrush ManaColorBrush
        {
            get => (SolidColorBrush)GetValue(ManaColorBrushProperty);
            set => SetValue(ManaColorBrushProperty, value);
        }


        public static readonly DependencyProperty MoveProperty = DependencyProperty.Register(
            "Move", typeof(int), typeof(InfoBar), new PropertyMetadata(default(int)));

        public int Move
        {
            get => (int)GetValue(MoveProperty);
            set
            { 
                this.SetValue(MoveProperty, value);
                this.SetValue(MoveColorBrushProperty, Utilities.Utilities.ColorPercent(value, MaxMove));
            }
        }

        public static readonly DependencyProperty MaxMoveProperty = DependencyProperty.Register(
            "MaxMove", typeof(int), typeof(InfoBar), new PropertyMetadata(default(int)));

        public int MaxMove
        {
            get => (int)GetValue(MaxMoveProperty);
            set => SetValue(MaxMoveProperty, value);
        }

        public static readonly DependencyProperty MoveColorBrushProperty = DependencyProperty.Register(
            "MoveColorBrush", typeof(SolidColorBrush), typeof(InfoBar), new PropertyMetadata(Brushes.White));

        public SolidColorBrush MoveColorBrush
        {
            get => (SolidColorBrush)GetValue(MoveColorBrushProperty);
            set => SetValue(MoveColorBrushProperty, value);
        }

        public static readonly DependencyProperty StanceProperty = DependencyProperty.Register(
            "Stance", typeof(string), typeof(InfoBar), new PropertyMetadata("Unknown"));

        public string Stance
        {
            get => (string)GetValue(StanceProperty);
            set => SetValue(StanceProperty, value);
        }

        public static readonly DependencyProperty WimpyProperty = DependencyProperty.Register(
            "Wimpy", typeof(int), typeof(InfoBar), new PropertyMetadata(default(int)));

        public int Wimpy
        {
            get => (int)GetValue(WimpyProperty);
            set => SetValue(WimpyProperty, value);
        }

        public static readonly DependencyProperty TickTimerProperty = DependencyProperty.Register(
            "TickTimer", typeof(int), typeof(InfoBar), new PropertyMetadata(default(int)));

        public int TickTimer
        {
            get => (int)GetValue(TickTimerProperty);
            set
            {
                this.SetValue(TickTimerProperty, value);

                if (value <= 5)
                {
                    this.SetValue(TickColorBrushProperty, Brushes.Red);
                }
                else
                {
                    this.SetValue(TickColorBrushProperty, Brushes.DarkGray);
                }
            } 
        }

        public static readonly DependencyProperty TickColorBrushProperty = DependencyProperty.Register(
            "TickColorBrush", typeof(SolidColorBrush), typeof(InfoBar), new PropertyMetadata(Brushes.DarkGray));

        public SolidColorBrush TickColorBrush
        {
            get => (SolidColorBrush)GetValue(TickColorBrushProperty);
            set => SetValue(TickColorBrushProperty, value);
        }

        public static readonly DependencyProperty ExitsProperty = DependencyProperty.Register(
            "Exits", typeof(string), typeof(InfoBar), new PropertyMetadata("None"));

        public string Exits
        {
            get => (string)GetValue(ExitsProperty);
            set => SetValue(ExitsProperty, value);
        }

        public static readonly DependencyProperty RoomProperty = DependencyProperty.Register(
            "Room", typeof(string), typeof(InfoBar), new PropertyMetadata("Limbo"));

        public string Room
        {
            get => (string)GetValue(RoomProperty);
            set => SetValue(RoomProperty, value);
        }

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
            "Time", typeof(string), typeof(InfoBar), new PropertyMetadata("12am"));

        public string Time
        {
            get => (string)GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
            "Target", typeof(string), typeof(InfoBar), new PropertyMetadata("Nobody"));

        public string Target
        {
            get => (string)GetValue(TargetProperty);
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = "Nobody";
                }

                this.SetValue(TargetProperty, value);

                if (value == "Nobody")
                {
                    this.SetValue(TargetColorBrushProperty, Brushes.DarkGray);
                }
                else
                {
                    this.SetValue(TargetColorBrushProperty, Brushes.Red);
                }

            }
        }

        public static readonly DependencyProperty TargetColorBrushProperty = DependencyProperty.Register(
            "TargetColorBrush", typeof(SolidColorBrush), typeof(InfoBar), new PropertyMetadata(Brushes.DarkGray));

        public SolidColorBrush TargetColorBrush
        {
            get => (SolidColorBrush)GetValue(TargetColorBrushProperty);
            set => SetValue(TickColorBrushProperty, value);
        }


    }
}
