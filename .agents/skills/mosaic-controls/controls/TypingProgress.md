# TypingProgress

**Base class:** `UserControl`\
**Namespace:** `Mosaic.UI.Wpf.Controls`\
**Source:** `src/Mosaic.UI.Wpf/Controls/Chat/TypingProgress.xaml.cs`

Represents a control that visually indicates typing progress, typically used in chat or messaging scenarios.

**Demo:** `src/MosaicWpfDemo/Views/Examples/TypingProgressExample.xaml`

### API starting points

| Member | Purpose |
|---|---|
| `bool IsRunning` | Gets or sets a value indicating whether the typing animation is currently running. |
| `void Start(…)` | Starts the operation by setting the running state to active. |
| `void Stop(…)` | Stops the operation and sets the running state to inactive. |
