# MarkdownEditor

**Base class:** `UserControl`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/MarkdownEditor/MarkdownEditor.xaml.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/MarkdownEditorExample.xaml`

## Description

A self-contained Markdown editor built on Mosaic `SyntaxEditor` / AvalonEdit. It includes a formatting toolbar, markdown-aware keyboard handling, snippet insertion, document title/modification tracking, save helpers, HTML copy, base64 image paste, and browser preview.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` | `string` | editor text | Gets or sets the Markdown document text. |
| `IsModified` | `bool` | `false` | Tracks unsaved changes and appends `*` to `DocumentTitle`. |
| `FilePath` | `string?` | `null` | Current file path used by `Save()` / `SaveAsync()`. |
| `FileName` | `string?` | `null` | Display name for virtual documents without a path. |
| `DocumentTitle` | `string` | `Untitled` | Computed title for host tabs/menus. |
| `StatusBarVisibility` | `Visibility` | `Collapsed` | Shows line, column, and character status. |

## Methods And Events

| Member | Description |
|---|---|
| `LoadFile(path)` | Loads Markdown from disk and clears modification state. |
| `NewDocument()` | Clears the editor and resets document metadata. |
| `Save()` / `SaveAsync()` / `SaveAsAsync()` | Writes the document to disk, prompting when needed for Save As. |
| `SurroundWith(marker)` | Wraps selection/current caret with markdown markers. |
| `Heading(level)` | Applies heading level 1-6 to the current line. |
| `InsertSnippet(name)` | Inserts `link`, `image`, or fenced `code` snippets. |
| `InsertTable()` | Inserts a starter Markdown table. |
| `CopyAsHtml()` | Converts selection or document to HTML and copies it. |
| `PasteImage()` | Converts clipboard image to base64 Markdown image syntax. |
| `PreviewInBrowser()` | Writes a temporary HTML preview and opens it. |
| `Saving` | Cancellable event raised before save operations. |

## XAML Example

```xml
<mosaic:MarkdownEditor
    Text="{Binding MarkdownText, Mode=TwoWay}"
    StatusBarVisibility="Visible" />
```

## Notes

- `Ctrl+B` and `Ctrl+I` apply bold/italic markers.
- Enter, Escape, Tab, and Shift+Tab have markdown-list aware handling.
- Implements `ISaveable`.
