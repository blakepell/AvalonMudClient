using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;

namespace Avalon.Mcp;

/// <summary>
/// Identifies an AI client that can connect to Avalon's MCP server.
/// </summary>
internal enum McpClient
{
    Codex,
    ClaudeCode
}

/// <summary>
/// Manages Avalon's registration in the current user's AI client configurations.
/// </summary>
internal static class McpClientRegistration
{
    private const string ServerName = "avalon";
    private static string UserProfile => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    /// <summary>
    /// Reads the registration and optionally changes it to the requested state.
    /// </summary>
    /// <param name="client">The AI client whose user configuration is checked.</param>
    /// <param name="register">The desired registration state, or null to check the current state.</param>
    /// <param name="port">The configured port of Avalon's MCP server.</param>
    /// <returns>The resulting registration state.</returns>
    internal static async Task<bool> ConfigureAsync(McpClient client, bool? register, int port)
    {
        var codex = client == McpClient.Codex;
        var clientName = codex ? "Codex" : "Claude Code";
        var executable = FindExecutable(codex ? "codex" : "claude")
            ?? throw new InvalidOperationException($"The {clientName} CLI was not found. Install it and make it available on PATH.");
        var registered = codex
            ? await IsCodexRegisteredAsync(executable).ConfigureAwait(false)
            : await IsClaudeRegisteredAsync().ConfigureAwait(false);
        if (register is null || register == registered)
        {
            return registered;
        }

        if (register == true && port is < 1 or > IPEndPoint.MaxPort)
        {
            throw new InvalidOperationException("Set a valid MCP server port in Client Settings.");
        }

        var endpoint = $"http://localhost:{port}/mcp";
        string[] arguments = (client, register.Value) switch
        {
            (McpClient.Codex, true) => ["add", ServerName, "--url", endpoint],
            (McpClient.Codex, false) => ["remove", ServerName],
            (McpClient.ClaudeCode, true) => ["add", "--transport", "http", "--scope", "user", ServerName, endpoint],
            _ => ["remove", "--scope", "user", ServerName]
        };

        await RunCliAsync(executable, arguments).ConfigureAwait(false);
        return register.Value;
    }

    private static async Task<bool> IsCodexRegisteredAsync(string executable)
    {
        var json = await RunCliAsync(executable, "list", "--json").ConfigureAwait(false);
        using var config = JsonDocument.Parse(json);
        foreach (var server in config.RootElement.EnumerateArray())
        {
            if (server.GetProperty("name").ValueEquals(ServerName))
            {
                return ValidateAvalonRegistration(server.GetProperty("transport"), "Codex");
            }
        }

        return false;
    }

    private static async Task<bool> IsClaudeRegisteredAsync()
    {
        // Inspect the user scope directly so that project entries and server health do not affect the menu.
        var configDir = Environment.GetEnvironmentVariable("CLAUDE_CONFIG_DIR");
        var path = Path.Combine(string.IsNullOrEmpty(configDir) ? UserProfile : Path.GetFullPath(configDir, UserProfile), ".claude.json");
        try
        {
            await using var stream = File.OpenRead(path);
            using var config = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);
            return config.RootElement.TryGetProperty("mcpServers", out var servers)
                && servers.TryGetProperty(ServerName, out var server)
                && ValidateAvalonRegistration(server, "Claude Code");
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
    }

    private static bool ValidateAvalonRegistration(JsonElement server, string clientName)
    {
        if (server.TryGetProperty("url", out var url) && Uri.TryCreate(url.GetString(), UriKind.Absolute, out var endpoint)
            && endpoint.Scheme == Uri.UriSchemeHttp && endpoint.IsLoopback && endpoint.AbsolutePath.TrimEnd('/') == "/mcp"
            && endpoint.Query.Length == 0 && endpoint.Fragment.Length == 0 && endpoint.UserInfo.Length == 0)
        {
            return true;
        }

        throw new InvalidOperationException($"{clientName} already has a different MCP server named '{ServerName}'.");
    }

    private static string? FindExecutable(string name)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? "";
        foreach (var entry in path.Split(Path.PathSeparator).Append(Path.Combine(UserProfile, ".local", "bin")))
        {
            var directory = entry.Trim().Trim('"');
            if (!Path.IsPathFullyQualified(directory))
            {
                continue;
            }

            var executable = Path.Combine(directory, name + ".exe");
            if (File.Exists(executable))
            {
                return executable;
            }

            var wrapper = Path.Combine(directory, name + ".cmd");
            if (File.Exists(wrapper))
            {
                return wrapper;
            }
        }

        return null;
    }

    private static async Task<string> RunCliAsync(string executable, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo(executable)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = UserProfile
        };
        if (Path.GetExtension(executable).Equals(".cmd", StringComparison.OrdinalIgnoreCase))
        {
            // npm installs a command wrapper on Windows. Pass its path as an environment value,
            // and only put fixed command tokens and the numeric-port loopback URL on the command line.
            startInfo.FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe");
            startInfo.Environment["AVALON_MCP_CLI"] = executable;
            startInfo.Arguments = $"/d /v:off /s /c \"\"%AVALON_MCP_CLI%\" mcp {string.Join(' ', arguments)}\"";
        }
        else
        {
            startInfo.ArgumentList.Add("mcp");
            foreach (var argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("The MCP registration command could not start.");
        process.StandardInput.Close();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var output = process.StandardOutput.ReadToEndAsync(timeout.Token);
        var error = process.StandardError.ReadToEndAsync(timeout.Token);

        try
        {
            await Task.WhenAll(process.WaitForExitAsync(timeout.Token), output, error).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }

            throw new TimeoutException("The MCP registration command timed out.");
        }

        var result = await output.ConfigureAwait(false);
        if (process.ExitCode != 0)
        {
            var details = await error.ConfigureAwait(false);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(details) ? result.Trim() : details.Trim());
        }

        return result;
    }
}
