# SymbolRating

**Base class:** `Control`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/SymbolRating/SymbolRating.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/SymbolRatingExample.xaml`

## Description

A star-rating control that renders `Count` symbols (default: stars) and lets the user select a rating by clicking or hovering. The symbol is any Segoe MDL2 / Segoe Fluent glyph string, making it work as a star-rating, heart-rating, flag-rating, etc.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Count` | `int` | `5` | Total number of symbols to display. |
| `SelectedCount` | `int` | `0` | Number of selected (filled) symbols. Bindable two-way. |
| `Symbol` | `string` | `"\uE735"` (★ filled) | Segoe MDL2/Fluent glyph string for each item. |
| `SymbolFont` | `FontFamily` | `Segoe MDL2 Assets` | Font family for the symbol glyph. |
| `SymbolSize` | `double` | `24` | Font size of each symbol (pixels). |
| `SelectedBrush` | `Brush` | accent | Brush applied to selected symbols. |
| `UnselectedBrush` | `Brush` | muted foreground | Brush applied to unselected symbols. |
| `HoverBrush` | `Brush` | accent-light | Brush applied to symbols during hover preview. |
| `ShowDeselectOption` | `bool` | `true` | When `true`, clicking the first already-selected symbol sets `SelectedCount` to 0. |
| `IsHoverPreviewEnabled` | `bool` | `true` | Whether hovering highlights potential selection. |
| `IsReadOnly` | `bool` | `false` | Prevents user interaction when `true`. |

## Events

| Event | Args | Description |
|---|---|---|
| `RatingChanged` | `RoutedPropertyChangedEventArgs<int>` | Raised when `SelectedCount` changes. Provides `OldValue` and `NewValue`. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<!-- Star rating bound to ViewModel -->
<mosaic:SymbolRating
    Count="5"
    SelectedCount="{Binding Rating, Mode=TwoWay}"
    RatingChanged="OnRatingChanged" />

<!-- Heart rating (Segoe Fluent heart glyph) -->
<mosaic:SymbolRating
    Count="5"
    SelectedCount="{Binding HeartRating, Mode=TwoWay}"
    Symbol="&#xEB51;"
    SymbolFont="Segoe Fluent Icons"
    SelectedBrush="HotPink" />

<!-- Read-only display -->
<mosaic:SymbolRating
    Count="5"
    SelectedCount="3"
    IsReadOnly="True" />
```

## Notes

- `SelectedCount` defaults to `0` (no selection).
- Keyboard arrow keys increment/decrement `SelectedCount` when the control has focus.
- `ShowDeselectOption="False"` locks the minimum rating to 1 star (clicking the first star does nothing).
