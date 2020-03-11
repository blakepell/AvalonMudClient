using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows;
using System.Windows.Forms.Design;
using Avalon.Common.Attributes;

namespace Avalon.Windows
{
    /// <summary>
    /// A string property editor which can determine what type of string is present in order to show
    /// the appropriate dialog.
    /// </summary>
    public class StringPropertyEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var svc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            if (svc == null)
            {
                return "";
            }

            // Set the initial text for the editor.
            var win = new StringEditor
            {
                Text = (string)value
            };

            // Custom attributes we'll look for on strings.
            var lua = new LuaAttribute();

            if (context.PropertyDescriptor.Attributes.Contains(lua))
            {
                // Set the initial type for highlighting.
                win.EditorMode = StringEditor.EditorType.Lua;
            }
            else
            {
                win.EditorMode = StringEditor.EditorType.Text;
            }

            // Show what alias is being edited in the status bar of the string editor window.
            win.StatusText = $"Property: {context.PropertyDescriptor.Name}";

            // Startup position of the dialog should be in the center of the parent window.  The
            // owner has to be set for this to work.
            win.Owner = App.MainWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Show the Lua dialog.
            var result = win.ShowDialog();

            // If the result
            if (result != null && result.Value)
            {
                return win.Text;
            }

            // If cancel happened, return the original value.
            return value;
        }
    }

}
