# VersionTextBlock

**Base class:** `TextBlock`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/VersionTextBlock/VersionTextBlock.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/VersionTextBlockExample.xaml`

## Description

A `TextBlock` that automatically populates its `Text` with the version string from an assembly. The assembly is resolved by `AssemblySource` (entry, executing, calling, this, or by name). No code-behind required.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `AssemblySource` | `VersionAssemblySource` (enum) | `EntryAssembly` | Which assembly to read the version from. |
| `AssemblyName` | `string` | `null` | When `AssemblySource` is `ByName`, the simple assembly name to look up. |

### `VersionAssemblySource` Enum Values

| Value | Resolves to |
|---|---|
| `ThisAssembly` | `Mosaic.UI.Wpf` assembly (the control library itself). |
| `ExecutingAssembly` | `Assembly.GetExecutingAssembly()` at the call site. |
| `CallingAssembly` | `Assembly.GetCallingAssembly()`. |
| `EntryAssembly` | `Assembly.GetEntryAssembly()` (the running EXE). |
| `ByName` | `AppDomain.CurrentDomain.GetAssemblies()` filtered by `AssemblyName`. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<!-- Show version of the running EXE (default) -->
<mosaic:VersionTextBlock />

<!-- Prefix the version string -->
<mosaic:VersionTextBlock AssemblySource="EntryAssembly">
    <!-- Text is auto-set; add a Label in a StackPanel to prefix -->
</mosaic:VersionTextBlock>

<!-- Wrap in a StackPanel with a label -->
<StackPanel Orientation="Horizontal">
    <TextBlock Text="Version: " />
    <mosaic:VersionTextBlock AssemblySource="EntryAssembly" />
</StackPanel>

<!-- Named assembly -->
<mosaic:VersionTextBlock AssemblySource="ByName" AssemblyName="Mosaic.UI.Wpf" />
```

## Notes

- The version string is read from `AssemblyInformationalVersionAttribute` if present; otherwise falls back to `AssemblyVersion`.
- `Text` is set once at load time and is not dynamically updated (assemblies do not change version at runtime).
- Inherits all `TextBlock` properties (`FontSize`, `Foreground`, `FontWeight`, etc.).
