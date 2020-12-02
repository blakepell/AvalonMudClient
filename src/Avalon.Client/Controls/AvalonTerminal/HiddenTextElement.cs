using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Media.TextFormatting;

namespace Avalon.Controls
{
    /// <summary>
    /// Represents a hidden element in the text editor.
    /// </summary>
    public class HiddenTextElement : VisualLineElement
    {
        public HiddenTextElement(int documentLength) : base(1, documentLength)
        {
        }

        public override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
        {
            return new TextHidden(1);
        }
    }
}