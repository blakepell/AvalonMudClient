using System.Buffers;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Avalon.Network;

/// <summary>
/// Connects to a MUD over Telnet and exposes its text and protocol messages in wire order.
/// </summary>
/// <param name="enableGmcp">Whether to accept GMCP packages.</param>
/// <param name="encoding">The ASCII-compatible encoding for game text. The default is UTF-8.</param>
public sealed class TelnetConnection(bool enableGmcp = true, Encoding? encoding = null) : IDisposable
{
    private const byte Iac = 255;
    private const byte Dont = 254;
    private const byte Do = 253;
    private const byte Wont = 252;
    private const byte Will = 251;
    private const byte SubBegin = 250;
    private const byte GoAhead = 249;
    private const byte SubEnd = 240;
    private const byte EndOfRecord = 239;

    private const byte Echo = 1;
    private const byte SuppressGoAhead = 3;
    private const byte TerminalType = 24;
    private const byte RecordOption = 25;
    private const byte Gmcp = 201;
    private const int MaxSubnegotiationSize = 1024 * 1024;
    private static readonly TimeSpan UnterminatedPromptDelay = TimeSpan.FromMilliseconds(250);

    private readonly TcpClient _client = new() { NoDelay = true };
    private readonly SemaphoreSlim _writeGate = new(1, 1);
    private readonly Encoding _encoding = encoding ?? Encoding.UTF8;
    private readonly bool[] _localOptions = new bool[256];
    private readonly bool[] _remoteOptions = new bool[256];
    private NetworkStream? _stream;
    private int _started;
    private int _reading;
    private volatile bool _disposed;
    private volatile bool _gmcpNegotiated;
    private volatile bool _gmcpEnabled = enableGmcp;

    /// <summary>
    /// Gets or sets whether received GMCP packages are passed on. Turning this off mid-session drops
    /// packages without renegotiating, since the server may not support turning the option back on.
    /// </summary>
    public bool GmcpEnabled
    {
        get => _gmcpEnabled;
        set => _gmcpEnabled = value;
    }

    /// <summary>
    /// Opens the connection. Create a new instance to reconnect.
    /// </summary>
    /// <param name="host">The hostname or IP address of the server.</param>
    /// <param name="port">The server's TCP port.</param>
    /// <param name="cancellationToken">A token that cancels connection establishment.</param>
    public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (Interlocked.Exchange(ref _started, 1) != 0)
        {
            throw new InvalidOperationException("Create a new Telnet connection to reconnect.");
        }

