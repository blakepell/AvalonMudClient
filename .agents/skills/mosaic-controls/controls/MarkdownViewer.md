# MarkdownViewer

**Base class:** `Control`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/MarkdownViewer/MarkdownViewer.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/MarkdownViewerExample.xaml`

## Description

A lookless, WPF-native Markdown viewer. It renders the `Markdown` string into a `FlowDocument` hosted by a read-only `RichTextBox`, so formatted content can be selected and copied as rich text. Rendering is defensive: invalid Markdown falls back to plain text instead of throwing.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Markdown` | `string` | `string.Empty` | Markdown source text to render. |
| `Source` | `Uri?` | `null` | WPF resource or local Markdown file to load. Relative Markdown links navigate in place. |
| `CanGoBack` | `bool` | `false` | Read-only value indicating whether `GoBack()` can return to a prior source. |
| `IsCopyEnabled` | `bool` | `true` | Enables selection/copy interaction in the hosted `RichTextBox`. |
| `IsDocumentReadOnly` | `bool` | `true` | Sets the hosted document read-only state. |

## XAML Example

```xml
<mosaic:MarkdownViewer
    FontSize="14"
    Markdown="{Binding PreviewMarkdown}" />
```

```xml
<mosaic:MarkdownViewer Source="/MyApp;component/Docs/index.md" />
```

## Notes

- Template part: `PART_RichTextBox`.
- `LinkClicked` is raised before default navigation and can intercept custom URI schemes.
- Relative links to Markdown resources navigate in the viewer; other links open through the system shell.
- `GoBack()` navigates to the previous source document.
- Ctrl+mouse-wheel zooms the base font from 8 through 32 pixels and scales explicit heading sizes proportionally.
- The rendered document inherits typography and foreground from the control/template so it follows Mosaic theme resources.
