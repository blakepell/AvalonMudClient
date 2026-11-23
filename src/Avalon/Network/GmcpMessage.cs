namespace Avalon.Network;

/// <summary>
/// Carries a GMCP package name and its optional JSON payload.
/// </summary>
/// <param name="Package">The package name, such as <c>Char.Vitals</c>.</param>
/// <param name="Json">The original JSON payload, if present.</param>
public sealed record GmcpMessage(string Package, string? Json)
{
    internal static GmcpMessage? Parse(string text)
    {
        var space = text.IndexOf(' ');
        var package = space < 0 ? text : text[..space];
        if (package.Length == 0 || package.Any(char.IsWhiteSpace))
        {
            return null;
        }

        return new GmcpMessage(package, space < 0 ? null : text[(space + 1)..].TrimStart());
    }
}
