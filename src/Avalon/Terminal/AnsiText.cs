using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Avalon.Terminal;

/// <summary>
/// Describes a run of styled text in a terminal update.
/// </summary>
/// <param name="Offset">The character offset within the update.</param>
/// <param name="Length">The number of characters in the run.</param>
/// <param name="Style">The ANSI style for the run.</param>
public readonly record struct AnsiRun(int Offset, int Length, AnsiStyle Style);

/// <summary>
/// Describes the colors and attributes set by ANSI SGR sequences.
/// </summary>
/// <param name="Foreground">A palette index, a packed RGB value with bit 24 set, or the default color.</param>
/// <param name="Background">A palette index, a packed RGB value with bit 24 set, or the default color.</param>
/// <param name="Bold">Whether to draw bold text.</param>
/// <param name="Italic">Whether to draw italic text.</param>
/// <param name="Underline">Whether to underline text.</param>
/// <param name="Inverse">Whether to swap the foreground and background.</param>
public readonly record struct AnsiStyle(
    int? Foreground = null, int? Background = null,
    bool Bold = false, bool Italic = false, bool Underline = false, bool Inverse = false);

internal static partial class AnsiText
{
    [GeneratedRegex(@"\{[{xwZ&_RrkDGgYyBbMmCcopnWu]")]
    private static partial Regex MudColorCodes();

    public static string FromMudColors(string text) => text.Contains('{')
        ? MudColorCodes().Replace(text, match => match.Value[1] switch
        {
            '{' => "{",
            'x' => "\x1b[0m",
            'Z' => "\x1b[40m",
            '&' => "\x1b[7m",
            '_' => "\x1b[4m",
            'k' => "\x1b[0;30m", 'D' => "\x1b[1;30m",
            'r' => "\x1b[0;31m", 'R' => "\x1b[1;31m",
            'g' => "\x1b[0;32m", 'G' => "\x1b[1;32m",
            'y' => "\x1b[0;33m", 'Y' => "\x1b[1;33m",
            'b' => "\x1b[0;34m", 'B' => "\x1b[1;34m",
            'M' => "\x1b[0;35m", 'm' or 'u' => "\x1b[1;35m",
            'c' => "\x1b[0;36m", 'C' => "\x1b[1;36m",
            'w' => "\x1b[0;37m", 'W' => "\x1b[1;37m",
            'o' => "\x1b[38;5;214m", 'p' => "\x1b[38;5;213m", 'n' => "\x1b[38;5;130m",
            _ => match.Value
        })
        : text;

    public static void AppendStyle(StringBuilder text, AnsiStyle style)
    {
        text.Append("\x1b[0");
        if (style.Bold) text.Append(";1");
        if (style.Italic) text.Append(";3");
        if (style.Underline) text.Append(";4");
        if (style.Inverse) text.Append(";7");
        AppendColor(style.Foreground, 38);
        AppendColor(style.Background, 48);
        text.Append('m');

        void AppendColor(int? color, int code)
        {
            if (color is not int value)
            {
                return;
            }

            if (value < 16)
            {
                // Basic colors use the native SGR ranges to retain bold and bright semantics.
                text.Append(';').Append((code == 38 ? 30 : 40) + (value < 8 ? value : 60 + value - 8));
            }
            else if (value >= 0x1000000)
            {
                text.Append(';').Append(code).Append(";2;").Append((byte)(value >> 16)).Append(';')
                    .Append((byte)(value >> 8)).Append(';').Append((byte)value);
            }
            else
            {
                text.Append(';').Append(code).Append(";5;").Append(value);
            }
        }
    }

