using System.Linq;

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
            VariableItemsRepeater.ItemsSource = App.Settings.ProfileSettings.Variables.Where(x => x.IsVisible).OrderBy(x => x.DisplayOrder);
        }

    }
}