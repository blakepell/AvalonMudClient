# Files

**Base class:** `Control`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/Files/Files.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/FilesExample.xaml`

## Description

A directory listing showing each entry's name with shell icon, modified date, and friendly size. It supports filtering, sorting, single or multi-selection, optional `FileSystemWatcher` refresh, folder navigation (with a `..` parent entry and an optional root boundary), file activation through event or command, Explorer-style incremental search, and F2 inline rename.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `DirectoryPath` | `string` | `string.Empty` | Folder whose contents are listed; updates as the user navigates. |
| `Filter` | `string` | `*` | Search pattern such as `*.txt` (applies to files only). |
| `ShowHiddenFiles` | `bool` | `false` | Includes hidden/system entries. |
| `SelectionMode` | `SelectionMode` | `Single` | Single, Multiple, or Extended selection. |
| `EnableFileWatcher` | `bool` | `false` | Watches the directory and coalesces refreshes. |
| `ShowFolders` | `bool` | `true` | Lists sub-folders and a `..` parent entry; activating a folder navigates into it. |
| `RootDirectory` | `string` | `string.Empty` | Bounds upward navigation; the user cannot navigate above this folder. Empty allows navigation anywhere. |
| `ShowNavigateErrorMessageBox` | `bool` | `true` | Shows a warning message box when folder navigation fails. The `NavigateError` event fires regardless. |
| `ShowRenameErrorMessageBox` | `bool` | `true` | Shows a warning message box when an inline rename fails. The `RenameError` event fires regardless. |
| `SelectedItem` | `FileItem?` | `null` | Two-way selected row. |
| `Items` | `ObservableCollection<FileItem>` | read-only | Displayed rows. |
| `FileActivatedCommand` | `ICommand?` | `null` | Executes on file activation with `FileInfo` parameter. |

Computed helpers: `SelectedFile`, `SelectedFiles` (both exclude folders).

Events: `FileDoubleClick`, `SelectionChanged`, `NavigateError`, `RenameError`.

Methods: `Refresh()`, `SortBy(memberPath, direction)`.

## Keyboard Behavior

- Typing while focus is within the file list performs a hidden prefix search in the active sorted/filtered view.
- Consecutive characters narrow the match; the buffer resets after one second or when navigation/focus changes.
- Repeating a single character cycles through visible names beginning with that character.
- `Enter` activates the selected file or folder.
- `F2` opens the selected row's `EditableTextBlock`; file names select the stem without the extension. Enter or focus loss commits the filesystem rename, while Escape cancels it.
- Text input is not intercepted when focus is in the inline rename field or another text-entry control.

```xml
<mosaic:Files
    DirectoryPath="{Binding Folder}"
    Filter="*.cs"
    EnableFileWatcher="True"
    SelectionMode="Extended"
    FileActivatedCommand="{Binding OpenFileCommand}" />
```
