using Argus.Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for VariableRepeater.xaml
    /// </summary>
    public partial class VariableRepeater : UserControl
    {

        public VariableRepeater()
        {
            InitializeComponent();
        }

        public void Bind()
        {
            repeater.ItemsSource = App.Settings.ProfileSettings.Variables.Where(x => x.IsVisible).OrderBy(x => x.DisplayOrder);
        }

    }
}
