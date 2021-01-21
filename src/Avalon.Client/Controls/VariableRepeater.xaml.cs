using System.Linq;
using System.Windows.Controls;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for VariableRepeater.xaml
    /// </summary>
    public partial class VariableRepeater
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