        try
        {
            await _client.ConnectAsync(host, port, cancellationToken).ConfigureAwait(false);
            _stream = _client.GetStream();
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    /// <summary>
    /// Reads incoming game text, prompt boundaries, echo changes, and GMCP packages.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the read loop.</param>
    /// <returns>An ordered stream of messages. Enumerate it once per connection.</returns>
    public async IAsyncEnumerable<TelnetMessage> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stream = GetStream();
        if (Interlocked.Exchange(ref _reading, 1) != 0)
        {
            throw new InvalidOperationException("The Telnet connection already has a reader.");
        }

        var parser = new Parser(_encoding);
        var buffer = new byte[8192];
        Task<int>? pendingRead = null;
        bool hasUnterminatedText = false;
        try
        {
            while (true)
            {
                pendingRead ??= stream.ReadAsync(buffer.AsMemory(), cancellationToken).AsTask();
                if (hasUnterminatedText)
                {
                    // A server can stop at a prompt without sending a newline, GA, or EOR.
                    // Keep the read pending across the idle boundary so incoming bytes aren't lost.
                    if (!await CompletesBeforePromptTimeoutAsync(pendingRead, cancellationToken).ConfigureAwait(false))
                    {
                        hasUnterminatedText = false;
                        yield return new TelnetMessage.Prompt();
                    }
                }

                int count = await pendingRead.ConfigureAwait(false);
                pendingRead = null;
                if (count == 0)
                {
                    break;
                }

                foreach (var item in parser.Parse(buffer.AsSpan(0, count)))
                {
                    switch (item)
                    {
                        case Negotiation negotiation:
                            if (await NegotiateAsync(negotiation, cancellationToken).ConfigureAwait(false) is { } echo)
                            {
                                yield return echo;
                            }

                            break;
                        case Subnegotiation { Option: Gmcp } sub when _gmcpEnabled:
                            if (GmcpMessage.Parse(Encoding.UTF8.GetString(sub.Data)) is { } gmcp)
                            {
                                yield return new TelnetMessage.Gmcp(gmcp);
                            }

                            break;
                        case Subnegotiation { Option: TerminalType, Data: [1] } when _localOptions[TerminalType]:
                            await WriteSubnegotiationAsync(TerminalType, "\0Avalon"u8.ToArray(), cancellationToken)
                                .ConfigureAwait(false);
                            break;
                        case Subnegotiation:
                            break;
                        case TelnetMessage.Text text:
                            hasUnterminatedText = !text.Value.EndsWith('\n');
                            yield return text;
                            break;
                        case TelnetMessage.Prompt prompt:
                            hasUnterminatedText = false;
                            yield return prompt;
                            break;
                        default:
                            yield return item;
                            break;
                    }
                }
            }

            if (parser.Finish() is { } remaining)
            {
                hasUnterminatedText = !remaining.Value.EndsWith('\n');
                yield return remaining;
            }

            if (hasUnterminatedText)
            {
                yield return new TelnetMessage.Prompt();
            }
        }
        finally
        {
            Dispose();
            if (pendingRead is not null)
            {
                // Observe completion if enumeration stops while an idle-boundary read is in flight.
                await ((Task)pendingRead).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
        }
    }

    private static async Task<bool> CompletesBeforePromptTimeoutAsync(Task read, CancellationToken token)
    {
        try
        {
            await read.WaitAsync(UnterminatedPromptDelay, token).ConfigureAwait(false);
            return true;
        }
        catch (TimeoutException) when (!read.IsFaulted)
        {
            return read.IsCompleted;
        }
    }

    /// <summary>
    /// Sends an input line with the Telnet CRLF terminator.
    /// </summary>
    /// <param name="line">The input line.</param>
    /// <param name="cancellationToken">A token that cancels the write.</param>
    public Task SendLineAsync(string line, CancellationToken cancellationToken = default)
    {
        // Normalize embedded line endings as well, including multiline raw sends.
        var text = line.ReplaceLineEndings("\r\n") + "\r\n";
        return WriteAsync(EscapeIac(_encoding.GetBytes(text)), cancellationToken);
    }

    /// <summary>
    /// Sends a GMCP package after the server has negotiated GMCP.
    /// </summary>
    /// <param name="message">The package to send.</param>
    /// <param name="cancellationToken">A token that cancels the write.</param>
    public Task SendGmcpAsync(GmcpMessage message, CancellationToken cancellationToken = default)
    {
        if (!_gmcpNegotiated)
        {
            throw new InvalidOperationException("The server has not negotiated GMCP.");
        }

        var payload = message.Json is null ? message.Package : $"{message.Package} {message.Json}";
        return WriteSubnegotiationAsync(Gmcp, Encoding.UTF8.GetBytes(payload), cancellationToken);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _disposed = true;
        _gmcpNegotiated = false;
        _client.Dispose();
    }

    private NetworkStream GetStream()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _stream ?? throw new InvalidOperationException("The Telnet connection is not open.");
    }

    private async Task<TelnetMessage.ServerEcho?> NegotiateAsync(Negotiation negotiation, CancellationToken token)
    {
        var (command, option) = negotiation;
        bool remote = command is Will or Wont;
        var options = remote ? _remoteOptions : _localOptions;
        bool wasEnabled = options[option];
        bool requested = command is Will or Do;
        bool supported = remote
            ? option is Echo or SuppressGoAhead or RecordOption || option == Gmcp && enableGmcp
            : option is TerminalType or SuppressGoAhead;

        bool enabled = requested && supported;
        options[option] = enabled;
        if (enabled != wasEnabled || requested && !supported)
        {
            byte reply = remote ? (enabled ? Do : Dont) : (enabled ? Will : Wont);
            await WriteAsync(new byte[] { Iac, reply, option }, token).ConfigureAwait(false);
        }

        if (remote && option == Gmcp && enabled != wasEnabled)
        {
            if (enabled)
            {
                await WriteSubnegotiationAsync(Gmcp, """Core.Hello {"client":"Avalon","version":"2"}"""u8.ToArray(), token)
                    .ConfigureAwait(false);
            }
            _gmcpNegotiated = enabled;
        }

        return remote && option == Echo && enabled != wasEnabled ? new TelnetMessage.ServerEcho(enabled) : null;
    }

