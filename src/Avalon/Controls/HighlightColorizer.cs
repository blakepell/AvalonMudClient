using System.Diagnostics;
using System.Text.RegularExpressions;
using Avalon.Models;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace Avalon.Controls;

/// <summary>
/// Colors the text that matches the game's enabled <see cref="HighlightTrigger"/>s, over the ANSI colors.
/// </summary>
internal sealed class HighlightColorizer(Game game) : DocumentColorizingTransformer
{
    protected override void ColorizeLine(DocumentLine line)
    {
        if (game.HighlightTriggers.Count == 0 || line.Length == 0)
        {
            return;
        }

        string text = CurrentContext.Document.GetText(line);
        foreach (var highlight in game.HighlightTriggers)
        {
            if (!highlight.Enabled || highlight.Regex is not { } regex)
            {
                continue;
            }

            long started = Stopwatch.GetTimestamp();
            try
            {
                foreach (Match match in regex.Matches(text))
                {
                    if (match.Length == 0)
                    {
                        continue;
                    }

                    ChangeLinePart(line.Offset + match.Index, line.Offset + match.Index + match.Length, element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(highlight.ForegroundColor);
                        element.TextRunProperties.SetBackgroundBrush(highlight.BackgroundColor);
                    });
                }
            }
            catch (RegexMatchTimeoutException)
            {
                // A slow pattern skips this line rather than freezing the terminal.
            }

            double elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            if (elapsed > highlight.MaxExecutionTime)
            {
                highlight.MaxExecutionTime = elapsed;
            }
        }
    }
}
