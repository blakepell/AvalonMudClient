# CommandBox

**Base class:** `TextEditor`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/CommandBox/CommandBox.cs`

A single line command input built on AvalonEdit's `TextEditor`. Pressing Enter raises `CommandExecuted`, Up and Down walk a persistent command history and an optional block caret gives the box a terminal feel.

**Demo:** `src/MosaicWpfDemo/Views/Examples/CommandBoxExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `event EventHandler<CommandExecutedEventArgs> PreviewCommandExecuted` | Raised before a command is dispatched. A handler may rewrite `CommandExecutedEventArgs.Command`, or veto the command outright by setting `RoutedEventArgs.Handled` to `true`, in which case the commit behavior is not applied and nothing is recorded in the history. |
| `event EventHandler<CommandExecutedEventArgs> CommandExecuted` | Raised when the user presses Enter on a command. The commit behavior has already been applied to the box by the time this fires, so a handler is free to push new text into it. |
| `event RoutedEventHandler EscapePressed` | Raised when the user presses Escape, whether or not `ClearOnEscape` cleared the box. A host typically uses this to move focus somewhere else. |
| `CommandBoxCommitBehavior CommitBehavior` | What the box does with the text it just dispatched. Defaults to `CommandBoxCommitBehavior.SelectAll`, which leaves the command visible and selected so the next keystroke replaces it. |
| `bool IsHistoryEnabled` | Whether executed commands are recorded and the arrow keys walk the history. Defaults to `true`. |
| `int MaxHistoryItems` | The maximum number of commands retained by `History`. Zero means unlimited. Defaults to 100. |
| `HistoryDuplicatePolicy HistoryDuplicatePolicy` | Whether a command that repeats an earlier one is recorded again. Defaults to `Controls.HistoryDuplicatePolicy.SkipConsecutive`. |
| `bool UseBlockCaret` | Whether the thin line caret is replaced with a solid block caret. Defaults to `false`. The renderer is installed and removed as this changes, so it can be toggled or bound at runtime. |
| `Brush? BlockCaretBrush` | The brush the block caret is filled with. When `null` (the default) the control's `Control.Foreground` is used, so the caret follows a theme switch without extra wiring. |
| `Brush? BlockCaretTextBrush` | The brush the character under the block caret is re-drawn in. When `null` (the default) black or white is chosen automatically, whichever reads better on the composited block color. |
| `double BlockCaretOpacity` | The opacity of the block caret, from 0.0 to 1.0. Defaults to 0.8. |
| `bool ClearOnEscape` | Whether Escape clears the box. Defaults to `true`. `EscapePressed` is raised either way. |
| `bool SelectAllOnFocus` | Whether the existing text is selected when the box receives keyboard focus. Defaults to `true`. |
| `bool SelectAllOnMouseFocus` | Whether the first click into an unfocused box focuses it and selects everything rather than placing the caret. Defaults to `true`. Subsequent clicks behave normally. |
| `bool IsTabCompletionEnabled` | Whether Tab completes the current text from the most recent matching history entry. Defaults to `true`. When there is no match the key falls through to normal focus navigation. |
| `bool TrimCommand` | Whether leading and trailing whitespace is stripped from a command before it is dispatched. Defaults to `true`. |
| `bool AllowEmptyCommands` | Whether pressing Enter on an empty box dispatches an empty command. Defaults to `false`, which makes a bare Enter do nothing. |
| `bool ShowPrompt` | Whether the `Prompt` glyph is shown at the left edge of the box. The prompt is drawn inside the control's own border and background, and the editable text starts to the right of it. |
| `string Prompt` | The prompt glyph drawn when `ShowPrompt` is `true`. It is decoration only: it is never part of `TextEditor.Text` and never part of the dispatched command. |
| `Brush? PromptBrush` | The brush the `Prompt` is painted with. When `null` (the default) the control's `Control.Foreground` is used. |
| `Thickness PromptPadding` | The space around the `Prompt`: the left value separates it from the border and the right value separates it from where the user starts typing. This sits inside the control's own `Control.Padding`. |
| `string Watermark` | Placeholder text painted behind the box while it is empty. |
| `Brush? WatermarkBrush` | The brush the `Watermark` is painted with. When `null` (the default) the control's `Control.Foreground` at half opacity is used. |
| `ICommand? Command` | An optional command executed after `CommandExecuted` has been raised, for MVVM hosts that would rather not handle a routed event. |
| `object? CommandParameter` | The parameter passed to `Command`. When left `null` the command text is passed instead. |
| `CommandBoxHistory History` | The command history behind the box. Exposed so a host can persist and restore it; storage is not the control's job. |
| `void ImportHistory(…)` | Appends previously persisted commands to `History`, oldest first. |
| `void ClearHistory(…)` | Empties `History` and resets the Up/Down cursor. |
| `bool ExecuteCommand(…)` | Normalizes the current text and dispatches it as a command, exactly as pressing Enter would. |
