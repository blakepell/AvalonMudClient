using System.Text;
using System.Windows;
using System.Windows.Media;
using Avalon.Models;
using Avalon.Terminal;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace Avalon.Controls;

internal sealed class TerminalTranscript : DocumentColorizingTransformer
{
    private const int MaxLines = 20000;
    private const int MaxCharacters = 2_000_000;
    private readonly TextEditor _editor;
    private readonly TextSegmentCollection<StyledSegment> _styles;
    private TerminalOutput? _preview;

    public TerminalTranscript(TextEditor editor)
    {
        _editor = editor;
        _styles = new TextSegmentCollection<StyledSegment>(editor.Document);
        editor.Document.UndoStack.SizeLimit = 0;
        editor.TextArea.TextView.LineTransformers.Add(this);
    }

    public ColorTheme Palette { get; set; } = new();

    public void Apply(TerminalOutput update)
    {
        var document = _editor.Document;
        var previous = _preview;
        using (document.RunUpdate())
        {
            if (update.Kind == TerminalOutputKind.Clear)
            {
                _preview = null;
                _styles.Clear();
                document.Remove(0, document.TextLength);
                return;
            }

            if (previous is { Text.Length: > 0 })
            {
                int offset = document.TextLength - previous.Text.Length;
                RemoveStyles(offset, previous.Text.Length);
                document.Remove(offset, previous.Text.Length);
            }

            Append(update);
            _preview = update.Kind switch
            {
                TerminalOutputKind.Preview => update,
                TerminalOutputKind.Local => previous,
                _ => null
            };
            if (update.Kind == TerminalOutputKind.Local && _preview is not null)
            {
                Append(_preview);
            }

            TrimScrollback();
        }
    }

    public void CommitPreview() => _preview = null;

    public string GetText(bool includeAnsi, int lines)
    {
        var document = _editor.Document;
        int firstLine = lines > 0 ? Math.Max(1, document.LineCount - lines + 1) : 1;
        int start = document.GetLineByNumber(firstLine).Offset;
        int end = document.TextLength;
        if (!includeAnsi)
        {
            return document.GetText(start, end - start);
        }

        var text = new StringBuilder(end - start);
        int position = start;
        foreach (var run in ReadStyles(start, end))
        {
            int offset = start + run.Offset;
            text.Append(document.GetText(position, offset - position));
            AnsiText.AppendStyle(text, run.Style);
            text.Append(document.GetText(offset, run.Length)).Append("\x1b[0m");
            position = offset + run.Length;
        }

        return text.Append(document.GetText(position, end - position)).ToString();
    }

