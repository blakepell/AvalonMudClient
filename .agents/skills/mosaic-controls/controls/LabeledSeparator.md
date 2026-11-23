# LabeledSeparator

**Base class:** `ContentControl`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/LabeledSeparator/LabeledSeparator.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/LabeledSeparatorExample.xaml`

## Description

A horizontal separator line with an embedded text label. The label can be positioned to the left, center, or right of the separator via `LabelPosition`.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` | `string` | `null` | Label text placed on/beside the separator line. |
| `LabelPosition` | `LabelPosition` | `Left` | Where the label appears: `Left`, `Center`, or `Right`. |

## Enum: `LabelPosition`

```csharp
public enum LabelPosition { Left, Center, Right }
```

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<!-- Left label -->
<mosaic:LabeledSeparator Text="Section Header" />

<!-- Centered -->
<mosaic:LabeledSeparator Text="Or" LabelPosition="Center" />

<!-- Right-aligned -->
<mosaic:LabeledSeparator Text="End of results" LabelPosition="Right" />
```

## Notes

- The separator line spans the full available width; the label interrupts the line.
- Style the label text via standard inherited properties (`FontSize`, `Foreground`, `FontWeight`).
- `Text` is a convenience string property. For richer content, use the inherited `Content` property of `ContentControl`.
