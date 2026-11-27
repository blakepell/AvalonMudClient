using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;
using Argus.IO;
using Argus.Memory;
using Avalon.Common;
using Avalon.Controls;
using Avalon.Models;
using Avalon.Scripting;
using Avalon.ViewModels;
using Mosaic.UI.Wpf;
using Mosaic.UI.Wpf.AvalonDock;
using Mosaic.UI.Wpf.AvalonDock.Layout;
using Mosaic.UI.Wpf.AvalonDock.Serialization;
using Mosaic.UI.Wpf.Common;
using Mosaic.UI.Wpf.Controls;
using Mosaic.UI.Wpf.Logging;
using Mosaic.UI.Wpf.Themes;

namespace Avalon
{
    /// <summary>
    /// The main window for Avalon.
    /// </summary>
    public partial class MainWindow
    {
        private const string GameDocumentIdPrefix = "game:";
        private const string VitalsIdPrefix = "vitals:";

        // AvalonDock exposes TabBackground as a CLR property, so bind through a dependency property.
        private static readonly DependencyProperty GameTabBackgroundProperty = DependencyProperty.RegisterAttached(
            "GameTabBackground", typeof(Brush), typeof(MainWindow),
            new PropertyMetadata(null, (target, args) => ((LayoutDocument)target).TabBackground = (Brush?)args.NewValue));
        private static readonly string MainWindowType = typeof(MainWindow).FullName!;

        private readonly Dictionary<Game, LayoutDocument> _terminalDocuments = new();
        private readonly Dictionary<string, LayoutAnchorable> _toolWindows = new();
        private readonly Dictionary<string, string> _scriptTerminalGroups = new();
        private PropertyGrid? _propertyGrid;

        public MainWindow()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                RestoreWindowSize();
            }
#if DEBUG
            AddDebugRibbonTab();
#endif
            // Mosaic can mark the tab selected without updating the Backstage's selected page.
            Backstage.AddHandler(Selector.SelectedEvent, new RoutedEventHandler(BackstagePage_OnSelected), handledEventsToo: true);
            GameLibrary.AddHandler(GameCard.ConnectRequestedEvent, new RoutedEventHandler(GameCard_OnConnectRequested));
            Closing += MainWindow_OnClosing;
            Closed += MainWindow_OnClosed;
        }

#if DEBUG
        private void AddDebugRibbonTab()
        {
            var icon = new System.Windows.Media.Imaging.BitmapImage(
                new Uri("pack://application:,,,/Assets/package-refresh-48.png"));
            var import = new RibbonButton
            {
                Key = "ImportContinuumPackage",
                Label = "Import Continuum Package",
                LargeImage = icon,
                SmallImage = icon,
                Command = AppCommands.ImportContinuumPackageCommand
            };
            RibbonItem.SetVariantBehavior(import, ItemVariantBehavior.AlwaysLarge);
            RibbonKeyTipService.SetKeyTip(import, "I");

            var group = new RibbonGroup { Header = "Packages" };
            group.Items.Add(import);
            var tab = new RibbonTabItem { Header = "Debug" };
            RibbonKeyTipService.SetKeyTip(tab, "D");
            tab.Items.Add(group);
            Ribbon.Items.Add(tab);
        }