    public void ReplaceText(string searchText, string replacementText, bool lastOnly)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            return;
        }

        var document = _editor.Document;
        var source = document.Text;
        var (text, runs, _) = AnsiText.Parse(replacementText ?? "", default);
        if (lastOnly)
        {
            int offset = source.LastIndexOf(searchText, StringComparison.Ordinal);
            if (offset >= 0)
            {
                Edit(offset, searchText.Length, text, runs);
            }
            return;
        }

        var offsets = new List<int>();
        for (int offset = source.IndexOf(searchText, StringComparison.Ordinal); offset >= 0;
             offset = source.IndexOf(searchText, offset + searchText.Length, StringComparison.Ordinal))
        {
            offsets.Add(offset);
        }

        using (document.RunUpdate())
        {
            for (int i = offsets.Count - 1; i >= 0; i--)
            {
                Edit(offsets[i], searchText.Length, text, runs);
            }
        }
    }

    public void RemoveLine(int lineNumber)
    {
        var document = _editor.Document;
        if (lineNumber >= 1 && lineNumber <= document.LineCount)
        {
            var line = document.GetLineByNumber(lineNumber);
            Edit(line.Offset, line.TotalLength, "", []);
        }
    }

    public void RemoveLinesBefore(int lineNumber)
    {
        var document = _editor.Document;
        if (lineNumber > 1 && lineNumber <= document.LineCount)
        {
            Edit(0, document.GetLineByNumber(lineNumber).Offset, "", []);
        }
    }

    public void TrimEnd()
    {
        var document = _editor.Document;
        int end = document.TextLength;
        while (end > 0 && char.IsWhiteSpace(document.GetCharAt(end - 1)))
        {
            end--;
        }

        Edit(end, document.TextLength - end, "", []);
    }

    public void StartSession()
    {
        var document = _editor.Document;
        if (document.TextLength > 0 && document.GetCharAt(document.TextLength - 1) != '\n')
        {
            document.Insert(document.TextLength, "\n");
        }
    }

    private void Append(TerminalOutput update)
    {
        var document = _editor.Document;
        int offset = document.TextLength;
        document.Insert(offset, update.Text);
        AddStyles(offset, update.Runs);
    }

    private void Edit(int offset, int length, string text, IReadOnlyList<AnsiRun> runs)
    {
        if (length == 0 && text.Length == 0)
        {
            return;
        }

        var document = _editor.Document;
        int end = offset + length;
        int previewStart = document.TextLength - (_preview?.Text.Length ?? 0);
        var overlaps = _styles.FindOverlappingSegments(offset, length)
            .Where(segment => segment.StartOffset < end && segment.EndOffset > offset).ToArray();
        using (document.RunUpdate())
        {
            foreach (var segment in overlaps)
            {
                _styles.Remove(segment);
            }

            document.Replace(offset, length, text);
            foreach (var segment in overlaps)
            {
                if (segment.StartOffset < offset)
                {
                    AddStyle(segment.StartOffset, offset - segment.StartOffset, segment.Style);
                }
                if (segment.EndOffset > end)
                {
                    AddStyle(offset + text.Length, segment.EndOffset - end, segment.Style);
                }
            }
            AddStyles(offset, runs);

            if (_preview is not null && end > previewStart)
            {
                int start = Math.Min(previewStart, offset);
                _preview = _preview with
                {
                    Text = document.GetText(start, document.TextLength - start),
                    Runs = ReadStyles(start, document.TextLength)
                };
            }
        }
    }

    private AnsiRun[] ReadStyles(int start, int end)
    {
        return _styles.FindOverlappingSegments(start, end - start)
            .Select(segment =>
            {
                int offset = Math.Max(segment.StartOffset, start);
                return new AnsiRun(offset - start, Math.Min(segment.EndOffset, end) - offset, segment.Style);
            })
            .Where(run => run.Length > 0)
            .ToArray();
    }

    private void AddStyles(int offset, IReadOnlyList<AnsiRun> runs)
    {
        foreach (var run in runs)
        {
            AddStyle(offset + run.Offset, run.Length, run.Style);
        }
    }

    private void AddStyle(int offset, int length, AnsiStyle style) =>
        _styles.Add(new StyledSegment { StartOffset = offset, Length = length, Style = style });

    private void TrimScrollback()
    {
        var document = _editor.Document;
        int remove = 0;
        if (document.LineCount > MaxLines)
        {
            remove = document.GetLineByNumber(document.LineCount - MaxLines + 1001).Offset;
        }

        if (document.TextLength > MaxCharacters)
        {
            int target = document.TextLength - MaxCharacters + 100_000;
            remove = Math.Max(remove, document.GetLineByOffset(target).Offset);
            if (remove == 0)
            {
                remove = target;
            }
        }

        int committed = document.TextLength - (_preview?.Text.Length ?? 0);
        remove = Math.Min(remove, committed);
        if (remove > 0)
        {
            RemoveStyles(0, remove);
            document.Remove(0, remove);
        }
    }

    private void RemoveStyles(int offset, int length)
    {
        foreach (var segment in _styles.FindOverlappingSegments(offset, length))
        {
            if (segment.StartOffset >= offset && segment.EndOffset <= offset + length)
            {
                _styles.Remove(segment);
            }
        }
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        foreach (var segment in _styles.FindOverlappingSegments(line.Offset, line.Length))
        {
            int start = Math.Max(segment.StartOffset, line.Offset);
            int end = Math.Min(segment.EndOffset, line.EndOffset);
            if (start >= end)
            {
                continue;
            }

            var style = segment.Style;
            var foreground = style.Foreground is { } fg ? GetBrush(fg, style.Bold) : null;
            var background = style.Background is { } bg ? GetBrush(bg, bold: false) : null;
            if (style.Inverse)
            {
                (foreground, background) = (background ?? Palette.Background, foreground ?? Palette.Default);
            }

            ChangeLinePart(start, end, element =>
            {
                var properties = element.TextRunProperties;
                if (foreground is not null)
                {
                    properties.SetForegroundBrush(foreground);
                }

                if (background is not null)
                {
                    properties.SetBackgroundBrush(background);
                }

                if (style.Bold || style.Italic)
                {
                    var typeface = properties.Typeface;
                    properties.SetTypeface(new Typeface(typeface.FontFamily,
                        style.Italic ? FontStyles.Italic : typeface.Style,
                        style.Bold ? FontWeights.Bold : typeface.Weight, typeface.Stretch));
                }
                if (style.Underline)
                {
                    properties.SetTextDecorations(TextDecorations.Underline);
                }
            });
        }
    }

    private SolidColorBrush GetBrush(int color, bool bold)
    {
        if (color < 16)
        {
            return (bold && color < 8 ? color + 8 : color) switch
            {
                0 => Palette.Black,
                1 => Palette.DarkRed,
                2 => Palette.DarkGreen,
                3 => Palette.DarkYellow,
                4 => Palette.DarkBlue,
                5 => Palette.DarkPurple,
                6 => Palette.DarkCyan,
                7 => Palette.LightGray,
                8 => Palette.DarkGray,
                9 => Palette.Red,
                10 => Palette.Green,
                11 => Palette.Yellow,
                12 => Palette.Blue,
                13 => Palette.Purple,
                14 => Palette.Cyan,
                _ => Palette.White
            };
        }

        byte red, green, blue;
        if (color >= 0x1000000)
        {
            red = (byte)(color >> 16);
            green = (byte)(color >> 8);
            blue = (byte)color;
        }
        else if (color >= 232)
        {
            red = green = blue = (byte)(8 + (color - 232) * 10);
        }
        else
        {
            int cube = color - 16;
            red = CubeLevel(cube / 36);
            green = CubeLevel(cube / 6 % 6);
            blue = CubeLevel(cube % 6);
        }

        var brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
        brush.Freeze();
        return brush;
    }

    private static byte CubeLevel(int level) => (byte)(level == 0 ? 0 : 55 + level * 40);

    private sealed class StyledSegment : TextSegment
    {
        public required AnsiStyle Style { get; init; }
    }
}
