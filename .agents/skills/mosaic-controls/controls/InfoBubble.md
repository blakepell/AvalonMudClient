# InfoBubble

**Base class:** `ContentControl`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/InfoBubble/InfoBubble.cs`

Displays content with an overlaid count, status, or notification indicator.

**Demo:** `src/MosaicWpfDemo/Views/Examples/InfoBubbleExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `object? InfoBubbleContent` | Gets or sets the content displayed in the indicator. |
| `DataTemplate? InfoBubbleContentTemplate` | Gets or sets the template used to display the indicator content. |
| `Brush? InfoBubbleBackground` | Gets or sets the brush that paints the indicator background. |
| `Brush? InfoBubbleForeground` | Gets or sets the brush that paints the indicator content. |
| `Brush? InfoBubbleBorderBrush` | Gets or sets the brush that paints the indicator border. |
| `Thickness InfoBubbleBorderThickness` | Gets or sets the thickness of the indicator border. |
| `CornerRadius InfoBubbleCornerRadius` | Gets or sets the radius of the indicator corners. |
| `InfoBubblePlacementMode InfoBubblePlacementMode` | Gets or sets the indicator position relative to the content. |
| `Thickness InfoBubbleMargin` | Gets or sets the margin used to fine-tune the indicator position. |
| `Thickness InfoBubbleOverhangMargin` | Gets the margin reserved around content for indicator overhang. |
| `Thickness InfoBubbleIndicatorMargin` | Gets the margin that moves the indicator inward. |
| `FontFamily InfoBubbleFontFamily` | Gets or sets the font family used by the indicator content. |
| `double InfoBubbleFontSize` | Gets or sets the indicator content font size. |
| `FontWeight InfoBubbleFontWeight` | Gets or sets the font weight used by the indicator content. |
| `double InfoBubbleMinWidth` | Gets or sets the minimum width of the indicator. |
| `double InfoBubbleMinHeight` | Gets or sets the minimum height of the indicator. |
| `Thickness InfoBubblePadding` | Gets or sets the padding around the indicator content. |
| `bool IsInfoBubbleVisible` | Gets or sets a value that indicates whether the indicator is visible. |
| `bool IsDot` | Gets or sets a value that indicates whether the indicator is displayed as a dot. |
| `int InfoBubbleMaxValue` | Gets or sets the largest numeric value displayed without a plus suffix. |
| `bool HideWhenZero` | Gets or sets a value that indicates whether a numeric zero hides the indicator. |
| `object? DisplayInfoBubbleContent` | Gets the formatted content currently displayed in the indicator. |
| `bool ComputedInfoBubbleVisibility` | Gets a value that indicates whether the indicator satisfies all visibility conditions. |


## Example

```xml
<mosaic:InfoBubble InfoBubbleContent="5">
    <Button Content="Notifications" />
</mosaic:InfoBubble>
```
