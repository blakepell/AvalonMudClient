# FlipPanel

**Base class:** `ContentControl`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/FlipPanel/FlipPanel.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/FlipPanelExample.xaml`

## Description

A panel that holds two faces — `FrontContent` and `BackContent` — and animates between them with a 3-D flip rotation. The flip direction and duration are configurable.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `FrontContent` | `object` | `null` | Content displayed on the front face. |
| `BackContent` | `object` | `null` | Content displayed on the back face. |
| `Direction` | `FlipDirection` | `Horizontal` | The axis of the flip animation (`Horizontal` or `Vertical`). |
| `IsFlipped` | `bool` | `false` | `true` = back face is visible. Bindable. |
| `FlipDuration` | `Duration` | `0:0:0.5` | Duration of the flip animation. |

## Methods

| Method | Description |
|---|---|
| `Flip()` | Toggles between front and back faces (same as toggling `IsFlipped`). |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:FlipPanel
    Width="128" Height="128"
    Direction="Horizontal"
    Cursor="Hand"
    MouseDown="FlipPanel_MouseDown">

    <mosaic:FlipPanel.FrontContent>
        <Image Source="/Assets/front.png" />
    </mosaic:FlipPanel.FrontContent>

    <mosaic:FlipPanel.BackContent>
        <StackPanel Background="CornflowerBlue">
            <TextBlock Text="Back side" HorizontalAlignment="Center" />
        </StackPanel>
    </mosaic:FlipPanel.BackContent>
</mosaic:FlipPanel>
```

## Code-Behind

```csharp
private void FlipPanel_MouseDown(object sender, MouseButtonEventArgs e)
{
    ((FlipPanel)sender).Flip();
}
```

## Notes

- Both `FrontContent` and `BackContent` accept any `UIElement` or data template content.
- `FlipDirection.Vertical` rotates around the horizontal axis (top-to-bottom).
- `IsFlipped` can be data-bound to a ViewModel boolean for MVVM-driven flipping.
