using System;
using System.Threading.Tasks;

namespace Avalon.Common.Interfaces
{
    public interface ITelnetClient : IDisposable
    {
        /// <summary>
        /// Connect and wait for incoming messages. 
        /// When this task completes you are connected. 
        /// You cannot call this method twice; if you need to reconnect, dispose of this instance and create a new one.
        /// </summary>
        Task ConnectAsync();

        /// <summary>
        /// Connect via SOCKS4 proxy. See http://en.wikipedia.org/wiki/SOCKS#SOCKS4.
        /// When this task completes you are connected. 
        /// You cannot call this method twice; if you need to reconnect, dispose of this instance and create a new one.
        /// </summary>
        /// <param name="socks4ProxyHost"></param>
        /// <param name="socks4ProxyPort"></param>
        /// <param name="socks4ProxyUser"></param>
        Task ConnectAsync(string socks4ProxyHost, int socks4ProxyPort, string socks4ProxyUser);

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message"></param>
        Task SendAsync(string message);

        /// <summary>
        /// Main task that waits for messages from the server.
        /// </summary>
        Task WaitForMessageAsync();

        /// <summary>
        /// Disconnecting will leave TelnetClient in an unusable state.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Polls the TcpClient to see if the connection is still open.
        /// </summary>
        bool IsConnected();

        /// <summary>
        /// Event for when a line is received.
        /// </summary>
        EventHandler<string> LineReceived { get; set; }

        /// <summary>
        /// Event for when any data is received.
        /// </summary>
        EventHandler<string> DataReceived { get; set; }

        /// <summary>
        /// Event for when a connection is closed.
        /// </summary>
        EventHandler ConnectionClosed { get; set; }

        void TraceInformation(string text);
        void TraceError(string text);
        void OnLineReceived(string message);
        void OnDataReceived(string message);
        void OnConnectionClosed();
    }
}