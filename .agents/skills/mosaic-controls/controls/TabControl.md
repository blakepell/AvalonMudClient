# TabControl

**Base class:** `System.Windows.Controls.TabControl`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/TabControl/TabControl.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/TabControlExample.xaml`

## Description

A drop-in replacement for the WPF `TabControl` styled to the Mosaic theme. Adds an `ActiveIndicatorPlacement` property that draws a colored accent line to indicate the selected tab — either above or below the tab header text.

The companion `TabItem` subclass (`Mosaic.UI.Wpf.Controls.TabItem`) should be used with this control.

## Key Properties (additions to standard TabControl)

| Property | Type | Default | Description |
|---|---|---|---|
| `ActiveIndicatorPlacement` | `ActiveIndicatorPlacement` (enum) | `Bottom` | Where the selected-tab accent line appears: `Top` or `Bottom`. |

## `TabItem` Subclass

Use `mosaic:TabItem` instead of the standard `TabItem` to get Mosaic-themed tab headers:

```xml
<mosaic:TabControl>
    <mosaic:TabItem Header="Overview">
        <TextBlock Text="Overview content" />
    </mosaic:TabItem>
    <mosaic:TabItem Header="Details">
        <TextBlock Text="Details content" />
    </mosaic:TabItem>
</mosaic:TabControl>
```

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:TabControl ActiveIndicatorPlacement="Bottom">
    <mosaic:TabItem Header="Dashboard">
        <!-- content -->
    </mosaic:TabItem>
    <mosaic:TabItem Header="Settings">
        <!-- content -->
    </mosaic:TabItem>
    <mosaic:TabItem Header="About">
        <!-- content -->
    </mosaic:TabItem>
</mosaic:TabControl>
```

## ItemsSource Binding

```xml
<mosaic:TabControl ItemsSource="{Binding Tabs}">
    <mosaic:TabControl.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Title}" />
        </DataTemplate>
    </mosaic:TabControl.ItemTemplate>
    <mosaic:TabControl.ContentTemplate>
        <DataTemplate>
            <ContentPresenter Content="{Binding Content}" />
        </DataTemplate>
    </mosaic:TabControl.ContentTemplate>
</mosaic:TabControl>
```

## Notes

- All standard `TabControl` properties (`SelectedIndex`, `SelectedItem`, `SelectionChanged`, etc.) are fully inherited.
- `TabStripPlacement` (Left, Right, Top, Bottom) is also inherited from the base class.
- Use `mosaic:TabItem` (not `TabItem`) so the Mosaic template applies to the tab headers.
