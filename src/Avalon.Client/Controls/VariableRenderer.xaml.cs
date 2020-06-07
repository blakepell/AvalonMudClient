using SQLitePCL;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for VariableDisplayer.xaml
    /// </summary>
    public partial class VariableRenderer : UserControl
    {
        private StringBuilder _sb;

        public VariableRenderer()
        {
            _sb = new StringBuilder();
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
        
        public void Refresh()
        {
            _sb.Clear();

            foreach (var item in App.Settings.ProfileSettings.Variables.Where(x => x.IsVisible).OrderBy(x => x.DisplayOrder))
            {
                _sb.Append("{W[");

                if (item.DisplayLabel)
                {
                    // 1,-15
                    if (string.IsNullOrWhiteSpace(item.Label))
                    {
                        _sb.AppendFormat("{0,-8}:{{x ", item.Key);
                    }
                    else
                    {
                        _sb.AppendFormat("{0,-8}:{{x ", item.Label);
                    }
                }

                if (string.IsNullOrWhiteSpace(item.Value))
                {
                    _sb.AppendFormat("{{W{0,-8}{{W]{{x", "N/A");
                }
                else
                {
                    _sb.AppendFormat("{{W{0,-8}{{W]{{x", item.Value);
                }
            }
            
            this.TerminalInfo.ClearText();
            this.TerminalInfo.AppendAnsi(_sb);
        }

    }
}
