using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Argus.Memory;
using Avalon.Common;
using Avalon.Controls;
using Avalon.Dialogs;
using Avalon.Models;
using Trigger = Avalon.Models.Trigger;
using Avalon.Scripting;
using CommunityToolkit.Mvvm.Input;
using Mosaic.UI.Wpf;

namespace Avalon.ViewModels;

/// <summary>
/// The application's commands, shared by the ribbon, title bar, key bindings and hash commands.
/// </summary>
/// <remarks>
/// Mirrors Continuum's AppCommands.  Commands that work on a game use the game whose terminal is showing; when no
/// terminal is open they ask which game to use.
/// </remarks>
public static class AppCommands
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static MainWindow? Main => Application.Current.MainWindow as MainWindow;

    private static AppViewModel ViewModel => AppServices.GetRequiredService<AppViewModel>();

    private static AppSettings Settings => AppServices.GetRequiredService<AppSettings>();

    // ----------------------------------------------------------------------------------------------- Games / profiles

    /// <summary>Creates a new game.</summary>
    public static ICommand NewGameCommand { get; } = new RelayCommand(() => Main?.ShowGameLibrary(newGame: true));

    /// <summary>Opens the Profiles Editor.</summary>
    public static ICommand EditGamesCommand { get; } = new RelayCommand(() =>
        ShowListEditor(Settings.Games, () => { Main?.ShowGameLibrary(newGame: true); return null; }));

    /// <summary>Opens the game library to pick a game to play.</summary>
    public static ICommand OpenGameCommand { get; } = new RelayCommand(() => Main?.ShowGameLibrary(newGame: false));

    /// <summary>Imports a game from a .profile file.</summary>
    public static ICommand ImportGameCommand { get; } = new RelayCommand(ImportGame);

    /// <summary>Exports a game to a .profile file.</summary>
    public static ICommand ExportGameCommand { get; } = new RelayCommand(ExportGame);

    /// <summary>Sets the Character variable for the active game.</summary>
    public static ICommand SetCurrentCharacterCommand { get; } = new RelayCommand(SetCurrentCharacter);

    /// <summary>Shows the active game's settings in the Properties window.</summary>
    public static ICommand ShowProfileSettingsCommand { get; } = new RelayCommand(() =>
    {
        if (GetGame() is { } game)
        {
            ShowProperties(game);
        }
    });

    // ----------------------------------------------------------------------------------------------- Game editors

    /// <summary>Creates an alias.</summary>
    public static ICommand NewAliasCommand { get; } = new RelayCommand(() => NewItem(g => g.Aliases, () => new Alias()));

    /// <summary>Opens the Aliases Editor.</summary>
    public static ICommand EditAliasesCommand { get; } = new RelayCommand(() => EditList(g => g.Aliases, () => new Alias()));

    /// <summary>Creates a trigger.</summary>
    public static ICommand NewTriggerCommand { get; } = new RelayCommand(() => NewItem(g => g.Triggers, () => new Trigger()));

    /// <summary>Opens the Triggers Editor.</summary>
    public static ICommand EditTriggersCommand { get; } = new RelayCommand(() => EditList(g => g.Triggers, () => new Trigger()));

    /// <summary>Creates a highlight trigger.</summary>
    public static ICommand NewHighlightTriggerCommand { get; } = new RelayCommand(() => NewItem(g => g.HighlightTriggers, () => new HighlightTrigger()));

    /// <summary>Opens the Highlight Triggers Editor.</summary>
    public static ICommand EditHighlightTriggersCommand { get; } = new RelayCommand(() => EditList(g => g.HighlightTriggers, () => new HighlightTrigger()));

    /// <summary>Resets the slowest execution time recorded for each trigger.</summary>
    public static ICommand ResetMaxExecutionStatsCommand { get; } = new RelayCommand(() =>
    {
        if (GetGame() is not { } game)
        {
            return;
        }

        foreach (var trigger in game.Triggers)
        {
            trigger.MaxExecutionTime = 0;
        }

        foreach (var highlight in game.HighlightTriggers)
        {
            highlight.MaxExecutionTime = 0;
        }

        SetStatus("Trigger execution statistics reset.");
    });

    /// <summary>Edits the game's On Profile Loaded script.</summary>
    public static ICommand EditOnProfileLoadedCommand { get; } = new RelayCommand(() => EditGameEvent(nameof(Game.OnProfileLoadedEvent), "On Profile Loaded Event"));

    /// <summary>Edits the game's On Profile Closed script.</summary>
    public static ICommand EditOnProfileClosedCommand { get; } = new RelayCommand(() => EditGameEvent(nameof(Game.OnProfileClosedEvent), "On Profile Closed Event"));

    /// <summary>Edits the game's On Tick script.</summary>
    public static ICommand EditOnTickCommand { get; } = new RelayCommand(() => EditGameEvent(nameof(Game.OnTickEvent), "On Tick Event"));

    /// <summary>Creates a direction.</summary>
    public static ICommand NewDirectionCommand { get; } = new RelayCommand(() => NewItem(g => g.Directions, () => new Direction()));

    /// <summary>Opens the Directions Editor.</summary>
    public static ICommand EditDirectionsCommand { get; } = new RelayCommand(() => EditList(g => g.Directions, () => new Direction()));

    /// <summary>Shows the directions available from the current room.</summary>
    public static ICommand ShowDirectionsCommand { get; } = new RelayCommand(ShowDirectionsDialog);

    /// <summary>Starts recording movement into a direction.</summary>
    public static ICommand StartRecordingCommand { get; } = new RelayCommand(() =>
    {
        if (ViewModel.ActiveTerminal is { } terminal)
        {
            terminal.StartRecording();
        }
        else
        {
            Info("Open a game before recording directions.");
        }
    });

    /// <summary>Stops recording and saves the new direction.</summary>
    public static ICommand StopRecordingCommand { get; } = new RelayCommand(StopRecording);

    /// <summary>Creates a macro.</summary>
    public static ICommand NewMacroCommand { get; } = new RelayCommand(() =>
    {
        if (GetGame() is not { } game)
        {
            return;
        }

        var editor = new MacroEditor(new Macro());
        if (editor.ShowDialog() == true)
        {
            game.Macros.Add(editor.Macro);
            Save();
        }
    });

    /// <summary>Opens the Macros Editor.</summary>
    public static ICommand EditMacrosCommand { get; } = new RelayCommand(() => EditList(g => g.Macros, () =>
    {
        var editor = new MacroEditor(new Macro());
        return editor.ShowDialog() == true ? editor.Macro : null;
    }));

    /// <summary>Creates a script.</summary>
    public static ICommand NewScriptCommand { get; } = new RelayCommand(() => NewItem(g => g.Scripts, () => new SourceCode()));

    /// <summary>Opens the Scripts Editor.</summary>
    public static ICommand EditScriptsCommand { get; } = new RelayCommand(() => EditList(g => g.Scripts, () => new SourceCode()));

    /// <summary>Creates a variable.</summary>
    public static ICommand NewVariableCommand { get; } = new RelayCommand(() => NewItem(g => g.Variables, () => new Variable()));

    /// <summary>Opens the Variables Editor.</summary>
    public static ICommand EditVariablesCommand { get; } = new RelayCommand(() => EditList(g => g.Variables, () => new Variable()));

    /// <summary>Shows the Notes window.</summary>
    public static ICommand ShowNotesCommand { get; } = new RelayCommand(() => Main?.ShowToolWindow(ToolWindows.Notes));

    // ----------------------------------------------------------------------------------------------- Terminal

    /// <summary>Opens the game library to start another terminal.</summary>
    public static ICommand NewTerminalCommand { get; } = new RelayCommand(() =>
    {
        if (Main is not { } main)
        {
            return;
        }

        var name = InputBox.Show("New Terminal", "What is the name of your new terminal?  Scripts use the name to write to it, so keep it short, for example Tells or Loot.");
        name = TerminalWindow.NormalizeName(name);
        if (name.Length > 0)
        {
            main.ShowTerminalWindow(name);
        }
    });

    /// <summary>Sends several lines of text to the game.</summary>
    public static ICommand SendMultilineCommand { get; } = new RelayCommand(() =>
    {
        if (ViewModel.ActiveTerminal is { } terminal)
        {
            terminal.ShowMultilineDialog();
        }
    });

    /// <summary>Processes text as though the game sent it, to test triggers.</summary>
    public static ICommand SimulateIncomingCommand { get; } = new RelayCommand(async () =>
    {
        if (ViewModel.ActiveTerminal is not { } terminal)
        {
            Info("Open a game to simulate incoming text.");
            return;
        }
        var editor = new StringEditor("") { Title = "Simulate Incoming Text" };
        if (editor.ShowDialog() == true && !string.IsNullOrEmpty(editor.Text))
        {
            await terminal.SimulateIncomingAsync(editor.Text);
        }
    });

    /// <summary>Closes every alert on the active terminal.</summary>
    public static ICommand CloseAllAlertsCommand { get; } = new RelayCommand(() => ViewModel.ActiveTerminal?.CloseAlerts());

    // ----------------------------------------------------------------------------------------------- Layout

    /// <summary>
    /// Prompts for the name of a vitals panel to open or create.
    /// </summary>
    public static ICommand AddVitalsPanelCommand { get; } = new RelayCommand(() =>
    {
        if (Main is { } main && InputBox.Show("Add Vitals Panel", "Enter a panel name. Scripts use this name to update its vitals.", "Vitals") is { } name
            && !string.IsNullOrWhiteSpace(name))
        {
            main.ShowVitalsPanel(name);
        }
    });

    /// <summary>Shows a tool window by its <see cref="ToolWindows"/> id.</summary>
    public static ICommand ShowToolWindowCommand { get; } = new RelayCommand<string>(id =>
    {
        if (id is not null)
        {
            Main?.ShowToolWindow(id);
        }
    });

    /// <summary>Restores the default docking layout.</summary>
    public static ICommand ResetLayoutCommand { get; } = new RelayCommand(() => Main?.ResetLayout());

    /// <summary>Shows the Properties window for the active game.</summary>
    public static ICommand ShowPropertiesCommand { get; } = new RelayCommand(() => Main?.ShowToolWindow(ToolWindows.Properties));

    /// <summary>Collapses or expands the ribbon.</summary>
    public static ICommand ToggleRibbonCommand { get; } = new RelayCommand(() => Main?.ToggleRibbon());

    // ----------------------------------------------------------------------------------------------- Developer

    /// <summary>Opens the Regular Expression Builder.</summary>
    public static ICommand ShowRegexTesterCommand { get; } = new RelayCommand(() => ShowRegexTester(""));

    /// <summary>Opens a new Interactive Script Editor document.</summary>
    public static ICommand NewInteractiveScriptEditorCommand { get; } = new RelayCommand(() => Main?.OpenScriptDocument(null, null, "Interactive Script Editor"));

    /// <summary>Opens a SQLite query document over the script database.</summary>
    public static ICommand SqliteQueryEditorCommand { get; } = new RelayCommand(() => Main?.OpenSqliteDocument());

    /// <summary>Creates a package.</summary>
    public static ICommand NewPackageCommand { get; } = new RelayCommand(() => NewItem(g => g.Packages, () => new PackageMetadata { Author = Environment.UserName }));

    /// <summary>Opens the Packages Editor.</summary>
    public static ICommand EditPackagesCommand { get; } = new RelayCommand(() => EditList(g => g.Packages, () => new PackageMetadata { Author = Environment.UserName }));

    /// <summary>Compiles a package into a .package file.</summary>
    public static ICommand CompilePackageCommand { get; } = new RelayCommand(() => PackageManager.CompileInteractive(GetGame()));

    /// <summary>Imports a .package file into the active game.</summary>
    public static ICommand ImportPackageCommand { get; } = new RelayCommand(() => PackageManager.ImportInteractive(GetGame()));

