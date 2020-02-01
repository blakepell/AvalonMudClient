using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

namespace Avalon.Controls
{
    public class LineHighlightBackgroundRenderer : IBackgroundRenderer
    {
        public LineHighlightBackgroundRenderer()
        {
        }

        public KnownLayer Layer { get; set; } = KnownLayer.Background;

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (textView.Document == null)
            {
                return;
            }

            for (int i = 0; i < textView.Document.LineCount; i++)
            {
                var line = textView.Document.GetLineByNumber(i + 1);

                foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, line))
                {
                    drawingContext.DrawRectangle(Brushes.DarkCyan, null, new Rect(rect.Location, new Size(rect.Width, rect.Height)));
                }
            }

        }

    }
}