using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Avalon.Common;
using Avalon.Controls.Editors;
using Avalon.Models;
using Mosaic.UI.Wpf.Controls;

namespace Avalon.Dialogs;

/// <summary>
/// Edits one model in a label / editor grid generated from its <see cref="ModelEditorAttribute"/> properties.
/// </summary>
/// <remarks>
/// A recreation of ApexGateUI's ModelEditor.  The dialog edits a copy so Cancel leaves the original untouched;
/// OK copies the edited values back and is only enabled while <see cref="IEditableModel.IsValid"/> is true.
/// </remarks>
public sealed class ModelEditor : Window
{
    private readonly object _original;
    private readonly object _working;
    private readonly Game? _profile;
    private readonly Button _ok;
    private readonly List<PropertyInfo> _properties;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelEditor"/> class.
    /// </summary>
    /// <param name="model">The model to edit.</param>
    /// <param name="isNew">Whether the model is being created, which only changes the title.</param>
    /// <param name="profile">The profile supplying the available package assignments.</param>
    public ModelEditor(object model, bool isNew, Game? profile = null)
    {
        _original = model;
        _working = model is IEditableModel ? ModelCloner.Clone(model) : model;
        _profile = profile;
        _properties = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.GetCustomAttribute<ModelEditorAttribute>() is not null).ToList();

        Style = (Style)Application.Current.FindResource("DialogWindowStyle");
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var typeName = SingularName(model.GetType());
        Title = isNew ? $"New {typeName}" : $"Edit {typeName}";
        Width = 520;
        SizeToContent = SizeToContent.Height;
        MaxHeight = SystemParameters.WorkArea.Height - 40;
        ResizeMode = ResizeMode.CanResizeWithGrip;
        Owner = Application.Current.MainWindow is { IsLoaded: true } owner && owner != this ? owner : null;

        _ok = new Button { Content = "OK", IsDefault = true, Style = (Style)FindResource("DialogButtonStyle") };
        _ok.Click += (_, _) => Commit();
        var cancel = new Button { Content = "Cancel", IsCancel = true, Style = (Style)FindResource("DialogButtonStyle") };