    private Task WriteSubnegotiationAsync(byte option, byte[] payload, CancellationToken token)
        => WriteAsync([Iac, SubBegin, option, .. EscapeIac(payload), Iac, SubEnd], token);

    private async Task WriteAsync(byte[] bytes, CancellationToken token)
    {
        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await GetStream().WriteAsync(bytes, token).ConfigureAwait(false);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    private static byte[] EscapeIac(byte[] bytes)
    {
        var count = bytes.AsSpan().Count(Iac);
        if (count == 0)
        {
            return bytes;
        }

        var escaped = new byte[bytes.Length + count];
        var index = 0;
        foreach (byte value in bytes)
        {
            escaped[index++] = value;
            if (value == Iac)
            {
                escaped[index++] = value;
            }
        }
        return escaped;
    }

    private sealed record Negotiation(byte Command, byte Option) : TelnetMessage;
    private sealed record Subnegotiation(byte Option, byte[] Data) : TelnetMessage;

    // Telnet commands and UTF-8 code points can both span reads.
    private sealed class Parser(Encoding encoding)
    {
        private enum State { Data, Command, Option, SubOption, SubData, SubCommand }

        private readonly Decoder _decoder = encoding.GetDecoder();
        private readonly ArrayBufferWriter<byte> _text = new();
        private readonly ArrayBufferWriter<byte> _sub = new();
        private State _state;
        private byte _command;
        private byte _option;
        private bool _discardSub;

        public List<TelnetMessage> Parse(ReadOnlySpan<byte> bytes)
        {
            var items = new List<TelnetMessage>();
            foreach (byte value in bytes)
            {
                switch (_state)
                {
                    case State.Data:
                        if (value == Iac)
                        {
                            _state = State.Command;
                        }
                        else if (value is not (0 or 13))
                        {
                            Append(_text, value);
                        }

                        break;
                    case State.Command:
                        _state = State.Data;
                        if (value == Iac)
                        {
                            Append(_text, Iac);
                        }
                        else if (value is Will or Wont or Do or Dont)
                        {
                            _command = value;
                            _state = State.Option;
                        }
                        else if (value == SubBegin)
                        {
                            _state = State.SubOption;
                        }
                        else if (value is GoAhead or EndOfRecord)
                        {
                            Add(new TelnetMessage.Prompt());
                        }

                        break;
                    case State.Option:
                        Add(new Negotiation(_command, value));
                        _state = State.Data;
                        break;
                    case State.SubOption:
                        _option = value;
                        _sub.Clear();
                        _discardSub = value is not (Gmcp or TerminalType);
                        _state = State.SubData;
                        break;
                    case State.SubData:
                        if (value == Iac)
                        {
                            _state = State.SubCommand;
                        }
                        else
                        {
                            AppendSub(value);
                        }

                        break;
                    case State.SubCommand:
                        if (value == SubEnd)
                        {
                            if (!_discardSub)
                            {
                                Add(new Subnegotiation(_option, _sub.WrittenSpan.ToArray()));
                            }

                            _state = State.Data;
                        }
                        else
                        {
                            if (value == Iac)
                            {
                                AppendSub(Iac);
                            }

                            _state = State.SubData;
                        }
                        break;
                }
            }

            FlushText(items);
            return items;

            void Add(TelnetMessage item)
            {
                FlushText(items);
                items.Add(item);
            }
        }

        public TelnetMessage.Text? Finish() => Decode(flush: true);

        private void FlushText(List<TelnetMessage> items)
        {
            if (Decode(flush: false) is { } text)
            {
                items.Add(text);
            }
        }

        private TelnetMessage.Text? Decode(bool flush)
        {
            if (_text.WrittenCount == 0 && !flush)
            {
                return null;
            }

            var buffer = ArrayPool<char>.Shared.Rent(encoding.GetMaxCharCount(_text.WrittenCount));
            try
            {
                int count = _decoder.GetChars(_text.WrittenSpan, buffer, flush);
                _text.Clear();
                return count == 0 ? null : new TelnetMessage.Text(new string(buffer, 0, count));
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        private void AppendSub(byte value)
        {
            if (_sub.WrittenCount >= MaxSubnegotiationSize)
            {
                _discardSub = true;
            }

            if (!_discardSub)
            {
                Append(_sub, value);
            }
        }

        private static void Append(ArrayBufferWriter<byte> buffer, byte value)
        {
            buffer.GetSpan(1)[0] = value;
            buffer.Advance(1);
        }
    }
}
