# AvalonDock

**Package/project:** `MosaicUIWpf.AvalonDock` / `src/Mosaic.UI.Wpf.AvalonDock`  
**Primary controls:** `Mosaic.UI.Wpf.AvalonDock.DockingManager`, `LayoutMarkdownEditor`, `LayoutSyntaxEditor`
**Theme:** `Mosaic.UI.Wpf.AvalonDock.Themes.MosaicTheme`  
**Example:** `src/MosaicWpfDemo/Views/Examples/AvalonDockExample.xaml`

## Description

Mosaic-themed AvalonDock integration for IDE-style layouts: document panes, anchorable tool windows, floating windows, auto-hide panes, draggable tabs, docking overlays, and layout chrome. It is a separate assembly/package from `Mosaic.UI.Wpf`.

## Namespace

```xml
xmlns:ad="https://github.com/blakepell/MosaicUIWpf"
xmlns:mosaic="http://schemas.apexgate.net/wpf/mosaic-ui"
```

The AvalonDock assembly maps that URI to `Mosaic.UI.Wpf.AvalonDock`, `.Controls`, `.Converters`, `.Layout`, and `.Themes`.

## XAML Example

```xml
<ad:DockingManager x:Name="DockingManager">
    <ad:DockingManager.Theme>
        <ad:MosaicTheme />
    </ad:DockingManager.Theme>

    <ad:LayoutRoot>
        <ad:LayoutPanel Orientation="Horizontal">
            <ad:LayoutAnchorablePane DockWidth="230">
                <ad:LayoutAnchorable Title="Explorer" ContentId="explorer">
                    <TreeView />
                </ad:LayoutAnchorable>
            </ad:LayoutAnchorablePane>

            <ad:LayoutDocumentPane>
                <ad:LayoutDocument Title="Document" ContentId="document" IsSelected="True">
                    <mosaic:SyntaxEditor Language="CSharp" />
                </ad:LayoutDocument>
            </ad:LayoutDocumentPane>
        </ad:LayoutPanel>
    </ad:LayoutRoot>
</ad:DockingManager>
```

## Theme Switching

The demo reapplies the theme when Mosaic's global theme changes:

```csharp
ThemeManager.ThemeChanged += (_, _) =>
    DockingManager.Theme = new Mosaic.UI.Wpf.AvalonDock.Themes.MosaicTheme();
```

## Notes

- The Mosaic theme maps AvalonDock document tabs, tool-window captions, auto-hide tabs, menus, floating windows, navigator window, and docking overlays to Mosaic theme tokens.
- `DockingManagerExtensions.Add(...)` can add a control as a new `LayoutDocument` without hand-building the layout tree.
- Layout serialization types live under `Mosaic.UI.Wpf.AvalonDock.Serialization`.
- `LayoutMarkdownEditor` and `LayoutSyntaxEditor` derive from `LayoutDocument`, implement `ISaveable`, track dirty titles, and provide file load/save helpers.
- `LayoutSyntaxEditor.Editor` and `.ToolBar` are public XAML fields. Consumers can merge buttons, separators, and other controls through `AdditionalToolBarItems`.
