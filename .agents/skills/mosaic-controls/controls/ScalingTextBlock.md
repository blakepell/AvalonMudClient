# ScalingTextBlock

**Base class:** `TextBlock`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/ScalingTextBlock/ScalingTextBlock.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/ScalingTextBlockExample.xaml`

## Description

A `TextBlock` that automatically adjusts `FontSize` downward until the text fits the available width or reaches `MinFontSize`. It is useful for compact labels, counters, and headings with variable-length text.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `MinFontSize` | `double` | `12.0` | Smallest font size the control will use. |
| `MaxFontSize` | `double` | `24.0` | Starting and largest font size the control will use. |
| `FontScaleIncrement` | `double` | `1.0` | Amount subtracted while searching for a fitting font size. Lower values are more precise but require more checks. |

All standard `TextBlock` properties (`Text`, `Foreground`, `FontWeight`, `TextTrimming`, etc.) apply normally.

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:ScalingTextBlock
    Width="180"
    Text="{Binding DisplayName}"
    MinFontSize="10"
    MaxFontSize="24"
    FontScaleIncrement="0.5" />
```

## Notes

- The control recalculates on load, render-size changes, and `Text` changes.
- It scales only by width; use normal `TextBlock` wrapping if multi-line text is desired.