#if DEBUG
    /// <summary>
    /// Imports a Continuum package into the selected game.
    /// </summary>
    public static ICommand ImportContinuumPackageCommand { get; } =
        new RelayCommand(() => PackageManager.ImportContinuumInteractive(GetGame()));
#endif

    // ----------------------------------------------------------------------------------------------- Help

    /// <summary>Shows the User Guide.</summary>
    public static ICommand ShowUserGuideCommand { get; } = new RelayCommand(() => Main?.OpenDocumentation("UserGuide.md"));

    /// <summary>Shows one of the embedded documentation pages.</summary>
    public static ICommand ShowDocumentationCommand { get; } = new RelayCommand<string>(name => Main?.OpenDocumentation(name ?? "UserGuide.md"));

    /// <summary>Gives every alias, trigger, script and direction that shares an ID a new one.</summary>
    public static ICommand FixDuplicateIdsCommand { get; } = new RelayCommand(FixDuplicateIds);

    // ----------------------------------------------------------------------------------------------- Backstage / app

    /// <summary>Opens both settings files as JSON documents.</summary>
    public static ICommand ViewSettingsFileCommand { get; } = new RelayCommand(() => Main?.OpenSettingsDocuments());

    /// <summary>Opens the settings folder.</summary>
    public static ICommand OpenSettingsFolderCommand { get; } = new RelayCommand(() => Shell(AppPaths.AppDataFolder));

    /// <summary>Opens the documents folder.</summary>
    public static ICommand OpenDocumentsFolderCommand { get; } = new RelayCommand(() => Shell(AppPaths.DocumentsFolder));

    /// <summary>Shows the client settings.</summary>
    public static ICommand ShowSettingsCommand { get; } = new RelayCommand(() => Main?.ShowToolWindow(ToolWindows.Settings));

    /// <summary>Exits after saving.</summary>
    public static ICommand ExitCommand { get; } = new RelayCommand(() => Main?.Close());

    /// <summary>Exits without saving the settings.</summary>
    public static ICommand ExitWithoutSavingCommand { get; } = new RelayCommand(() =>
    {
        if (Main is { } main)
        {
            main.SkipSaveOnExit = true;
            main.Close();
        }
    });

    // ----------------------------------------------------------------------------------------------- Helpers

    /// <summary>
    /// Returns the game whose terminal is showing, or asks for one when no terminal is open.
    /// </summary>
    public static Game? GetGame()
    {
        if (ViewModel.ActiveTerminal?.Game is { } active)
        {
            return active;
        }

        var games = Settings.Games;
        if (games.Count == 0)
        {
            Info("Create a game first (Game > Profiles > New).");
            Main?.ShowGameLibrary(newGame: true);
            return null;
        }
        return games.Count == 1 ? games[0] : GamePicker.Pick(games);
    }

    /// <summary>
    /// Opens the right editor for an alias, trigger, script or other item.
    /// </summary>
    public static void EditItem(Game game, object item)
    {
        if (item is SourceCode script)
        {
            Main?.OpenScriptDocument(script, nameof(SourceCode.Code), script.Name);
            return;
        }
        if (item is Macro macro)
        {
            new MacroEditor(macro).ShowDialog();
            Save();
            return;
        }
        if (ModelEditor.Edit(item, profile: game))
        {
            Save();
        }
    }

    /// <summary>
    /// Shows the directions available from the Room variable and walks the chosen one.
    /// </summary>
    public static void ShowDirectionsDialog()
    {
        if (ViewModel.ActiveTerminal is not { } terminal)
        {
            Info("Open a game to use directions.");
            return;
        }

        var dialog = new DirectionsSelectWindow(terminal.Game, terminal.Interpreter.GetVariable("Room"));
        if (dialog.ShowDialog() == true && dialog.Selected is { } direction)
        {
            _ = terminal.SendCommandAsync($"#go {direction.Name}");
        }
    }

    /// <summary>
    /// Opens the Regular Expression Builder, optionally seeded with a sample line.
    /// </summary>
    public static void ShowRegexTester(string sample) => new RegexBuilderWindow("", sample).Show();

    /// <summary>
    /// Shows an object in the Properties window.
    /// </summary>
    public static void ShowProperties(object item) => Main?.ShowProperties(item);

    /// <summary>
    /// Saves the settings.
    /// </summary>
    public static void Save()
    {
        try
        {
            App.SaveAppSettings();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(ex.Message, "Save Settings", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void NewItem<T>(Func<Game, ObservableCollection<T>> collection, Func<T> factory) where T : class
    {
        if (GetGame() is not { } game)
        {
            return;
        }

        var item = factory();
        if (item is SourceCode script)
        {
            if (InputBox.Show("New Script", "Name of the script:", "NewScript") is not { Length: > 0 } name)
            {
                return;
            }

            script.Name = name;
            script.Code = $"function {name.Replace('.', '_').Replace(' ', '_')}() {{\n    \n}}\n";
            collection(game).Add(item);
            Save();
            Main?.OpenScriptDocument(script, nameof(SourceCode.Code), script.Name);
            return;
        }

        if (ModelEditor.Edit(item, isNew: true, profile: game))
        {
            collection(game).Add(item);
            if (item is Trigger)
            {
                SortTriggers(game);
            }

            Save();
        }
    }

    private static void EditList<T>(Func<Game, ObservableCollection<T>> collection, Func<T?> factory) where T : class
    {
        if (GetGame() is not { } game)
        {
            return;
        }

        var items = collection(game);
        ShowListEditor(items, () =>
        {
            var item = factory();
            if (item is null)
            {
                return null;
            }

            if (item is Macro)
            {
                items.Add(item);
                return item;
            }
            if (!ModelEditor.Edit(item, isNew: true, profile: game))
            {
                return null;
            }

            items.Add(item);
            return item;
        }, game);

        if (typeof(T) == typeof(Trigger))
        {
            SortTriggers(game);
        }
    }

    private static void ShowListEditor(IList items, Func<object?> createNew, Game? game = null)
    {
        var itemType = items.GetType().GetGenericArguments().FirstOrDefault() ?? typeof(object);
        var dialog = new DataListDialog(items, itemType, createNew, game);
        var list = dialog.List;

        if (game is not null)
        {
            dialog.Editor = item => { EditItem(game, item); return true; };
        }

        switch (itemType.Name)
        {
            case nameof(Alias):
                list.AddMenuItem("Edit Alias Command", "vs-script-16.png", item =>
                {
                    var alias = (Alias)item;
                    ModelEditor.EditText(dialog, TextEditorKind.Command, alias.Command, alias, "Alias Command", text => alias.Command = text);
                });
                break;
            case nameof(Trigger):
                list.AddMenuItem("Edit Trigger Command", "vs-script-16.png", item =>
                {
                    var trigger = (Trigger)item;
                    ModelEditor.EditText(dialog, TextEditorKind.Command, trigger.Command, trigger, "Trigger Command", text => trigger.Command = text);
                });
                list.AddMenuItem("Open Pattern in Regex Builder", "vs-regex-16.png", item =>
                {
                    var trigger = (Trigger)item;
                    ModelEditor.EditText(dialog, TextEditorKind.Regex, trigger.Pattern, trigger, "Pattern", text => trigger.Pattern = text);
                });
                list.AddMenuItem("Copy Trigger ID", "vs-copy-16.png", item => Clipboard.SetDataObject(((Trigger)item).Id, true));
                break;
            case nameof(SourceCode):
                list.AddMenuItem("Edit Source Code", "vs-js-project-16.png", item =>
                {
                    var script = (SourceCode)item;
                    dialog.Close();
                    Main?.OpenScriptDocument(script, nameof(SourceCode.Code), script.Name);
                });
                break;
            case nameof(Direction):
                list.AddMenuItem("Create Reverse Direction", "vs-refresh-16.png", item =>
                {
                    var direction = (Direction)item;
                    var reverse = new Direction
                    {
                        Name = string.IsNullOrWhiteSpace(direction.StartingRoom) ? $"{direction.Name} (Reverse)" : direction.StartingRoom,
                        Speedwalk = Direction.Reverse(direction.Speedwalk),
                        StartingRoom = direction.EndingRoom,
                        EndingRoom = direction.StartingRoom,
                        Package = direction.Package
                    };
                    items.Add(reverse);
                    list.Select(reverse);
                });
                break;
            case nameof(PackageMetadata):
                list.AddMenuItem("Compile Package", "package-add-48.png", item => PackageManager.CompileInteractive(game, (PackageMetadata)item));
                list.AddMenuItem("Uninstall (Remove Package Items)", "vs-delete-16.png", item => PackageManager.UninstallInteractive(game, (PackageMetadata)item));
                break;
        }

        dialog.ShowDialog();
        Save();
    }

    private static void SortTriggers(Game game)
    {
        var sorted = game.Triggers.OrderBy(t => t.Priority).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            int index = game.Triggers.IndexOf(sorted[i]);
            if (index != i)
            {
                game.Triggers.Move(index, i);
            }
        }
    }

    private static void EditGameEvent(string property, string title)
    {
        if (GetGame() is not { } game)
        {
            return;
        }

        var info = typeof(Game).GetProperty(property)!;
        var editor = new ScriptEditorWindow(info.GetValue(game) as string ?? "", SaveProperty) { Title = $"{game.Name}: {title}" };
        if (editor.ShowDialog() == true)
        {
            SaveProperty(editor.Text);
        }

        void SaveProperty(string text)
        {
            info.SetValue(game, text);
            Save();
        }
    }

    private static void StopRecording()
    {
        if (ViewModel.ActiveTerminal is not { IsRecording: true } terminal)
        {
            Info("Directions are not being recorded.");
            return;
        }

        if (terminal.StopRecording() is not { } direction)
        {
            Info("No movement was recorded.");
            return;
        }

        direction.Name = string.IsNullOrWhiteSpace(direction.EndingRoom) ? "New Direction" : direction.EndingRoom;
        if (ModelEditor.Edit(direction, isNew: true, profile: terminal.Game))
        {
            terminal.Game.Directions.Add(direction);
            Save();
        }
    }

    private static void SetCurrentCharacter()
    {
        if (ViewModel.ActiveTerminal is not { } terminal)
        {
            Info("Open a game to set the current character.");
            return;
        }

        var current = terminal.Interpreter.GetVariable("Character");
        if (InputBox.Show("Set Current Character", "Character name (used by character specific aliases, triggers and variables):", current) is { } name)
        {
            _ = terminal.Interpreter.ExecuteAsync(ExecuteType.Command, $"#set Character {name.Trim()}");
        }
    }

    private static void ImportGame()
    {
        var path = WpfUtilities.OpenFileRequest("Import Profile", "Avalon or Continuum profile (*.profile;*.json)|*.profile;*.json");
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            var game = JsonSerializer.Deserialize<Game>(File.ReadAllText(path));
            if (game is null)
            {
                return;
            }

            if (Settings.Games.Any(g => g.Id == game.Id))
            {
                game.Id = Guid.NewGuid();
            }

            if (string.IsNullOrWhiteSpace(game.Name))
            {
                game.Name = Path.GetFileNameWithoutExtension(path);
            }

            Settings.Games.Add(game);
            Save();
            SetStatus($"Imported {game.Name}.");
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            MessageBox.Show(ex.Message, "Import Profile", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void ExportGame()
    {
        if (GetGame() is not { } game)
        {
            return;
        }

        var path = WpfUtilities.SaveFileRequest("Export Profile", "Avalon profile (*.profile)|*.profile");
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            File.WriteAllText(path, JsonSerializer.Serialize(game, JsonOptions));
            SetStatus($"Exported {game.Name} to {path}.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(ex.Message, "Export Profile", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void FixDuplicateIds()
    {
        int fixedCount = 0;
        foreach (var game in Settings.Games)
        {
            fixedCount += Fix(game.Aliases, a => a.Id, (a, id) => a.Id = id);
            fixedCount += Fix(game.Triggers, t => t.Id, (t, id) => t.Id = id);
            fixedCount += Fix(game.Scripts, s => s.Id, (s, id) => s.Id = id);
            fixedCount += Fix(game.Directions, d => d.Id, (d, id) => d.Id = id);
        }
        Save();
        MessageBox.Show($"{fixedCount} duplicate ID(s) were replaced.", "Fix Duplicate IDs", MessageBoxButton.OK, MessageBoxImage.Information);

        static int Fix<T>(IEnumerable<T> items, Func<T, string> get, Action<T, string> set)
        {
            int count = 0;
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items)
            {
                if (seen.Add(get(item)))
                {
                    continue;
                }

                set(item, Guid.NewGuid().ToString());
                count++;
            }
            return count;
        }
    }

    private static void Shell(string path)
    {
        try
        {
            Directory.CreateDirectory(Directory.Exists(path) ? path : Path.GetDirectoryName(path) ?? path);
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or IOException)
        {
            MessageBox.Show(ex.Message, "Open", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void SetStatus(string text) => ViewModel.StatusText = text;

    private static void Info(string text) => MessageBox.Show(text, "Avalon", MessageBoxButton.OK, MessageBoxImage.Information);
}

/// <summary>
/// The ids of the tool windows the Layout menu can show.
/// </summary>
public static class ToolWindows
{
    /// <summary>The Properties window.</summary>
    public const string Properties = "properties";

    /// <summary>The Variables window.</summary>
    public const string Variables = "variables";

    /// <summary>The Alias Button Actions window.</summary>
    public const string AliasButtons = "aliasButtons";

    /// <summary>The Movement compass.</summary>
    public const string Movement = "movement";

    /// <summary>The Log viewer.</summary>
    public const string Log = "logViewer";

    /// <summary>The Notes editor.</summary>
    public const string Notes = "notes";

    /// <summary>The terminal Color Theme editor.</summary>
    public const string ColorTheme = "terminalColors";

    /// <summary>The client Settings.</summary>
    public const string Settings = "appSettings";
}
