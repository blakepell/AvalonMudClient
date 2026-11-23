using Avalon.Network;
using Avalon.Scripting;
using Mosaic.UI.Wpf.Controls.Scripting;
using Mosaic.UI.Wpf.Scripting;

namespace Avalon.Terminal;

public sealed partial class Interpreter
{
    private readonly List<ScriptEffect> _scriptEffects = [];
    private ScriptEnvironment? _environment;
    private string[] _loadedSources = [];
    private int _scriptDepth;
    private bool _flushQueued;

    private enum ScriptEffectKind { Send, SendRaw, VariableChanged, Gmcp, Execute }
    private sealed record ScriptEffect(ScriptEffectKind Kind, string Text, string? Json = null);

    /// <summary>
    /// Gets the scripting environment shared by this terminal's aliases, triggers, scripts and script editors.
    /// </summary>
    /// <remarks>
    /// Created on first use with the Mosaic defaults, Avalon's command objects, and every script in the game.
    /// </remarks>
    public ScriptEnvironment Environment
    {
        get
        {
            if (_environment is null)
            {
                _environment = ScriptEnvironmentFactory.Create(this);
                LoadScripts(force: true);
            }
            return _environment;
        }
    }

    /// <summary>
    /// Occurs when the number of running scripts changes.
    /// </summary>
    public event EventHandler? ScriptsActiveChanged;

    /// <summary>
    /// Re-runs every script definition in the environment, picking up edits, as Continuum's
    /// "Refresh Scripting Environment" does.
    /// </summary>
    public void RefreshScripts() => LoadScripts(force: true);

    /// <summary>
    /// Runs JavaScript with the terminal's environment, as the On Tick and profile events do.
    /// </summary>
    public Task RunScriptTextAsync(string code, CancellationToken cancellationToken = default)
        => string.IsNullOrWhiteSpace(code)
            ? Task.CompletedTask
            : InOrderAsync(() => RunScriptAsync(code, expression: false, silent: false, 0, cancellationToken), cancellationToken);

    private async Task RunGmcpScriptsAsync(GmcpMessage message, CancellationToken token)
    {
        bool handled = false;
        foreach (var source in _game.Scripts.ToArray())
        {
            if (source.ScriptType != ScriptType.JavaScript)
            {
                continue;
            }

            if (string.Equals(source.GmcpPackage, message.Package, StringComparison.OrdinalIgnoreCase))
            {
                handled = true;
                await RunScriptAsync(source.Code, expression: false, silent: false, 0, token, message);
            }
            else if (string.IsNullOrWhiteSpace(source.GmcpPackage) &&
                     source.Name.Equals(message.Package, StringComparison.OrdinalIgnoreCase))
            {
                handled = true;
                await InvokeScriptAsync(FunctionName(source.Name), [message.Json ?? "null"], silent: false, 0, token);
            }
        }

        // Like Continuum, an unknown package gets a handler stub the user can fill in.
        if (!handled && !string.IsNullOrWhiteSpace(message.Package) && _game.GmcpEnabled)
        {
            _game.Scripts.Add(new SourceCode
            {
                Name = message.Package,
                Description = $"Handles the {message.Package} GMCP package.",
                Code = $"function {FunctionName(message.Package)}(json) {{\n    // let data = JSON.parse(json);\n}}\n"
            });
        }
    }

    private static string FunctionName(string package) => package.Replace('.', '_').Replace(' ', '_');

    private Task<object?> RunScriptAsync(string code, bool expression, bool silent, int depth, CancellationToken token,
        GmcpMessage? message = null)
        => RunScriptActionAsync(async (engine, scriptToken) =>
        {
            var scope = engine.GlobalScope.NewChildScope();
            if (message is not null)
            {
                scope.SetValue("package", message.Package);
                scope.SetValue("json", message.Json ?? "null");
            }

            if (expression)
            {
                return await scope.ExecuteExpressionAsync(code, scriptToken);
            }

            await scope.ExecuteScriptAsync(code, scriptToken);
            return null;
        }, silent, depth, token);

    private Task<object?> InvokeScriptAsync(string function, object[] args, bool silent, int depth, CancellationToken token)
        => RunScriptActionAsync(async (engine, scriptToken) =>
            await engine.InvokeFunctionAsync(function, scriptToken, args), silent, depth, token);

    /// <summary>
    /// Calls a script function by name, used by #script.
    /// </summary>
    internal Task<object?> InvokeNestedAsync(string function, object[] args, bool silent, int depth, CancellationToken token)
        => InvokeScriptAsync(function, args, silent, depth + 1, token);

