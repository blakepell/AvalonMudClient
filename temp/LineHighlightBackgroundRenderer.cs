using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Avalon.Common.Colors;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;

public class LineHighlightBackgroundRenderer : IBackgroundRenderer
{
    private TextEditor _editor;

    public LineHighlightBackgroundRenderer(TextEditor editor)
    {
        _editor = editor;
    }

    public KnownLayer Layer { get; set; } = KnownLayer.Background;

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (_editor.Document == null)
            return;

        for (int i = 0; i < textView.Document.LineCount; i++)
        {
            var line = _editor.Document.GetLineByNumber(i + 1);

            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, line))
            {
                drawingContext.DrawRectangle(AnsiColors.DarkCyan.Brush, null, new Rect(rect.Location, new Size(rect.Width, rect.Height)));
            }
        }

    }

}