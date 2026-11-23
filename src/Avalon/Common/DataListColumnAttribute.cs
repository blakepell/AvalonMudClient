using System.Windows;

namespace Avalon.Common;

/// <summary>
/// Describes how a model property is shown as a column in a <see cref="Controls.Editors.DataList"/>.
/// </summary>
/// <remarks>
/// Mirrors the attribute Continuum uses so the editors are generated from the model in the same way.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataListColumnAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the column header, defaulting to the property name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the column width, or a non-positive value to size automatically.
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Gets or sets whether the column is shown.  Hidden columns are still searchable.
    /// </summary>
    public Visibility Visibility { get; set; } = Visibility.Visible;

    /// <summary>
    /// Gets or sets whether a boolean column shows an editable check box.
    /// </summary>
    public bool CheckBox { get; set; }

    /// <summary>
    /// Gets or sets whether long text is trimmed to one line with an ellipsis.
    /// </summary>
    public bool Trim { get; set; }

    /// <summary>
    /// Gets or sets the display order, with lower values first.  Properties keep declaration order otherwise.
    /// </summary>
    public int Order { get; set; } = int.MaxValue;
}

/// <summary>
/// Marks a model property as editable in the <see cref="Dialogs.ModelEditor"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ModelEditorAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the kind of text editor the "…" button opens for a string property.
    /// </summary>
    public TextEditorKind TextEditor { get; set; } = TextEditorKind.None;

    /// <summary>
    /// Gets or sets whether a string property is edited in a taller multi-line box.
    /// </summary>
    public bool MultiLine { get; set; }
}

/// <summary>
/// The kind of editor opened for a string property.
/// </summary>
public enum TextEditorKind
{
    /// <summary>
    /// Only the inline text box.
    /// </summary>
    None,

    /// <summary>
    /// A command or script, chosen by the model's ExecuteAs property when it has one.
    /// </summary>
    Command,

    /// <summary>
    /// Always a JavaScript editor.
    /// </summary>
    Script,

    /// <summary>
    /// A plain multi-line text editor.
    /// </summary>
    Text,

    /// <summary>
    /// The regular expression builder.
    /// </summary>
    Regex
}

/// <summary>
/// Implemented by models that can be edited in the generic editors.
/// </summary>
public interface IEditableModel
{
    /// <summary>
    /// Returns whether the model can be saved.
    /// </summary>
    bool IsValid();

    /// <summary>
    /// Returns a copy with a new identity, used by paste.
    /// </summary>
    IEditableModel CloneNew();
}

/// <summary>
/// Copies models through their JSON form so event subscriptions are not shared with the copy.
/// </summary>
public static class ModelCloner
{
    /// <summary>
    /// Returns a deep copy of the persisted state of <paramref name="value"/>.
    /// </summary>
    public static T Clone<T>(T value) where T : class
    {
        var json = System.Text.Json.JsonSerializer.Serialize(value, value.GetType());
        return (T)System.Text.Json.JsonSerializer.Deserialize(json, value.GetType())!;
    }
}
