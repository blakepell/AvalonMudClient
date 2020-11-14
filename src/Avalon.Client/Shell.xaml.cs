using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Argus.Extensions;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Avalon.Extensions;
using ModernWpf.Controls;

namespace Avalon
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : Window, IWindow
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="uc">The UserControl to host as the main content.</param>
        public Shell(UserControl uc)
        {
            InitializeComponent();
            this.Container.Content = uc;
            this.DataContext = this;            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="uc">The UserControl to host as the main content.</param>
        public Shell(UserControl uc, UIElement blurredElement)
        {
            InitializeComponent();

            this.Container.Content = uc;
            this.DataContext = this;

            // If blurring is requested of the parent window and the parent window effect
            // is null then set it, we'll get rid of it when this dialog closes.
            if (blurredElement != null && blurredElement.Effect == null)
            {
                this.BlurredElement = blurredElement;
                blurredElement.Effect = new BlurEffect();
            }
        }

        public string PrimaryButtonText
        {
            get { return (string)GetValue(PrimaryButtonTextProperty); }
            set { SetValue(PrimaryButtonTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PrimaryButtonText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrimaryButtonTextProperty =
            DependencyProperty.Register("PrimaryButtonText", typeof(string), typeof(Shell), new PropertyMetadata("Ok"));


        public Visibility PrimaryButtonVisibility
        {
            get { return (Visibility)GetValue(PrimaryButtonVisibilityProperty); }
            set { SetValue(PrimaryButtonVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PrimaryButtonVisbility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrimaryButtonVisibilityProperty =
            DependencyProperty.Register("PrimaryButtonVisibility", typeof(Visibility), typeof(Shell), new PropertyMetadata(Visibility.Visible));

        public string SecondaryButtonText
        {
            get { return (string)GetValue(SecondaryButtonTextProperty); }
            set { SetValue(SecondaryButtonTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecondaryButtonText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondaryButtonTextProperty =
            DependencyProperty.Register("SecondaryButtonText", typeof(string), typeof(Shell), new PropertyMetadata("Cancel"));

        public Visibility SecondaryButtonVisibility
        {
            get { return (Visibility)GetValue(SecondaryButtonVisibilityProperty); }
            set { SetValue(SecondaryButtonVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecondaryButtonVisbility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondaryButtonVisibilityProperty =
            DependencyProperty.Register("SecondaryButtonVisibility", typeof(Visibility), typeof(Shell), new PropertyMetadata(Visibility.Visible));


        public Visibility StatusBarVisibility
        {
            get { return (Visibility)GetValue(StatusBarVisibilityProperty); }
            set { SetValue(StatusBarVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StatusBarVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusBarVisibilityProperty =
            DependencyProperty.Register("StatusBarVisibility", typeof(Visibility), typeof(Shell), new PropertyMetadata(Visibility.Visible));

        public bool BlurParent { get; private set; } = false;

        public UIElement BlurredElement { get; private set; }

        public string StatusText { get; set; }

        public WindowType WindowType { get; set; } = WindowType.Default;

        /// <summary>
        /// The Symbol to display on the TabItem.
        /// </summary>
        public Symbol HeaderIcon
        {
            get => (Symbol)GetValue(HeaderIconProperty);
            set => SetValue(HeaderIconProperty, value);
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderIconProperty =
            DependencyProperty.Register(nameof(HeaderIcon), typeof(Symbol), typeof(Shell), new PropertyMetadata(Symbol.NewWindow));


        /// <summary>
        /// Whether the progress ring is currently spinning.
        /// </summary>
        public bool ProgressRingIsActive
        {
            get { return (bool)GetValue(ProgressRingIsActiveProperty); }
            set
            {
                SetValue(ProgressRingIsActiveProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for ProgressRingIsActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressRingIsActiveProperty =
            DependencyProperty.Register("ProgressRingIsActive", typeof(bool), typeof(Shell), new PropertyMetadata(false));


        /// <summary>
        /// Whether the progress ring is visible.
        /// </summary>
        public Visibility ProgressRingVisibility
        {
            get { return (Visibility)GetValue(ProgressRingVisibilityProperty); }
            set { SetValue(ProgressRingVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for visibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressRingVisibilityProperty =
            DependencyProperty.Register("ProgressRingVisibility", typeof(Visibility), typeof(Shell), new PropertyMetadata(Visibility.Collapsed));


        /// <summary>
        /// The status bar text that should exist on the far left.
        /// </summary>
        public string StatusBarLeftText
        {
            get { return (string)GetValue(StatusBarLeftTextProperty); }
            set { SetValue(StatusBarLeftTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StatusBarLeftText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusBarLeftTextProperty =
            DependencyProperty.Register("StatusBarLeftText", typeof(string), typeof(Shell), new PropertyMetadata(""));


        /// <summary>
        /// The status bar text that should exist on the far right.
        /// </summary>
        public string StatusBarRightText
        {
            get { return (string)GetValue(StatusBarRightTextProperty); }
            set { SetValue(StatusBarRightTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StatusBarRightText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusBarRightTextProperty =
            DependencyProperty.Register("StatusBarRightText", typeof(string), typeof(Shell), new PropertyMetadata(""));


        public string HeaderTitle
        {
            get { return (string)GetValue(HeaderTitleProperty); }
            set { SetValue(HeaderTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderTitleProperty =
            DependencyProperty.Register("HeaderTitle", typeof(string), typeof(Shell), new PropertyMetadata("Untitled"));

        private void ShellWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Now that the window is fully loaded, track it.  This will allow the #window command
            // to also manipulate this.
            App.Conveyor.WindowList.Add(this);            
        }

        /// <summary>
        /// Cleanup code when the window closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShellWindow_Closed(object sender, EventArgs e)
        {
            // Clear the hosted control reference.
            if (this.Container.Content != null)
            {
                this.Container.Content = null;
            }

            // If the blurring was originally requested, get rid of the effect on the parent window.
            if (this.BlurredElement != null && this.BlurredElement.Effect != null)
            {
                this.BlurredElement.Effect = null;
                this.BlurredElement = null;
            }

            // If this window exists in the tracked Window list, remove it.
            App.Conveyor.WindowList.Remove(this);
        }

        /// <summary>
        /// Allows for the moving of the window with the left mouse button if the area where the title bar
        /// would be is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// What to do when the secondary (usually Cancel or Close) button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSecondary_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsModal())
            {
                this.DialogResult = false;
            }

            IShellControl control = this.Container?.Content as IShellControl;

            if (control != null)
            {
                control.SecondaryButtonClick();
            }

            this.Close();
        }

        /// <summary>
        /// What to do when the primary (usually Ok or Save or the Action) button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPrimary_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsModal())
            {
                this.DialogResult = true;
            }

            IShellControl control = this.Container?.Content as IShellControl;

            if (control != null)
            {
                control.PrimaryButtonClick();
            }

            this.Close();
        }

        /// <summary>
        /// Sets the position in the center of the MainWindow and also makes it a proportion of it's size.
        /// </summary>
        /// <param name="percent">A percent as represented between 0 and 1.</param>
        public void SetSizeAndPosition(double percent)
        {

            this.Width = App.MainWindow.Width * percent;
            this.Height = App.MainWindow.Height * percent;
            this.Left = App.MainWindow.Left + (App.MainWindow.Width - (App.MainWindow.Width * percent)) / 2;
            this.Top = App.MainWindow.Top + (App.MainWindow.Height - (App.MainWindow.Height * percent)) / 2;
        }

        /// <summary>
        /// Activates the Window and brings it to the forefront and focused.
        /// </summary>
        void IWindow.Activate()
        {
            base.Activate();
        }

    }

    /// <summary>
    /// ViewModel which allows for properties to be displayed in the designer.
    /// </summary>
    public class DesignTimeViewModel
    {
        public DesignTimeViewModel()
        {
            this.PrimaryButtonText = "Ok";
            this.SecondaryButtonText = "Cancel";
            this.HeaderTitle = "New Window";
        }
        public string PrimaryButtonText { get; private set; }

        public string SecondaryButtonText { get; private set; }

        public string HeaderTitle { get; private set; }

    }

}
