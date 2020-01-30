using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalon.Common.Colors;

namespace Avalon.Common.Interfaces
{
    public interface IInterpreter
    {
        /// <summary>
        /// Sends a command string to the mud.
        /// </summary>
        /// <param name="cmd"></param>
        Task Send(string cmd);

        /// <summary>
        /// Sends a command string to the mud.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="silent">Whether the commands should be outputted to the game window.</param>
        Task Send(string cmd, bool silent, bool addToInputHistory);

        /// <summary>
        /// Connects to the mud server.  Requires that the event handlers for required events be passed in here where they will
        /// be wired up.
        /// </summary>        
        void Connect(EventHandler<string> lineReceived, EventHandler<string> dataReceived, EventHandler connectionClosed);

        /// <summary>
        /// Disconnects from the mud server if there is a connection.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Parses a command, also adds it to the history list.
        /// </summary>
        /// <param name="cmd"></param>
        List<string> ParseCommand(string cmd);

        /// <summary>
        /// Clears the History and resets the position.
        /// </summary>
        void ClearHistory();

        /// <summary>
        /// Adds a command into the history buffer.
        /// </summary>
        /// <param name="cmd"></param>
        void AddInputHistory(string cmd);

        /// <summary>
        /// Returns the next item in history.
        /// </summary>
        /// <returns></returns>
        string InputHistoryNext();

        /// <summary>
        /// Returns the current history position.
        /// </summary>
        /// <returns></returns>
        string InputHistoryCurrent();

        /// <summary>
        /// Returns the previous item in the history.
        /// </summary>
        /// <returns></returns>
        string InputHistoryPrevious();

        /// <summary>
        /// Tells the implementing window or form that it needs to echo some text to it's terminal.
        /// </summary>
        /// <param name="text"></param>
        void EchoText(string text);

        /// <summary>
        /// Tells the implementing window or form that it needs to echo some text to it's terminal.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="foregroundColor"></param>
        void EchoText(string text, AnsiColor foregroundColor);

        /// <summary>
        /// Tells the implementing window or form that it needs to echo some text to it's terminal.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="foregroundColor"></param>
        /// <param name="reverseColors"></param>
        void EchoText(string text, AnsiColor foregroundColor, bool reverseColors);

        /// <summary>
        /// Event handler when the interpreter needs to send data to echo on the client.
        /// </summary>
        event EventHandler Echo;

        /// <summary>
        /// The current position in the history the caller is at
        /// </summary>
        int InputHistoryPosition { get; set; }

        /// <summary>
        /// The TCP/IP TelnetClient that will handle communication to the game.
        /// </summary>
        ITelnetClient Telnet { get; set; }

        /// <summary>
        /// A list of the hash commands (we will avoid reflection).
        /// </summary>
        List<IHashCommand> HashCommands { get; set; }

        /// <summary>
        /// The Conveyor class that can be used to interact with the UI.
        /// </summary>
        IConveyor Conveyor { get; set; }
    }
}