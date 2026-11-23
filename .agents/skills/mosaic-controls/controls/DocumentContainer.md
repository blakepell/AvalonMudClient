# DocumentContainer

**Base class:** `Mosaic.UI.Wpf.Controls.TabControl`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/DocumentContainer/DocumentContainer.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/DocumentContainerExample.xaml`

## Overview

`DocumentContainer` displays an `ObservableCollection<Document>` as Fluent-styled document tabs. It tracks the active document, supports drag-and-drop reordering, provides close buttons and `Ctrl+F4`, and exposes a shared content area at the right side of the tab strip.

## Key Properties

- `Documents`: The observable document collection.
- `ActiveDocument`: The selected document; binds two-way by default.
- `HeaderContent`: Shared controls displayed at the right side of the tab strip.
- `DocumentClosingCommand`: An optional command invoked before removal.
- `ActiveTabBackground`: Active tab background brush; defaults to `#007ACC`.
- `ActiveTabForeground`: Active tab foreground brush; defaults to white.
- `TabOverflowMode`: Uses multiple rows with `Wrap` by default or a single button-scrollable row with `Scroll`.

Each `Document` provides `Title`, `Content`, and `CanClose` properties.

## Closing

`DocumentClosing` is a cancelable routed event. `DocumentClosed` is raised after removal. `TryCloseDocument` runs the same close pipeline used by the close button and keyboard command.

```xml
<mosaic:DocumentContainer
    ActiveDocument="{Binding ActiveDocument}"
    ActiveTabBackground="DarkGreen"
    ActiveTabForeground="White"
    Documents="{Binding Documents}"
    TabOverflowMode="Scroll">
    <mosaic:DocumentContainer.HeaderContent>
        <Button Command="{Binding AddDocumentCommand}" Content="Add" />
    </mosaic:DocumentContainer.HeaderContent>
</mosaic:DocumentContainer>
```
