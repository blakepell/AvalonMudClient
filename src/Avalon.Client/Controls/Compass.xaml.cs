using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Avalon.Controls
{
    /// <summary>
    /// A compass with a needle pointer.
    /// </summary>
    public partial class Compass : UserControl
    {
        /// <summary>
        /// Static constructor.  The OverrideMetadata calls can only happen in this constructor.  They will cause undefined
        /// behavior if run elsewhere (and from C we all know undefined behavior is meh).
        /// </summary>
        static Compass()
        {
            // Calculations that need to occur when default dependency properties change.
            HeightProperty.OverrideMetadata(typeof(Compass), new FrameworkPropertyMetadata(0.0, HeightChanged));
            WidthProperty.OverrideMetadata(typeof(Compass), new FrameworkPropertyMetadata(0.0, WidthChanged));
            FontSizeProperty.OverrideMetadata(typeof(Compass), new FrameworkPropertyMetadata(20.0, FontSizeChanged));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Compass()
        {
            InitializeComponent();
            this.DataContext = this;

            this.Angle = 0;
        }

        /// <summary>
        /// Specific directions.
        /// </summary>
        public enum Direction
        {
            North = 0,
            Northeast = 45,
            East = 90,
            Southeast = 135,
            South = 180,
            Southwest = 225,
            West = 270,
            Northwest = 315
        }

        /// <summary>
        /// Rotates the direction requested via <see cref="Direction"/>.
        /// </summary>
        /// <param name="angle"></param>
        public void SetAngle(Direction angle)
        {
            this.Angle = (int)angle;
        }

        /// <summary>
        /// Rotates to the direction requested via a <see cref="string"/>.
        /// </summary>
        /// <param name="direction"></param>
        public void SetAngle(string direction)
        {
            if (direction.Equals("north", StringComparison.Ordinal) || direction.Equals("n", StringComparison.Ordinal))
            {
                SetAngle(Direction.North);
                return;
            }

            if (direction.Equals("northeast", StringComparison.Ordinal) || direction.Equals("ne", StringComparison.Ordinal))
            {
                SetAngle(Direction.Northeast);
                return;
            }

            if (direction.Equals("east", StringComparison.Ordinal) || direction.Equals("e", StringComparison.Ordinal))
            {
                SetAngle(Direction.East);
                return;
            }

            if (direction.Equals("southeast", StringComparison.Ordinal) || direction.Equals("se", StringComparison.Ordinal))
            {
                SetAngle(Direction.Southeast);
                return;
            }

            if (direction.Equals("south", StringComparison.Ordinal) || direction.Equals("s", StringComparison.Ordinal))
            {
                SetAngle(Direction.South);
                return;
            }

            if (direction.Equals("southwest", StringComparison.Ordinal) || direction.Equals("sw", StringComparison.Ordinal))
            {
                SetAngle(Direction.Southwest);
                return;
            }

            if (direction.Equals("west", StringComparison.Ordinal) || direction.Equals("w", StringComparison.Ordinal))
            {
                SetAngle(Direction.West);
                return;
            }

            if (direction.Equals("northwest", StringComparison.Ordinal) || direction.Equals("nw", StringComparison.Ordinal))
            {
                SetAngle(Direction.Northwest);
                return;
            }
        }

        /// <summary>
        /// Sets the value of the angle.
        /// </summary>
        /// <param name="angle"></param>
        public void SetAngle(double angle)
        {
            this.Angle = (int)angle;
        }

        /// <summary>
        /// Sets the value of the angle.
        /// </summary>
        /// <param name="angle"></param>
        public void SetAngle(int angle)
        {
            this.Angle = angle;
        }

        /// <summary>
        /// Sets the label with the string value of the direction if it's exactly the angle of
        /// one of the nine directions (N, NE, E, SE, S, SW, W or NW).
        /// </summary>
        /// <param name="angle"></param>
        public void SetAngleDirection(double angle)
        {
            if (angle >= 0 && angle < 45)
            {
                LabelDirection.Content = "N";
            }
            else if (angle >= 45 && angle < 90)
            {
                LabelDirection.Content = "NE";
            }
            else if (angle >= 90 && angle < 135)
            {
                LabelDirection.Content = "E";
            }
            else if (angle >= 135 && angle < 180)
            {
                LabelDirection.Content = "SE";
            }
            else if (angle >= 180 && angle < 225)
            {
                LabelDirection.Content = "S";
            }
            else if (angle >= 225 && angle < 270)
            {
                LabelDirection.Content = "SW";
            }
            else if (angle >= 270 && angle < 315)
            {
                LabelDirection.Content = "W";
            }
            else if (angle >= 315 && angle < 360)
            {
                LabelDirection.Content = "NW";
            }
            else if (angle == 360)
            {
                LabelDirection.Content = "N";
            }
            else
            {
                LabelDirection.Content = "?";
            }
        }

        /// <summary>
        /// Normalizes an angle to a value between 0 and 360.
        /// </summary>
        /// <param name="angle"></param>
        public static double NormalizeAngle(double angle)
        {
            double times = Math.Floor(angle / 360);
            double reduction = times * 360;
            return angle - reduction;
        }

        /// <summary>
        /// Update all elements when the <see cref="Height"/> dependency property changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void HeightChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (Compass)sender;

            if (control?.Canvas1 == null)
            {
                return;
            }

            // Update the height of all the elements
            control.Canvas1.Height = (double)e.NewValue;
            control.Ellipse1.Height = (double)e.NewValue;
            control.Rotation.CenterY = control.Canvas1.Height / 2;

            // Update the needle position and height
            control.Line1.Y2 = control.Canvas1.Height / 2;
            control.Line1.X1 = control.Canvas1.Width / 2;
            control.Line1.X2 = control.Canvas1.Width / 2;
        }

        /// <summary>
        /// Update all elements when the <see cref="Width"/> dependency property changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void WidthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (Compass)sender;

            if (control?.Canvas1 == null)
            {
                return;
            }

            control.Canvas1.Width = (double)e.NewValue;
            control.Ellipse1.Width = (double)e.NewValue;
            control.Rotation.CenterX = control.Canvas1.Width / 2;

            // Update the needle position and height
            control.Line1.Y2 = control.Canvas1.Height / 2;
            control.Line1.X1 = control.Canvas1.Width / 2;
            control.Line1.X2 = control.Canvas1.Width / 2;
        }

        /// <summary>
        /// Update all elements when the <see cref="Width"/> dependency property changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void FontSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (Compass)sender;
            control.LabelDirection.FontSize = (double)e.NewValue;
        }

        /// <summary>
        /// The angle the needle is pointing.
        /// </summary>
        public double Angle
        {
            get => (double)GetValue(AngleProperty);
            set
            {
                double angle = NormalizeAngle(value);
                SetAngleDirection(angle);
                SetValue(AngleProperty, angle);
            }
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register(nameof(Angle), typeof(double), typeof(Compass), new PropertyMetadata(0.0, AngleChanged));

        /// <summary>
        /// Performs animation when the Angle property changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static async void AngleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (Compass)sender;

            // Ditch out, not enabled.
            if (!control.EnableAnimation)
            {
                return;
            }

            double oldValue = (double)e.OldValue;
            double newValue = (double)e.NewValue;

            // This makes the value go the shortest route 350-10 being 20 degrees vs. 350 to 10 being 140.  This is
            // pretty hacky, should probably use a Storyboard.
            if ((oldValue - newValue) > 180)
            {
                // Calculate the time that each segment should run so it's mostly smooth.
                double amountBefore = 360 - oldValue;
                double total = amountBefore + newValue;
                double percentBefore = amountBefore / total;
                double percentAfter = newValue / total;

                var durationOne = TimeSpan.FromSeconds(control.Duration * percentBefore);
                var durationTwo = TimeSpan.FromSeconds(control.Duration * percentAfter);

                control.Rotation.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(oldValue, 360.0, durationOne));
                await Task.Delay(durationOne);
                control.Rotation.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(0, newValue, durationTwo));
                return;
            }

            if (control.EnableAnimation)
            {
                var duration = TimeSpan.FromSeconds(control.Duration);
                control.Rotation.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation((double)e.NewValue, duration));
            }
        }

        /// <summary>
        /// The fill color inside the compass.
        /// </summary>
        public SolidColorBrush EllipseFillColor
        {
            get => (SolidColorBrush)GetValue(EllipseFillColorProperty);
            set => SetValue(EllipseFillColorProperty, value);
        }

        public static readonly DependencyProperty EllipseFillColorProperty =
            DependencyProperty.Register(nameof(EllipseFillColor), typeof(SolidColorBrush), typeof(Compass), new PropertyMetadata(Brushes.Black));

        /// <summary>
        /// The border color of the compass.
        /// </summary>
        public SolidColorBrush EllipseBorderColor
        {
            get => (SolidColorBrush)GetValue(EllipseBorderColorProperty);
            set => SetValue(EllipseBorderColorProperty, value);
        }

        public static readonly DependencyProperty EllipseBorderColorProperty =
            DependencyProperty.Register(nameof(EllipseBorderColor), typeof(SolidColorBrush), typeof(Compass), new PropertyMetadata(Brushes.Gray));

        /// <summary>
        /// The color of the directional needle.
        /// </summary>
        public SolidColorBrush NeedleColor
        {
            get => (SolidColorBrush)GetValue(NeedleColorProperty);
            set => SetValue(NeedleColorProperty, value);
        }

        public static readonly DependencyProperty NeedleColorProperty =
            DependencyProperty.Register(nameof(NeedleColor), typeof(SolidColorBrush), typeof(Compass), new PropertyMetadata(Brushes.Red));



        /// <summary>
        /// Whether or not the label with the direction is visible.
        /// </summary>
        public Visibility LabelVisible
        {
            get => (Visibility)GetValue(LabelVisibleProperty);
            set => SetValue(LabelVisibleProperty, value);
        }

        public static readonly DependencyProperty LabelVisibleProperty =
            DependencyProperty.Register(nameof(LabelVisible), typeof(Visibility), typeof(Compass), new PropertyMetadata(Visibility.Visible));


        /// <summary>
        /// The foreground color of the directional label.
        /// </summary>
        public SolidColorBrush LabelForegroundColor
        {
            get => (SolidColorBrush)GetValue(LabelForegroundColorProperty);
            set => SetValue(LabelForegroundColorProperty, value);
        }

        public static readonly DependencyProperty LabelForegroundColorProperty =
            DependencyProperty.Register(nameof(LabelForegroundColor), typeof(SolidColorBrush), typeof(SolidColorBrush), new PropertyMetadata(Brushes.White));

        /// <summary>
        /// The duration the animation should take.
        /// </summary>
        public double Duration { get; set; } = 0.25;

        /// <summary>
        /// Whether or not the animation of the angle line moving is enabled.
        /// </summary>
        public bool EnableAnimation { get; set; } = true;

    }
}
