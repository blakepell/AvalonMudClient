# SplitButton

**Base class:** `ContentControl` (implements `ICommandSource`)  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/SplitButton/SplitButton.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/SplitButtonExample.xaml`

## Description

A button with two distinct hit areas: a primary action surface (left) and a drop-down chevron (right). The primary surface triggers `Click` / `Command`; the chevron opens the control's `ContextMenu`. Both sides are independently enable/disable-able.

## Template Parts

| Part | Type | Description |
|---|---|---|
| `PART_PrimaryButton` | `FrameworkElement` | The primary clickable area. |
| `PART_DropDownButton` | `FrameworkElement` | The chevron/drop-down trigger. |

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Command` | `ICommand` | `null` | Command invoked by the primary button. Can be disabled independently. |
| `CommandParameter` | `object` | `null` | Parameter passed to `Command`. |
| `CommandTarget` | `IInputElement` | `null` | Target for routed commands. |
| `IsDropDownOpen` | `bool` | `false` | Whether the context menu is currently open. Bindable two-way. |
| `DropDownPlacement` | `PlacementMode` | `Bottom` | How the context menu is positioned relative to the control. |
| `MatchDropDownWidth` | `bool` | `true` | When `true`, the context menu is at least as wide as the button. |

## Events

| Event | Description |
|---|---|
| `Click` | Raised when the primary button surface is clicked. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:SplitButton
    Width="150"
    Content="Save"
    Command="{Binding SaveCommand}"
    Click="SplitButton_Click">
    <mosaic:SplitButton.ContextMenu>
        <ContextMenu DataContext="{Binding PlacementTarget.DataContext,
                     RelativeSource={RelativeSource Self}}">
            <MenuItem Header="Save" Command="{Binding SaveCommand}" />
            <MenuItem Header="Save As…" Command="{Binding SaveAsCommand}" />
            <Separator />
            <MenuItem Header="Export…" Command="{Binding ExportCommand}" />
        </ContextMenu>
    </mosaic:SplitButton.ContextMenu>
</mosaic:SplitButton>
```

## Notes

- When `Command.CanExecute` returns `false`, only the primary button is disabled; the drop-down chevron remains active so users can still access menu options.
- `DropDownPlacement="Right"` positions the menu to the right of the button.
- `MatchDropDownWidth="False"` lets the context menu size independently of the button width.
- The `SplitButtonAutomationPeer` class provides accessibility support.
