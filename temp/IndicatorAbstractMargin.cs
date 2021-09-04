using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;

namespace Avalon.Controls
{
    public class IndicatorAbstractMargin : AbstractMargin
    {
        private const int MarginWidth = 4;
        //private static readonly Pen Pen = new Pen(Brushes.LimeGreen, 0);
        //private static readonly SolidColorBrush Brush = Brushes.DodgerBlue;
        private readonly TextEditor _editor;

        static IndicatorAbstractMargin()
        {
            //Pen.Freeze();
        }

        public IndicatorAbstractMargin(TextEditor editor)
        {
            _editor = editor;
            this.Margin = new Thickness(0, 0, 5, 0);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(MarginWidth, 0);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var textView = TextView;
            Size renderSize = this.RenderSize;

            if (textView != null && textView.VisualLinesValid)
            {
                foreach (VisualLine line in textView.VisualLines)
                {
                    int lineNumber = line.FirstDocumentLine.LineNumber;
                    double y = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop);

                    Brush brush;

                    var text = textView.Document.GetText(line.StartOffset, line.VisualLength).AsSpan();

                    if (text.IndexOf("red") > -1)
                    {
                        brush = Brushes.Red;
                    }
                    else if (text.IndexOf("green") > -1)
                    {
                        brush = Brushes.Green;
                    }
                    else if (text.IndexOf("blue") > -1)
                    {
                        brush = Brushes.Blue;
                    }
                    else
                    {
                        brush = Brushes.Black;
                    }

                    var pen = new Pen(brush, 0);
                    pen.Freeze();

                    //var visualLine = textView.GetVisualLine(line.LineNumber);
                    var lineTop = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.LineTop);
                    var lineBottom = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.LineBottom);

                    //var lineBottom = line.GetTextLineVisualYPosition(line.TextLines[line.TextLines.Count - 1], VisualYPosition.LineBottom);
                    drawingContext.DrawRectangle(brush, pen, new Rect(new Point(0, lineTop - textView.VerticalOffset), new Size(MarginWidth, lineBottom - lineTop)));
                }
            }
        }

        protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
        {
            if (oldTextView != null)
            {
                oldTextView.VisualLinesChanged -= TextViewVisualLinesChangedHandler;
                TextView.Document.TextChanged -= DocumentTextChangedHandler;
            }

            base.OnTextViewChanged(oldTextView, newTextView);

            if (newTextView != null)
            {
                newTextView.VisualLinesChanged += TextViewVisualLinesChangedHandler;
                TextView.Document.TextChanged += DocumentTextChangedHandler;
            }

            InvalidateVisual();
        }

        private void DocumentTextChangedHandler(object sender, EventArgs eventArgs)
        {
            InvalidateVisual();
        }

        private void TextViewVisualLinesChangedHandler(object sender, EventArgs eventArgs)
        {
            InvalidateVisual();
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }
    }
}
