# FavoriteCheckBox

**Base class:** `CheckBox`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/FavoriteCheckBox/FavoriteCheckBox.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/FavoriteCheckBoxExample.xaml`

## Description

A symbol-only `CheckBox` for favorite/star toggles. It uses a configurable symbol and brushes for checked, unchecked, and hover states while preserving normal checkbox binding and keyboard behavior.

## Template Parts

| Part | Type | Description |
|---|---|---|
| `PART_SymbolTextBlock` | `TextBlock` | Displays the configured symbol and state brush. |

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Symbol` | `string` | star glyph | Character or glyph displayed by the control. |
| `SymbolFont` | `FontFamily` | `Segoe UI Symbol` | Font family used for `Symbol`. |
| `SymbolSize` | `double` | `20.0` | Font size used for the symbol. |
| `CheckedBrush` | `Brush` | `Gold` | Foreground brush when `IsChecked` is `true`. |
| `UncheckedBrush` | `Brush` | `LightGray` | Foreground brush when `IsChecked` is `false`. |
| `HoverBrush` | `Brush` | `Orange` | Foreground brush while hovered. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:FavoriteCheckBox
    IsChecked="{Binding IsFavorite, Mode=TwoWay}" />

<mosaic:FavoriteCheckBox
    Symbol="&#x2665;"
    SymbolSize="24"
    CheckedBrush="Crimson"
    UncheckedBrush="LightGray"
    IsChecked="{Binding IsLiked, Mode=TwoWay}" />
```

## Notes

- Use `IsChecked` for MVVM state, just like a normal `CheckBox`.
- The disabled state uses `SystemColors.GrayTextBrush`.
