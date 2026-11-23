using System.Windows;
using Mosaic.UI.Wpf.Controls;

namespace Avalon.Controls;

/// <summary>
/// Labels a section of a Ribbon menu and preserves its appearance in overflow menus.
/// </summary>
public sealed class RibbonMenuHeading : RibbonMenuItem
{
    /// <inheritdoc/>
    public override FrameworkElement? CreatePresentation(RibbonPresentationKind kind) =>
        kind == RibbonPresentationKind.Overflow
            ? new RibbonMenuHeading { Header = Header, Style = Style }
            : null;
}
