using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace Avalon.Controls
{
    public class TruncateLongLines : VisualLineElementGenerator
    {
        const int maxLength = 180;
        const string ellipsis = "...";
        const int charactersAfterEllipsis = 0;

        public override int GetFirstInterestedOffset(int startOffset)
        {
            DocumentLine line = CurrentContext.VisualLine.LastDocumentLine;
            if (line.Length > maxLength)
            {
                int ellipsisOffset = line.Offset + maxLength - charactersAfterEllipsis - ellipsis.Length;
                if (startOffset <= ellipsisOffset)
                    return ellipsisOffset;
            }
            return -1;
        }

        public override VisualLineElement ConstructElement(int offset)
        {
            return new FormattedTextElement(ellipsis, CurrentContext.VisualLine.LastDocumentLine.EndOffset - offset - charactersAfterEllipsis);
        }
    }
}
