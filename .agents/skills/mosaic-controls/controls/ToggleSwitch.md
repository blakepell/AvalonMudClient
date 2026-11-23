# ToggleSwitch

**Base class:** `Control`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/ToggleSwitch/ToggleSwitch.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/ToggleSwitchExample.xaml`

## Description

A mobile-style on/off toggle switch (like iOS UISwitch or the WinUI `ToggleSwitch`). Shows a sliding thumb with optional "On"/"Off" labels. Supports command binding and two events for MVVM and code-behind integration.

## Template Parts

| Part | Type | Description |
|---|---|---|
| `PART_Thumb` | `Thumb` | The draggable indicator. |
| `PART_OnTextBlock` | `TextBlock` | Label shown when the switch is on. |
| `PART_OffTextBlock` | `TextBlock` | Label shown when the switch is off. |

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `IsOn` | `bool` | `false` | The current toggle state. Bindable two-way. |
| `OnText` | `string` | `"On"` | Label text shown in the On state. |
| `OffText` | `string` | `"Off"` | Label text shown in the Off state. |
| `OnBackgroundBrush` | `Brush` | theme accent | Track color when `IsOn` is `true`. |
| `OffBackgroundBrush` | `Brush` | theme control background | Track color when `IsOn` is `false`. |
| `CornerRadius` | `CornerRadius` | `10` | Corner radius of the switch track. |
| `Command` | `ICommand` | `null` | Command executed when state changes. |
| `CommandParameter` | `object` | `null` | Parameter passed to `Command`. |

## Events

| Event | Type | Description |
|---|---|---|
| `Toggled` | Routed (`RoutedEventArgs`) | Bubbles up when `IsOn` changes. |
| `Changed` | CLR (`EventHandler<bool>`) | Same trigger; passes new value as `bool`. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<!-- Basic MVVM binding -->
<mosaic:ToggleSwitch IsOn="{Binding DarkMode, Mode=TwoWay}" />

<!-- Custom labels -->
<mosaic:ToggleSwitch
    IsOn="{Binding Enabled, Mode=TwoWay}"
    OnText="Enabled"
    OffText="Disabled" />

<!-- Command pattern -->
<mosaic:ToggleSwitch
    IsOn="{Binding IsActive, Mode=TwoWay}"
    Command="{Binding ToggleActiveCommand}"
    CommandParameter="{Binding RelativeSource={RelativeSource Self}, Path=IsOn}" />

<!-- Code-behind event -->
<mosaic:ToggleSwitch Toggled="OnToggled" />
```

## Code-Behind

```csharp
private void OnToggled(object sender, RoutedEventArgs e)
{
    var toggle = (ToggleSwitch)sender;
    bool isOn = toggle.IsOn;
    // respond to state change
}
```

## Notes

- `Space` / `Enter` keys toggle the state when the control has focus.
- `ToggleSwitchAutomationPeer` provides the `Toggle()` automation pattern.
- The thumb can be dragged or clicked.
