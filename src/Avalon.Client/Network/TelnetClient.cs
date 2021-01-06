using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalon.Common.Interfaces;

namespace Avalon.Network
{

    /// <summary>
    /// Telnet client.
    /// </summary>
    public class TelnetClient : IDisposable, ITelnetClient
    {
        private readonly int _port;
        private readonly string _host;
        private readonly TimeSpan _sendRate;
        private readonly SemaphoreSlim _sendRateLimit;
        private readonly CancellationTokenSource _internalCancellation;
        private readonly bool _traceEnabled = true;
        private const byte GO_AHEAD_CODE = 0xF9;

        private TcpClient _tcpClient;
        private StreamReader _tcpReader;
        private StreamWriter _tcpWriter;

        public EventHandler<string> LineReceived { get; set; }
        public EventHandler<string> DataReceived { get; set; }
        public EventHandler ConnectionClosed { get; set; }

        /// <summary>
        /// Simple telnet client
        /// </summary>
        /// <param name="host">Destination Hostname or IP</param>
        /// <param name="port">Destination TCP port number</param>
        /// <param name="sendRate">Minimum time span between sends. This is a throttle to prevent flooding the server.</param>
        /// <param name="token"></param>
        public TelnetClient(string host, int port, TimeSpan sendRate, CancellationToken token)
        {
            _host = host;
            _port = port;
            _sendRate = sendRate;
            _sendRateLimit = new SemaphoreSlim(1);
            _internalCancellation = new CancellationTokenSource();

            token.Register(() => _internalCancellation.Cancel());
        }

        /// <summary>
        /// Connect and wait for incoming messages. 
        /// When this task completes you are connected. 
        /// You cannot call this method twice; if you need to reconnect, dispose of this instance and create a new one.
        /// </summary>
        public async Task ConnectAsync()
        {
            if (_tcpClient != null)
            {
                throw new NotSupportedException($"{nameof(ConnectAsync)} aborted: Reconnecting is not supported. You must dispose of this instance and instantiate a new TelnetClient");
            }

            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(_host, _port);

            _tcpReader = new StreamReader(_tcpClient.GetStream());
            _tcpWriter = new StreamWriter(_tcpClient.GetStream()) { AutoFlush = true };

            // Fire-and-forget looping task that waits for messages to arrive
            WaitForMessageAsync();
        }

        /// <summary>
        /// Connect via SOCKS4 proxy. See http://en.wikipedia.org/wiki/SOCKS#SOCKS4.
        /// When this task completes you are connected. 
        /// You cannot call this method twice; if you need to reconnect, dispose of this instance and create a new one.
        /// </summary>
        /// <param name="socks4ProxyHost"></param>
        /// <param name="socks4ProxyPort"></param>
        /// <param name="socks4ProxyUser"></param>
        public async Task ConnectAsync(string socks4ProxyHost, int socks4ProxyPort, string socks4ProxyUser)
        {
            if (_tcpClient != null)
            {
                throw new NotSupportedException($"{nameof(ConnectAsync)} aborted: Reconnecting is not supported. You must dispose of this instance and instantiate a new TelnetClient");
            }

            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(socks4ProxyHost, socks4ProxyPort);

            // Simple implementation of http://en.wikipedia.org/wiki/SOCKS#SOCKS4
            // Similar to http://biko.codeplex.com/
            byte[] hostAddress = (await Dns.GetHostAddressesAsync(_host)).First().GetAddressBytes();
            byte[] hostPort = new byte[2]; // 16-bit number
            hostPort[0] = Convert.ToByte(_port / 256);
            hostPort[1] = Convert.ToByte(_port % 256);
            byte[] proxyUserId = Encoding.ASCII.GetBytes(socks4ProxyUser ?? string.Empty); // Can't pass in null

            // Request
            // - We build a "please connect me" packet to send to the proxy
            byte[] proxyRequest = new byte[9 + proxyUserId.Length];

            proxyRequest[0] = 4; // SOCKS4;
            proxyRequest[1] = 0x01; // Connect (we don't support Bind);

            hostPort.CopyTo(proxyRequest, 2);
            hostAddress.CopyTo(proxyRequest, 4);
            proxyUserId.CopyTo(proxyRequest, 8);

            proxyRequest[8 + proxyUserId.Length] = 0x00; // UserId terminator

            // Send proxy request
            // - Then we wait for an ack
            // - If successful, we can use the TelnetClient directly and traffic will be proxied
            await _tcpClient.GetStream().WriteAsync(proxyRequest, 0, proxyRequest.Length, _internalCancellation.Token);

            // Response
            // - First byte is null
            // - Second byte is our result code (we want 0x5a Request granted)
            // - Last 6 bytes should be ignored
            byte[] proxyResponse = new byte[8];

            // Wait for proxy response
            await _tcpClient.GetStream().ReadAsync(proxyResponse, 0, proxyResponse.Length, _internalCancellation.Token);

            if (proxyResponse[1] != 0x5a) // Request granted
            {
                switch (proxyResponse[1])
                {
                    case 0x5b:
                        throw new InvalidOperationException("Proxy connect request rejected or failed");
                    case 0x5c:
                        throw new InvalidOperationException("Proxy connect request failed because client is not running identd (or not reachable from the server)");
                    case 0x5d:
                        throw new InvalidOperationException("Proxy connect request failed because client's identd could not confirm the user ID string in the request");
                    default:
                        throw new InvalidOperationException("Proxy connect request failed, unknown error occurred");
                }
            }

            _tcpReader = new StreamReader(_tcpClient.GetStream());
            _tcpWriter = new StreamWriter(_tcpClient.GetStream()) { AutoFlush = true };

            // Fire-and-forget looping task that waits for messages to arrive
            WaitForMessageAsync();
        }

