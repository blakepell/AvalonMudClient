/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System.Windows;
using System.Windows.Media;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for InfoBar.xaml
    /// </summary>
    public partial class InfoBar
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
            nameof(Health), typeof(int), typeof(InfoBar), new PropertyMetadata(default(int)));

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
            nameof(MaxHealth), typeof(int), typeof(InfoBar), new PropertyMetadata(default(int)));

        public int MaxHealth
        {
            get => (int)GetValue(MaxHealthProperty);
            set => SetValue(MaxHealthProperty, value);
        }

        public static readonly DependencyProperty HealthColorBrushProperty = DependencyProperty.Register(
            nameof(HealthColorBrush), typeof(SolidColorBrush), typeof(InfoBar), new PropertyMetadata(Brushes.White));

        public SolidColorBrush HealthColorBrush
        {
            get => (SolidColorBrush)GetValue(HealthColorBrushProperty);
            set => SetValue(HealthColorBrushProperty, value);
        }

        public static readonly DependencyProperty ManaProperty = DependencyProperty.Register(
            nameof(Mana), typeof(int), typeof(InfoBar), new PropertyMetadata(default(int)));

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
            nameof(MaxMana), typeof(int), typeof(InfoBar), new PropertyMetadata(default(int)));

        public int MaxMana
        {
            get => (int)GetValue(MaxManaProperty);
            set => this.SetValue(MaxManaProperty, value);
        }

        public static readonly DependencyProperty ManaColorBrushProperty = DependencyProperty.Register(
            nameof(ManaColorBrush), typeof(SolidColorBrush), typeof(InfoBar), new PropertyMetadata(Brushes.White));

        public SolidColorBrush ManaColorBrush
        {
            get => (SolidColorBrush)GetValue(ManaColorBrushProperty);
            set => SetValue(ManaColorBrushProperty, value);
        }


        public static readonly DependencyProperty MoveProperty = DependencyProperty.Register(
            nameof(Move), typeof(int), typeof(InfoBar), new PropertyMetadata(default(int)));

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
            nameof(MaxMove), typeof(int), typeof(InfoBar), new PropertyMetadata(default(int)));

        public int MaxMove
        {
            get => (int)GetValue(MaxMoveProperty);
            set => SetValue(MaxMoveProperty, value);
        }

        public static readonly DependencyProperty MoveColorBrushProperty = DependencyProperty.Register(
            nameof(MoveColorBrush), typeof(SolidColorBrush), typeof(InfoBar), new PropertyMetadata(Brushes.White));

        public SolidColorBrush MoveColorBrush
        {
            get => (SolidColorBrush)GetValue(MoveColorBrushProperty);
            set => SetValue(MoveColorBrushProperty, value);
        }

        public static readonly DependencyProperty StanceProperty = DependencyProperty.Register(
            nameof(Stance), typeof(string), typeof(InfoBar), new PropertyMetadata("Unknown"));

        public string Stance
        {
            get => (string)GetValue(StanceProperty);
            set => SetValue(StanceProperty, value);
        }

        public static readonly DependencyProperty TickTimerProperty = DependencyProperty.Register(
            nameof(TickTimer), typeof(int), typeof(InfoBar), new PropertyMetadata(default(int)));

        public int TickTimer
        {
            get => (int)GetValue(TickTimerProperty);
            set
            {
                this.SetValue(TickTimerProperty, value);
                this.SetValue(TickColorBrushProperty, value <= 5 ? Brushes.Red : Brushes.DarkGray);
            } 
        }

        public static readonly DependencyProperty TickColorBrushProperty = DependencyProperty.Register(
            nameof(TickColorBrush), typeof(SolidColorBrush), typeof(InfoBar), new PropertyMetadata(Brushes.DarkGray));

        public SolidColorBrush TickColorBrush
        {
            get => (SolidColorBrush)GetValue(TickColorBrushProperty);
            set => SetValue(TickColorBrushProperty, value);
        }

        public static readonly DependencyProperty ExitsProperty = DependencyProperty.Register(
            nameof(Exits), typeof(string), typeof(InfoBar), new PropertyMetadata("None"));

        public string Exits
        {
            get => (string)GetValue(ExitsProperty);
            set => SetValue(ExitsProperty, value);
        }

        public static readonly DependencyProperty RoomProperty = DependencyProperty.Register(
            nameof(Room), typeof(string), typeof(InfoBar), new PropertyMetadata("Limbo"));

        public string Room
        {
            get => (string)GetValue(RoomProperty);
            set => SetValue(RoomProperty, value);
        }

    }
}
