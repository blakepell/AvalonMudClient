# Shield

**Base class:** `Control`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/Shield/Shield.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/ShieldExample.xaml`

## Description

A GitHub-style shield/badge control consisting of two sections: a grey `Label` on the left and a colored `Value` section on the right. The `Value` section background and foreground are fully configurable. Commonly used to display status badges such as version, build status, or license.

The `Value` property is the `[ContentProperty]`, so it supports any WPF content (text or a control like `VersionTextBlock`).

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Label` | `string` | `null` | Text displayed in the left (grey) section. |
| `Value` | `object` | `null` | Content displayed in the right (colored) section. Use as element content. |
| `ValueBackgroundBrush` | `Brush` | `CornflowerBlue` | Background color of the right section. |
| `ValueForegroundBrush` | `Brush` | `White` | Foreground color of the right section text. |
| `CornerRadius` | `CornerRadius` | `2` | Corner radius for the badge shape. |

## XAML Examples

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<!-- Simple text badge -->
<mosaic:Shield
    Label="Build Status"
    Value="Passing"
    ValueBackgroundBrush="Green"
    ValueForegroundBrush="White" />

<!-- Version badge using VersionTextBlock -->
<mosaic:Shield Label="Version">
    <mosaic:VersionTextBlock />
</mosaic:Shield>

<!-- Bound value -->
<mosaic:Shield
    Label="Mosaic.UI.Wpf"
    ValueBackgroundBrush="CornflowerBlue"
    ValueForegroundBrush="White"
    Value="{Binding AppVersion}" />
```

## Notes

- The `CornerRadius` uses `ShieldCornerRadiusConverter` internally to apply rounded corners only to the outer left and outer right edges.
- Use `Cursor="Hand"` and a `PreviewMouseDoubleClick` handler if you want the badge to be clickable.
- Nesting `VersionTextBlock` inside `Shield` is a common pattern to show the assembly version in badge form.
