# BindablePasswordBox

**Base class:** `ContentControl`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/BindablePasswordBox/BindablePasswordBox.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/BindablePasswordBoxExample.xaml`

## Description

A `ContentControl` that wraps a standard WPF `PasswordBox` and exposes a bindable `Password` string property. The standard `PasswordBox` cannot be data-bound; this control bridges that gap by synchronizing internally while keeping the secure character buffer in the inner `PART_PasswordBox`.

## Template Parts

| Part | Type | Description |
|---|---|---|
| `PART_PasswordBox` | `PasswordBox` | The underlying WPF PasswordBox that handles secure input. |

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Password` | `string` | `""` | The current password value. Bindable two-way via `FrameworkPropertyMetadataOptions.BindsTwoWayByDefault`. |

Standard `ContentControl` properties (`BorderBrush`, `BorderThickness`, `Padding`) apply. Default `BorderThickness = 1`.

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:BindablePasswordBox
    Width="240"
    Password="{Binding UserPassword, Mode=TwoWay}" />
```

## ViewModel Example

```csharp
public string UserPassword
{
    get => _userPassword;
    set => SetProperty(ref _userPassword, value);
}
```

## Notes

- `Password` is flagged `UpdateSourceTrigger.PropertyChanged`, so bindings update on every keystroke.
- The inner `PasswordBox` is the only thing the user interacts with; it stores the actual secure string.
- Event handlers are detached on `Unloaded` to prevent memory leaks.
- Do **not** log or serialize `Password` values in plain text.
