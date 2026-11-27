using System.Windows;
using Mosaic.UI.Wpf.Controls;

namespace Avalon.Controls;

/// <summary>
/// Raises the AI menu's open event when a mirrored ribbon presentation is opened.
/// </summary>
public class McpRegistrationMenu : RibbonMenuButton
{
    /// <inheritdoc />
    public override FrameworkElement? CreatePresentation(RibbonPresentationKind kind)
    {
        var presentation = base.CreatePresentation(kind);
        if (presentation is RibbonMenuItem submenu)
        {
            submenu.SubmenuOpened += (_, e) =>
            {
                if (e.Source == submenu)
                {
                    RaiseEvent(new RoutedEventArgs(DropDownOpenedEvent, this));
                }
            };
        }
        else if (presentation is RibbonMenuButton menu)
        {
            menu.DropDownOpened += (_, _) => RaiseEvent(new RoutedEventArgs(DropDownOpenedEvent, this));
        }

        return presentation;
    }
}
