using System.Windows;

namespace MoonSharp.Interpreter.Wpf.Controls
{
    public partial class CodeMetadataControl
    {
        public CodeMetadataControl()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(CodeMetadataControl), new PropertyMetadata(default(string)));

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty ReturnTypeProperty = DependencyProperty.Register(
            nameof(ReturnType), typeof(string), typeof(CodeMetadataControl), new PropertyMetadata(default(string)));

        /// <summary>
        /// The return type of the property or method.
        /// </summary>
        public string ReturnType
        {
            get => (string)GetValue(ReturnTypeProperty);
            set => SetValue(ReturnTypeProperty, value);
        }

        public static readonly DependencyProperty MemberTypeProperty = DependencyProperty.Register(
            nameof(MemberType), typeof(string), typeof(CodeMetadataControl), new PropertyMetadata(default(string)));

        /// <summary>
        /// Text description of what the member type is (e.g. Method vs. Function vs. Property)
        /// </summary>
        public string MemberType
        {
            get => (string)GetValue(MemberTypeProperty);
            set => SetValue(MemberTypeProperty, value);
        }

        public static readonly DependencyProperty AutoCompleteHintProperty = DependencyProperty.Register(
            nameof(AutoCompleteHint), typeof(string), typeof(CodeMetadataControl), new PropertyMetadata(default(string)));

        /// <summary>
        /// Any auto complete information that should be displayed.
        /// </summary>
        public string AutoCompleteHint
        {
            get => (string)GetValue(AutoCompleteHintProperty);
            set => SetValue(AutoCompleteHintProperty, value);
        }
    }
}
