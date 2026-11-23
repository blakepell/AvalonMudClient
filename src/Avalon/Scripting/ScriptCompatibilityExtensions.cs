using Avalon.Terminal;

namespace Avalon.Scripting;

/// <summary>
/// String extensions used by Continuum scripts.
/// </summary>
internal static class ScriptCompatibilityExtensions
{
    public static string RemoveAnsiCodes(this string text) => AnsiText.Parse(text, default).Text;

    public static Tuple<string, string> FirstArgument(this string text)
    {
        var trimmed = text.AsSpan().TrimStart();
        int end = 0;
        while (end < trimmed.Length && !char.IsWhiteSpace(trimmed[end]))
        {
            end++;
        }

        return Tuple.Create(trimmed[..end].ToString(), trimmed[end..].TrimStart().ToString());
    }
}
