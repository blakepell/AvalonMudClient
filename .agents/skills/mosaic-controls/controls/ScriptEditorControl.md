# ScriptEditorControl

**Base class:** `Control`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Scripting/ScriptEditorControl.cs`

Provides a themed, executable JavaScript editor for a document or dock tool window.

### API starting points

| Member | Purpose |
|---|---|
| `ScriptEnvironment? Environment` | Gets or sets the engine and registration context; null disables execution and completion. |
| `string Text` | Gets or sets script text without replacing the host's DataContext. |
| `string? FilePath` | Gets or sets the save destination; saving without one displays a file picker. |
| `object? SaveObject` | Gets or sets the object whose `SaveToProperty` receives the text on save; when both are set, saving writes to it instead of a file. |
| `string? SaveToProperty` | Gets or sets the name of a public, writable string property on `SaveObject` that receives the text on save. |
| `bool IsReadOnly` | Gets or sets whether editing is disabled; execution remains available. |
| `Visibility ToolBarVisibility` | Gets or sets the visibility of save, run and stop actions. |
| `bool StopOnUnload` | Gets or sets whether unloading the control cancels a running script. Hosts that unload content they keep, such as tabbed documents switching tabs, can turn this off and call `Stop` when closing. |
| `bool IsRunning` | Gets whether execution is queued or running. |
| `bool IsModified` | Gets whether text differs from the last loaded or saved version. |
| `Exception? LastError` | Gets the most recent execution or save error. |
| `SyntaxEditor? Editor` | Gets the inner Mosaic editor after its template is applied. |
| `ICommand RunCommand` | Gets the run command (F5). |
| `ICommand StopCommand` | Gets the cancellation command (F6 or Shift+F5). |
| `ICommand SaveCommand` | Gets the save command (Ctrl+S). |
| `ICommand CompletionCommand` | Gets the completion command (Ctrl+Space or Ctrl+period). |
| `ICommand SnippetsCommand` | Gets the snippet command (F1). |
| `ICommand SignatureHelpCommand` | Gets the parameter information command (Ctrl+Shift+Space), which shows the overloads of the call around the caret. |
| `Func<string, CancellationToken, Task>? SaveTextAsync` | Gets or sets an optional application save callback, used instead of file saving. |
| `event RoutedEventHandler Executing` | Occurs before execution, on the UI thread. |
| `event RoutedEventHandler Executed` | Occurs after success, cancellation or failure, once the running state has been reset. |
| `async Task RunAsync(…)` | Runs a snapshot of the current text and propagates errors to the caller. |
| `void Stop(…)` | Requests cooperative cancellation; custom .NET calls must return before execution can stop. |
| `async Task LoadAsync(…)` | Loads a file and marks its contents as unmodified. |
| `async Task SaveAsync(…)` | Saves a text snapshot through the callback, the `SaveObject` property, or the file path, prompting for a path when needed. |
| `void MarkSaved(…)` | Marks the current bound text as the saved baseline. |


## Usage notes

Configure a `ScriptEnvironment` and bind `Text`. F5 runs, F6 or Shift+F5 stops, Ctrl+S saves, Ctrl+Space requests completion, and F1 opens snippets. The control lives in `Mosaic.UI.Wpf.Controls`; its runtime environment lives in `Mosaic.UI.Wpf.Scripting`. Read `src/Mosaic.UI.Wpf/Controls/Scripting/README.md` and `src/Mosaic.UI.Wpf/Scripting/README.md` for hosting and module registration.

## Example

```xml
<mosaic:ScriptEditorControl Environment="{Binding Scripts}"
                            Text="{Binding ScriptText, Mode=TwoWay}" />
```