        var footer = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(12) };
        footer.Children.Add(_ok);
        footer.Children.Add(cancel);

        var root = new DockPanel();
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);
        root.Children.Add(new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = BuildGrid(),
            Padding = new Thickness(12, 12, 12, 0)
        });
        Content = root;

        if (_working is INotifyPropertyChanged notify)
        {
            notify.PropertyChanged += (_, _) => UpdateOk();
        }

        UpdateOk();
        Loaded += (_, _) => MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
    }

    /// <summary>
    /// Shows the editor for <paramref name="model"/> and returns whether OK was pressed.
    /// </summary>
    public static bool Edit(object model, bool isNew = false, Game? profile = null) =>
        new ModelEditor(model, isNew, profile).ShowDialog() == true;

    private Grid BuildGrid()
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MinWidth = 120 });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        foreach (var property in _properties)
        {
            int row = grid.RowDefinitions.Count;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var description = property.GetCustomAttribute<DescriptionAttribute>()?.Description;
            var label = new TextBlock
            {
                Text = property.GetCustomAttribute<DisplayAttribute>()?.Name ?? property.Name,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 8),
                ToolTip = description
            };
            Grid.SetRow(label, row);
            grid.Children.Add(label);

            var editor = CreateEditor(property);
            editor.Margin = new Thickness(0, 0, 0, 8);
            editor.ToolTip ??= description;
            Grid.SetRow(editor, row);
            Grid.SetColumn(editor, 1);
            grid.Children.Add(editor);
        }

        return grid;
    }

    private FrameworkElement CreateEditor(PropertyInfo property)
    {
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        var binding = new Binding(property.Name) { Source = _working, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
        var attribute = property.GetCustomAttribute<ModelEditorAttribute>()!;

        if (_profile is not null && property.Name == nameof(Alias.Package) && type == typeof(string))
        {
            var picker = new PackagePicker { Profile = _profile, Height = 28 };
            picker.SetBinding(PackagePicker.PackageNameProperty, binding);
            return picker;
        }

        if (type == typeof(bool))
        {
            var check = new CheckBox { VerticalAlignment = VerticalAlignment.Center };
            check.SetBinding(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, binding);
            return check;
        }

        if (type.IsEnum)
        {
            var combo = new ComboBox { ItemsSource = Enum.GetValues(type), Height = 28 };
            combo.SetBinding(System.Windows.Controls.Primitives.Selector.SelectedItemProperty, binding);
            return combo;
        }

        if (type == typeof(SolidColorBrush))
        {
            var color = new HexColorTextBox { Height = 28, HorizontalContentAlignment = HorizontalAlignment.Stretch };
            binding.Converter = new SolidColorBrushColorConverter();
            color.SetBinding(HexColorTextBox.ColorProperty, binding);
            return color;
        }

        if (type == typeof(int) || type == typeof(double))
        {
            var number = new TextBox { Height = 28, VerticalContentAlignment = VerticalAlignment.Center };
            binding.UpdateSourceTrigger = UpdateSourceTrigger.LostFocus;
            binding.ValidatesOnExceptions = true;
            number.SetBinding(TextBox.TextProperty, binding);
            return number;
        }

        if (type == typeof(Guid) || type == typeof(DateTime))
        {
            var text = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
            text.SetBinding(TextBlock.TextProperty, new Binding(property.Name) { Source = _working, Mode = BindingMode.OneWay });
            return text;
        }

        bool isTriggerCommand = _working is Models.Trigger && property.Name == nameof(Models.Trigger.Command);
        bool multiLine = attribute.MultiLine || attribute.TextEditor is TextEditorKind.Script or TextEditorKind.Text;
        var box = new TextBox
        {
            MinHeight = 28,
            VerticalContentAlignment = multiLine ? VerticalAlignment.Top : VerticalAlignment.Center,
            AcceptsReturn = multiLine,
            TextWrapping = multiLine ? TextWrapping.Wrap : TextWrapping.NoWrap,
            Height = multiLine ? 70 : double.NaN,
            VerticalScrollBarVisibility = multiLine ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled,
            FontFamily = attribute.TextEditor is TextEditorKind.None ? FontFamily : new FontFamily("Consolas")
        };
        if (isTriggerCommand)
        {
            box.Height = 28;
        }
        box.SetBinding(TextBox.TextProperty, binding);

        if (attribute.TextEditor == TextEditorKind.None)
        {
            return box;
        }

        var more = new Button { Content = "…", Width = 28, Margin = new Thickness(4, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top, Height = 28, ToolTip = "Open in an editor" };
        more.Click += (_, _) =>
        {
            var current = property.GetValue(_working) as string ?? "";
            var title = property.GetCustomAttribute<DisplayAttribute>()?.Name ?? property.Name;
            void UpdateProperty(string value) => property.SetValue(_working, value);

            if (EditText(this, attribute.TextEditor, current, _working, title, UpdateProperty) is { } result)
            {
                UpdateProperty(result);
            }
        };

        var panel = new DockPanel();
        DockPanel.SetDock(more, Dock.Right);
        panel.Children.Add(more);
        panel.Children.Add(box);
        return panel;
    }

    /// <summary>
    /// Opens the editor that suits <paramref name="kind"/> and returns the edited text, or null when cancelled.
    /// </summary>
    /// <param name="owner">The window that owns the editor.</param>
    /// <param name="kind">The kind of editor to open.</param>
    /// <param name="text">The text to edit.</param>
    /// <param name="model">The model being edited; its ExecuteAs decides between a script and a command editor.</param>
    /// <param name="title">The editor title.</param>
    /// <param name="saveProperty">An optional callback to update the property when the script toolbar saves.</param>
    public static string? EditText(Window? owner, TextEditorKind kind, string text, object? model, string title, Action<string>? saveProperty = null)
    {
        if (kind == TextEditorKind.Command)
        {
            kind = model?.GetType().GetProperty("ExecuteAs")?.GetValue(model) is ExecuteType.JavaScript
                ? TextEditorKind.Script
                : TextEditorKind.Text;
        }

        switch (kind)
        {
            case TextEditorKind.Script:
                var script = new ScriptEditorWindow(text, saveProperty) { Title = $"{title} (JavaScript)", Owner = owner };
                return script.ShowDialog() == true ? script.Text : null;
            case TextEditorKind.Regex:
                var regex = new RegexBuilderWindow(text) { Owner = owner, IsSaveVisible = true };
                return regex.ShowDialog() == true ? regex.Pattern : null;
            default:
                var editor = new StringEditor(text) { Title = title, Owner = owner };
                return editor.ShowDialog() == true ? editor.Text : null;
        }
    }

    /// <summary>
    /// Returns the friendly singular name of a model type, such as "Highlight Trigger".
    /// </summary>
    public static string SingularName(Type type) => type.Name switch
    {
        nameof(Scripting.SourceCode) => "Script",
        nameof(PackageMetadata) => "Package",
        nameof(Game) => "Game",
        _ => System.Text.RegularExpressions.Regex.Replace(type.Name, "(?<=[a-z])([A-Z])", " $1")
    };

    /// <summary>
    /// Returns the friendly plural name of a model type from its <see cref="DisplayAttribute"/>.
    /// </summary>
    public static string PluralName(Type type) => type.GetCustomAttribute<DisplayAttribute>()?.Name ?? type.Name;

    private void UpdateOk() => _ok.IsEnabled = _working is not IEditableModel model || model.IsValid();

    private void Commit()
    {
        // Push a focused text box whose binding updates on lost focus.
        if (Keyboard.FocusedElement is TextBox focused)
        {
            focused.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }

        if (_working is IEditableModel model && !model.IsValid())
        {
            return;
        }

        if (!ReferenceEquals(_working, _original))
        {
            foreach (var property in _properties)
            {
                property.SetValue(_original, property.GetValue(_working));
            }
        }

        DialogResult = true;
    }
}
