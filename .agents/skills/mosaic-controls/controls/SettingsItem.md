# SettingsItem

**Base class:** `ContentControl`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/SettingsItem/SettingsItem.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/SettingsItemExample.xaml`

## Description

A settings-page row control. Renders a left icon, a title, a description text, and a right-side content slot for a toggle, button, or other control. Commonly used with `ToggleButton` or `ToggleSwitch` in the content area.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `IconSource` | `string` | `null` | Path to the icon image (relative or pack URI). |
| `Title` | `string` | `null` | Main label for the setting. |
| `Description` | `string` | `null` | Secondary description text (smaller, lighter). |
| `CornerRadius` | `CornerRadius` | `0` | Corner radius for the item background. |
| `IconSize` | `double` | `32` | Width and height of the icon image. |

The control's `Content` property holds the right-side control (e.g., `ToggleButton`, `CheckBox`, `ComboBox`).

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:SettingsItem
    Title="Dark Mode"
    Description="Switch the application to a dark color theme."
    IconSource="/Assets/theme-48.png">
    <mosaic:ToggleButton IsChecked="{Binding IsDarkMode, Mode=TwoWay}" />
</mosaic:SettingsItem>

<mosaic:SettingsItem
    Title="Notifications"
    Description="Receive desktop notifications for new events."
    IconSource="/Assets/bell-48.png">
    <mosaic:ToggleSwitch IsOn="{Binding NotificationsEnabled, Mode=TwoWay}" />
</mosaic:SettingsItem>
```

## Notes

- Designed to be used in a `StackPanel` or `ItemsControl` for a settings-page layout.
- The icon image is loaded via a standard WPF `Image` element inside the template; use pack URIs for embedded resources.
- Keep `Description` short (one or two sentences) for visual consistency.
