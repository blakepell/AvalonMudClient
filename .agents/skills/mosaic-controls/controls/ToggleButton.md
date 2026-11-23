# ToggleButton

**Base class:** `System.Windows.Controls.Primitives.ToggleButton`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/ToggleButton/ToggleButton.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/ToggleButtonExample.xaml`

## Description

A themed drop-in replacement for the standard WPF `ToggleButton`. Adds explicit `CheckedBackground` and `UncheckedBackground` brush properties so the checked/unchecked visual state can be configured in XAML without re-templating.

## Key Properties (additions to standard ToggleButton)

| Property | Type | Default | Description |
|---|---|---|---|
| `CheckedBackground` | `Brush` | theme accent | Background brush applied when `IsChecked` is `true`. |
| `UncheckedBackground` | `Brush` | theme control background | Background brush applied when `IsChecked` is `false`. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<!-- Basic usage -->
<mosaic:ToggleButton Content="Bold" IsChecked="{Binding IsBold, Mode=TwoWay}" />

<!-- Custom checked / unchecked colors -->
<mosaic:ToggleButton
    Content="Enable"
    IsChecked="{Binding IsEnabled, Mode=TwoWay}"
    CheckedBackground="#2196F3"
    UncheckedBackground="#9E9E9E" />

<!-- Toolbar-style group (only one checked at a time) -->
<StackPanel Orientation="Horizontal">
    <mosaic:ToggleButton Content="Left" GroupName="Alignment" />
    <mosaic:ToggleButton Content="Center" GroupName="Alignment" IsChecked="True" />
    <mosaic:ToggleButton Content="Right" GroupName="Alignment" />
</StackPanel>
```

## Notes

- All standard `ToggleButton` properties apply: `IsChecked`, `IsThreeState`, `Checked`, `Unchecked`, `Indeterminate`, `Command`, `GroupName`.
- `IsChecked` is nullable (`bool?`); when `IsThreeState="True"` and `IsChecked` is `null`, the indeterminate visual state is used.
- `CheckedBackground` and `UncheckedBackground` use `DynamicResource` fallbacks so they automatically update on theme change.
