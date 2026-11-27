using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Avalon.Models;
using Avalon.Network;
using Avalon.Terminal;
using Avalon.Timers;
using ICSharpCode.AvalonEdit;
using Microsoft.Xaml.Behaviors;
using Mosaic.UI.Wpf.Behaviors;
using Mosaic.UI.Wpf.Logging;

namespace Avalon.Controls;

/// <summary>
/// Hosts an interactive Telnet session in the main window's document area.
/// </summary>
public partial class MudTerminal : UserControl, IDisposable, ITerminalHost
{
    private static readonly DependencyPropertyKey IsConnectedPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsConnected), typeof(bool), typeof(MudTerminal), new PropertyMetadata(false));

    /// <summary>
    /// Identifies the <see cref="IsConnected"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsConnectedProperty = IsConnectedPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey IsSessionActivePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsSessionActive), typeof(bool), typeof(MudTerminal), new PropertyMetadata(false));

    /// <summary>
    /// Identifies the <see cref="IsSessionActive"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsSessionActiveProperty = IsSessionActivePropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey StatusPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Status), typeof(string), typeof(MudTerminal), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Identifies the <see cref="Status"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty StatusProperty = StatusPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey ScriptsActivePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(ScriptsActive), typeof(int), typeof(MudTerminal), new PropertyMetadata(0));

    /// <summary>
    /// Identifies the <see cref="ScriptsActive"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ScriptsActiveProperty = ScriptsActivePropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey IsRecordingPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsRecording), typeof(bool), typeof(MudTerminal), new PropertyMetadata(false));

    /// <summary>
    /// Identifies the <see cref="IsRecording"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsRecordingProperty = IsRecordingPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey CharacterPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Character), typeof(string), typeof(MudTerminal), new PropertyMetadata(null));

    /// <summary>
    /// Identifies the <see cref="Character"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CharacterProperty = CharacterPropertyKey.DependencyProperty;

    private readonly AppViewModel _viewModel;
    private readonly AppSettings _settings;
    private readonly TerminalTranscript _transcript;
    private readonly HighlightColorizer _highlights;
    private readonly TickTimer _tickTimer;
    private readonly List<string> _recording = [];
    private string _recordingStartRoom = "";
    private Session? _session;
    private DispatcherOperation? _scroll;
    private bool _disposed;
    private bool _profileLoaded;
    private int _statusVersion;
    private string? _statusBeforeScript;

    /// <summary>
    /// Initializes a new instance of the MudTerminal class.
    /// </summary>
    /// <param name="game">The game to play.</param>
    /// <param name="viewModel">The application view model that provides the terminal palette.</param>
    public MudTerminal(Game game, AppViewModel viewModel)
    {
        Game = game;
        DataContext = viewModel;
        InitializeComponent();
        _viewModel = viewModel;
        _settings = viewModel.AppSettings;
        _transcript = new TerminalTranscript(Output) { Palette = _settings.ColorTheme };
        _highlights = new HighlightColorizer(game);
        InitializeTerminal(Output);
        InitializeTerminal(BackBuffer);
        InitializeBackBuffer();
        InitializeOutputHistory();
        Output.TextArea.TextView.LineTransformers.Add(_highlights);
        BackBuffer.TextArea.TextView.LineTransformers.Add(_highlights);

        Interpreter = new Interpreter(game, this);
        Interpreter.OutputChanged += Interpreter_OnOutputChanged;
        Interpreter.CommandSent += Interpreter_OnCommandSent;
        Interpreter.ScriptsActiveChanged += (_, _) => Game.ViewModel.ScriptsActive = ScriptsActive = Interpreter.ScriptsActive;
        _tickTimer = new TickTimer(Interpreter);
        Interpreter.VariableChanged += (_, key) =>
        {
            if (key.Equals("Character", StringComparison.OrdinalIgnoreCase))
            {
                Game.ViewModel.Character = Character = NullIfEmpty(Interpreter.GetVariable("Character"));
            }
        };
        Game.ViewModel.Character = Character = NullIfEmpty(Interpreter.GetVariable("Character"));
        PropertyChangedEventManager.AddHandler(Game.ViewModel, ProfileViewModel_OnCharacterChanged, nameof(ProfileViewModel.Character));
        AlertsHost.ItemsSource = Alerts;

        // CommandBox setup
        Input.TextArea.TextView.Options.EnableEmailHyperlinks = false;
        Input.TextArea.TextView.Options.EnableHyperlinks = false;
        Input.TextArea.TextView.Options.ShowBoxForControlCharacters = false;

        var blockCaret = new AvalonEditBlockCaretBehavior();
        BindingOperations.SetBinding(blockCaret, AvalonEditBlockCaretBehavior.IsEnabledProperty,
            new Binding(nameof(AppSettings.UseBlockCaret)) { Source = _settings });
        BindingOperations.SetBinding(blockCaret, AvalonEditBlockCaretBehavior.CaretBrushProperty,
            new Binding(nameof(AppSettings.CommandBoxForegroundColor)) { Source = _settings });
        BindingOperations.SetBinding(blockCaret, AvalonEditBlockCaretBehavior.CaretTextBrushProperty,
            new Binding(nameof(AppSettings.CommandBoxBackgroundColor)) { Source = _settings });
        Interaction.GetBehaviors(Input).Add(blockCaret);

        PropertyChangedEventManager.AddHandler(_settings, Settings_OnPropertyChanged, nameof(AppSettings.ColorTheme));
        PropertyChangedEventManager.AddHandler(_transcript.Palette, Palette_OnPropertyChanged, string.Empty);
        Loaded += MudTerminal_OnLoaded;
        SetStatus("Disconnected");
    }

    /// <summary>
    /// Gets the saved game associated with this terminal.
    /// </summary>
    public Game Game { get; }

    /// <summary>
    /// Gets the interpreter that runs this terminal's commands, triggers and scripts.
    /// </summary>
    public Interpreter Interpreter { get; }

    /// <summary>
    /// Gets the alerts showing over the terminal.
    /// </summary>
    public ObservableCollection<Alert> Alerts { get; } = [];

    /// <summary>
    /// Occurs when a script asks for the tab title to change.
    /// </summary>
    public event EventHandler<string>? TitleRequested;

    /// <summary>
    /// Gets whether this terminal has a live connection to its game.
    /// </summary>
    public bool IsConnected
    {
        get => (bool)GetValue(IsConnectedProperty);
        private set => SetValue(IsConnectedPropertyKey, value);
    }

    /// <summary>
    /// Gets whether a session is connecting or connected, which is when it can be disconnected.
    /// </summary>
    public bool IsSessionActive
    {
        get => (bool)GetValue(IsSessionActiveProperty);
        private set => SetValue(IsSessionActivePropertyKey, value);
    }

    /// <summary>
    /// Gets the latest connection status message, shown on the main window's status bar while this
    /// terminal is the active one.
    /// </summary>
    public string Status
    {
        get => (string)GetValue(StatusProperty);
        private set => SetValue(StatusPropertyKey, value);
    }

    /// <summary>
    /// Gets the number of scripts running in this terminal.
    /// </summary>
    public int ScriptsActive
    {
        get => (int)GetValue(ScriptsActiveProperty);
        private set => SetValue(ScriptsActivePropertyKey, value);
    }

    /// <summary>
    /// Gets whether movement is being recorded into a new direction.
    /// </summary>
    public bool IsRecording
    {
        get => (bool)GetValue(IsRecordingProperty);
        private set => SetValue(IsRecordingPropertyKey, value);
    }

    /// <summary>
    /// Gets the current character (the Character variable), or null when it is not set.
    /// </summary>
    public string? Character
    {
        get => (string?)GetValue(CharacterProperty);
        private set => SetValue(CharacterPropertyKey, value);
    }

    /// <inheritdoc/>
    public int LineCount => Output.Document.LineCount;

    /// <summary>
    /// Turns triggers on or off for this game.
    /// </summary>
    public void ToggleTriggers() => Game.TriggersEnabled = !Game.TriggersEnabled;

    /// <summary>
    /// Turns aliases on or off for this game.
    /// </summary>
    public void ToggleAliases() => Game.AliasesEnabled = !Game.AliasesEnabled;

    /// <summary>
    /// Turns GMCP on or off for this game and, like Continuum, tells the game with "gmcp on" or
    /// "gmcp off" when connected.
    /// </summary>
    public async Task ToggleGmcpAsync()
    {
        Game.GmcpEnabled = !Game.GmcpEnabled;
        if (_session is { } current)
        {
            current.Connection.GmcpEnabled = Game.GmcpEnabled;
        }

        var command = Game.GmcpEnabled ? "gmcp on" : "gmcp off";
        await SendAsync(session => Interpreter.SendAsync(command, session.Cancellation.Token));
    }

    /// <summary>
    /// Sends a command as if it were typed into the command box.
    /// </summary>
    public Task SendCommandAsync(string command) => SendAsync(session => Interpreter.SendAsync(command, session.Cancellation.Token));

    /// <summary>
    /// Processes text as if the server sent it, so triggers can be tested.
    /// </summary>
    public Task SimulateIncomingAsync(string text) => Interpreter.SimulateIncomingAsync(text);

    /// <summary>
    /// Connects to the current game address and runs the session until it ends.
    /// </summary>
    public Task ConnectAsync() => RunSessionAsync(executeOnConnected: true);

    Task<bool> ITerminalHost.ConnectAsync(bool executeOnConnected)
    {
        if (_session is null)
        {
            _ = RunSessionAsync(executeOnConnected);
        }

        return _session?.Connected.Task ?? Task.FromResult(false);
    }

    private async Task RunSessionAsync(bool executeOnConnected)
    {
        if (_disposed || _session is not null)
        {
            return;
        }

        var host = Game.IpAddress?.Trim().Trim('[', ']');
        if (string.IsNullOrWhiteSpace(host) || Game.Port is < 1 or > 65535)
        {
            SetStatus("Enter a valid game hostname and port");
            return;
        }

        using var cancellation = new CancellationTokenSource();
        using var connection = new TelnetConnection(Game.GmcpEnabled);
        var session = new Session(connection, cancellation);
        _session = session;
        Interpreter.ResetSession();
        Interpreter.Connection = connection;
        IsSessionActive = true;
        SetStatus($"Connecting to {Game.Address}…");

        string status = "Disconnected";
        try
        {
            using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation.Token))
            {
                timeout.CancelAfter(TimeSpan.FromSeconds(15));
                await connection.ConnectAsync(host, Game.Port, timeout.Token);
            }

            cancellation.Token.ThrowIfCancellationRequested();
            _transcript.StartSession();
            IsConnected = CommandInput.IsEnabled = true;
            session.Connected.TrySetResult(true);
            SetStatus($"Connected to {Game.Address}");
            Game.RecordPlay();
            _tickTimer.Start();
            if (IsKeyboardFocusWithin || ReferenceEquals(_viewModel.ActiveTerminal, this))
            {
                FocusInput();
            }

            if (executeOnConnected && !string.IsNullOrWhiteSpace(Game.OnConnectCommand))
            {
                await Interpreter.SendAsync(Game.OnConnectCommand, cancellation.Token);
            }

            await foreach (var message in connection.ReadAsync(cancellation.Token))
            {
                await Interpreter.ReceiveAsync(message, cancellation.Token);
                if (message is TelnetMessage.ServerEcho)
                {
                    SetPrivateInput(Interpreter.ServerEchoEnabled);
                }
            }
            if (!cancellation.IsCancellationRequested)
            {
                status = "Disconnected by the server";
            }
        }
        catch (Exception ex) when (cancellation.IsCancellationRequested &&
            ex is OperationCanceledException or ObjectDisposedException or IOException or SocketException)
        {
            // Disconnect and tab closure intentionally stop outstanding socket operations.
        }
        catch (OperationCanceledException)
        {
            status = "Connection timed out";
        }
        catch (Exception ex) when (ex is IOException or SocketException or ArgumentException or InvalidOperationException)
        {
            status = IsConnected ? $"Connection lost. {ex.Message}" : $"Could not connect. {ex.Message}";
            Logger.LogError(status);
        }
        finally
        {
            session.Connected.TrySetResult(false);
            cancellation.Cancel();
            _tickTimer.Stop();
            Interpreter.Connection = null;
            _session = null;
            IsConnected = IsSessionActive = false;
            _transcript.CommitPreview();
            if (!_disposed)
            {
                _completionWindow?.Close();
                _multilineDialog?.Close(false);
                CommandInput.IsEnabled = false;
                SetPrivateInput(false);
                SetStatus(status);
            }
        }
    }

    /// <inheritdoc/>
    public void FocusInput()
    {
        Dispatcher.InvokeAsync(() =>
        {
            if (_disposed || !IsVisible)
            {
                return;
            }

            if (!IsConnected)
            {
                Output.TextArea.Focus();
            }
            else if (IsPrivateInput)
            {
                FocusPrivateInput();
            }
            else
            {
                Input.TextArea.Focus();
            }
        }, DispatcherPriority.Input);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(Game.OnProfileClosedEvent))
        {
            _ = Interpreter.RunScriptTextAsync(Game.OnProfileClosedEvent);
        }

        _disposed = true;
        ClearPrivateInput();
        _completionWindow?.Close();
        _multilineDialog?.Close(false);
        _tickTimer.Stop();
        Interpreter.ScheduledTasks.Clear();
        Interpreter.BatchTasks.Clear();
        StopSession();
        _scroll?.Abort();
        Interpreter.OutputChanged -= Interpreter_OnOutputChanged;
        Interpreter.CommandSent -= Interpreter_OnCommandSent;
        PropertyChangedEventManager.RemoveHandler(Game.ViewModel, ProfileViewModel_OnCharacterChanged, nameof(ProfileViewModel.Character));
        PropertyChangedEventManager.RemoveHandler(_settings, Settings_OnPropertyChanged, nameof(AppSettings.ColorTheme));
        PropertyChangedEventManager.RemoveHandler(_transcript.Palette, Palette_OnPropertyChanged, string.Empty);
    }

    /// <summary>
    /// Ends the current session, or cancels it while it is still connecting.
    /// </summary>
    public void Disconnect() => StopSession();

    /// <inheritdoc/>
    public IReadOnlyList<string> LastLines(int count)
    {
        var document = Output.Document;
        int last = document.LineCount;
        // Skip a trailing empty line left by the final newline.
        if (last > 1 && document.GetLineByNumber(last).Length == 0)
        {
            last--;
        }

        int first = Math.Max(1, last - Math.Max(0, count) + 1);
        var lines = new List<string>(last - first + 1);
        for (int number = first; number <= last; number++)
            lines.Add(document.GetText(document.GetLineByNumber(number)));
        return lines;
    }

    /// <inheritdoc/>
    public string LastNonEmptyLine()
    {
        var document = Output.Document;
        for (int number = document.LineCount; number >= 1; number--)
        {
            var line = document.GetText(document.GetLineByNumber(number));
            if (!string.IsNullOrWhiteSpace(line))
            {
                return line;
            }
        }

        return "";
    }

    /// <inheritdoc/>
    public void RemoveLastLine()
    {
        var document = Output.Document;
        int number = document.LineCount;
        if (number > 1 && document.GetLineByNumber(number).Length == 0)
        {
            number--;
        }

        RemoveLine(number);
    }

    /// <inheritdoc/>
    public void ScrollToEnd() => FollowLiveOutput();

    /// <inheritdoc/>
    public void ReplaceText(string searchText, string replacementText, bool lastOnly) =>
        _transcript.ReplaceText(searchText, replacementText, lastOnly);

    /// <inheritdoc/>
    public void RemoveLine(int lineNumber) => _transcript.RemoveLine(lineNumber);

    /// <inheritdoc/>
    public void TrimEnd() => _transcript.TrimEnd();

    /// <inheritdoc/>
    public void ClearBackBuffer()
    {
        // Both views share a transcript. Keep the live screen and drop the history above it.
        var view = Output.TextArea.TextView;
        if (view.VisualLinesValid && view.VisualLines.FirstOrDefault() is { } firstLine)
        {
            _transcript.RemoveLinesBefore(firstLine.FirstDocumentLine.LineNumber);
        }
    }

    /// <inheritdoc/>
    async Task ITerminalHost.SetStatusAsync(string text, int milliseconds)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(milliseconds, -1);
        _statusBeforeScript ??= Status;
        int version = ++_statusVersion;
        UpdateStatus(text);
        await Task.Delay(milliseconds);
        if (!_disposed && _statusVersion == version)
        {
            SetStatus(_statusBeforeScript!);
        }
    }

    /// <inheritdoc/>
    public void SetTitle(string title) => TitleRequested?.Invoke(this, title);

    /// <inheritdoc/>
    public void ShowAlert(Alert alert)
    {
        Alerts.Add(alert);
        ScrollToEnd();
        if (alert.Permanent)
        {
            return;
        }

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(alert.Duration > 0 ? alert.Duration : 10) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            Alerts.Remove(alert);
        };
        timer.Start();
    }

    /// <inheritdoc/>
    public void CloseAlerts() => Alerts.Clear();

    /// <inheritdoc/>
    public void RemoveAlertsByMessage(string message)
    {
        for (int i = Alerts.Count - 1; i >= 0; i--)
        {
            if (string.Equals(Alerts[i].Message, message, StringComparison.Ordinal))
            {
                Alerts.RemoveAt(i);
            }
        }
    }

    /// <inheritdoc/>
    public void Redraw() => RedrawTranscript();

    /// <summary>
    /// Starts recording typed movement into a speedwalk.
    /// </summary>
    public void StartRecording()
    {
        _recording.Clear();
        _recordingStartRoom = Interpreter.GetVariable("Room");
        Game.ViewModel.StartRecordingRoom = _recordingStartRoom;
        Game.ViewModel.IsRecording = IsRecording = true;
        Interpreter.EchoInfo("Recording directions.  Use Stop Recording when you arrive.");
    }

    /// <summary>
    /// Stops recording and returns the new direction, or null when nothing was recorded.
    /// </summary>
    public Direction? StopRecording()
    {
        Game.ViewModel.IsRecording = IsRecording = false;
        if (_recording.Count == 0)
        {
            return null;
        }

        return new Direction
        {
            Speedwalk = Direction.Compress(_recording),
            StartingRoom = _recordingStartRoom,
            EndingRoom = Interpreter.GetVariable("Room")
        };
    }

    private void StopSession()
    {
        if (_session is not { } session)
        {
            return;
        }

        session.Cancellation.Cancel();
        session.Connection.Dispose();
    }

    private void SetStatus(string status)
    {
        _statusVersion++;
        _statusBeforeScript = null;
        UpdateStatus(status);
    }

    private void UpdateStatus(string status)
    {
        Status = status;
        if (ReferenceEquals(_viewModel.ActiveTerminal, this))
        {
            _viewModel.StatusText = status;
        }
    }

    private void ProfileViewModel_OnCharacterChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (Game.ViewModel.Character != Character)
        {
            Interpreter.QueueSetVariable("Character", Game.ViewModel.Character ?? "");
        }
    }

    private async void MudTerminal_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_profileLoaded)
        {
            return;
        }

        _profileLoaded = true;
        if (!string.IsNullOrWhiteSpace(Game.OnProfileLoadedEvent))
        {
            await Interpreter.RunScriptTextAsync(Game.OnProfileLoadedEvent);
        }
    }

    /// <summary>
    /// Macros are checked before anything else in the terminal sees the key, like Continuum which
    /// processes them in the main window's PreviewKeyDown.
    /// </summary>
    private async void MudTerminal_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!IsConnected || IsPrivateInput || Game.Macros.Count == 0)
        {
            return;
        }

        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift
            or Key.LWin or Key.RWin)
        {
            return;
        }

        var modifiers = Keyboard.Modifiers;
        var combination = string.Concat(
            modifiers.HasFlag(ModifierKeys.Control) ? "Ctrl+" : "",
            modifiers.HasFlag(ModifierKeys.Alt) ? "Alt+" : "",
            modifiers.HasFlag(ModifierKeys.Shift) ? "Shift+" : "",
            modifiers.HasFlag(ModifierKeys.Windows) ? "Win+" : "",
            key.ToString());
        var macro = Game.Macros.FirstOrDefault(item => item.Enabled &&
            item.KeyCombination.Equals(combination, StringComparison.OrdinalIgnoreCase));
        if (macro is null)
        {
            return;
        }

        e.Handled = true;
        FocusInput();
        await SendAsync(session => Interpreter.RunMacroAsync(macro, session.Cancellation.Token));
    }

    private async Task SendAsync(Func<Session, Task> send)
    {
        if (!IsConnected || _session is not { } session || session.Cancellation.IsCancellationRequested)
        {
            return;
        }

        try
        {
            await send(session);
        }
        catch (Exception ex) when (session.Cancellation.IsCancellationRequested &&
            ex is OperationCanceledException or ObjectDisposedException or IOException or SocketException)
        {
            // The active connection has already ended.
        }
        catch (Exception ex) when (ex is IOException or SocketException or InvalidOperationException)
        {
            // The reader owns connection status and will report the end of the session.
            Logger.LogError($"Could not send a game command. {ex.Message}");
            session.Connection.Dispose();
        }
    }

    private void Interpreter_OnOutputChanged(object? sender, TerminalOutput update)
    {
        if (_disposed)
        {
            return;
        }

        // The main terminal always follows the live output, the back buffer is where scrolling happens.
        _transcript.Apply(update);
        FollowLiveOutput();
    }

    private void Interpreter_OnCommandSent(object? sender, string command)
    {
        RecordSentCommand(command);
        if (IsRecording && Direction.IsDirection(command.Trim().ToLowerInvariant()))
        {
            _recording.Add(command.Trim().ToLowerInvariant());
        }
    }

    private void Alert_OnClosed(object sender, EventArgs e)
    {
        if (sender is FrameworkElement { DataContext: Alert alert })
        {
            Alerts.Remove(alert);
        }
    }

    private async void Alert_OnActionClicked(object sender, EventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: Alert alert })
        {
            return;
        }

        Alerts.Remove(alert);
        alert.Action?.Invoke(alert);
        if (!string.IsNullOrWhiteSpace(alert.Command))
        {
            await Interpreter.SendAsync(alert.Command);
        }

        if (!string.IsNullOrWhiteSpace(alert.JavaScript))
        {
            await Interpreter.RunScriptTextAsync(alert.JavaScript);
        }
    }

    /// <summary>
    /// Applies the settings shared by the game terminal and its back buffer.
    /// </summary>
    private static void InitializeTerminal(TextEditor terminal)
    {
        terminal.TextArea.TextView.Options.EnableEmailHyperlinks = false;
        terminal.TextArea.TextView.Options.EnableHyperlinks = false;
        terminal.TextArea.TextView.Options.ShowBoxForControlCharacters = false;
        terminal.TextArea.Caret.CaretBrush = Brushes.Transparent;
    }

    private void Settings_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        PropertyChangedEventManager.RemoveHandler(_transcript.Palette, Palette_OnPropertyChanged, string.Empty);
        _transcript.Palette = _settings.ColorTheme;
        PropertyChangedEventManager.AddHandler(_transcript.Palette, Palette_OnPropertyChanged, string.Empty);
        RedrawTranscript();
    }

    private void Palette_OnPropertyChanged(object? sender, PropertyChangedEventArgs e) => RedrawTranscript();

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record Session(TelnetConnection Connection, CancellationTokenSource Cancellation)
    {
        public TaskCompletionSource<bool> Connected { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
