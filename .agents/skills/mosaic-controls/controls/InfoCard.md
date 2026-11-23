# InfoCard

**Base class:** `ContentControl`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/InfoCard/InfoCard.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/InfoCardExample.xaml`

## Description

A card control with an animated left accent bar, a title, and three content zones: `HeaderContent` (top right), body `Content`, and `FooterContent` (bottom). When `AccentBrush` changes, the left bar color transitions with an animation.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `AccentBrush` | `Brush` | `CornflowerBlue` | Color of the animated left accent bar. |
| `Title` | `string` | `null` | Title text displayed at the top of the card. |
| `HeaderContent` | `object` | `null` | Content placed in the top-right area of the header row. |
| `FooterContent` | `object` | `null` | Content placed below the body content. |
| `SeparatorVisibility` | `Visibility` | `Visible` | Visibility of the horizontal separator between title and body. |

The body `Content` property is the standard `ContentControl.Content`.

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:InfoCard
    Title="Indexed Files"
    Width="300" Height="120"
    AccentBrush="CornflowerBlue">

    <mosaic:InfoCard.HeaderContent>
        <Button Content="Refresh" HorizontalAlignment="Right" />
    </mosaic:InfoCard.HeaderContent>

    <mosaic:InfoCard.Content>
        <TextBlock FontSize="36" Text="{Binding FileCount}" />
    </mosaic:InfoCard.Content>

    <mosaic:InfoCard.FooterContent>
        <TextBlock Text="Last updated: today" FontSize="11" />
    </mosaic:InfoCard.FooterContent>
</mosaic:InfoCard>
```

## Notes

- Changing `AccentBrush` at runtime produces a smooth color transition animation.
- `SeparatorVisibility="Collapsed"` hides the divider line for a more compact layout.
- The card does not enforce a fixed size — set `Width`/`Height` or let it auto-size to content.
