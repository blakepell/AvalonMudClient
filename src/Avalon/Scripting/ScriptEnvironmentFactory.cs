using System.Windows;
using Argus.Memory;
using Avalon.Models;
using Trigger = Avalon.Models.Trigger;
using Avalon.Terminal;
using Mosaic.UI.Wpf.Controls.Scripting;
using Mosaic.UI.Wpf.AvalonDock.Layout;

namespace Avalon.Scripting;

/// <summary>
/// Builds the scripting environment aliases, triggers and the script editors share.
/// </summary>
/// <remarks>
/// Starts from Mosaic's defaults (JSON, globals, process, hash, clipboard, http, screenshot, mouse, environ, log,
/// ai, regex and the type aliases) and adds Avalon's objects, matching the globals Continuum scripts expect:
/// terminal, ui, db, audio, game, settings and the model types.
/// </remarks>
public static class ScriptEnvironmentFactory
{
    private static ScriptEnvironment? _standalone;

    /// <summary>
    /// Creates the environment for a terminal's interpreter.
    /// </summary>
    public static ScriptEnvironment Create(Interpreter interpreter)
    {
        var environment = CreateForProfile(interpreter.Game);
        environment.RegisterObject("terminal", new TerminalScriptCommands(interpreter));
        return environment;
    }

    /// <summary>
    /// Creates an environment for editing a profile whose terminal is closed.
    /// </summary>
    public static ScriptEnvironment CreateForProfile(Game profile)
    {
        var environment = CreateBase();
        environment.RegisterObject("game", profile);
        // Continuum scripts use "profile" for the same object.
        environment.RegisterObject("profile", profile);
        environment.RegisterObject("vm", profile.ViewModel);
        return environment;
    }

    /// <summary>
    /// Returns the active terminal's environment, or a shared one without a terminal when no game is open.
    /// </summary>
    public static ScriptEnvironment ForActiveGame()
    {
        if (AppServices.IsRegistered<AppViewModel>() && AppServices.GetRequiredService<AppViewModel>().ActiveTerminal is { } terminal)
        {
            return terminal.Interpreter.Environment;
        }

        return _standalone ??= CreateBase();
    }

    private static ScriptEnvironment CreateBase()
    {
        // Script runs stay on the UI thread: the game, terminal and settings objects are bound to WPF.
        var environment = new ScriptEnvironment { ExecuteOnWorkerThread = false };
        environment.RegisterType(typeof(Thickness), "Thickness");
        environment.RegisterType(typeof(Game), "Profile");
        environment.RegisterType(typeof(AnchorSide), "Side");
        environment.RegisterType(typeof(ExecuteType), nameof(ExecuteType));
        environment.RegisterObject("ui", new UiScriptCommands());
        environment.RegisterObject("db", DbScriptCommands.Instance);
        environment.RegisterObject("audio", new AudioScriptCommands());
        if (AppServices.IsRegistered<AppSettings>())
        {
            var settings = AppServices.GetRequiredService<AppSettings>();
            environment.RegisterObject("settings", settings);
            if (settings.ClientSettings is { } client)
            {
                environment.RegisterObject("client", client);
            }
        }

        foreach (var type in new[] { typeof(Alias), typeof(Trigger), typeof(HighlightTrigger), typeof(Variable),
                     typeof(Direction), typeof(Alert), typeof(Macro), typeof(SourceCode), typeof(InfoBarSeverity), typeof(Affect) })
        {
            environment.RegisterType(type, type.Name);
        }

        environment.Import("Avalon.Models");
        environment.Engine.AddExtensionMethods(typeof(ScriptCompatibilityExtensions));
        return environment;
    }
}
