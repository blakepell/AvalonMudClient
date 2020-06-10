using Argus.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for VariableRepeater.xaml
    /// </summary>
    public partial class VariableRepeater : UserControl
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public VariableRepeater()
        {
            InitializeComponent();
            //repeater.ItemsSource = App.Settings.ProfileSettings.Variables;
        }

        public void Bind()
        {
            repeater.ItemsSource = App.Settings.ProfileSettings.Variables.Where(x => x.IsVisible).OrderBy(x => x.DisplayOrder);
        }

    }
}
