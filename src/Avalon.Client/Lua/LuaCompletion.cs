using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Collections.Generic;

namespace Avalon.Lua
{
    public static class LuaCompletion
    {
        /// <summary>
        /// Loads the completion data based on the pattern.
        /// </summary>
        /// <remarks>
        /// TODO - Get all of these from reflection so we never have to update this.
        /// </remarks>
        public static void LoadCompletionData(IList<ICompletionData> data, string pattern)
        {
            if (pattern == "lua")
            {
                // data.Add(new LuaCompletionData("", ""));
                data.Add(new LuaCompletionData("Send", "Sends a command to the game."));
                data.Add(new LuaCompletionData("GetVariable", "Gets a global variable that is shared with the entire mud client."));
                data.Add(new LuaCompletionData("SetVariable", "Sets a global variable that is shared with the entire mud client."));
                data.Add(new LuaCompletionData("RemoveVariable", "Removes a global variable that is shared with the entire mud client."));
                data.Add(new LuaCompletionData("Echo", "Echos text to the game terminal."));
                data.Add(new LuaCompletionData("EchoEvent", "Echos a highlighted set of text to the game terminal."));
                data.Add(new LuaCompletionData("EchoWindow", "Echos to a specified custom window.  This operation appends text by default."));
                data.Add(new LuaCompletionData("ClearWindow", "Clears any text in the terminal of a custom window."));
                data.Add(new LuaCompletionData("Coalesce", "Returns the first non null and non empty value from the list"));
                data.Add(new LuaCompletionData("GetTime", "Gets the current time.  Accepts a true/false parameter indicating whether to use 12 or 24 hour time."));
                data.Add(new LuaCompletionData("GetMinute", "Gets the current minute."));
                data.Add(new LuaCompletionData("GetHour", "Gets the current hour."));
                data.Add(new LuaCompletionData("GetSecond", "Gets the current second."));
                data.Add(new LuaCompletionData("GetMillisecond", "Gets the current millisecond."));
                data.Add(new LuaCompletionData("DailyMillisecondsElapsed", "Gets the number of milliseconds that have elapsed since midnight."));
                data.Add(new LuaCompletionData("DailyMinutesElapsed", "Gets the number of minutes that have elapsed since midnight."));
                data.Add(new LuaCompletionData("DailyHoursElapsed", "Gets the number of hours that have elapsed since midnight."));
                data.Add(new LuaCompletionData("Sleep", "Pauses the execution of the script for a specified set of milliseconds."));
                data.Add(new LuaCompletionData("RandomNumber", "Returns a random number between two values."));
                data.Add(new LuaCompletionData("RandomChoice", "Returns a random choice out of the specified values."));
                data.Add(new LuaCompletionData("Guid", "Returns a cryptographically unique guid (global identifier)"));
                data.Add(new LuaCompletionData("SetTitle", "Sets the title of the mud client."));
                data.Add(new LuaCompletionData("GetScrapedText", "Gets the text that is in the current scape buffer."));
                data.Add(new LuaCompletionData("Trim", "Trims whitespace off the front and end of a string.  With a parameter will trim the specified parameter."));
                data.Add(new LuaCompletionData("TrimStart", "Trims whitespace off the front of a string.  With a parameter will trim the specified parameter."));
                data.Add(new LuaCompletionData("TrimEnd", "Trims whitespace off the end of a string.  With a parameter will trim the specified parameter."));
                data.Add(new LuaCompletionData("Split", "Splits the specified string with a delimiter providing an array returned."));
                data.Add(new LuaCompletionData("ArrayContains", "Whether or not an array contains an element."));
                data.Add(new LuaCompletionData("LastNonEmptyLine", "Returns the text of the last non empty line in the main game terminal."));
                data.Add(new LuaCompletionData("LastLines", "Returns the requested number of lines from the end of the game termianl as a string array.  Specifying a second boolean parameter will determine if the returned list is returned in reverse order."));
                data.Add(new LuaCompletionData("ListAdd", "Adds an item to a list."));
                data.Add(new LuaCompletionData("ListAddStart", "Adds an item to the start of a list."));
                data.Add(new LuaCompletionData("ListAddIfNotExist", "Adds an item to the end of a list if that item doesn't already exist."));
                data.Add(new LuaCompletionData("ListRemove", "Removes all items from a list that match the provided key."));
                data.Add(new LuaCompletionData("ListExists", "Whether an item exists in a list."));
                data.Add(new LuaCompletionData("LogInfo", "Logs an informational message."));
                data.Add(new LuaCompletionData("LogWarning", "Logs a warning message."));
                data.Add(new LuaCompletionData("LogError", "Logs an error message."));
                data.Add(new LuaCompletionData("LogSuccess", "Logs a success message."));
                data.Add(new LuaCompletionData("Replace", "Replaces a pattern in text with another pattern."));
                data.Add(new LuaCompletionData("EnableGroup", "Enables a group of aliases and triggers."));
                data.Add(new LuaCompletionData("DisableGroup", "Disables a group of aliases and triggers."));
                data.Add(new LuaCompletionData("AddScheduledTask", "Adds a scheduled task to be performed after a period of time."));
                data.Add(new LuaCompletionData("AddBatchTask", "Adds a batch task to be performed one after another with a delay in between them."));
                data.Add(new LuaCompletionData("ClearTasks", "Clears all tasks."));
                data.Add(new LuaCompletionData("FormatNumber", "Formats a number with commas."));
                data.Add(new LuaCompletionData("MD5", "Returns the MD5 hash of the specified string."));
                data.Add(new LuaCompletionData("SHA256", "Returns the SHA256 hash of the specified string."));
                data.Add(new LuaCompletionData("SHA512", "Returns the SHA512 hash of the specified string."));
                data.Add(new LuaCompletionData("RemoveLinesEndingWith", "Removes all lines from a string that end with a specified value."));
                data.Add(new LuaCompletionData("RemoveLinesStartingWith", "Removes all lines from a string that starts with a specified value."));
                data.Add(new LuaCompletionData("ProfileDirectory", "The location of the current profile directory."));
                data.Add(new LuaCompletionData("AppDataDirectory", "The location of the AppData directory."));
                data.Add(new LuaCompletionData("RemoveNonAlpha", "Removes non alpha chars from a string.  A second parameter of exception chars is allowed.\r\nSyntax: lua.RemoveNonAlpha(string, string)"));
                data.Add(new LuaCompletionData("StartsWith", "If a string starts with another string."));
                data.Add(new LuaCompletionData("EndsWith", "If a string ends with another string."));
                data.Add(new LuaCompletionData("LuaScriptsActive", "The number of Lua scripts that are actively running."));
                data.Add(new LuaCompletionData("DbExecute", "Executes a parameterized SQL statement.\r\nExecute(string sql, params string[] parameters)\r\nReturns integer"));
                data.Add(new LuaCompletionData("DbSelectValue", "Selects the value from the first column of the first row of any result set.\r\nSelectValue(string sql, params string[] parameters)\r\nReturns object"));
                data.Add(new LuaCompletionData("DbSelect", "Selects a record set from the database.\r\nSelect(string sql, params string[] parameters)\r\nReturns a tabled record set."));
                data.Add(new LuaCompletionData("DbFlush", "Forces all pending database transactions to be committed to the database.\r\nDbFlush()"));
                data.Add(new LuaCompletionData("DbExecuteImmediate", "Executes a SQL command immediately outside of a transaction.  Used for scenarios where statements can't run in a transaction like for CREATE TABLE.\r\nSyntax: DbExecuteImmediate(string sql, params string[] parameters)"));
                data.Add(new LuaCompletionData("HttpGet", "Downloads a string from a provided URL using a HTTP GET method.\r\nHttpGet(string url)"));
                data.Add(new LuaCompletionData("HttpPost", "Downloads a string from a provided URL using the POST method.\r\nHttpPost(string url, string data)\r\nThe data parameter format is for form fields is: 'name=Rhien hobby=muds'"));
                data.Add(new LuaCompletionData("CaptureOn", "Turns on the text capturing from the incoming game text."));
                data.Add(new LuaCompletionData("CaptureOff", "Turns off the text capturing from the incoming game text."));
                data.Add(new LuaCompletionData("CaptureClear", "Clears the capture buffer."));
                data.Add(new LuaCompletionData("Left", "Returns the specified number of characters from the left side of the string. If more characters were requested than exist the full string is returned.\r\nLeft(string str, int length)"));
                data.Add(new LuaCompletionData("Right", "Returns the specified number of characters from the right side of the string. If more characters were requested than exist the full string is returned.\r\nRight(string str, int length)"));
                data.Add(new LuaCompletionData("Substring", "Returns the substring starting at the specified index for the specified length.\r\nSyntax: Substring(string str, int startIndex)\r\nSyntax: Substring(string str, int startIndex, int length)"));
                data.Add(new LuaCompletionData("RemoveElementsEmpty", "Removes all empty/whitespace elements from an array.\r\nSyntax: RemoveElementsEmpty(string[] array)"));
                data.Add(new LuaCompletionData("RemoveElementsContains", "Removes all elements from an array that contain the specified text.\r\nSyntax: RemoveElementsContains(string[] array, string str)"));
                data.Add(new LuaCompletionData("RemoveElementsEndingWith", "Removes all elements from an array that end with the specified text.\r\nSyntax: RemoveElementsEndingWith(string[] array, string str)"));
                data.Add(new LuaCompletionData("RemoveElementsStartsWith", "Removes all elements from an array that start with the specified text.\r\nSyntax: RemoveElementsStartsWith(string[] array, string str)"));
                data.Add(new LuaCompletionData("LastLinesBetweenContains", "Removes all elements from an array between the last occurrence of lines that contain a start and optional ending pattern.\r\nSyntax: LastLinesBetweenContains(string startLineContains)\r\nSyntax: LastLinesBetweenContains(string startLineContains, string endLineContains)"));
                data.Add(new LuaCompletionData("LastLinesBetweenStartsWith", "Removes all elements from an array between the last occurrence of lines that start with a start and optional ending pattern.\r\nSyntax: LastLinesBetweenStartsWith(string startLineStartsWith)\r\nSyntax: LastLinesBetweenStartsWith(string startLineStartsWith, string endLineStartsWith)"));
                data.Add(new LuaCompletionData("RemoveAnsiCodes", "Removes all ANSI codes from a string or array.\r\nSyntax: RemoveAnsiCodes(string[] array)\r\nSyntax: RemoveAnsiCodes(string str)"));
                data.Add(new LuaCompletionData("SetStatusText", "Sets the text of the status bar.\r\nSyntax: SetStatusText(string message)"));
                data.Add(new LuaCompletionData("IndexOf", "Returns the zero based index of the first occurrence of a string in another string.\r\nSyntax: public int IndexOf(string str, string search)\r\nSyntax: public int IndexOf(string str, string search, int start)\r\nSyntax: public int IndexOf(string str, string search, int start, int length)"));
                data.Add(new LuaCompletionData("LastIndexOf", "Returns the zero based index of the last occurrence of a string in another string.\r\nSyntax: public int LastIndexOf(string str, string search)\r\nSyntax: public int LastIndexOf(string str, string search, int start)\r\nSyntax: public int LastIndexOf(string str, string search, int start, int length)"));
                data.Add(new LuaCompletionData("IsNumber", "Returns whether the string is a number.\r\nSyntax: IsNumber(string value)"));
                data.Add(new LuaCompletionData("IsEven", "Returns whether a provided number is even.\r\nSyntax: IsEven(int value)"));
                data.Add(new LuaCompletionData("IsOdd", "Returns whether a number is odd.\r\nSyntax: IsOdd(int value)"));
                data.Add(new LuaCompletionData("IsInterval", "Returns whether the number is of a specified interval.\r\n:Syntax: IsInterval(int value, int interval)"));
                data.Add(new LuaCompletionData("Clamp", "Returns the value if it falls in the range of the max and min.  Otherwise it returns the upper or lower boundary depending on which one the value passed.\r\nSyntax: Clamp(int value, int min, int max)"));
                data.Add(new LuaCompletionData("DeleteLeft", "Deletes the specified number of characters off the start of the string.  If the length is greater than the length of the string an empty string is returned.\r\nSyntax: DeleteLeft(string buf, int length)"));
                data.Add(new LuaCompletionData("DeleteRight", "Deletes the specified number of characters off the end of the string.  If the length is greater than the length of the string an empty string is returned.\r\nSyntax: DeleteRight(string buf, int length)"));
                data.Add(new LuaCompletionData("FirstWord", "Returns the first word in the specified string.\r\nSyntax: FirstWord(string text)"));
                data.Add(new LuaCompletionData("SecondWord", "Returns the second word in the specified string.\r\nSyntax: SecondWord(string text)"));
                data.Add(new LuaCompletionData("ThirdWord", "Returns the third word in the specified string.\r\n Syntax: ThirdWord(string text"));
                data.Add(new LuaCompletionData("ParseWord", "Returns the word by index from the provided string as delimited by spaces.  The delimiter can also be provided to specify a different split character.\r\nSyntax:  ParseWord(string buf, int wordNumber, string delimiter)"));
                data.Add(new LuaCompletionData("RemoveWord", "Returns a string with the specified word removed by index.\r\nSyntax: string RemoveWord(string buf, int wordIndex)"));
                data.Add(new LuaCompletionData("Between", "Returns the string between the start marker and the end marker.\r\nSyntax: Between(string buf, string beginMarker, string endMarker)"));
                data.Add(new LuaCompletionData("ToBase64", "Converts a string to Base64.\r\nSyntax: ToBase64(string buf)"));
                data.Add(new LuaCompletionData("FromBase64", "Converts a Base64 string back to it's original state.\r\nSyntax: FromBase64(string buf"));
                data.Add(new LuaCompletionData("HtmlEncode", "HTML Encodes a string.\r\nSyntax: HtmlEncode(string buf)"));
                data.Add(new LuaCompletionData("HtmlDecode", "HTML decodes a string.\r\nSyntax: HtmlDecode(string buf)"));
                data.Add(new LuaCompletionData("UrlEncode", "URL encodes a string.\r\nSyntax: UrlEncode(string buf)"));
                data.Add(new LuaCompletionData("UrlDecode", "URL decodes a string.\r\nSyntax: UrlDecode(string buf)"));
                data.Add(new LuaCompletionData("WordCount", "Returns the word count in the specified string.\r\nSyntax: WordCount(string buf)"));
                data.Add(new LuaCompletionData("PadLeft", "Returns a string that right aligns the instance by padding characters onto the the left until the total width is attained.  If the total width is less than the provided string the provided string is returned.\r\nSyntax: PadLeft(string buf, int totalWidth)"));
                data.Add(new LuaCompletionData("PadRight", "Returns a string that right aligns the instance by padding characters onto the the left until the total width is attained.  If the total width is less than the provided string the provided string is returned.\r\nSyntax: PadRight(string buf, int totalWidth)"));
            }
        }

        public static void LoadCompletionDatasnippets(IList<ICompletionData> data)
        {
            data.Add(new LuaCompletionData("Scheduled Tasks", "A snippet to show how to use scheduled tasks", ""));
            data.Add(new LuaCompletionData("For Loop", "A snippet to show how to use scheduled tasks", ""));
            data.Add(new LuaCompletionData("For Loop Pairs", "A snippet to show how to use scheduled tasks", ""));
        }

    }
}
