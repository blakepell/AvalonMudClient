/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Models;
using Avalon.Common.Utilities;
using System.Windows.Controls;

namespace Avalon.Controls
{
    /// <summary>
    /// The base class that inherits from our editor base and specifies the generic type for
    /// this data entry control.
    /// </summary>
    public class GenericVariableListBase : EditorControlBase<Variable>
    {
        public GenericVariableListBase(FullyObservableCollection<Variable> source) : base(source)
        {

        }
    }

    /// <summary>
    /// The base class that hides the generic from the partial/class XAML of the <see cref="UserControl"/> we'll
    /// surface on the <see cref="Shell"/>.
    /// </summary>
    public class VariableListBase : GenericVariableListBase
    {
        public VariableListBase(FullyObservableCollection<Variable> source) : base(source)
        {

        }
    }

    /// <summary>
    /// Interaction logic for the VariableList editor.
    /// </summary>
    public partial class VariableList
    {
        public VariableList(FullyObservableCollection<Variable> source) : base(source)
        {
            InitializeComponent();

            // Set the DataContext to null, we are using the DataContext for the item that has been
            // clicked on that will be edited.
            DataContext = null;
        }

        /// <summary>
        /// The actual filter that's used to filter down the DataGrid.
        /// </summary>
        /// <param name="item"></param>
        public override bool Filter(object item)
        {
            if (string.IsNullOrWhiteSpace(TextFilter.Text))
            {
                return true;
            }

            var variable = (Variable)item;

            return (variable?.Key?.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (variable?.Value?.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (variable?.Character?.Contains(TextFilter.Text, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        /// <summary>
        /// Close the window with an Ok or success.
        /// </summary>
        public override void PrimaryButtonClick()
        {
            App.MainWindow.VariableRepeater.Bind();
        }

        /// <summary>
        /// Close the window with a cancel or close.
        /// </summary>
        public override void SecondaryButtonClick()
        {
            App.MainWindow.VariableRepeater.Bind();
        }

        /// <summary>
        /// Event that fires when the selected variable changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var v = e.AddedItems[0] as Variable;

                if (v == null)
                {
                    App.Settings.ProfileSettings.Variables.Add(new Variable("", ""));
                    return;
                }

                // Set the context to the variable that the user has clicked on and will possibly be editing.  The
                // Lua editor needs to know where it's saving to, set that up also.
                this.DataContext = v;
                LuaEditor.SaveObject = v;
                LuaEditor.SaveProperty = "OnChangeEvent";
                LuaEditor.Editor.Text = !string.IsNullOrWhiteSpace(v.OnChangeEvent) ? v.OnChangeEvent : string.Empty;

                App.MainWindow.VariableRepeater.Bind();
            }
        }

        /// <summary>
        /// Limits the number box to only digits.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextPriority_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (e.Text.Any(c => !char.IsDigit(c)))
            {
                e.Handled = true;
            }
        }
    }
}