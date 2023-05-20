/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Colors;
using Avalon.Common;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Avalon.Common.Scripting;
using Avalon.Extensions;
using Avalon.Lua;
using Avalon.Network;
using Cysharp.Text;
using MoonSharp.Interpreter;
using System.Threading;

namespace Avalon
{

    /// <summary>
    /// The main interpreter for taking the user's commands and doing something with them.
    /// </summary>
    public class Interpreter : IInterpreter
    {
        public Interpreter()
        {
            this.Conveyor = AppServices.GetService<Conveyor>();

            // Reflect over all of the IHashCommands that are available.
            var type = typeof(IHashCommand);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => type.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

            foreach (var t in types)
            {
                var cmd = (IHashCommand)Activator.CreateInstance(t, this);
                this.HashCommands.Add(cmd);
            }

            // Setup the scripting environment.  Error handlers are set here allowing the actual scripting
            // environment to stay generic while the client worries about the implementation details.
            _scriptCommands = new ScriptCommands(this);
            _winScriptCommands = new WindowScriptCommands(this);

            this.MoonSharpInit();
        }

        /// <summary>
        /// Sets up the MoonSharp Lua script engine.
        /// </summary>
        private void MoonSharpInit()
        {
            this.ScriptHost = AppServices.GetService<ScriptHost>();
            this.ScriptHost.RegisterObject<ScriptCommands>(_scriptCommands.GetType(), _scriptCommands, "lua");
            this.ScriptHost.RegisterObject<WindowScriptCommands>(_winScriptCommands.GetType(), _winScriptCommands, "win");

            // Events for before and after execution of a script.
            this.ScriptHost.MoonSharp.PreScriptExecute += (sender, e) =>
            {
                App.MainWindow.ViewModel.LuaScriptsActive = this.ScriptHost.Statistics.ScriptsActive;
            };

            this.ScriptHost.MoonSharp.PostScriptExecute += (sender, e) =>
            {
                App.MainWindow.ViewModel.LuaScriptsActive = this.ScriptHost.Statistics.ScriptsActive;
            };

            this.ScriptHost.MoonSharp.ExceptionHandler = (exd) =>
            {
                // InterpreterException's give us more info, like the line number and column the
                // error occurred on.
                if (exd.Exception is InterpreterException luaEx)
                {
                    this.Conveyor.EchoError($"Lua Exception: {luaEx.DecoratedMessage}");
                }
                else
                {
                    this.Conveyor.EchoError($"Lua Exception: {exd?.Exception?.Message ?? "(null)"}");
                }

                if (exd?.Exception?.InnerException is InterpreterException innerEx)
                {
                    this.Conveyor.EchoError($"Lua Inner Exception: {innerEx?.DecoratedMessage}");
                }

                if (!string.IsNullOrWhiteSpace(exd.FunctionName))
                {
                    this.Conveyor.EchoError($"Lua Function: {exd.FunctionName}");
                }

                if (!string.IsNullOrWhiteSpace(exd.Description))
                {
                    this.Conveyor.EchoDebug($"Lua Internal Data: {exd.Description}");
                }
            };
        }

        /// <summary>
        /// Sends a command string to the mud.
        /// </summary>
        /// <param name="cmd"></param>
        public Task Send(string cmd)
        {
            // Elided this call.
            return this.Send(cmd, false, true);
        }

