# MessageBox

**Kind:** static helper class  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/MessageBox/MessageBox.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/MessageBoxExample.xaml.cs`

## Description

A themed drop-in replacement for `System.Windows.MessageBox`. It mirrors the normal `Show` overloads and uses standard WPF enums: `MessageBoxButton`, `MessageBoxImage`, `MessageBoxResult`, and `MessageBoxOptions`.

## Usage

```csharp
using MessageBox = Mosaic.UI.Wpf.Controls.MessageBox;

MessageBoxResult result = MessageBox.Show(
    "Save changes before closing?",
    "Unsaved Changes",
    MessageBoxButton.YesNoCancel,
    MessageBoxImage.Question,
    MessageBoxResult.Cancel);
```

## Notes

- Automatically chooses the active application window or main window as owner when no owner is supplied.
- Honors the active Mosaic light/dark/high-contrast theme.
