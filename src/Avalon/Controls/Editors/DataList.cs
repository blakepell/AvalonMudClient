using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Avalon.Common;
using Avalon.Models;
using GongSolutions.Wpf.DragDrop;
using Mosaic.UI.Wpf.Controls;

namespace Avalon.Controls.Editors;

/// <summary>
/// A searchable, sortable list of models whose columns come from <see cref="DataListColumnAttribute"/>.
/// </summary>
/// <remarks>
/// A recreation of the DataList Continuum uses for every maintenance screen: a search box over a grid view,
/// header click sorting, drag to reorder, and a Copy / Paste / Delete context menu.  Double clicking a row raises
/// <see cref="ItemActivated"/>, which the dialog uses to open the record editor.
/// </remarks>
public sealed class DataList : UserControl
{
    private readonly SearchBox _search;
    private readonly ListView _list;
    private readonly GridView _grid = new();
    private readonly DispatcherTimer _searchTimer;
    private readonly List<(string Header, string? Icon, Action<object> Action)> _menuItems = [];
    private ICollectionView? _view;
    private string? _sortProperty;
    private ListSortDirection _sortDirection;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataList"/> class.
    /// </summary>
    public DataList()
    {
        _search = new SearchBox { Margin = new Thickness(0, 0, 0, 6), Height = 28 };
        SetWatermark(_search, "Search");
        _search.TextChanged += (_, _) =>
        {
            _searchTimer!.Stop();
            _searchTimer.Start();
        };

        _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _searchTimer.Tick += (_, _) =>
        {
            _searchTimer.Stop();
            _view?.Refresh();
        };

        _list = new ListView
        {
            View = _grid,
            SelectionMode = SelectionMode.Extended,
            HorizontalContentAlignment = HorizontalAlignment.Stretch
        };
        _list.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(Header_OnClick));
        _list.MouseDoubleClick += List_OnMouseDoubleClick;
        _list.PreviewKeyDown += List_OnPreviewKeyDown;
        _list.ContextMenuOpening += (_, _) => BuildContextMenu();
        _list.ContextMenu = new ContextMenu();
        GongSolutions.Wpf.DragDrop.DragDrop.SetIsDragSource(_list, true);
        GongSolutions.Wpf.DragDrop.DragDrop.SetIsDropTarget(_list, true);
        GongSolutions.Wpf.DragDrop.DragDrop.SetDropHandler(_list, new ReorderDropHandler(this));

