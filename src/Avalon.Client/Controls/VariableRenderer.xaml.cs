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
                    if (string.IsNullOrWhiteSpace(item.Label))
                    {
                        _sb.Append(item.Key.AsSpan());
                        _sb.Append(": {x");
                    }
                    else
                    {
                        _sb.Append(item.Label.AsSpan());
                        _sb.Append(": {x");
                    }
                }

                if (string.IsNullOrWhiteSpace(item.Value))
                {
                    _sb.Append(item.Value.AsSpan());
                    _sb.Append("N/A{W]{x ");
                }
                else
                {
                    _sb.Append(item.Value.AsSpan());
                    _sb.Append("{W]{x ");
                }
            }
            
            this.TerminalInfo.ClearText();
            this.TerminalInfo.AppendAnsi(_sb);
        }

    }
}
