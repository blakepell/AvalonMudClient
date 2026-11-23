# SideMenu

**Base class:** `UserControl`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/SideMenu/SideMenu.xaml.cs`  
**Example:** `src/MosaicWpfDemo/MainWindow.xaml` (the demo app's main navigation)

## Description

A collapsible vertical navigation menu. Items are `SideMenuItem` objects, each with an icon, label, and optional sub-items. The menu supports compact (icon-only) mode, search, drag-and-drop reorder, and integration with an `AppServices` DI container for singleton view navigation.

`SideMenu` is a `UserControl` and cannot be re-templated.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `MenuItems` | `ObservableCollection<SideMenuItem>` | `null` | The list of navigation items. |
| `SelectedItem` | `SideMenuItem` | `null` | The currently selected item. |
| `ContentPresenter` | `ContentControl` | `null` | The content area that the menu navigates into (bind to an outer `ContentControl`). |
| `Compact` | `bool` | `false` | When `true`, shows only icons (collapsed mode). |
| `SearchBoxVisibility` | `Visibility` | `Visible` | Show/hide the search box at the top of the menu. |
| `VerticalScrollViewerVisibility` | `ScrollBarVisibility` | `Auto` | Scroll bar visibility for the items list. |
| `EnableDragAndDropReordering` | `bool` | `false` | Enables drag-and-drop item reordering. |
| `FocusSearchBoxOnLoad` | `bool` | `false` | Focuses the search box when the menu loads. |

## Events

| Event | Args | Description |
|---|---|---|
| `SelectedItemChanged` | `RoutedEventArgs` | Raised when the selected item changes. |
| `ItemClicked` | `SideMenuItemClickEventArgs` | Raised when any item is clicked. `SideMenuItemClickEventArgs` carries the `Item`. |

## Methods

| Method | Description |
|---|---|
| `SelectItem(SideMenuItem)` | Programmatically selects an item. |
| `SelectByIndex(int)` | Selects the item at the given index. |
| `ExpandAll()` | Expands all items that have sub-items. |
| `CollapseAll()` | Collapses all items that have sub-items. |

## SideMenuItem Properties

| Property | Type | Description |
|---|---|---|
| `Text` | `string` | Label shown beside the icon. |
| `ImageSource` | `string` | Path to the 48px icon image. |
| `ContentType` | `Type` | `Type` of the view to navigate to when clicked. |
| `ContentTypeIsSingleton` | `bool` | When `true`, `AppServices` resolves one shared instance of `ContentType`. |
| `ParameterCollection` | `SideMenuParameterCollection` | Key/value pairs passed to the view (see below). |
| `SubItems` | collection | Nested child items. |

## XAML Example

```xml
xmlns:mosaic="http://schemas.apexgate.net/wpf/mosaic-ui"

<mosaic:SideMenu
    x:Name="SideMenu"
    ContentPresenter="{x:Reference MainContentArea}"
    SearchBoxVisibility="Visible">
    <mosaic:SideMenu.MenuItems>
        <mosaic:SideMenuItem
            ContentType="{x:Type local:DashboardView}"
            ContentTypeIsSingleton="True"
            ImageSource="/Assets/dashboard-48.png"
            Text="Dashboard" />
    </mosaic:SideMenu.MenuItems>
</mosaic:SideMenu>

<ContentControl x:Name="MainContentArea" />
```

## SideMenuParameterCollection Usage (Demo Pattern)

The demo app passes parameters to `ShellView` via:

```xml
<mosaic:SideMenuItem.ParameterCollection>
    <mosaic:SideMenuParameterCollection>
        <mosaic:SideMenuParameter Key="Title" Value="My Control" />
        <mosaic:SideMenuParameter Key="XamlFile" Value="MosaicWpfDemo.LinkedSources.MyExample.xaml" />
        <mosaic:SideMenuParameter Key="CodeFile" Value="MosaicWpfDemo.LinkedSources.MyExample.xaml.cs" />
        <mosaic:SideMenuParameter Key="DocumentationType" Value="{x:Type mosaic:MyControl}" />
        <mosaic:SideMenuParameter Key="ExampleType" Value="{x:Type example:MyExample}" />
        <mosaic:SideMenuParameter Key="ImageSource" Value="/Assets/icon-48.png" />
    </mosaic:SideMenuParameterCollection>
</mosaic:SideMenuItem.ParameterCollection>
```

## Notes

- `ISideMenuRecipient` can be implemented by a view to receive the `SideMenuItem` when navigation occurs.
- `SideMenuHeader` is a non-navigable grouping item (no icon, no navigation).
- Sub-items create collapsible tree nodes in the menu.
