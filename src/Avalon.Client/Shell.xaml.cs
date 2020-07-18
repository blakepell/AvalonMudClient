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
using Avalon.Extensions;
using ModernWpf.Controls;

namespace Avalon
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : Window
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

        public string HeaderTitle { get; set; } = "Untitled";

        public string PrimaryButtonText { get; set; } = "Ok";

        public Visibility PrimaryButtonVisibility { get; set; }

        public string SecondaryButtonText { get; set; } = "Cancel";

        public Visibility SecondaryButtonVisibility { get; set; }

        public bool BlurParent { get; private set; } = false;

        public UIElement BlurredElement { get; private set; }

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

            this.Close();
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