        /// <summary>
        /// Sends a command string to the mud.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="silent">Whether the commands should be outputted to the game window.</param>
        /// <param name="addToInputHistory">Whether the command should be added to the input history the user can scroll back through.</param>
        public async Task Send(string cmd, bool silent, bool addToInputHistory)
        {
            // Add the whole thing to the command history
            if (addToInputHistory)
            {
                this.AddInputHistory(cmd);
            }

            _aliasRecursionDepth = 0;
            
            foreach (string item in this.ParseCommand(cmd))
            {
                if (this.Telnet == null)
                {
                    this.Conveyor.EchoLog("You are not connected to the game.", LogType.Error);
                    return;
                }

                try
                {
                    if (!item.StartsWith('#'))
                    {
                        // Show the command in the window that was sent.
                        if (!silent)
                        {
                            this.EchoText(item, AnsiColors.Yellow);
                        }

                        // Spam guard
                        if (App.Settings.ProfileSettings.SpamGuard)
                        {
                            if (_spamGuardLastCommand == item && item.Length > 2)
                            {
                                // Increment, we don't need to change the last command, it was the same.
                                _spamGuardCounter++;
                            }
                            else
                            {
                                _spamGuardLastCommand = item;
                                _spamGuardCounter = 0;
                            }

                            if (_spamGuardCounter == 15)
                            {
                                this.EchoText(App.Settings.ProfileSettings.SpamGuardCommand, AnsiColors.Yellow);
                                await this.Telnet.SendAsync(App.Settings.ProfileSettings.SpamGuardCommand);
                                _spamGuardCounter = 0;
                            }
                        }

                        await this.Telnet.SendAsync(item);
                    }
                    else
                    {
                        string firstWord = item.FirstWord();

                        // Avoided a closure allocation by loop instead of using Linq.
                        IHashCommand? hashCmd = null;

                        for (int index = 0; index < this.HashCommands.Count; index++)
                        {
                            var x = this.HashCommands[index];

                            if (x.Name.Equals(firstWord, StringComparison.Ordinal))
                            {
                                hashCmd = x;
                                break;
                            }
                        }

                        if (hashCmd == null)
                        {
                            this.Conveyor.EchoLog($"Hash command '{firstWord}' was not found.\r\n", LogType.Error);
                        }
                        else
                        {
                            hashCmd.Parameters = item.RemoveWord(1);

                            if (hashCmd.IsAsync)
                            {
                                await hashCmd.ExecuteAsync();
                            }
                            else
                            {
                                // ReSharper disable once MethodHasAsyncOverload
                                hashCmd.Execute();
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    App.Conveyor.EchoError(ex.Message);

                    if (this.Telnet == null || !this.Telnet.IsConnected())
                    {
                        App.Conveyor.SetText("Disconnected from server.", TextTarget.StatusBarText);
                    }
                }
            }

            // Add words to our unique HashSet if the settings allow for it (and after the
            // commands for the game have been sent.
            if (App.Settings.AvalonSettings.AutoCompleteWord && addToInputHistory)
            {
                InputAutoCompleteKeywords.AddWords(cmd);
            }
        }

        /// <summary>
        /// Sends a command string to the mud and does not do typical line processing like splitting commands, identifying
        /// if an alias was run, identifying if a hash command was run or tracking the spam guard.
        /// </summary>
        /// <param name="cmd">The raw unprocessed command to send.</param>
        /// <param name="silent">Whether the commands should be outputted to the game window.</param>
        public async Task SendRaw(string cmd, bool silent)
        {
            if (this.Telnet == null)
            {
                this.Conveyor.EchoLog("You are not connected to the game.", LogType.Error);
                return;
            }

            // Show the command in the window that was sent.
            if (!silent)
            {
                this.EchoText(cmd, AnsiColors.Yellow);
            }

            try
            {
                await this.Telnet.SendAsync(cmd);
            }
            catch (Exception ex)
            {
                App.Conveyor.EchoError(ex.Message);

                if (this.Telnet == null || !this.Telnet.IsConnected())
                {
                    App.Conveyor.SetText("Disconnected from server.");
                }
            }
        }

        /// <summary>
        /// Connects to the mud server.  Requires that the event handlers for required events be passed in here where they will
        /// be wired up.
        /// </summary>        
        public async Task Connect(EventHandler<string> lineReceived, EventHandler<string> dataReceived, EventHandler connectionClosed)
        {
            try
            {
                if (this.Telnet != null)
                {
                    this.Telnet.Dispose();
                    this.Telnet = null;
                }

                // If there was a last host and it was not the current IP to connect to it likely means
                // a new profile was loaded, in that case we're going to reset the ScriptingHost to it's default
                // so things aren't hanging around.
                if (!string.IsNullOrWhiteSpace(_lastHost) && !string.Equals(_lastHost, App.Settings.ProfileSettings.IpAddress))
                {
                    this.ScriptHost?.Reset();
                    this.Conveyor.EchoLog("Host change detected: Scripting environment reset.", LogType.Information);

                    // Refresh the scripts so they will load when needed.
                    this.ScriptHost?.RefreshScripts();
                }

                // We can set this now, when we come back in if the IP changes they we'll reset above.
                _lastHost = App.Settings.ProfileSettings.IpAddress;

                this.Conveyor.EchoLog($"Connecting: {App.Settings.ProfileSettings.IpAddress}:{App.Settings.ProfileSettings.Port}", LogType.Information);

                var ctc = new CancellationTokenSource();
                this.Telnet = new TelnetClient(App.Settings.ProfileSettings.IpAddress, App.Settings.ProfileSettings.Port, TimeSpan.FromSeconds(0), ctc.Token);
                this.Telnet.ConnectionClosed += connectionClosed;
                this.Telnet.LineReceived += lineReceived;
                this.Telnet.DataReceived += dataReceived;
                await this.Telnet.ConnectAsync();
            }
            catch (Exception ex)
            {
                this.Telnet?.Dispose();
                this.Conveyor.EchoLog($"Connection Failed: {ex.Message}", LogType.Error);
            }
        }

        /// <summary>
        /// Disconnects from the mud server if there is a connection.
        /// </summary>
        public void Disconnect()
        {
            this.Telnet?.Dispose();
            this.Telnet = null;
        }

        /// <summary>
        /// Parses a command, also adds it to the history list.
        /// </summary>
        /// <param name="cmd"></param>
        public List<string> ParseCommand(string cmd)
        {
            _aliasRecursionDepth += 1;

            if (_aliasRecursionDepth >= 10)
            {
                this.Conveyor.EchoLog($"Alias error: Reached max recursion depth of {_aliasRecursionDepth.ToString()}.\r\n", LogType.Error);
                return new List<string>();
            }

            // Swap known variables out, both system and user defined. Strings are immutable but if no variable
            // is found it won't create a new string, it will return the reference to the current one.
            cmd = this.Conveyor.ReplaceVariablesWithValue(cmd);

            // Get the current character
            string characterName = this.Conveyor.GetVariable("Character");

            // Split the list
            var list = new List<string>();

            foreach (var item in cmd.Split(App.Settings.AvalonSettings.CommandSplitCharacter))
            {
                var first = item.FirstArgument();

                Alias? alias = null;

                for (int index = 0; index < App.Settings.ProfileSettings.AliasList.Count; index++)
                {
                    var x = App.Settings.ProfileSettings.AliasList[index];

                    if (x.AliasExpression == first.Item1 && x.Enabled && (string.IsNullOrEmpty(x.Character) || x.Character == characterName))
                    {
                        alias = x;
                        break;
                    }
                }

                if (alias == null)
                {
                    // No alias was found, just add the item.
                    list.Add(item);
                }
                else
                {
                    // See if the aliases are globally disabled.  We're putting this here so we can let the user know they
                    // tried to use an alias but they're disabled.
                    if (!App.Settings.ProfileSettings.AliasesEnabled)
                    {
                        this.Conveyor.EchoError($"Alias found for '{alias.AliasExpression ?? "null"}' but aliases are globally disabled.");
                        list.Add(item);
                        continue;
                    }

                    // Increment that we used it
                    alias.Count++;

                    // If the alias is Lua then variables will be swapped in if necessary and then executed.
                    if (alias.IsLua || alias.ExecuteAs == ExecuteType.LuaMoonsharp)
                    {
                        list.Clear();
                        _ = this.ScriptHost.MoonSharp.ExecuteFunctionAsync<object>(alias.FunctionName, item.Split(' ', StringSplitOptions.RemoveEmptyEntries));

                        return list;
                    }

                    // We have an alias, see if it's a simple alias where we put all of the text at the end or if
                    // it's one where we'll let the user place the words where they need to be.
                    if (!alias.Command.Contains('%'))
                    {
                        list.AddRange(this.ParseCommand($"{alias.Command} {first.Item2}".Trim()));
                        _aliasRecursionDepth--;
                    }
                    else
                    {
                        // Alias where the arguments are specified, we will support up to 9 arguments at this time.
                        if (alias.Command.Contains("%", StringComparison.Ordinal))
                        {
                            var sb = ZString.CreateStringBuilder();
                            sb.Append(alias.Command);

                            // %0 will represent the entire matched string.
                            sb.Replace("%0", first.Item2);

                            // %1-%9
                            for (int i = 1; i <= 9; i++)
                            {
                                sb.Replace($"%{i.ToString()}", first.Item2.ParseWord(i, " "));
                            }

                            list.AddRange(this.ParseCommand(sb.ToString()));
                            sb.Dispose();

                            _aliasRecursionDepth--;
                        }
                        else
                        {
                            list.AddRange(this.ParseCommand(alias.Command));
                            _aliasRecursionDepth--;
                        }
                    }
                }
            }

            // Return the final list.
            return list;
        }

        /// <summary>
        /// Clears the History and resets the position.
        /// </summary>
        public void ClearHistory()
        {
            this.InputHistoryPosition = 0;
            InputHistory.Clear();
        }

        private static HashSet<string> _directions = new() { "n", "s", "e", "w", "nw", "sw", "ne", "se", "u", "d" };

        /// <summary>
        /// Adds a command into the history buffer.
        /// </summary>
        /// <param name="cmd"></param>
        public void AddInputHistory(string cmd)
        {
            // Tracking all commands that make it here because they are recording them.  This will track -everything-, they can
            // sort out the results as they see fit then.
            if (this.IsRecordingCommands)
            {
                this.RecordedCommands.Add(cmd);
            }

            // Add the command to the history if it wasn't a blank return
            if (cmd.Length > 0)
            {
                // If the command was a duplicate of the last command entered also don't enter it.
                // We will have people spamming stuff in the game and it doesn't make sense (nor does
                // any other client) duplicate those in the input history.
                if (InputHistory.Count > 0 && InputHistory.Last() == cmd)
                {
                    return;
                }

                // Don't add it if it's two letters or less, these are common for directions which are spammed.
                // n, e, s, w, ne, se, sw, nw, u, d, etc.  Also don't add it if it was the same as the previous
                // last command.
                if (cmd.Length > 2)
                {
                    InputHistory.Add(cmd);
                    this.Conveyor.SetVariable("LastCommand", cmd);
                }
                else
                {
                    if (!_directions.Contains(cmd))
                    {
                        InputHistory.Add(cmd);
                        this.Conveyor.SetVariable("LastCommand", cmd);
                    }
                }

            }

            // Reset the history position.
            this.InputHistoryPosition = 0;
        }

        /// <summary>
        /// Returns the next item in history.
        /// </summary>
        public string InputHistoryNext()
        {
            if (this.InputHistoryPosition < InputHistory.Count - 1)
            {
                this.InputHistoryPosition += 1;
            }

            // No history available, return blank.
            if (this.InputHistoryPosition == 0 && InputHistory.Count == 0)
            {
                return "";
            }

            return InputHistory[(InputHistory.Count - this.InputHistoryPosition) - 1];
        }

        /// <summary>
        /// Returns the current history position.
        /// </summary>
        public string InputHistoryCurrent()
        {
            return InputHistory[(InputHistory.Count - this.InputHistoryPosition) - 1];
        }

        /// <summary>
        /// Returns the previous item in the history.
        /// </summary>
        public string InputHistoryPrevious()
        {
            if (this.InputHistoryPosition > 0)
            {
                this.InputHistoryPosition -= 1;
            }

            // No history available, return blank.
            if ((this.InputHistoryPosition == 0 && InputHistory.Count == 0) || this.InputHistoryPosition < 0)
            {
                return "";
            }

            return InputHistory[(InputHistory.Count - this.InputHistoryPosition) - 1];
        }

        /// <summary>
        /// Tells the implementing window or form that it needs to echo some text to it's terminal.
        /// </summary>
        /// <param name="text"></param>
        public void EchoText(string text)
        {
            var sb = Argus.Memory.StringBuilderPool.Take(text);
            Colorizer.MudToAnsiColorCodes(sb);

            var e = new EchoEventArgs
            {
                Text = $"{sb}\r\n",
                UseDefaultColors = true,
                ForegroundColor = AnsiColors.Default,
                Terminal = TerminalTarget.Main
            };

            Argus.Memory.StringBuilderPool.Return(sb);

            this.OnEcho(e);
        }

        /// <summary>
        /// Tells the implementing window or form that it needs to echo some text to it's terminal.
        /// </summary>
        /// <param name="sb"></param>
        public void EchoText(StringBuilder sb)
        {
            Colorizer.MudToAnsiColorCodes(sb);

            var e = new EchoEventArgs
            {
                Text = $"{sb}\r\n",
                UseDefaultColors = true,
                ForegroundColor = AnsiColors.Default,
                Terminal = TerminalTarget.Main
            };

            this.OnEcho(e);
        }

        /// <summary>
        /// Tells the implementing window or form that it needs to echo some text to it's terminal.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="foregroundColor"></param>
        public void EchoText(string text, AnsiColor foregroundColor)
        {
            var e = new EchoEventArgs
            {
                Text = $"{text}\r\n",
                UseDefaultColors = false,
                ForegroundColor = foregroundColor,
                Terminal = TerminalTarget.Main
            };

            this.OnEcho(e);
        }

        /// <summary>
        /// Tells the implementing window or form that it needs to echo some text to it's terminal.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="foregroundColor"></param>
        /// <param name="reverseColors"></param>
        /// <param name="terminal">The terminal that the main window should try to echo to.</param>
        /// <param name="processMudColors">Whether or not the mud color codes prefixed with an '{' should be processed into color.</param>
        public void EchoText(string text, AnsiColor foregroundColor, bool reverseColors, TerminalTarget terminal, bool processMudColors)
        {
            var sb = Argus.Memory.StringBuilderPool.Take(text);

            if (processMudColors)
            {
                Colorizer.MudToAnsiColorCodes(sb);
            }

            var e = new EchoEventArgs
            {
                Text = $"{sb}\r\n",
                UseDefaultColors = false,
                ForegroundColor = foregroundColor,
                ReverseColors = reverseColors,
                Terminal = terminal
            };

            Argus.Memory.StringBuilderPool.Return(sb);

            this.OnEcho(e);
        }

        /// <summary>
        /// Event handler when the interpreter needs to send data to echo on the client.
        /// </summary>
        public event EventHandler<EchoEventArgs> Echo;

        /// <summary>
        /// Event handler when the interpreter needs to send data to echo on the client.
        /// </summary>
        /// <param name="e">The event arguments including the text, foreground and background colors to echo.</param>
        private void OnEcho(EchoEventArgs e)
        {
            var handler = this.Echo;
            handler?.Invoke(this, e);
        }

        private int _aliasRecursionDepth = 0;

        private string _spamGuardLastCommand = "";

        private int _spamGuardCounter = 0;

        /// <summary>
        /// The last host the client connected to (used to determine if the host has changed).
        /// </summary>
        private string _lastHost = "";

        /// <summary>
        /// A list of all unique words that have been entered as part of a command.  If the setting
        /// is enabled this will be used to auto-complete a word based on a "shift+tab" input.
        /// </summary>
        public List<string> InputAutoCompleteKeywords = new();

        /// <summary>
        /// The history of all commands.
        /// </summary>
        public List<string> InputHistory = new();

        /// <summary>
        /// The current position in the history the caller is at
        /// </summary>
        public int InputHistoryPosition { get; set; } = 0;

        /// <summary>
        /// The TCP/IP TelnetClient that will handle communication to the game.
        /// </summary>
        public ITelnetClient Telnet { get; set; }

        /// <summary>
        /// A list of the hash commands (we will avoid reflection).
        /// </summary>
        public List<IHashCommand> HashCommands { get; set; } = new();

        /// <summary>
        /// The Conveyor class that can be used to interact with the UI.
        /// </summary>
        public IConveyor Conveyor { get; set; }

        /// <summary>
        /// Whether or not the commands should be recorded into 
        /// </summary>
        public bool IsRecordingCommands { get; set; } = false;

        /// <summary>
        /// Commands that have been recorded
        /// </summary>
        public List<string> RecordedCommands { get; set; } = new();

        /// <summary>
        /// The Scripting host that contains all of our supported scripting environments.
        /// </summary>
        public ScriptHost ScriptHost { get; set; }

        /// <summary>
        /// An interop object that allows scripts to interact with the .NET environment.
        /// </summary>
        private static ScriptCommands _scriptCommands;

        /// <summary>
        /// An interop object that allows scripts to interact with our Window API.
        /// </summary>
        public static WindowScriptCommands _winScriptCommands;

    }

}