        var panel = new DockPanel();
        DockPanel.SetDock(_search, Dock.Top);
        panel.Children.Add(_search);
        panel.Children.Add(_list);
        Content = panel;
    }

    /// <summary>
    /// Occurs when a row is double clicked or Enter is pressed on it.
    /// </summary>
    public event EventHandler<object>? ItemActivated;

    /// <summary>
    /// Occurs after rows are deleted, pasted or reordered.
    /// </summary>
    public event EventHandler? ItemsChanged;

    /// <summary>
    /// Gets the model type the columns are generated from.
    /// </summary>
    public Type? ItemType { get; private set; }

    /// <summary>
    /// Gets the list being edited.
    /// </summary>
    public IList? Items { get; private set; }

    /// <summary>
    /// Gets the first selected item.
    /// </summary>
    public object? SelectedItem => _list.SelectedItem;

    /// <summary>
    /// Gets the search box text.
    /// </summary>
    public string SearchText
    {
        get => _search.Text;
        set => _search.Text = value;
    }

    /// <summary>
    /// Binds the list to <paramref name="items"/>, building columns for <paramref name="itemType"/>.
    /// </summary>
    public void Bind(IList items, Type itemType, Game? profile = null)
    {
        Items = items;
        ItemType = itemType;
        BuildColumns(itemType, profile);
        _list.ItemsSource = items;
        _view = CollectionViewSource.GetDefaultView(items);
        _view.Filter = Filter;
    }

    /// <summary>
    /// Adds a context menu item that runs for the selected row.
    /// </summary>
    public void AddMenuItem(string header, string? icon, Action<object> action) => _menuItems.Add((header, icon, action));

    /// <summary>
    /// Selects and scrolls to <paramref name="item"/>.
    /// </summary>
    public void Select(object item)
    {
        _list.SelectedItem = item;
        _list.ScrollIntoView(item);
    }

    /// <summary>
    /// Re-applies the filter and sort, used after an item changes in the record editor.
    /// </summary>
    public void Refresh() => _view?.Refresh();

    /// <summary>
    /// Moves keyboard focus to the search box.
    /// </summary>
    public void FocusSearch() => _search.Focus();

    private bool Filter(object item)
    {
        var text = _search.Text;
        return string.IsNullOrWhiteSpace(text) || (item.ToString()?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private void BuildColumns(Type type, Game? profile)
    {
        _grid.Columns.Clear();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select((p, index) => (Property: p, Index: index, Column: p.GetCustomAttribute<DataListColumnAttribute>()))
            .Where(x => x.Column is { Visibility: Visibility.Visible })
            .OrderBy(x => x.Column!.Order).ThenBy(x => x.Index);

        foreach (var (property, _, column) in properties)
        {
            var header = new GridViewColumnHeader
            {
                Content = column!.Name ?? property.Name,
                Tag = property.Name,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(6, 2, 6, 2)
            };

            var gridColumn = new GridViewColumn { Header = header, CellTemplate = CreateCellTemplate(property, column, profile) };
            if (column.Width > 0)
            {
                gridColumn.Width = column.Width;
            }

            _grid.Columns.Add(gridColumn);
        }
    }

    private static DataTemplate CreateCellTemplate(PropertyInfo property, DataListColumnAttribute column, Game? profile)
    {
        FrameworkElementFactory factory;
        if (profile is not null && property.Name == nameof(Alias.Package) && property.PropertyType == typeof(string))
        {
            factory = new FrameworkElementFactory(typeof(PackagePicker));
            factory.SetValue(PackagePicker.ProfileProperty, profile);
            factory.SetValue(Control.PaddingProperty, new Thickness(6, 1, 5, 1));
            factory.SetValue(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            factory.SetValue(GongSolutions.Wpf.DragDrop.DragDrop.DragSourceIgnoreProperty, true);
            factory.SetBinding(PackagePicker.PackageNameProperty,
                new Binding(property.Name) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        }
        else if (column.CheckBox && property.PropertyType == typeof(bool))
        {
            factory = new FrameworkElementFactory(typeof(CheckBox));
            factory.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            factory.SetBinding(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty,
                new Binding(property.Name) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        }
        else
        {
            factory = new FrameworkElementFactory(typeof(TextBlock));
            factory.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            var binding = new Binding(property.Name) { Mode = BindingMode.OneWay };
            if (property.PropertyType == typeof(DateTime))
            {
                binding.StringFormat = "g";
            }
            else if (property.PropertyType == typeof(int))
            {
                binding.StringFormat = "N0";
            }
            else if (property.PropertyType == typeof(double))
            {
                binding.StringFormat = "0.##";
            }
            else if (property.PropertyType == typeof(string))
            {
                binding.Converter = FirstLineConverter.Instance;
            }

            factory.SetBinding(TextBlock.TextProperty, binding);
            if (column.Trim)
            {
                factory.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
                factory.SetValue(TextBlock.TextWrappingProperty, TextWrapping.NoWrap);
            }
        }

        return new DataTemplate { VisualTree = factory };
    }

    private void Header_OnClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not GridViewColumnHeader { Tag: string property } || _view is null)
        {
            return;
        }

        _sortDirection = _sortProperty == property && _sortDirection == ListSortDirection.Ascending
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending;
        _sortProperty = property;
        _view.SortDescriptions.Clear();
        _view.SortDescriptions.Add(new SortDescription(property, _sortDirection));
    }

    private void List_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject source && ItemsControl.ContainerFromElement(_list, source) is ListViewItem { DataContext: { } item }
            && source is not CheckBox && !IsInPackagePicker(source))
        {
            ItemActivated?.Invoke(this, item);
        }
    }

    private void List_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.OriginalSource is DependencyObject source && IsInPackagePicker(source))
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Delete:
                DeleteSelected();
                e.Handled = true;
                break;
            case Key.Enter when _list.SelectedItem is { } item:
                ItemActivated?.Invoke(this, item);
                e.Handled = true;
                break;
            case Key.C when Keyboard.Modifiers == ModifierKeys.Control:
                CopySelected();
                e.Handled = true;
                break;
            case Key.V when Keyboard.Modifiers == ModifierKeys.Control:
                Paste();
                e.Handled = true;
                break;
        }
    }

    private static bool IsInPackagePicker(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is PackagePicker)
            {
                return true;
            }

            source = source is Visual ? VisualTreeHelper.GetParent(source) : LogicalTreeHelper.GetParent(source);
        }

        return false;
    }

    private void BuildContextMenu()
    {
        var menu = _list.ContextMenu!;
        menu.Items.Clear();
        bool hasSelection = _list.SelectedItem is not null;

        menu.Items.Add(CreateMenuItem("Edit", "edit-property-48.png", hasSelection, () => ItemActivated?.Invoke(this, _list.SelectedItem!)));
        foreach (var (header, icon, action) in _menuItems)
        {
            menu.Items.Add(CreateMenuItem(header, icon, hasSelection, () => action(_list.SelectedItem!)));
        }

        menu.Items.Add(new Separator());
        menu.Items.Add(CreateMenuItem("Copy", "vs-copy-16.png", hasSelection, CopySelected));
        menu.Items.Add(CreateMenuItem("Paste", null, Clipboard.ContainsText(), Paste));
        menu.Items.Add(new Separator());
        menu.Items.Add(CreateMenuItem("Delete", "vs-delete-16.png", hasSelection, DeleteSelected));
    }

    private static MenuItem CreateMenuItem(string header, string? icon, bool enabled, Action action)
    {
        var item = new MenuItem { Header = header, IsEnabled = enabled };
        if (icon is not null)
        {
            item.Icon = new Image
            {
                Source = new BitmapImage(new Uri($"pack://application:,,,/Assets/{icon}")),
                Width = 16,
                Height = 16
            };
        }
        item.Click += (_, _) => action();
        return item;
    }

    private void CopySelected()
    {
        if (ItemType is null || _list.SelectedItems.Count == 0)
        {
            return;
        }

        var listType = typeof(List<>).MakeGenericType(ItemType);
        var list = (IList)Activator.CreateInstance(listType)!;
        foreach (var item in _list.SelectedItems)
        {
            list.Add(item);
        }

        Clipboard.SetText(JsonSerializer.Serialize(list, listType, new JsonSerializerOptions { WriteIndented = true }));
    }

    private void Paste()
    {
        if (Items is null || ItemType is null || !Clipboard.ContainsText())
        {
            return;
        }

        try
        {
            var listType = typeof(List<>).MakeGenericType(ItemType);
            if (JsonSerializer.Deserialize(Clipboard.GetText(), listType) is not IList pasted)
            {
                return;
            }

            object? last = null;
            foreach (var item in pasted)
            {
                last = item is IEditableModel model ? model.CloneNew() : item;
                Items.Add(last);
            }

            if (last is not null)
            {
                Select(last);
            }

            ItemsChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (JsonException)
        {
            MessageBox.Show(Window.GetWindow(this)!, "The clipboard does not contain items of this type.", "Paste",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void DeleteSelected()
    {
        if (Items is null || _list.SelectedItems.Count == 0)
        {
            return;
        }

        int count = _list.SelectedItems.Count;
        if (MessageBox.Show(Window.GetWindow(this)!, count == 1 ? "Delete the selected item?" : $"Delete the {count} selected items?",
                "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
        {
            return;
        }

        foreach (var item in _list.SelectedItems.Cast<object>().ToList())
        {
            Items.Remove(item);
        }

        ItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    private static void SetWatermark(SearchBox box, string text)
    {
        try
        {
            box.SetValue(SearchBox.WatermarkProperty, text);
        }
        catch (ArgumentException)
        {
            // Older Mosaic versions type the watermark differently; the box still works without it.
        }
    }

    private sealed class FirstLineConverter : IValueConverter
    {
        public static FirstLineConverter Instance { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string text)
            {
                return "";
            }

            int lineBreak = text.AsSpan().IndexOfAny("\r\n\u0085\u2028\u2029");
            return lineBreak < 0 ? text : text[..lineBreak];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }

    /// <summary>
    /// Moves rows within the bound list when they are dragged, ignoring drops from elsewhere.
    /// </summary>
    private sealed class ReorderDropHandler(DataList owner) : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo?.VisualSource == owner._list && owner._sortProperty is null)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (owner.Items is not { } items || dropInfo.Data is null)
            {
                return;
            }

            var moved = (dropInfo.Data as IEnumerable)?.Cast<object>().ToList() ?? [dropInfo.Data];
            if (dropInfo.Data is not IEnumerable || dropInfo.Data is string)
            {
                moved = [dropInfo.Data];
            }

            int target = dropInfo.UnfilteredInsertIndex;
            foreach (var item in moved)
            {
                int index = items.IndexOf(item);
                if (index < 0)
                {
                    continue;
                }

                items.RemoveAt(index);
                if (index < target)
                {
                    target--;
                }

                items.Insert(Math.Clamp(target, 0, items.Count), item);
                target++;
            }

            owner.ItemsChanged?.Invoke(owner, EventArgs.Empty);
        }

        public void DragEnter(IDropInfo dropInfo)
        {
        }

        public void DragLeave(IDropInfo dropInfo)
        {
        }
    }
}