#endif

        /// <summary>
        /// Gets or sets whether closing skips saving the settings (Exit without saving Settings).
        /// </summary>
        public bool SkipSaveOnExit { get; set; }

        private AppViewModel ViewModel => (AppViewModel)DataContext;

        private WindowViewState? FindWindowViewState() => ViewModel.AppSettings.WindowViewStates?.FirstOrDefault(state =>
            state is not null && state.WindowType == MainWindowType &&
            string.Equals(state.MachineName, Environment.MachineName, StringComparison.OrdinalIgnoreCase));

        private void RestoreWindowSize()
        {
            var state = FindWindowViewState();
            if (state is not null)
            {
                if (double.IsFinite(state.Width) && state.Width > 0)
                {
                    Width = state.Width;
                }

                if (double.IsFinite(state.Height) && state.Height > 0)
                {
                    Height = state.Height;
                }
            }

            // CenterScreen uses the monitor under the mouse. WorkArea belongs to the primary monitor.
            var workArea = SystemParameters.WorkArea;
            Width = Math.Min(Width, workArea.Width);
            Height = Math.Min(Height, workArea.Height);
            Left = workArea.Left + (workArea.Width - Width) / 2;
            Top = workArea.Top + (workArea.Height - Height) / 2;
        }

        private void SaveWindowSize()
        {
            var bounds = RestoreBounds;
            if (bounds.IsEmpty || bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            var state = FindWindowViewState();
            if (state is null)
            {
                state = new WindowViewState { MachineName = Environment.MachineName, WindowType = MainWindowType };
                (ViewModel.AppSettings.WindowViewStates ??= []).Add(state);
            }

            state.Width = bounds.Width;
            state.Height = bounds.Height;
            state.WindowState = WindowState.Normal;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_OnLoaded;
            ToastManager.Initialize(WindowContent);
            RestoreLayout();
        }

        private async void GameCard_OnConnectRequested(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is not GameCard { DataContext: Game game })
            {
                return;
            }

            e.Handled = true;
            var terminal = OpenTerminal(game, autoConnect: false);
            Ribbon.CloseBackstage();
            terminal.FocusInput();
            await terminal.ConnectAsync();
        }

        /// <summary>
        /// Opens or activates the terminal for a game, honoring Auto Connect when opening a new tab.
        /// </summary>
        public MudTerminal OpenTerminal(Game game) => OpenTerminal(game, autoConnect: true);

        /// <summary>
        /// Returns the open terminal for a game, or null when the game has no tab open.
        /// </summary>
        public MudTerminal? FindTerminal(Game game) =>
            _terminalDocuments.TryGetValue(game, out var document) ? document.Content as MudTerminal : null;

        private MudTerminal OpenTerminal(Game game, bool autoConnect)
        {
            var created = false;
            if (!_terminalDocuments.TryGetValue(game, out var document))
            {
                game.Id ??= Guid.NewGuid();
                document = new LayoutDocument { ContentId = $"{GameDocumentIdPrefix}{game.Id:D}", CanFloat = true };
                document.Content = CreateGameTerminal(game, document);
                _terminalDocuments.Add(game, document);
                DocumentPane.Children.Add(document);
                created = true;
            }

            document.IsSelected = document.IsActive = true;
            var active = (MudTerminal)document.Content!;
            SetActiveTerminal(active);
            if (created && autoConnect)
            {
                _ = AutoConnectAsync(active);
            }

            return active;
        }

        private static async Task AutoConnectAsync(MudTerminal terminal)
        {
            if (!terminal.Game.AutoConnect)
            {
                return;
            }

            try
            {
                await terminal.ConnectAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Could not auto-connect to {terminal.Game.Address}. {ex.Message}");
            }
        }

        private MudTerminal CreateGameTerminal(Game game, LayoutDocument document)
        {
            var terminal = new MudTerminal(game, ViewModel);
            BindingOperations.SetBinding(document, LayoutContent.TitleProperty, new Binding(nameof(Game.Name))
            {
                Source = game,
                TargetNullValue = "Game"
            });
            BindingOperations.SetBinding(document, GameTabBackgroundProperty, new Binding(nameof(Game.TabColor)) { Source = game });

            terminal.TitleRequested += (_, title) => document.Title = title;
            document.IsSelectedChanged += (_, _) =>
            {
                if (document.IsSelected)
                {
                    SetActiveTerminal(terminal);
                }
            };
            document.Closed += (_, _) =>
            {
                _terminalDocuments.Remove(game);
                terminal.Dispose();
                if (ReferenceEquals(ViewModel.ActiveTerminal, terminal))
                {
                    SetActiveTerminal(_terminalDocuments.Values.FirstOrDefault(item => item.IsSelected)?.Content as MudTerminal
                        ?? _terminalDocuments.Values.LastOrDefault()?.Content as MudTerminal);
                }
                if (!SkipSaveOnExit)
                {
                    AppCommands.Save();
                }
            };

            return terminal;
        }

        /// <summary>
        /// Gets the pane documents open in, which can change when a saved layout is restored.
        /// </summary>
        private LayoutDocumentPane DocumentPane
        {
            get
            {
                var pane = WorkspaceDock.Layout.RootPanel.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
                if (pane is not null)
                {
                    return pane;
                }

                pane = new LayoutDocumentPane();
                WorkspaceDock.Layout.RootPanel.Children.Add(pane);
                return pane;
            }
        }

        /// <summary>
        /// Keeps the status bar on the game document that is showing. Tool windows becoming active
        /// leave the last game terminal in place, like Continuum.
        /// </summary>
        private void WorkspaceDock_OnActiveContentChanged(object? sender, EventArgs e)
        {
            if (WorkspaceDock.ActiveContent is MudTerminal terminal)
            {
                SetActiveTerminal(terminal);
            }
        }

        private void SetActiveTerminal(MudTerminal? terminal)
        {
            if (ReferenceEquals(ViewModel.ActiveTerminal, terminal))
            {
                return;
            }

            ViewModel.ActiveTerminal = terminal;
            ViewModel.StatusText = terminal?.Status ?? "Ready";
        }

        /// <summary>
        /// Opens the Backstage game library, optionally starting a new game.
        /// </summary>
        public async void ShowGameLibrary(bool newGame)
        {
            Backstage.SetCurrentValue(Selector.SelectedIndexProperty, 0);
            Ribbon.OpenBackstage();
            if (newGame)
            {
                await GameLibrary.NewGameAsync();
            }
        }

        /// <summary>
        /// Collapses or expands the ribbon.
        /// </summary>
        public void ToggleRibbon()
        {
            ViewModel.AppSettings.IsRibbonMinimized = !ViewModel.AppSettings.IsRibbonMinimized;
            ApplyRibbonMinimized();
        }

        private void ApplyRibbonMinimized()
        {
            var property = Ribbon.GetType().GetProperty("IsMinimized", BindingFlags.Public | BindingFlags.Instance);
            if (property?.CanWrite == true)
            {
                property.SetValue(Ribbon, ViewModel.AppSettings.IsRibbonMinimized);
            }
            else
            {
                Ribbon.Visibility = ViewModel.AppSettings.IsRibbonMinimized ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        /// <summary>
        /// Connects the game tab that is showing, or opens the game library to pick one when no game
        /// is open.
        /// </summary>
        private async void Connect_OnClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ActiveTerminal is not { } terminal)
            {
                ShowGameLibrary(newGame: false);
                return;
            }

            OpenTerminal(terminal.Game, autoConnect: false);
            terminal.FocusInput();
            await terminal.ConnectAsync();
        }

        private void Disconnect_OnClick(object sender, RoutedEventArgs e) => ViewModel.ActiveTerminal?.Disconnect();

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button { ContextMenu: { } menu } button)
            {
                menu.PlacementTarget = button;
                menu.Placement = PlacementMode.Bottom;
                menu.IsOpen = true;
            }
        }

        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            if (SkipSaveOnExit)
            {
                return;
            }

            SaveWindowSize();
            SaveLayout();
            foreach (var document in _terminalDocuments.Values)
            {
                (document.Content as MudTerminal)?.Dispose();
            }

            _terminalDocuments.Clear();
            DbScriptCommands.Instance.FlushSynchronously();
            AppCommands.Save();
        }

        private void MainWindow_OnClosed(object? sender, EventArgs e)
        {
            foreach (var document in _terminalDocuments.Values)
            {
                (document.Content as MudTerminal)?.Dispose();
            }

            _terminalDocuments.Clear();
        }

        private void BackstagePage_OnSelected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is RibbonBackstageTabItem page && Backstage.Items.Contains(page) && Backstage.SelectedItem != page)
            {
                Backstage.SetCurrentValue(Selector.SelectedItemProperty, page);
            }
        }

        private void InfoLink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            e.Handled = true;

            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show(ex.Message, "Open Link", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ------------------------------------------------------------------------------------------- Tool windows

        /// <summary>
        /// Shows a tool window, creating it on first use.
        /// </summary>
        public void ShowToolWindow(string id)
        {
            if (!_toolWindows.TryGetValue(id, out var pane))
            {
                pane = CreateToolWindow(id);
                if (pane is null)
                {
                    return;
                }

                _toolWindows[id] = pane;
                if (pane.Content is TerminalWindow)
                {
                    // Named terminals share one pane under the documents, like Continuum's Communication window.
                    var existing = _toolWindows.Values.FirstOrDefault(p => p != pane && p.Content is TerminalWindow
                        && p.ContentId is { } contentId && !_scriptTerminalGroups.ContainsKey(contentId)
                        && p.Parent is LayoutAnchorablePane);
                    if (existing?.Parent is LayoutAnchorablePane shared)
                    {
                        shared.Children.Add(pane);
                    }
                    else
                    {
                        pane.AddToLayout(WorkspaceDock, AnchorableShowStrategy.Bottom);
                        if (pane.Parent is LayoutAnchorablePane bottom)
                        {
                            bottom.DockHeight = new GridLength(220);
                        }
                    }
                }
                else
                {
                    pane.AddToLayout(WorkspaceDock, AnchorableShowStrategy.Right);
                    if (pane.Parent is LayoutAnchorablePane parent)
                    {
                        parent.DockWidth = new GridLength(id == ToolWindows.Log ? 480 : 340);
                    }
                }
            }
            else if (pane.IsHidden)
            {
                pane.Show();
            }

            pane.IsSelected = true;
            pane.IsActive = true;
        }

        /// <summary>
        /// The prefix that marks a named terminal's tool window id.
        /// </summary>
        public const string TerminalIdPrefix = "terminal:";

        /// <summary>
        /// Returns the named terminal tool window, or null when it doesn't exist.
        /// </summary>
        public TerminalWindow? FindTerminalWindow(string name)
            => _toolWindows.TryGetValue(TerminalIdPrefix + name, out var pane) ? pane.Content as TerminalWindow : null;

        /// <summary>
        /// Gets the names of the terminal tool windows.
        /// </summary>
        public IEnumerable<string> TerminalWindowNames
            => _toolWindows.Keys.Where(id => id.StartsWith(TerminalIdPrefix, StringComparison.Ordinal)).Select(id => id[TerminalIdPrefix.Length..]);

        /// <summary>
        /// Shows the named terminal tool window, creating it when it doesn't exist.
        /// </summary>
        /// <param name="name">The terminal's name, which scripts use to write to it.</param>
        /// <param name="activate">Whether to select and focus the window.</param>
        public TerminalWindow ShowTerminalWindow(string name, bool activate = true)
        {
            var id = TerminalIdPrefix + name;
            var previous = WorkspaceDock.ActiveContent;
            ShowToolWindow(id);
            var terminal = (TerminalWindow)_toolWindows[id].Content;
            if (activate)
            {
                terminal.ScrollToEnd();
                NotificationReset(id);
            }
            else if (previous is not null && !ReferenceEquals(previous, terminal))
            {
                WorkspaceDock.ActiveContent = previous;
            }

            return terminal;
        }

        internal void AddScriptTerminal(string name, string containerName, bool autoHide, AnchorSide side)
        {
            var id = TerminalIdPrefix + name;
            containerName ??= "";
            if (_toolWindows.ContainsKey(id))
            {
                _scriptTerminalGroups.TryAdd(id, containerName);
                ActivateScriptTerminal(name, focus: false);
                FindTerminalWindow(name)?.ScrollToEnd();
                return;
            }

            var pane = CreateTerminalToolWindow(id);
            _toolWindows.Add(id, pane);
            _scriptTerminalGroups.Add(id, containerName);
            var group = _scriptTerminalGroups
                .Where(entry => entry.Key != id && entry.Value == containerName)
                .Select(entry => _toolWindows[entry.Key].Parent)
                .FirstOrDefault(parent => parent is LayoutAnchorablePane or LayoutAnchorGroup);

            if (group is LayoutAnchorablePane docked)
            {
                docked.Children.Add(pane);
            }
            else if (group is LayoutAnchorGroup hidden)
            {
                hidden.Children.Add(pane);
            }
            else
            {
                var layout = WorkspaceDock.Layout;
                var orientation = side is AnchorSide.Left or AnchorSide.Right ? Orientation.Horizontal : Orientation.Vertical;
                if (layout.RootPanel.Orientation != orientation)
                {
                    layout.RootPanel = new LayoutPanel(layout.RootPanel) { Orientation = orientation };
                }

                var newGroup = new LayoutAnchorablePane(pane);
                int index = side is AnchorSide.Left or AnchorSide.Top ? 0 : layout.RootPanel.Children.Count;
                layout.RootPanel.Children.Insert(index, newGroup);
            }

            if (autoHide && !pane.IsAutoHidden)
            {
                pane.ToggleAutoHide();
            }
        }

        internal void ActivateScriptTerminal(string id, bool focus)
        {
            var document = _terminalDocuments.Values.FirstOrDefault(item =>
                string.Equals(item.ContentId, id, StringComparison.Ordinal)
                || string.Equals(item.Title, id, StringComparison.Ordinal));
            if (document is not null)
            {
                document.IsSelected = true;
                if (focus)
                {
                    document.IsActive = true;
                    (document.Content as MudTerminal)?.FocusInput();
                }
                return;
            }

            string name = TerminalWindow.NormalizeName(id);
            if (name.Length == 0)
            {
                return;
            }

            if (!_toolWindows.TryGetValue(id, out var pane) && !_toolWindows.TryGetValue(TerminalIdPrefix + name, out pane))
            {
                ShowTerminalWindow(name, activate: focus);
                return;
            }

            if (pane.IsHidden)
            {
                pane.Show();
            }
            pane.IsSelected = true;
            if (focus)
            {
                pane.IsActive = true;
            }
        }

        private void NotificationReset(string id)
        {
            if (_toolWindows.TryGetValue(id, out var pane) && pane.Content is TerminalWindow terminal)
            {
                pane.Title = terminal.TerminalName;
            }
        }

        private LayoutAnchorable CreateTerminalToolWindow(string id, LayoutAnchorable? pane = null)
        {
            var name = id[TerminalIdPrefix.Length..];
            var terminal = new TerminalWindow(name, ViewModel.AppSettings);
            pane ??= new LayoutAnchorable { ContentId = id };
            pane.Title = name;
            pane.Content = terminal;
            terminal.NotificationCountChanged += (_, _) =>
                pane.Title = terminal.NotificationCount > 0 ? $"{name} ({terminal.NotificationCount})" : name;
            return pane;
        }

        /// <summary>
        /// Shows an object in the Properties window.
        /// </summary>
        public void ShowProperties(object item)
        {
            ShowToolWindow(ToolWindows.Properties);
            if (_propertyGrid is not null)
            {
                _propertyGrid.Object = item;
            }
        }

        internal void ShowVitalsPanel(string name)
        {
            var id = VitalsIdPrefix + name.Trim();
            ShowToolWindow(FindVitalsPane(id)?.ContentId ?? id);
        }

        internal VitalsPanel? FindVitalsPanel(string? name) =>
            string.IsNullOrWhiteSpace(name) ? null : FindVitalsPane(VitalsIdPrefix + name.Trim())?.Content as VitalsPanel;

        private LayoutAnchorable? FindVitalsPane(string id) => _toolWindows.Values.FirstOrDefault(pane =>
            string.Equals(pane.ContentId, id, StringComparison.OrdinalIgnoreCase));

        private LayoutAnchorable? CreateToolWindow(string id, LayoutAnchorable? pane = null)
        {
            if (id.StartsWith(TerminalIdPrefix, StringComparison.Ordinal) && id.Length > TerminalIdPrefix.Length)
            {
                return CreateTerminalToolWindow(id, pane);
            }

            if (id.StartsWith(VitalsIdPrefix, StringComparison.Ordinal) && id.Length > VitalsIdPrefix.Length)
            {
                var name = id[VitalsIdPrefix.Length..];
                var settings = ViewModel.AppSettings;
                var key = name.ToUpperInvariant();
                var labels = settings.VitalsPanelLabels?.GetValueOrDefault(key) ?? ["HP", "Mana", "Move"];
                pane ??= new LayoutAnchorable { ContentId = id };
                pane.Title = name;
                pane.Content = new VitalsPanel(labels, updated => (settings.VitalsPanelLabels ??= [])[key] = updated);
                pane.Closed += (_, _) => _toolWindows.Remove(id);
                return pane;
            }

            var (title, content) = id switch
            {
                ToolWindows.Properties => ("Properties", (object)(_propertyGrid = CreatePropertyGrid(ViewModel.ActiveTerminal?.Game))),
                ToolWindows.Variables => ("Variables", new VariableRepeater { DataContext = ViewModel }),
                ToolWindows.AliasButtons => ("Alias Button Actions", new AliasButtonsRepeater { DataContext = ViewModel }),
                ToolWindows.Movement => ("Movement", new MovementView { DataContext = ViewModel }),
                ToolWindows.Notes => ("Notes", CreateNotesEditor()),
                ToolWindows.ColorTheme => ("Terminal Colors", TemplateHost("TerminalColorsEditor")),
                ToolWindows.Settings => ("Settings", CreatePropertyGrid(ViewModel.AppSettings)),
                ToolWindows.Log => ("Log Viewer", new LogViewer
                {
                    IpAddressVisible = false,
                    UsernameVisible = false,
                    CountVisible = false,
                    DateTimeVisible = false,
                    TimeVisible = true
                }),
                _ => ("", null)
            };

            if (content is null)
            {
                return null;
            }

            pane ??= new LayoutAnchorable { ContentId = id };
            pane.Title = title;
            pane.Content = content;
            return pane;
        }

        private PropertyGrid CreatePropertyGrid(object? target)
        {
            var grid = new PropertyGrid();
            grid.Resources.Add("TextEditor", Resources["PropertyGridTextEditor"]);
            grid.Object = target;
            return grid;
        }

        private void EditScriptProperty_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: PropertyDescriptor property, CommandParameter: { } target })
            {
                var title = target is Game game ? $"{game.Name}: {property.DisplayName}" : property.DisplayName;
                OpenScriptDocument(target, property.Name, title);
            }
        }

        private object CreateNotesEditor()
        {
            var editor = new MarkdownEditor
            {
                SaveObject = ViewModel.AppSettings,
                SaveToProperty = nameof(AppSettings.Notes)
            };
            editor.Text = ViewModel.AppSettings.Notes ?? "";
            return editor;
        }

        private ContentControl TemplateHost(string templateKey) => new()
        {
            Content = DataContext,
            ContentTemplate = (DataTemplate)Resources[templateKey],
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };

        // ------------------------------------------------------------------------------------------- Documents

        /// <summary>
        /// Opens the application and local settings in read-only JSON document tabs.
        /// </summary>
        internal void OpenSettingsDocuments()
        {
            Ribbon.CloseBackstage();
            App.SaveAppSettings();

            var appDocument = AppServices.IsRegistered<JsonFileService>()
                ? OpenSettingsDocument(Path.Combine(AppServices.GetRequiredService<JsonFileService>().FolderPath, "AppSettings.json"))
                : null;
            var localDocument = OpenSettingsDocument(Path.Combine(AppPaths.AppDataFolder, "LocalSettings.json"));

            if ((appDocument ?? localDocument) is { } document)
            {
                document.IsSelected = document.IsActive = true;
            }
        }

        private LayoutDocument? OpenSettingsDocument(string path)
        {
            var title = Path.GetFileName(path);
            string text;
            try
            {
                text = File.ReadAllText(path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(ex.Message, $"View {title}", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            var existing = WorkspaceDock.Layout.Descendents().OfType<LayoutDocument>()
                .FirstOrDefault(d => string.Equals(d.ContentId, path, StringComparison.OrdinalIgnoreCase));
            if (existing?.Content is SyntaxEditor editor)
            {
                if (editor.Text != text)
                {
                    editor.Text = text;
                }

                return existing;
            }

            var document = new LayoutDocument
            {
                Title = title,
                ContentId = path,
                ToolTip = path,
                Content = new SyntaxEditor { Language = SyntaxLanguage.Json, IsReadOnly = true, Text = text }
            };
            DocumentPane.Children.Add(document);
            return document;
        }

        /// <summary>
        /// Opens a JavaScript document.  With a target, Save writes the code back to that object's property;
        /// without one it is the Interactive Script Editor.
        /// </summary>
        public void OpenScriptDocument(object? target, string? property, string title, Game? profile = null)
        {
            Ribbon.CloseBackstage();
            if (target is not null)
            {
                var existing = WorkspaceDock.Layout.Descendents().OfType<LayoutDocument>()
                    .FirstOrDefault(d => d.Content is ScriptEditorControl editor &&
                        ReferenceEquals(editor.SaveObject, target) && editor.SaveToProperty == property);
                if (existing is not null)
                {
                    existing.IsSelected = existing.IsActive = true;
                    return;
                }
            }

            var scriptProperty = target is not null && property is not null ? TypeDescriptor.GetProperties(target)[property] : null;
            profile ??= target as Game;
            var interpreter = profile is null ? ViewModel.ActiveTerminal?.Interpreter :
                _terminalDocuments.TryGetValue(profile, out var profileDocument) && profileDocument.Content is MudTerminal terminal
                    ? terminal.Interpreter
                    : null;
            var control = new ScriptEditorControl
            {
                Text = scriptProperty?.GetValue(target) as string ?? "",
                Environment = interpreter?.Environment ?? (profile is null
                    ? ScriptEnvironmentFactory.ForActiveGame()
                    : ScriptEnvironmentFactory.CreateForProfile(profile)),
                SaveObject = target,
                SaveToProperty = property
            };
            control.MarkSaved();
            control.Executing += (_, _) => AdjustScriptsActive(1);
            control.Executed += (_, _) => AdjustScriptsActive(-1);

            var document = new LayoutDocument { Title = title, Content = control };
            if (target is null)
            {
                control.SaveTextAsync = async (text, token) =>
                {
                    var path = WpfUtilities.SaveFileRequest("Save Script", "JavaScript (*.js)|*.js|All files (*.*)|*.*", AppPaths.DocumentsFolder);
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        await File.WriteAllTextAsync(path, text, token);
                    }
                };
            }
            else if (scriptProperty is not null)
            {
                control.SaveTextAsync = (text, _) =>
                {
                    scriptProperty.SetValue(target, text);
                    if (target is LocalSettings)
                    {
                        App.SaveClientSettings();
                    }
                    else
                    {
                        AppCommands.Save();
                    }

                    // Source definitions are cached by the terminal. Event scripts are read when the event fires.
                    if (target is SourceCode && profile is not null &&
                        _terminalDocuments.TryGetValue(profile, out var ownerDocument) && ownerDocument.Content is MudTerminal ownerTerminal)
                    {
                        ownerTerminal.Interpreter.RefreshScripts();
                    }
                    return Task.CompletedTask;
                };
            }

            DependencyPropertyDescriptor.FromProperty(ScriptEditorControl.IsModifiedProperty, typeof(ScriptEditorControl))
                .AddValueChanged(control, (_, _) => document.Title = control.IsModified ? $"{title} *" : title);
            DocumentPane.Children.Add(document);
            document.IsActive = true;

            void AdjustScriptsActive(int change)
            {
                if (interpreter is not null)
                {
                    interpreter.AdjustScriptsActive(change);
                }
                else
                {
                    ViewModel.StandaloneScriptsActive += change;
                }
            }
        }

        /// <summary>
        /// Opens a SQLite query document over the script database.
        /// </summary>
        public void OpenSqliteDocument()
        {
            DbScriptCommands.Instance.FlushSynchronously();
            var control = new SqliteQueryControl { ConnectionString = DbScriptCommands.ConnectionString };
            var document = new LayoutDocument { Title = "SQLite Query Editor", Content = control };
            document.Closed += (_, _) => control.Dispose();
            DocumentPane.Children.Add(document);
            document.IsActive = true;
        }

        /// <summary>
        /// Opens one of the embedded documentation pages.
        /// </summary>
        public void OpenDocumentation(string name)
        {
            var existing = WorkspaceDock.Layout.Descendents().OfType<LayoutDocument>().FirstOrDefault(d => d.ContentId == "docs");
            var viewer = existing?.Content as UserGuideView ?? new UserGuideView();
            viewer.Navigate(name);
            if (existing is null)
            {
                existing = new LayoutDocument
                {
                    Title = "User Guide",
                    ContentId = "docs",
                    Content = viewer,
                    TabBackground = new SolidColorBrush(Color.FromRgb(16, 124, 16))
                };
                DocumentPane.Children.Add(existing);
            }
            existing.IsActive = true;
        }

        // ------------------------------------------------------------------------------------------- Layout

        /// <summary>
        /// Restores the default layout with the Variables window on the right.
        /// </summary>
        public void ResetLayout()
        {
            foreach (var pane in _toolWindows.Values.ToList())
            {
                pane.Hide();
            }

            ShowToolWindow(ToolWindows.Variables);
        }

        private void SaveLayout()
        {
            ViewModel.AppSettings.OpenToolWindows = _toolWindows.Where(pair => !pair.Value.IsHidden && pair.Value.Parent is not null)
                .Select(pair => pair.Key).ToList();

            try
            {
                var folder = Directory.CreateDirectory(AppPaths.AppDataFolder);
                var tempPath = Path.Combine(folder.FullName, Path.GetRandomFileName());
                try
                {
                    new JsonLayoutSerializer(WorkspaceDock).Serialize(tempPath);
                    File.Move(tempPath, DockLayoutPath, overwrite: true);
                }
                finally
                {
                    File.Delete(tempPath);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or ArgumentException or InvalidOperationException or NotSupportedException)
            {
                Logger.LogError($"Could not save the dock layout. {ex.Message}");
            }
        }

        private static string DockLayoutPath => Path.Combine(AppPaths.AppDataFolder, "DockLayout.json");

        private void RestoreLayout()
        {
            ApplyRibbonMinimized();
            if (TryRestoreDockLayout())
            {
                return;
            }

            var open = ViewModel.AppSettings.OpenToolWindows;
            if (open is null)
            {
                ResetLayout();
                return;
            }

            foreach (var id in open)
            {
                ShowToolWindow(id);
            }
        }

        private bool TryRestoreDockLayout()
        {
            var defaultLayout = WorkspaceDock.Layout;
            var serializer = new JsonLayoutSerializer(WorkspaceDock);
            serializer.LayoutSerializationCallback += (_, args) =>
            {
                // Temporary documents (such as script editors) have no content to restore.
                if (args.Model is LayoutAnchorable { ContentId: { } toolId } pane && !_toolWindows.ContainsKey(toolId)
                    && CreateToolWindow(toolId, pane) is not null)
                {
                    _toolWindows.Add(toolId, pane);
                    args.Content = pane.Content;
                }
                else if (args.Model is LayoutDocument { ContentId: { } documentId } document
                    && documentId.StartsWith(GameDocumentIdPrefix, StringComparison.Ordinal)
                    && Guid.TryParse(documentId.AsSpan(GameDocumentIdPrefix.Length), out var gameId)
                    && ViewModel.AppSettings.Games.FirstOrDefault(game => game.Id == gameId) is { } game
                    && !_terminalDocuments.ContainsKey(game))
                {
                    args.Content = CreateGameTerminal(game, document);
                    _terminalDocuments.Add(game, document);
                }
                else
                {
                    args.Cancel = true;
                }
            };

            try
            {
                serializer.Deserialize(DockLayoutPath);
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or ArgumentException or InvalidOperationException or NotSupportedException)
            {
                WorkspaceDock.Layout = defaultLayout;
                foreach (var document in _terminalDocuments.Values)
                {
                    (document.Content as MudTerminal)?.Dispose();
                }

                _terminalDocuments.Clear();
                _toolWindows.Clear();
                _scriptTerminalGroups.Clear();
                SetActiveTerminal(null);
                _propertyGrid = null;
                Logger.LogError($"Could not restore the dock layout. {ex.Message}");
                return false;
            }

            var lastGame = _terminalDocuments.Values.FirstOrDefault(document => document.IsLastFocusedDocument)
                ?? _terminalDocuments.Values.MaxBy(document => document.LastActivationTimeStamp);
            SetActiveTerminal(lastGame?.Content as MudTerminal);
            foreach (var terminal in _terminalDocuments.Values.Select(document => document.Content).OfType<MudTerminal>().ToArray())
            {
                _ = AutoConnectAsync(terminal);
            }

            return true;
        }

        // ------------------------------------------------------------------------------------------- Theme / appearance

        private void ButtonToggleTheme_OnClick(object sender, RoutedEventArgs e)
        {
            var theme = AppServices.GetRequiredService<ThemeManager>();
            theme.CycleTheme();

            if (AppServices.IsRegistered<AppSettings>())
            {
                AppServices.GetRequiredService<AppSettings>().Theme = theme.Theme;
            }
        }

        private void SetTheme_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is RibbonMenuItem { Tag: MosaicThemeMode mode })
            {
                AppServices.GetRequiredService<ThemeManager>().Theme = mode;
                AppServices.GetRequiredService<AppSettings>().Theme = mode;
            }
        }

        private void AppearanceButton_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not RibbonMenuButton button)
            {
                return;
            }

            button.ApplyTemplate();
            // Mosaic uses separate template content for the large and inline chevrons.
            var largeContentName = button is RibbonSplitButton ? "LargeToggleContent" : "LargeLayout";
            if (button.Template.FindName(largeContentName, button) is StackPanel largeContent &&
                largeContent.Children.OfType<System.Windows.Shapes.Path>().FirstOrDefault() is { } chevron)
            {
                chevron.Margin = new Thickness(0, 5, 0, chevron.Margin.Bottom);
            }
        }

        private void SetRibbonLayout_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is RibbonMenuItem { Tag: RibbonLayoutMode layout })
            {
                Ribbon.SetCurrentValue(Mosaic.UI.Wpf.Controls.Ribbon.LayoutModeProperty, layout);
            }
        }

        private void EditTerminalColors_OnClick(object sender, RoutedEventArgs e) => ShowToolWindow(ToolWindows.ColorTheme);

        private void ResetTerminalColors_OnClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (MessageBox.Show(this, "Restore the default terminal and info bar colors?", "Reset Terminal Colors",
                        MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    AppServices.GetRequiredService<AppSettings>().ColorTheme = new ColorTheme();
                }
            });
        }
    }
}