        public async Task SendAsync(string message)
        {
            try
            {
                // Wait for any previous send commands to finish and release the semaphore
                // This throttles our commands
                await _sendRateLimit.WaitAsync(_internalCancellation.Token);

                // Send command + params
                await _tcpWriter.WriteLineAsync(message);

                // Block other commands until our timeout to prevent flooding
                await Task.Delay(_sendRate, _internalCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                // We're waiting to release our semaphore, and someone cancelled the task on us (I'm looking at you, WaitForMessages...)
                // This happens if we've just sent something and then disconnect immediately
                TraceInformation($"{nameof(SendAsync)} aborted: {nameof(_internalCancellation.IsCancellationRequested)} == true");
            }
            catch (ObjectDisposedException)
            {
                // This happens during ReadLineAsync() when we call Disconnect() and close the underlying stream
                // This is an expected exception during disconnection if we're in the middle of a send
                TraceInformation($"{nameof(SendAsync)} failed: {nameof(_tcpWriter)} or {nameof(_tcpWriter.BaseStream)} disposed");
            }
            catch (IOException)
            {
                // This happens when we start WriteLineAsync() if the socket is disconnected unexpectedly
                TraceError($"{nameof(SendAsync)} failed: Socket disconnected unexpectedly");
                throw;
            }
            catch (Exception error)
            {
                TraceError($"{nameof(SendAsync)} failed: {error}");
                throw;
            }
            finally
            {
                // Exit our semaphore
                _sendRateLimit.Release();
            }
        }

        public async Task WaitForMessageAsync()
        {
            // We're going to store the receiveBuffer and it's string value in these variables that will
            // be cleared.  This is more of a micro optimization.  They will need to be cleared each iteration.
            var receiveBuffer = new char[4096];
            var lineBuffer = new StringBuilder(4096);
            var dataBuffer = new StringBuilder(4096);

            try
            {
                while (true)
                {
                    if (_internalCancellation.IsCancellationRequested)
                    {
                        TraceInformation($"{nameof(WaitForMessageAsync)} aborted: {nameof(_internalCancellation.IsCancellationRequested)} == true");
                        break;
                    }

                    // Clear out buffers
                    Array.Clear(receiveBuffer, 0, receiveBuffer.Length);

                    try
                    {
                        if (!_tcpClient.Connected)
                        {
                            TraceInformation($"{nameof(WaitForMessageAsync)} aborted: {nameof(_tcpClient)} is not connected");
                            break;
                        }

                        int dataReceivedLength = await _tcpReader.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);

                        // No data, break!
                        if (dataReceivedLength == 0)
                        {
                            break;
                        }

                        // The data buffer will be stored regardless of whether there is a line and will be sent
                        // out as DataReceived so that the UI can render it.  The actual line will be sent separate
                        // because we don't know when the line terminator will come indicating a full line has
                        // been sent and can be processed for triggers and whatever else as a complete line.
                        for (int i = 0; i < receiveBuffer.Length; i++)
                        {
                            // If it's a carriage return or a null character ignore it and move on.
                            if (receiveBuffer[i] == 13 || receiveBuffer[i] == 0)
                            {
                                continue;
                            }

                            dataBuffer.Append(receiveBuffer[i]);

                            // This was a newline or a telnetga (telnet go ahead), process it like it's a line.
                            if (receiveBuffer[i] == 10 || receiveBuffer[i] == GO_AHEAD_CODE)
                            {
                                // A complete line was found, send it on to the line handler.  Send the real
                                // time data first to the OnDataReceived.
                                this.OnDataReceived(dataBuffer.ToString());
                                this.OnLineReceived(lineBuffer.ToString());
                                dataBuffer.Clear();
                                lineBuffer.Clear();
                                continue;
                            }

                            lineBuffer.Append(receiveBuffer[i]);
                        }

                        // We had data, a partial line, go ahead send it so the OnDataReceived event so it can be
                        // processed in real time if required.
                        if (dataBuffer.Length > 0)
                        {
                            this.OnDataReceived(dataBuffer.ToString());
                            dataBuffer.Clear();
                        }

                        if (receiveBuffer == null)
                        {
                            TraceInformation($"{nameof(WaitForMessageAsync)} aborted: {nameof(_tcpReader)} reached end of stream");
                            break;
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // This happens during ReadLineAsync() when we call Disconnect() and close the underlying stream
                        // This is an expected exception during disconnection
                        TraceInformation($"{nameof(WaitForMessageAsync)} aborted: {nameof(_tcpReader)} or {nameof(_tcpReader.BaseStream)} disposed. This is expected after calling Disconnect()");
                        break;
                    }
                    catch (IOException)
                    {
                        // This happens when we start ReadLineAsync() if the socket is disconnected unexpectedly
                        TraceError($"{nameof(WaitForMessageAsync)} aborted: Socket disconnected unexpectedly");
                        break;
                    }
                    catch (Exception error)
                    {
                        TraceError($"{nameof(WaitForMessageAsync)} aborted: {error}");
                        break;
                    }

                }
            }
            finally
            {
                TraceInformation($"{nameof(WaitForMessageAsync)} completed");
            }
        }

        /// <summary>
        /// Disconnecting will leave TelnetClient in an unusable state.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                // Blow up any outstanding tasks
                _internalCancellation.Cancel();

                // Both reader and writer use the TcpClient.GetStream(), and closing them will close the underlying stream
                // So closing the stream for TcpClient is redundant but it means we're triple sure.
                _tcpReader?.Close();
                _tcpReader?.Dispose();
                _tcpReader = null;

                _tcpWriter?.Close();
                _tcpWriter?.Dispose();
                _tcpWriter = null;

                _tcpClient?.Close();
                _tcpClient?.Dispose();
                _tcpClient = null;
            }
            catch (Exception ex)
            {
                TraceError($"{nameof(Disconnect)} error: {ex}");
            }
            finally
            {
                OnConnectionClosed();
            }
        }

        /// <summary>
        /// A hard check of whether the TCP/IP connection is still open by peeking.
        /// </summary>
        public bool IsConnected()
        {
            try
            {
                if (!_tcpClient.Connected)
                {
                    return false;
                }

                if (_tcpClient.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (_tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                    {
                        // Client disconnected
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void TraceInformation(string text)
        {
            if (_traceEnabled)
            {
                Trace.TraceInformation(text);
            }
        }

        public void TraceError(string text)
        {
            if (_traceEnabled)
            {
                Trace.TraceError(text);
            }
        }

        public void OnLineReceived(string message)
        {
            LineReceived?.Invoke(this, message);
        }

        public void OnDataReceived(string message)
        {
            DataReceived?.Invoke(this, message);
        }

        public void OnConnectionClosed()
        {
            ConnectionClosed?.Invoke(this, new EventArgs());
        }

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Disconnect();
            }

            _disposed = true;
        }
    }
}