# TagBox

**Base class:** `Control`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/TagBox/TagBox.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/TagBoxExample.xaml`

## Description

A tag/token entry control. Typing text and pressing Enter commits a removable tag chip; Backspace at the start of the input removes the last tag. Changes flow through the bindable `Tags` collection and can be vetoed.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Tags` | `ObservableCollection<string>` | empty | Backing tag collection. |
| `Watermark` | `string` | `Add a tag...` | Placeholder when empty. |
| `AllowDuplicates` | `bool` | `false` | Allows case-insensitive duplicate tags. |
| `TagBackground` / `TagForeground` | `Brush?` | theme | Chip fill/text brushes. |
| `TagBorderBrush` | `Brush?` | theme | Chip border brush. |
| `TagDeleteHoverBackground` | `Brush?` | theme | Delete glyph hover background. |
| `TagCornerRadius` | `CornerRadius` | `10` | Chip radius. |
| `CornerRadius` | `CornerRadius` | `0` | Outer control radius. |
| `ShowWatermark` | `bool` | read-only | Template flag for watermark visibility. |

## Members

| Member | Description |
|---|---|
| `AddTag(string?)` | Attempts to add a trimmed tag. |
| `RemoveTag(string?)` | Attempts to remove a tag. |
| `DeleteTagCommand` | Command used by chip delete buttons. |
| `TagChanging` | Cancellable event before add/remove. |
| `TagChanged` | Event after add/remove. |

```xml
<mosaic:TagBox
    Tags="{Binding Tags}"
    Watermark="Add label..."
    AllowDuplicates="False" />
```
