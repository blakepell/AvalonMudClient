# AvalonEdit Behaviors

**Namespace:** `Mosaic.UI.Wpf.Behaviors`\
**Sources:** `src/Mosaic.UI.Wpf/Behaviors/AvalonEdit*.cs`\
**Example:** `src/MosaicWpfDemo/Views/Examples/AvalonEditVtTerminalBehaviorExample.xaml`

## AvalonEditVtTerminalBehavior

Applies a retro VT/CRT terminal skin to an AvalonEdit `TextEditor`: phosphor foreground/background colors, block caret, scan lines, vignette, glow, and subtle noise overlay.

| Property | Type | Default | Description |
|---|---|---|---|
| `IsActive` | `bool` | `true` | Enables or disables the skin and restores previous editor styling when disabled. |
| `ForegroundColor` | `Color` | `#33FF33` | Phosphor foreground and caret color. |
| `BackgroundColor` | `Color` | `#050505` | Terminal background color. |

```xml
<avalonedit:TextEditor>
    <i:Interaction.Behaviors>
        <behaviors:AvalonEditVtTerminalBehavior IsActive="True" />
    </i:Interaction.Behaviors>
</avalonedit:TextEditor>
```

## AvalonTextEditorBindingBehavior

Adds binding-friendly dependency properties to AvalonEdit `TextEditor`.

| Property | Type | Description |
|---|---|---|
| `Text` | `string` | Two-way editor text binding. Avoid for very large documents because AvalonEdit's rope document is converted to strings. |
| `SelectedText` | `string` | Two-way selected text binding. |
| `Selection` | `(int start, int length)` | Two-way selection range binding. |
| `CursorPosition` | `int` | Two-way caret offset binding. |

## AvalonEditPropertiesBehavior

Small property bridge for AvalonEdit runtime styling and hyperlink behavior.

| Property | Type | Description |
|---|---|---|
| `CaretBrush` | `Brush` | Sets `TextArea.Caret.CaretBrush`. |
| `EnableHyperLinks` | `bool` | Enables AvalonEdit hyperlink detection/clicking and disables the Ctrl modifier requirement. |

## AvalonEditCopyBehavior

Attached to a `Button` (not the editor). On click, copies the target `TextEditor`'s selected text to the clipboard, or the entire text if nothing is selected.

| Property | Type | Description |
|---|---|---|
| `TargetTextEditor` | `TextEditor` | The editor to copy from. |

```xml
<Button Content="Copy">
    <i:Interaction.Behaviors>
        <behaviors:AvalonEditCopyBehavior TargetTextEditor="{Binding ElementName=XamlEditor}" />
    </i:Interaction.Behaviors>
</Button>
```

## AvalonEditBlockCaretBehavior

**Base class:** `Behavior<TextEditor>`\
**Namespace:** `Mosaic.UI.Wpf.Behaviors`\
**Source:** `src/Mosaic.UI.Wpf/Behaviors/AvalonEditBlockCaretBehavior.cs`

Attaches to an AvalonEdit `TextEditor` and swaps the default thin line caret for a solid block caret. The `IsEnabled` dependency property controls which caret style is shown and can be toggled at runtime (including via data binding). The character sitting underneath the block is re-drawn on top of it in a contrasting color so that it stays legible.

### API starting points

| Member | Purpose |
|---|---|
| `bool IsEnabled` | Determines whether the block caret is shown in place of the editor's normal caret. Defaults to `true`. |
| `Brush? CaretBrush` | The brush the block caret is filled with. When `null` (the default) the editor's `Control.Foreground` is used. A `SolidColorBrush` is recommended; it is the only brush type the automatic `CaretTextBrush` contrast calculation can inspect. |
| `Brush? CaretTextBrush` | The brush used to re-draw the character the block caret covers. When `null` (the default) a contrasting color is chosen automatically based on the composited color of the block. |
| `double CaretOpacity` | The opacity of the block caret in the range 0.0 to 1.0. Defaults to 0.8. |
| `KnownLayer Layer` | The layer the block caret is drawn on. |
| `void Draw(…)` | Draws the block and, when there is one, the character it covers. |
