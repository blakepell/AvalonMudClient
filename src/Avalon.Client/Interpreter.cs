using Argus.Extensions;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Avalon.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Avalon
{

    /// <summary>
    /// The main interpreter for taking the user's commands and doing something with them.
    /// </summary>
    public class Interpreter : IInterpreter
    {
        public Interpreter(IConveyor c)
        {
            this.Conveyor = c;

            // Reflect over all of the IHashCommands that are available.
            var type = typeof(IHashCommand);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => type.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

            foreach (var t in types)
            {
                var cmd = (IHashCommand)Activator.CreateInstance(t, this);
                HashCommands.Add(cmd);
            }
        }

        /// <summary>
        /// Sends a command string to the mud.
        /// </summary>
        /// <param name="cmd"></param>
        public async Task Send(string cmd)
        {
            await Send(cmd, false, true);
        }

        /// <summary>
        /// Sends a command string to the mud.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="silent">Whether the commands should be outputted to the game window.</param>
        public async Task Send(string cmd, bool silent, bool addToInputHistory)
        {
            // Add the whole thing to the command history
            if (addToInputHistory)
            {
                AddInputHistory(cmd);
            }

            _aliasRecursionDepth = 0;
            var cmdList = ParseCommand(cmd);

            foreach (string item in cmdList)
            {
                if (Telnet == null)
                {
                    EchoText("You are not connected to the game.", AnsiColors.Red);
                    return;
                }

                try
                {
                    if (item.SafeLeft(1) != "#")
                    {
                        // Show the command in the window that was sent.
                        if (!silent)
                        {
                            EchoText(item, AnsiColors.Yellow);
                        }

                        // Spam guard
                        if (App.Settings.ProfileSettings.SpamGuard)
                        {
                            // TODO, make this check carriage return and > 1 instead of > 2
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
                                EchoText("where", AnsiColors.Yellow);
                                await Telnet.Send("where");
                                _spamGuardCounter = 0;
                            }
                        }


                        await Telnet.Send(item);
                    }
                    else
                    {
                        var hashCmd = HashCommands.Where(x => x.Name == item.FirstWord()).FirstOrDefault();

                        if (hashCmd == null)
                        {
                            EchoText("");
                            EchoText($"--> Hash command '{item.FirstWord()}' was not found.", AnsiColors.Red);
                            EchoText("");
                        }
                        else
                        {
                            string parameters = item.RemoveWord(1);
                            hashCmd.Parameters = parameters;

                            if (hashCmd.IsAsync)
                            {
                                await hashCmd.ExecuteAsync();
                            }
                            else
                            {
                                hashCmd.Execute();
                            }

                        }

                    }
                }
                catch (Exception ex)
                {
                    EchoText(ex.Message, AnsiColors.Red);
                }
            }
        }

        /// <summary>
        /// Connects to the mud server.  Requires that the event handlers for required events be passed in here where they will
        /// be wired up.
        /// </summary>        
        public async void Connect(EventHandler<string> lineReceived, EventHandler<string> dataReceived, EventHandler connectionClosed)
        {
            try
            {
                if (Telnet != null)
                {
                    Telnet.Dispose();
                    Telnet = null;
                }

                EchoText($"Connecting: {App.Settings.ProfileSettings.IpAddress}:{App.Settings.ProfileSettings.Port}\r\n", AnsiColors.Cyan);
                var ctc = new CancellationTokenSource();
                Telnet = new TelnetClient(App.Settings.ProfileSettings.IpAddress, App.Settings.ProfileSettings.Port, TimeSpan.FromSeconds(0), ctc.Token);
                Telnet.ConnectionClosed += connectionClosed;
                Telnet.LineReceived += lineReceived;
                Telnet.DataReceived += dataReceived;
                await Telnet.Connect();
            }
            catch (Exception ex)
            {
                Telnet.Dispose();
                EchoText($"Connection Failed: {ex.Message}\r\n", AnsiColors.Red);
            }

        }

        /// <summary>
        /// Disconnects from the mud server if there is a connection.
        /// </summary>
        public void Disconnect()
        {
            if (Telnet != null)
            {
                Telnet.Dispose();
                Telnet = null;
                return;
            }
        }

        /// <summary>
        /// Parses a command, also adds it to the history list.
        /// </summary>
        /// <param name="cmd"></param>
        public List<string> ParseCommand(string cmd)
        {
            _aliasRecursionDepth += 1;

            if (_aliasRecursionDepth >= 5)
            {
                EchoText($"--> Alias error: Reached max recursion depth of {_aliasRecursionDepth}.", AnsiColors.Red);
                EchoText("", AnsiColors.Default);
                return new List<string>();
            }

            // Swap known variables out, both system and user defined. Strings are immutable but if no variable
            // is found it won't create a new string, it will return the reference to the current one.
            if (cmd.Contains("@"))
            {
                cmd = cmd.Replace("@Username", App.Settings.ProfileSettings.Username);
                cmd = cmd.Replace("@Password", App.Settings.ProfileSettings.Password);
                
                foreach (var item in App.Settings.ProfileSettings.Variables)
                {
                    cmd = cmd.Replace($"@{item.Key}", item.Value);
                }
            }

            // Get the current character
            string characterName = Conveyor.GetVariable("Character");

            // Split the list
            var initialList = cmd.Split(';').ToList();
            var list = new List<string>();

            foreach (var item in initialList)
            {
                var first = item.FirstArgument();
                var alias = App.Settings.ProfileSettings.AliasList.FirstOrDefault(x => x.AliasExpression == first.Item1 && x.Enabled == true && (x.Character == "" || x.Character == characterName));

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
                        EchoText($"--> Alias found but aliases are globally disabled.", AnsiColors.Red);
                        list.Add(item);
                        continue;
                    }

                    // Increment that we used it
                    alias.Count++;

                    // If the alias is defined as Lua it will be processed verbatim.  A lua alias anywhere in the command short circuits 
                    // any other input and only that Lua command gets run (and only the first one).
                    // TODO: Deal with % 1 variables for passing
                    // TODO: Make this work better.  Is an alias the right way to do this?
                    if (alias.IsLua)
                    {
                        list.Clear();

                        // TODO: Put this into it's own function.
                        // Alias where the arguments are specified, we will support up to 9 arguments at this time.
                        string lua = alias.Command;

                        // %0 will represent the entire matched string.
                        lua = lua.Replace("%0", first.Item2);

                        // %1-%9
                        for (int i = 1; i <= 9; i++)
                        {
                            lua = lua.Replace($"%{i}", first.Item2.ParseWord(i, " "));
                        }

                        list.Add($"#lua {lua}");
                        return list;
                    }

                    // We have an alias, see if it's a simple alias where we put all of the text at the end or if
                    // it's one where we'll let the user place the words where they need to be.
                    if (!alias.Command.Contains("%"))
                    {
                        list.AddRange(ParseCommand($"{alias.Command} {first.Item2}".Trim()));
                    }
                    else
                    {
                        // Alias where the arguments are specified, we will support up to 9 arguments at this time.
                        string aliasStr = alias.Command;

                        // %0 will represent the entire matched string.
                        aliasStr = aliasStr.Replace("%0", first.Item2);

                        // %1-%9
                        for (int i = 1; i <= 9; i++)
                        {
                            aliasStr = aliasStr.Replace($"%{i}", first.Item2.ParseWord(i, " "));
                        }

                        list.AddRange(ParseCommand(aliasStr));
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
            InputHistoryPosition = 0;
            InputHistory.Clear();
        }

        /// <summary>
        /// Adds a command into the history buffer.
        /// </summary>
        /// <param name="cmd"></param>
        public void AddInputHistory(string cmd)
        {
            // Add the command to the history if it wasn't a blank return
            if (cmd.Length > 0)
            {
                // If the command was a duplicate of the last command entered also don't enter it.
                // We will have people spamming stuff in the game and it doesn't make sense (nor does
                // any other client) dupliate those in the input history.
                if (InputHistory.Count > 0 && InputHistory.Last() == cmd)
                {
                    return;
                }

                // Don't add it if it's two letters or less, these are common for directions which are spammed.
                // n, e, s, w, ne, se, sw, nw, u, d, etc.  Also don't add it if it was the same as the previous
                // last command.
                if (cmd.Length > 2)
                {
                    this.InputHistory.Add(cmd);
                    this.Conveyor.SetVariable("LastCommand", cmd);
                }
                else
                {
                    if (!cmd.In(new string[] { "n", "s", "e", "w", "nw", "sw", "ne", "se", "u", "d" }))
                    {
                        this.InputHistory.Add(cmd);
                        this.Conveyor.SetVariable("LastCommand", cmd);
                    }
                }

            }

            // Reset the history position.
            InputHistoryPosition = 0;
        }

        /// <summary>
        /// Returns the next item in history.
        /// </summary>
        /// <returns></returns>
        public string InputHistoryNext()
        {
            if (InputHistoryPosition < InputHistory.Count - 1)
            {
                InputHistoryPosition += 1;
            }

            // No history available, return blank.
            if (InputHistoryPosition == 0 && InputHistory.Count == 0)
            {
                return "";
            }

            return InputHistory[(InputHistory.Count - InputHistoryPosition) - 1];
        }

        /// <summary>
        /// Returns the current history position.
        /// </summary>
        /// <returns></returns>
        public string InputHistoryCurrent()
        {
            return InputHistory[(InputHistory.Count - InputHistoryPosition) - 1];
        }

        /// <summary>
        /// Returns the previous item in the history.
        /// </summary>
        /// <returns></returns>
        public string InputHistoryPrevious()
        {
            if (InputHistoryPosition > 0)
            {
                InputHistoryPosition -= 1;
            }

            // No history available, return blank.
            if ((InputHistoryPosition == 0 && InputHistory.Count == 0) || InputHistoryPosition < 0)
            {
                return "";
            }

            return InputHistory[(InputHistory.Count - InputHistoryPosition) - 1];
        }

        /// <summary>
        /// Tells the implementing window or form that it needs to echo some text to it's terminal.
        /// </summary>
        /// <param name="text"></param>
        public void EchoText(string text)
        {
            var e = new EchoEventArgs
            {
                Text = $"{text}\r\n",
                UseDefaultColors = true,
                ForegroundColor = AnsiColors.Default
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
            };

            this.OnEcho(e);
        }

        /// <summary>
        /// Tells the implementing window or form that it needs to echo some text to it's terminal.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="foregroundColor"></param>
        /// <param name="reverseColors"></param>
        public void EchoText(string text, AnsiColor foregroundColor, bool reverseColors)
        {
            var e = new EchoEventArgs
            {
                Text = $"{text}\r\n",
                UseDefaultColors = false,
                ForegroundColor = foregroundColor,
                ReverseColors = reverseColors
            };

            this.OnEcho(e);
        }

        /// <summary>
        /// Event handler when the interpreter needs to send data to echo on the client.
        /// </summary>
        public event EventHandler Echo;

        /// <summary>
        /// Event handler when the interpreter needs to send data to echo on the client.
        /// </summary>
        /// <param name="e">The event arguments including the text, foreground and background colors to echo.</param>
        protected virtual void OnEcho(EchoEventArgs e)
        {
            var handler = Echo;
            handler?.Invoke(this, e);
        }

        private int _aliasRecursionDepth = 0;

        private string _spamGuardLastCommand = "";

        private int _spamGuardCounter = 0;

        /// <summary>
        /// The history of all commands.
        /// </summary>
        public List<string> InputHistory = new List<string>();

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
        public List<IHashCommand> HashCommands { get; set; } = new List<IHashCommand>();

        /// <summary>
        /// The Conveyor class that can be used to interact with the UI.
        /// </summary>
        public IConveyor Conveyor { get; set; }

    }

}
