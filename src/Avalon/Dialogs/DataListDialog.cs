using System.Collections;
using System.Windows;
using System.Windows.Controls;
using Avalon.Common;
using Avalon.Controls.Editors;
using Avalon.Models;

namespace Avalon.Dialogs;

/// <summary>
/// The "X Editor" window used to maintain a game's aliases, triggers, variables and the rest.
/// </summary>
/// <remarks>
/// A recreation of Continuum's DataListDialog: a <see cref="DataList"/> over the collection with Add New and OK.
/// Double clicking a row opens the <see cref="ModelEditor"/>.
/// </remarks>
public sealed class DataListDialog : Window
{
    private readonly Game? _profile;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataListDialog"/> class.
    /// </summary>
    /// <param name="items">The collection to edit in place.</param>
    /// <param name="itemType">The model type the columns come from.</param>
    /// <param name="createNew">Creates a new item for Add New, or null to hide the button.</param>
    /// <param name="profile">The profile supplying the available package assignments.</param>
    public DataListDialog(IList items, Type itemType, Func<object?>? createNew, Game? profile = null)
    {
        _profile = profile;
        Style = (Style)Application.Current.FindResource("DialogWindowStyle");
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Title = $"{ModelEditor.PluralName(itemType)} Editor";
        Width = 900;
        Height = 500;
        MinWidth = 500;
        MinHeight = 300;
        ResizeMode = ResizeMode.CanResizeWithGrip;
        if (Application.Current.MainWindow is { IsLoaded: true } owner)
        {
            Owner = owner;
        }

        List = new DataList { Margin = new Thickness(12, 12, 12, 0) };
        List.Bind(items, itemType, profile);
        List.ItemActivated += (_, item) => EditItem(item);

        var footer = new DockPanel { Margin = new Thickness(12), LastChildFill = false };
        if (createNew is not null)
        {
            var add = new Button { Content = "Add New", Style = (Style)FindResource("DialogButtonStyle"), Margin = new Thickness(0) };
            add.Click += (_, _) =>
            {
                if (createNew() is { } item)
                {
                    List.SearchText = "";
                    List.Select(item);
                    ItemsAdded?.Invoke(this, item);
                }
            };
            DockPanel.SetDock(add, Dock.Left);
            footer.Children.Add(add);
        }

        var ok = new Button { Content = "OK", IsDefault = false, Style = (Style)FindResource("DialogButtonStyle") };
        ok.Click += (_, _) => Close();
        DockPanel.SetDock(ok, Dock.Right);
        footer.Children.Add(ok);

        var root = new DockPanel();
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);
        root.Children.Add(List);
        Content = root;
        Loaded += (_, _) => List.FocusSearch();
    }

    /// <summary>
    /// Gets the list hosted by the dialog, for adding type specific menu items.
    /// </summary>
    public DataList List { get; }

    /// <summary>
    /// Occurs when Add New creates an item.
    /// </summary>
    public event EventHandler<object>? ItemsAdded;

    /// <summary>
    /// Gets or sets the action that edits an item, defaulting to the <see cref="ModelEditor"/>.
    /// </summary>
    public Func<object, bool>? Editor { get; set; }

    private void EditItem(object item)
    {
        bool changed = Editor is not null ? Editor(item) : ModelEditor.Edit(item, profile: _profile);
        if (changed)
        {
            List.Refresh();
        }
    }
}
