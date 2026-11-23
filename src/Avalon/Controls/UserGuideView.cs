using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mosaic.UI.Wpf.Controls;

namespace Avalon.Controls;

/// <summary>
/// Shows the built in documentation (Docs\*.md) with Back and Contents buttons, like Continuum's UserGuideView.
/// </summary>
public sealed class UserGuideView : UserControl
{
    private readonly MarkdownViewer _viewer = new();
    private readonly Button _back;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserGuideView"/> class.
    /// </summary>
    public UserGuideView()
    {
        _back = ToolbarButton("", "Back", () =>
        {
            if (_viewer.CanGoBack)
            {
                _viewer.GoBack();
            }
        });
        var contents = ToolbarButton("", "Contents", () => Navigate("Index.md"));

        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(6, 4, 6, 4) };
        toolbar.Children.Add(_back);
        toolbar.Children.Add(contents);

        var root = new DockPanel();
        DockPanel.SetDock(toolbar, Dock.Top);
        root.Children.Add(toolbar);
        root.Children.Add(_viewer);

        Content = root;
    }

    /// <summary>
    /// Shows a documentation page, such as Index.md or Scripting/README.md.
    /// </summary>
    public void Navigate(string page)
    {
        if (page.Equals("UserGuide.md", StringComparison.OrdinalIgnoreCase))
        {
            page = "Index.md";
        }

        _viewer.Source = new Uri($"pack://application:,,,/Docs/{page.TrimStart('/')}", UriKind.Absolute);
    }

    private static Button ToolbarButton(string glyph, string text, Action action)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        panel.Children.Add(new TextBlock { FontFamily = new FontFamily("Segoe MDL2 Assets"), Text = glyph, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) });
        panel.Children.Add(new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center });
        var button = new Button { Content = panel, Padding = new Thickness(8, 3, 8, 3), Margin = new Thickness(0, 0, 4, 0) };
        button.Click += (_, _) => action();
        return button;
    }
}
