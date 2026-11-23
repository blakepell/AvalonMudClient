# ChatThread

**Base class:** `UserControl`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/Chat/ChatThread.xaml.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/ChatThreadExample.xaml`

## Description

A chat conversation control that renders a scrollable list of `Message` objects. Sent and received messages use separate templates (driven by `MessageTemplateSelector` and the `MessageDirection` enum).

## Supporting Types

```csharp
// Message model (in Mosaic.UI.Wpf.Controls)
public class Message
{
    public string Text { get; set; }
    public string Author { get; set; }
    public DateTime Timestamp { get; set; }
    public MessageDirection Direction { get; set; } // Sent | Received
    public ImageSource? Avatar { get; set; }
}

public enum MessageDirection { Sent, Received }
```

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Messages` | `ObservableCollection<Message>` | `null` | The collection of messages to display. Bind to a VM property. |
| `VerticalOffset` | `double` | `0` | Programmatic scroll offset. |

## Key Methods

| Method | Description |
|---|---|
| `ScrollConversationToEnd()` | Scrolls the thread to the latest (bottom) message. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:ChatThread Messages="{Binding Messages}" />
```

## Code-Behind / ViewModel

```csharp
// ViewModel
public ObservableCollection<Message> Messages { get; } = new();

private void AddMessage(string text, MessageDirection dir)
{
    Messages.Add(new Message
    {
        Text = text,
        Author = dir == MessageDirection.Sent ? "You" : "Bot",
        Timestamp = DateTime.Now,
        Direction = dir
    });
}
```

## Notes

- `ChatThread` is a `UserControl` and cannot be re-templated. Style the outer container instead.
- Call `ScrollConversationToEnd()` after adding new messages to keep the latest message in view.
- `TypingProgress` is a companion control (in the same Chat folder) that shows a typing indicator; add it below `ChatThread` for a full chat UI.
