# AccentButton

**Base class:** `Button`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/AccentButton/AccentButton.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/AccentButtonExample.xaml`

## Description

A themed `Button` with built-in accent color variants. Use it when a command should look like a primary, success, danger, gray, or default action without creating a one-off button style.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `AccentButtonType` | `AccentButtonType` | `ThemeAccent` | Selects the visual treatment for background, border, and hover states. |

## AccentButtonType Values

| Value | Description |
|---|---|
| `Default` | Uses the normal button treatment. |
| `ThemeAccent` | Uses the current Mosaic theme accent. |
| `Gray` | Neutral gray action button. |
| `FluentGreen` | Green success-style action. |
| `FluentRed` | Red destructive/error-style action. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:AccentButton
    Content="Save"
    AccentButtonType="ThemeAccent" />

<mosaic:AccentButton
    Content="Delete"
    AccentButtonType="FluentRed" />
```

## Notes

- Inherits standard `Button` command, content, focus, and keyboard behavior.
- The default style is loaded from `Controls/AccentButton/AccentButton.xaml` via `Themes/Generic.xaml`.
