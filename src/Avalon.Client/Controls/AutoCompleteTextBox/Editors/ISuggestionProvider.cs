using System.Collections;

namespace Avalon.Controls.AutoCompleteTextBox.Editors
{
    public interface ISuggestionProvider
    {
        IEnumerable GetSuggestions(string filter);
    }
}