    public static (string Text, IReadOnlyList<AnsiRun> Runs, AnsiStyle EndStyle) Parse(string raw, AnsiStyle style)
    {
        if (!raw.AsSpan().ContainsAnyInRange('\0', '\x1f'))
        {
            return (raw, style == default || raw.Length == 0 ? [] : [new AnsiRun(0, raw.Length, style)], style);
        }

        var text = new StringBuilder(raw.Length);
        var runs = new List<AnsiRun>();
        var runStart = 0;
        for (int index = 0; index < raw.Length; index++)
        {
            char value = raw[index];
            if (value == '\x1b')
            {
                if (++index >= raw.Length)
                {
                    break;
                }

                if (raw[index] == '[')
                {
                    int start = ++index;
                    while (index < raw.Length && raw[index] is < '@' or > '~')
                    {
                        index++;
                    }

                    if (index == raw.Length)
                    {
                        break;
                    }

                    if (raw[index] == 'm')
                    {
                        var next = ApplySgr(raw.AsSpan(start, index - start), style);
                        if (next != style)
                        {
                            FinishRun();
                            style = next;
                        }
                    }
                }
                else if (raw[index] is ']' or 'P' or '_' or '^')
                {
                    // OSC and other terminal strings end with BEL or ST.
                    while (++index < raw.Length && raw[index] != '\a')
                    {
                        if (raw[index] == '\x1b' && index + 1 < raw.Length && raw[index + 1] == '\\')
                        {
                            index++;
                            break;
                        }
                    }
                }
            }
            else if (value == '\b' && text.Length > 0)
            {
                text.Length--;
                if (runStart > text.Length)
                {
                    runStart--;
                    if (runs.Count > 0 && runs[^1].Offset + runs[^1].Length > runStart)
                    {
                        var last = runs[^1];
                        if (last.Length == 1)
                        {
                            runs.RemoveAt(runs.Count - 1);
                        }
                        else
                        {
                            runs[^1] = last with { Length = last.Length - 1 };
                        }
                    }
                }
            }
            else if (!char.IsControl(value) || value is '\t' or '\n')
            {
                text.Append(value);
            }
        }

        FinishRun();
        return (text.ToString(), runs, style);

        void FinishRun()
        {
            if (style != default && text.Length > runStart)
            {
                runs.Add(new AnsiRun(runStart, text.Length - runStart, style));
            }

            runStart = text.Length;
        }
    }

    private static AnsiStyle ApplySgr(ReadOnlySpan<char> parameters, AnsiStyle style)
    {
        Span<int> values = stackalloc int[32];
        int count = 0;
        foreach (var range in parameters.Split(';'))
        {
            if (count == values.Length)
            {
                return style;
            }

            var part = parameters[range];
            values[count++] = part.IsEmpty ? 0 :
                int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out int value) ? value : -1;
        }

        if (count == 0)
        {
            return default;
        }

        for (int i = 0; i < count; i++)
        {
            var code = values[i];
            if (code is 38 or 48)
            {
                if (++i == count)
                {
                    break;
                }

                int? color = null;
                if (values[i] == 5 && i + 1 < count)
                {
                    color = values[++i] is >= 0 and <= 255 ? values[i] : null;
                }
                else if (values[i] == 2 && i + 3 < count)
                {
                    int red = values[++i], green = values[++i], blue = values[++i];
                    if ((uint)red <= 255 && (uint)green <= 255 && (uint)blue <= 255)
                    {
                        color = 0x1000000 | (red << 16) | (green << 8) | blue;
                    }
                }
                else
                {
                    break;
                }

                if (color.HasValue)
                {
                    style = code == 38 ? style with { Foreground = color } : style with { Background = color };
                }

                continue;
            }

            style = code switch
            {
                0 => default,
                1 => style with { Bold = true },
                3 => style with { Italic = true },
                4 => style with { Underline = true },
                7 => style with { Inverse = true },
                22 => style with { Bold = false },
                23 => style with { Italic = false },
                24 => style with { Underline = false },
                27 => style with { Inverse = false },
                >= 30 and <= 37 => style with { Foreground = code - 30 },
                >= 40 and <= 47 => style with { Background = code - 40 },
                >= 90 and <= 97 => style with { Foreground = code - 90 + 8 },
                >= 100 and <= 107 => style with { Background = code - 100 + 8 },
                39 => style with { Foreground = null },
                49 => style with { Background = null },
                _ => style
            };
        }
        return style;
    }
}
