/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Avalon.Extensions;
using ModernWpf.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;

namespace Avalon
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : IWindow
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
            get => (string)GetValue(PrimaryButtonTextProperty);
            set => SetValue(PrimaryButtonTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for PrimaryButtonText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrimaryButtonTextProperty =
            DependencyProperty.Register("PrimaryButtonText", typeof(string), typeof(Shell), new PropertyMetadata("Ok"));


        public Visibility PrimaryButtonVisibility
        {
            get => (Visibility)GetValue(PrimaryButtonVisibilityProperty);
            set => SetValue(PrimaryButtonVisibilityProperty, value);
        }

        // Using a DependencyProperty as the backing store for PrimaryButtonVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrimaryButtonVisibilityProperty =
            DependencyProperty.Register("PrimaryButtonVisibility", typeof(Visibility), typeof(Shell), new PropertyMetadata(Visibility.Visible));

        public string SecondaryButtonText
        {
            get => (string)GetValue(SecondaryButtonTextProperty);
            set => SetValue(SecondaryButtonTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for SecondaryButtonText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondaryButtonTextProperty =
            DependencyProperty.Register("SecondaryButtonText", typeof(string), typeof(Shell), new PropertyMetadata("Cancel"));

        public Visibility SecondaryButtonVisibility
        {
            get => (Visibility)GetValue(SecondaryButtonVisibilityProperty);
            set => SetValue(SecondaryButtonVisibilityProperty, value);
        }

        // Using a DependencyProperty as the backing store for SecondaryButtonVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondaryButtonVisibilityProperty =
            DependencyProperty.Register("SecondaryButtonVisibility", typeof(Visibility), typeof(Shell), new PropertyMetadata(Visibility.Visible));


        public Visibility StatusBarVisibility
        {
            get => (Visibility)GetValue(StatusBarVisibilityProperty);
            set => SetValue(StatusBarVisibilityProperty, value);
        }

        // Using a DependencyProperty as the backing store for StatusBarVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusBarVisibilityProperty =
            DependencyProperty.Register("StatusBarVisibility", typeof(Visibility), typeof(Shell), new PropertyMetadata(Visibility.Visible));

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
            get => (bool)GetValue(ProgressRingIsActiveProperty);
            set => SetValue(ProgressRingIsActiveProperty, value);
        }

        // Using a DependencyProperty as the backing store for ProgressRingIsActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressRingIsActiveProperty =
            DependencyProperty.Register("ProgressRingIsActive", typeof(bool), typeof(Shell), new PropertyMetadata(false));


        /// <summary>
        /// Whether the progress ring is visible.
        /// </summary>
        public Visibility ProgressRingVisibility
        {
            get => (Visibility)GetValue(ProgressRingVisibilityProperty);
            set => SetValue(ProgressRingVisibilityProperty, value);
        }

        // Using a DependencyProperty as the backing store for visibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressRingVisibilityProperty =
            DependencyProperty.Register("ProgressRingVisibility", typeof(Visibility), typeof(Shell), new PropertyMetadata(Visibility.Collapsed));


        /// <summary>
        /// The status bar text that should exist on the far left.
        /// </summary>
        public string StatusBarLeftText
        {
            get => (string)GetValue(StatusBarLeftTextProperty);
            set => SetValue(StatusBarLeftTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for StatusBarLeftText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusBarLeftTextProperty =
            DependencyProperty.Register("StatusBarLeftText", typeof(string), typeof(Shell), new PropertyMetadata(""));


        /// <summary>
        /// The status bar text that should exist on the far right.
        /// </summary>
        public string StatusBarRightText
        {
            get => (string)GetValue(StatusBarRightTextProperty);
            set => SetValue(StatusBarRightTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for StatusBarRightText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusBarRightTextProperty =
            DependencyProperty.Register("StatusBarRightText", typeof(string), typeof(Shell), new PropertyMetadata(""));


        public string HeaderTitle
        {
            get => (string)GetValue(HeaderTitleProperty);
            set => SetValue(HeaderTitleProperty, value);
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
            if (BlurredElement?.Effect != null)
            {
                this.BlurredElement.Effect = null;
                this.BlurredElement = null;
            }

            // If this window exists in the tracked Window list, remove it.
            App.Conveyor.WindowList.Remove(this);
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

            if (this.Container?.Content is IShellControl control)
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

            if (this.Container?.Content is IShellControl control)
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

        public Visibility SecondaryButtonVisibility { get; private set; }

        public Visibility PrimaryButtonVisibility { get; private set; }

        public Visibility StatusBarVisibility { get; private set; }

        public Visibility ProgressRingVisibility { get; private set; }

        public bool ProgressRingIsActive { get; private set; }

        public string StatusBarLeftText { get; private set; }

        public string StatusBarRightText { get; private set; }

    }

}
