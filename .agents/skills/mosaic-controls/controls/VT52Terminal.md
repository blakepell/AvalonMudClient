# VT52Terminal

**Base class:** AvalonEdit `TextEditor`  
**Namespace:** `Mosaic.UI.Wpf.Controls.VT52Terminal`  
**Source:** `src/Mosaic.UI.Wpf/Controls/VT52Terminal/VT52Terminal.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/VT52TerminalExample.xaml`

> **Note:** Despite the name, this is a full xterm-compatible terminal emulator (ANSI / VT100 / VT220 / xterm), **not** a literal VT52 emulator.

## Description

A WPF terminal emulator hosted inside an AvalonEdit `TextEditor`. Supports:
- ANSI escape sequences (colors, cursor movement, erase commands)
- 256-color palette and basic RGB color support  
- DEC line-drawing character set
- Configurable scrollback buffer
- Application cursor / keypad modes
- OSC title sequences
- UTF-8 input/output

Because it extends `TextEditor`, it requires the AvalonEdit assembly reference (`ICSharpCode.AvalonEdit`).

## XAML Namespace

Use the canonical URI with `<mosaic:VT52Terminal>`. A CLR declaration is also available:

```xml
xmlns:vt52="clr-namespace:Mosaic.UI.Wpf.Controls.VT52Terminal;assembly=Mosaic.UI.Wpf"
```

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `LocalEcho` | `bool` | `false` | When `true`, keystrokes are echoed to the display locally (useful for testing). |
| `BackspaceSendsDelete` | `bool` | `true` | When `true`, Backspace sends DEL (`0x7F`); when `false`, it sends BS (`0x08`) for classic BBS compatibility. |
| `ShowLineNumbers` | `bool` | `false` | Inherited from `TextEditor`; shows a line-number gutter. |
| `DisconnectOnUnload` | `bool` | `true` | Automatically signals disconnect when the control is unloaded. |

## Writing to the Terminal

Feed incoming data from a stream or string using the control's write methods:

```csharp
// Write a string (may contain ANSI escape sequences)
terminal.Write("Hello, \x1b[32mWorld\x1b[0m!\r\n");

// Write a byte array (e.g., from a network stream)
terminal.Write(buffer, 0, bytesRead);
```

## Reading User Keystrokes

Subscribe to the `DataReceived` event to get keystrokes the user types:

```csharp
terminal.DataReceived += (sender, data) =>
{
    // Send data to the connected process or network stream
    stream.Write(data, 0, data.Length);
};
```

## XAML Example

```xml
xmlns:vt52="clr-namespace:Mosaic.UI.Wpf.Controls.VT52Terminal;assembly=Mosaic.UI.Wpf"

<vt52:VT52Terminal
    x:Name="Terminal"
    Background="Black"
    Foreground="LightGray"
    FontFamily="Cascadia Mono"
    FontSize="14"
    LocalEcho="False"
    DisconnectOnUnload="True" />
```

## Code-Behind: SSH / Process Integration Pattern

```csharp
// Redirect a Process's standard I/O to the terminal
var psi = new ProcessStartInfo("bash")
{
    RedirectStandardInput = true,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false
};

var process = Process.Start(psi)!;

// Pipe process output → terminal
Task.Run(async () =>
{
    var buffer = new byte[4096];
    int read;
    while ((read = await process.StandardOutput.BaseStream.ReadAsync(buffer)) > 0)
        Dispatcher.Invoke(() => Terminal.Write(buffer, 0, read));
});

// Pipe keystrokes → process
Terminal.DataReceived += (_, data) =>
    process.StandardInput.BaseStream.Write(data, 0, data.Length);
```

## Notes

- `VT52Colorizer` handles 256-color SGR sequences; it is wired automatically.
- `TerminalScrollback` manages the scrollback history; configure its `MaxLines` property to limit memory usage.
- Mouse clicks and text selection use AvalonEdit's selection caret without moving the terminal-controlled block cursor.
- The control is **not** a VT52 emulator despite its name — the name is legacy.
- Use a monospaced font (Cascadia Mono, Consolas, Courier New) for correct rendering of DEC line-drawing characters.
