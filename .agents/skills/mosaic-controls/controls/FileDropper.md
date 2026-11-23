# FileDropper

**Base class:** `Control`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/FileDropper/FileDropper.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/FileDropperExample.xaml`

## Description

A drag-and-drop target for files from the operating system. It displays a prompt, accepted file-type text, and validation state; valid drops raise a routed event and can execute a command with the dropped file paths.

## Template Parts

| Part | Type | Description |
|---|---|---|
| `PART_Border` | `Border` | Main drop target border used by the default template. |

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Prompt` | `string` | `"Drop files here"` | Top-line prompt text. |
| `AcceptedFileTypes` | `ObservableCollection<string>` | empty collection | Wildcard or extension patterns such as `*.png`, `.txt`, or `*.*`; empty accepts all files. |
| `AcceptedFileTypesDisplay` | `string` | `*.*` | Read-only comma-separated display string for accepted patterns. |
| `DragState` | `FileDropState` | `None` | Read-only state used by the template: `None`, `Valid`, or `Invalid`. |
| `CornerRadius` | `CornerRadius` | `6` | Corner radius for the drop target border. |
| `FileDropCommand` | `ICommand` | `null` | Command executed on valid drop; parameter is the dropped `string[]` file paths. |

## Events

| Event | Type | Description |
|---|---|---|
| `FileDrop` | Routed (`FileDropEventArgs`) | Raised after one or more accepted files are dropped. `FileDropEventArgs.Files` exposes an `IReadOnlyList<string>`. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"
xmlns:sys="clr-namespace:System;assembly=System.Runtime"

<mosaic:FileDropper
    Prompt="Drop images here"
    FileDropCommand="{Binding ImportFilesCommand}">
    <mosaic:FileDropper.AcceptedFileTypes>
        <sys:String>*.png</sys:String>
        <sys:String>*.jpg</sys:String>
        <sys:String>*.jpeg</sys:String>
    </mosaic:FileDropper.AcceptedFileTypes>
</mosaic:FileDropper>
```

## Code-Behind

```csharp
private void OnFileDrop(object sender, FileDropEventArgs e)
{
    foreach (string file in e.Files)
    {
        // import file
    }
}
```

## Notes

- `AcceptedFileTypes` supports `*`, `*.*`, wildcard patterns, bare extensions (`png`), and dotted extensions (`.png`).
- All dropped files must match an accepted pattern; mixed valid/invalid drops are rejected.
