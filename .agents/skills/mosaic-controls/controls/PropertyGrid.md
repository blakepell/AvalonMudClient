# PropertyGrid

**Base class:** `Control`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/PropertyGrid/PropertyGrid.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/PropertyGridExample.xaml`

## Description

A property inspector that reflects the public properties of any .NET object using `TypeDescriptor`. Properties are grouped by `[Category]`, sorted alphabetically within each group, and rendered with appropriate inline editors (text, checkbox, etc.). Custom attributes and a file-path editor are supported.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Object` | `object` | `null` | The target object whose properties are displayed. |
| `Properties` | `ObservableCollection<PropertyItem>` | auto-populated | The list of `PropertyItem` view-model wrappers (populated from `Object`). |
| `RevertInvalidValues` | `bool` | `true` | When `true`, reverts a property to its previous value if validation fails. |

## Supporting Attribute: `[PropertyGridAttribute]`

Apply to a property to control its display in the grid:

```csharp
[Category("Appearance")]
[Description("Foreground color hex string")]
[PropertyGridAttribute(IsReadOnly = true, Order = 1)]
public string ForegroundHex { get; set; }
```

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:PropertyGrid Object="{Binding SelectedItem}" />
```

## Code-Behind

```csharp
// Any POCO works — PropertyGrid reads [Category], [Description], [DefaultValue]
public class AppSettings
{
    [Category("General")]
    [Description("Application display name")]
    public string DisplayName { get; set; } = "My App";

    [Category("Appearance")]
    [Description("Enable dark mode")]
    public bool DarkMode { get; set; }
}
```

## Notes

- Properties are discovered via `TypeDescriptor.GetProperties(obj)` — this respects `[Browsable(false)]` to hide a property.
- `PropertyEditorSelector` selects the inline editor; file-path properties use `FilePropertyEditor`.
- `PropertyGridValidation` is applied to individual `PropertyItem` instances for WPF `IDataErrorInfo` support.
- Setting `Object = null` clears the grid.
