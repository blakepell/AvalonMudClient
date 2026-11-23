# SyntaxEditor

**Base class:** `ICSharpCode.AvalonEdit.TextEditor`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/SyntaxEditor/SyntaxEditor.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/SyntaxEditorExample.xaml`

## Description

An AvalonEdit-based code editor integrated with Mosaic themes. It ships bundled syntax highlighting definitions, a themed search panel, JSON format/minify/validate commands, line comment commands, and line move commands.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Theme` | `MosaicThemeMode` | `Light` | Mosaic theme used for editor surface colors and themed highlighting resources. |
| `Language` | `SyntaxLanguage` | `None` | Bundled syntax highlighting language to apply. |
| `FollowGlobalTheme` | `bool` | `true` | Tracks `ThemeManager.ThemeChanged` while loaded and updates `Theme` automatically. |

Common AvalonEdit properties such as `Text`, `Document`, `IsReadOnly`, `ShowLineNumbers`, `FontFamily`, and `FontSize` are available. The constructor defaults to Consolas 12 with line numbers and current-line highlighting enabled.

## SyntaxLanguage Values

`None`, `Json`, `CSharp`, `Xml`, `JavaScript`, `Sql`, `Markdown`, `C`, `Lua`, `Python`, `Ini`, `Java`, `Swift`, `Basic`, `VbNet`, `Perl`, and `Php`.

Use `SyntaxLanguageMap.FromExtension(pathOrExtension)` to resolve from a file path or extension.

## Commands

| Command | Shortcut | Description |
|---|---|---|
| `ApplicationCommands.Find` | `Ctrl+F` | Opens the AvalonEdit search panel (find only). |
| `ApplicationCommands.Replace` | `Ctrl+H` | Opens the search panel with the replace row visible. Also exposed as `OpenReplacePanel()`. |
| `ReplaceNextCommand` | `Enter` (in the replace box) | Replaces the selected match and selects the next one; if the selection is not a match it only moves to the next match. Also exposed as `ReplaceNext()`. |
| `ReplaceAllCommand` | `Ctrl+Alt+Enter` (in the replace box) | Replaces every match in one undo step. Also exposed as `ReplaceAll()`, which returns the count. |
| `ToggleReplaceCommand` | none (chevron button) | Shows or hides the replace row. |
| `FormatJsonCommand` | none | Pretty-prints the current JSON document. |
| `MinifyJsonCommand` | none | Compacts the current JSON document. |
| `ValidateJsonCommand` | none | Validates the current JSON document and shows a result dialog. |
| `CommentSelectionCommand` | `Ctrl+K, Ctrl+C` | Comments selected lines or the current line when supported by the language. |
| `UncommentSelectionCommand` | `Ctrl+K, Ctrl+U` | Removes line comments when supported by the language. |
| `MoveSelectionUpCommand` | `Ctrl+Up` | Moves selected lines, or current line, up one line. |
| `MoveSelectionDownCommand` | `Ctrl+Down` | Moves selected lines, or current line, down one line. |

## Events

| Event | Type | Description |
|---|---|---|
| `ContextMenuRequested` | Routed (`SyntaxEditorContextMenuEventArgs`) | Raised after the default context menu is populated and before it opens so consumers can customize it. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:SyntaxEditor
    Text="{Binding SourceText, Mode=TwoWay}"
    Language="CSharp"
    FollowGlobalTheme="True" />
```

## Code Example

```csharp
editor.Language = SyntaxLanguageMap.FromExtension(filePath);
editor.Text = File.ReadAllText(filePath);
```

## Notes

- Highlighting definitions are embedded as resources under `src/Mosaic.UI.Wpf/Assets`.
- JSON commands show WPF message boxes on invalid or empty input.
- `ContextMenuRequested` exposes the `ContextMenu` and active `SyntaxLanguage`.
