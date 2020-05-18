﻿using ICSharpCode.AvalonEdit.CodeCompletion;
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
                data.Add(new LuaCompletionData("Send", "Sends a command to the game."));
                data.Add(new LuaCompletionData("GetVariable", "Gets a global variable that is shared with the entire mud client."));
                data.Add(new LuaCompletionData("SetVariable", "Sets a variable that is shared with the entire mud client."));
                data.Add(new LuaCompletionData("Echo", "Echos text to the game terminal."));
                data.Add(new LuaCompletionData("EchoEvent", "Echos a highlighted set of text to the game terminal."));
                data.Add(new LuaCompletionData("Coalesce", "Returns the first non null and non empty value from the list"));
                data.Add(new LuaCompletionData("GetTime", "Gets the current time."));
                data.Add(new LuaCompletionData("GetMinute", "Gets the current minute."));
                data.Add(new LuaCompletionData("GetHour", "Gets the current hour."));
                data.Add(new LuaCompletionData("GetSecond", "Gets the current second."));
                data.Add(new LuaCompletionData("GetMillisecond", "Gets the current millisecond."));
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
                data.Add(new LuaCompletionData("ListAdd", "Adds an item to a list."));
                data.Add(new LuaCompletionData("ListAddStart", "Adds an item to the start of a list."));
                data.Add(new LuaCompletionData("ListAddIfNotExist", "Adds an item to the end of a list if that item doesn't already exist."));
                data.Add(new LuaCompletionData("ListRemove", "Removes all items from a list that match the provided key."));
                data.Add(new LuaCompletionData("ListExists", "Whether an item exists in a list."));
                data.Add(new LuaCompletionData("Replace", "Replaces a pattern in text with another pattern."));
                data.Add(new LuaCompletionData("EnableGroup", "Enables a group of aliases and triggers."));
                data.Add(new LuaCompletionData("DisableGroup", "Disables a group of aliases and triggers."));
                data.Add(new LuaCompletionData("AddScheduledTask", "Adds a scheduled task to be performed after a period of time."));
                data.Add(new LuaCompletionData("AddBatchTask", "Adds a batch task to be performed one after another with a delay in between them."));
                data.Add(new LuaCompletionData("ClearTasks", "Clears all tasks."));
                data.Add(new LuaCompletionData("FormatNumber", "Formats a number with commas."));
            }
        }

        public static void LoadCompletionDataSnippits(IList<ICompletionData> data)
        {
            data.Add(new LuaCompletionData("Scheduled Tasks", "A snippit to show how to use scheduled tasks", ""));
            data.Add(new LuaCompletionData("For Loop", "A snippit to show how to use scheduled tasks", ""));
            data.Add(new LuaCompletionData("For Loop Pairs", "A snippit to show how to use scheduled tasks", ""));

        }

    }
}