    private async Task<object?> RunScriptActionAsync(Func<ScriptEngine, CancellationToken, Task<object?>> action,
        bool silent, int depth, CancellationToken token)
    {
        if (depth >= MaxAutomationDepth)
        {
            Echo("Script execution exceeded the recursion limit.\n");
            return null;
        }

        using var scriptTimeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        scriptTimeout.CancelAfter(TimeSpan.FromSeconds(5));
        object? result;
        ScriptEffect[] effects;
        _scriptDepth++;
        AdjustScriptsActive(1);
        try
        {
            LoadScripts(force: false);
            result = await action(Environment.Engine, scriptTimeout.Token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            Echo("Script execution timed out.\n");
            return null;
        }
        catch (Exception ex)
        {
            EchoError($"Script error: {ex.Message}");
            return null;
        }
        finally
        {
            _scriptDepth--;
            AdjustScriptsActive(-1);
            effects = _scriptDepth == 0 ? DrainEffects() : [];
        }

        await ApplyEffectsAsync(effects, silent, depth, token);
        return result;
    }

    private ScriptEffect[] DrainEffects()
    {
        var effects = _scriptEffects.ToArray();
        _scriptEffects.Clear();
        return effects;
    }

    private async Task ApplyEffectsAsync(ScriptEffect[] effects, bool silent, int depth, CancellationToken token)
    {
        foreach (var effect in effects)
        {
            token.ThrowIfCancellationRequested();
            switch (effect.Kind)
            {
                case ScriptEffectKind.Send:
                    await SendCoreAsync(effect.Text, silent, depth + 1, token);
                    break;
                case ScriptEffectKind.SendRaw:
                    await SendLineAsync(effect.Text, token);
                    break;
                case ScriptEffectKind.VariableChanged:
                    await RunScriptAsync(effect.Text, expression: false, silent, depth + 1, token);
                    break;
                case ScriptEffectKind.Execute:
                    await RunScriptAsync(effect.Text, expression: false, silent, depth + 1, token);
                    break;
                case ScriptEffectKind.Gmcp:
                    await SendGmcpAsync(effect.Text, effect.Json, token);
                    break;
            }
        }
    }

    /// <summary>
    /// Queues an effect from a script.  Effects run after the current script returns so aliases and variable
    /// callbacks never re-enter an active script scope.  Effects queued outside an interpreter run (from the
    /// Interactive Script Editor, a timer or a button) are applied on the next turn of the dispatcher.
    /// </summary>
    private void QueueEffect(ScriptEffect effect)
    {
        _scriptEffects.Add(effect);
        if (_scriptDepth > 0 || _flushQueued)
        {
            return;
        }

        _flushQueued = true;
        System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            _flushQueued = false;
            if (_scriptDepth > 0)
            {
                return;
            }

            try
            {
                await InOrderAsync(() => ApplyEffectsAsync(DrainEffects(), silent: false, 0, CancellationToken.None), CancellationToken.None);
            }
            catch (Exception ex) when (ex is System.IO.IOException or InvalidOperationException or ObjectDisposedException)
            {
                EchoError(ex.Message);
            }
        });
    }

    internal void QueueSend(string command) => QueueEffect(new(ScriptEffectKind.Send, command));

    internal void QueueSendRaw(string command) => QueueEffect(new(ScriptEffectKind.SendRaw, command));

    internal void QueueGmcp(string package, string? json) => QueueEffect(new(ScriptEffectKind.Gmcp, package, json));

    internal void QueueScript(string code) => QueueEffect(new(ScriptEffectKind.Execute, code));

    internal void QueueSetVariable(string key, string value)
    {
        if (SetVariable(key, value) is { } onChange)
        {
            QueueEffect(new(ScriptEffectKind.VariableChanged, onChange));
        }
    }

    /// <summary>
    /// Updates this terminal's running script count for interpreter and editor runs.
    /// </summary>
    /// <param name="change">The amount to add to the running script count.</param>
    internal void AdjustScriptsActive(int change)
    {
        ScriptsActive = Math.Max(0, ScriptsActive + change);
        ScriptsActiveChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Runs every script definition (functions shared by all automation) into the engine when they change.
    /// </summary>
    private void LoadScripts(bool force)
    {
        var engine = Environment.Engine;
        var definitions = _game.Scripts.Where(source => source.ScriptType == ScriptType.JavaScript
             && string.IsNullOrWhiteSpace(source.GmcpPackage) && !string.IsNullOrWhiteSpace(source.Code))
            .Select(source => (source.Name, source.Code)).ToArray();
        var sources = definitions.Select(source => source.Code).ToArray();
        if (!force && sources.SequenceEqual(_loadedSources))
        {
            return;
        }

        _loadedSources = sources;
        foreach (var (name, code) in definitions)
        {
            try
            {
                // Script definitions are declarations, so they run synchronously without awaiting I/O.
                var task = engine.ExecuteScriptAsync(code, CancellationToken.None);
                if (!task.IsCompleted)
                {
                    task.ContinueWith(t => EchoError($"Script '{name}': {t.Exception?.GetBaseException().Message}"),
                        CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.FromCurrentSynchronizationContext());
                }
                else if (task.IsFaulted)
                {
                    EchoError($"Script '{name}': {task.Exception?.GetBaseException().Message}");
                }
            }
            catch (Exception ex)
            {
                EchoError($"Script '{name}': {ex.Message}");
            }
        }
    }
}
