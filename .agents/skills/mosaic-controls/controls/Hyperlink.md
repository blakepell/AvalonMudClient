# Hyperlink

**Base class:** `ContentControl`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/HyperLink/Hyperlink.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/HyperlinkExample.xaml`

## Description

A Mosaic-styled clickable hyperlink. It can open a URI via Windows Shell (`Process.Start("explorer.exe", uri)`) or execute an `ICommand`. When `ChangeVisitedLinkColor` is `true`, the foreground brush changes after the first click to distinguish visited links.

Note: the class name is `Hyperlink` but it lives in `Mosaic.UI.Wpf.Controls`, not the standard WPF `System.Windows.Documents` namespace.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` | `string` | `null` | Link text displayed in the control. |
| `NavigateUri` | `string` | `null` | URI opened via Shell when clicked (if `Command` is null). |
| `Command` | `ICommand` | `null` | Command executed when clicked. Takes priority over `NavigateUri`. |
| `CommandParameter` | `object` | `null` | Parameter passed to `Command`. |
| `HoverBrush` | `Brush` | theme `HyperLinkHoverBrush` | Foreground brush on hover. |
| `EnableAutoToolTip` | `bool` | `true` | Automatically shows a tooltip with the `NavigateUri`. |
| `HasVisited` | `bool` | `false` | Set to `true` after the first click. |
| `ChangeVisitedLinkColor` | `bool` | `true` | Changes foreground color once `HasVisited` is `true`. |
| `TextWrapping` | `TextWrapping` | default | Text wrapping behavior. |
| `TextTrimming` | `TextTrimming` | default | Text trimming behavior. |

## XAML Examples

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<!-- URI link -->
<mosaic:Hyperlink
    Text="Visit the project site"
    NavigateUri="https://www.apexgate.net" />

<!-- Command link -->
<mosaic:Hyperlink
    Text="Open Settings"
    Command="{Binding OpenSettingsCommand}" />

<!-- No visited-color change -->
<mosaic:Hyperlink
    Text="Documentation"
    NavigateUri="https://docs.example.com"
    ChangeVisitedLinkColor="False" />
```

## Notes

- Foreground defaults to the theme's `MosaicTheme.HyperLinkBrush` token.
- `HoverBrush` defaults to `MosaicTheme.HyperLinkHoverBrush`.
- `OnClick` is a `RelayCommand` (CommunityToolkit.Mvvm) wired into the template button.
- If both `Command` and `NavigateUri` are set, `Command` wins.
