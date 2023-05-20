/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

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