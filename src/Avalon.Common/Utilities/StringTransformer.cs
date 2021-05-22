/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Avalon.Common.Utilities
{
    /// <summary>
    /// A class to manage a list of lines who require operations to be performed on those lines.  The lines are kept internally
    /// and removed or updated.  When ToString or ToStringBuilder are called the outputted text is rebuilt from what
    /// is left in the Lines list.  The lines are parsed when initially passed in and stored in the <see cref="Lines"/>
    /// property against which all operations will occur.  Transforms that need to occur against the full string will force
    /// the full string to be realized.
    /// </summary>
    /// <remarks>
    /// I don't know that I'm in love with this class.  It feels overly complicated and inefficient but it works for
    /// now to accomplish string updates I need sooner than later.
    /// </remarks>
    public class StringTransformer
    {
        /// <summary>
        /// A single internal StringBuilder used to process and return the output.
        /// </summary>
        private readonly StringBuilder _sb = new StringBuilder();

        /// <summary>
        /// The list of individual lines that we will filter down.
        /// </summary>
        public List<string> Lines { get; set; } = new List<string>();

        /// <summary>
        /// Returns the number of lines currently in the Lines list.
        /// </summary>
        public int LineCount => this.Lines.Count;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text"></param>
        public StringTransformer(string text)
        {
            BuildLines(text);
        }

        /// <summary>
        /// Builds the lines from the stored <see cref="StringBuilder" />.
        /// </summary>
        private void BuildLines()
        {
            this.Lines = _sb.ToString().Replace("\r", "").Split('\n').ToList();
        }

        /// <summary>
        /// Builds the lines from the provided text.
        /// </summary>
        /// <param name="text"></param>
        private void BuildLines(string text)
        {
            this.Lines = text.Replace("\r", "").Split('\n').ToList();
        }

        /// <summary>
        /// Builds the <see cref="StringBuilder"/> from the contents of <see cref="Lines"/>.
        /// </summary>
        private void BuildString()
        {
            _sb.Clear();

            foreach (string line in this.Lines)
            {
                _sb.AppendLine(line);
            }
        }

        /// <summary>
        /// Removes a line if it starts with the supplied pattern.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="compareType"></param>
        public void RemoveLineIfStartsWith(string pattern, StringComparison compareType = StringComparison.Ordinal)
        {
            for (int i = this.Lines.Count - 1; i >= 0; i--)
            {
                if (this.Lines[i].StartsWith(pattern, compareType))
                {
                    this.Lines.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Removes a line if it starts with any of the supplied patterns.
        /// </summary>
        /// <param name="patterns"></param>
        /// <param name="compareType"></param>
        public void RemoveLineIfStartsWith(string[] patterns, StringComparison compareType = StringComparison.Ordinal)
        {
            for (int i = this.Lines.Count - 1; i >= 0; i--)
            {
                foreach (string pattern in patterns)
                {
                    if (this.Lines[i].StartsWith(pattern, compareType))
                    {
                        // If the match is found, remove it, then break out of the inner loop.
                        this.Lines.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Removes a line if it starts with the supplied pattern.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="compareType"></param>
        public void RemoveLineIfEndsWith(string pattern, StringComparison compareType = StringComparison.Ordinal)
        {
            for (int i = this.Lines.Count - 1; i >= 0; i--)
            {
                if (this.Lines[i].EndsWith(pattern, compareType))
                {
                    this.Lines.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Removes a line if it ends with any of the supplied patterns.
        /// </summary>
        /// <param name="patterns"></param>
        /// <param name="compareType"></param>
        public void RemoveLineIfEndsWith(string[] patterns, StringComparison compareType = StringComparison.Ordinal)
        {
            for (int i = this.Lines.Count - 1; i >= 0; i--)
            {
                foreach (string pattern in patterns)
                {
                    if (this.Lines[i].EndsWith(pattern, compareType))
                    {
                        // If the match is found, remove it, then break out of the inner loop.
                        this.Lines.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Removes a line if it starts with the supplied pattern.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="compareType"></param>
        public void RemoveLineIfContains(string pattern, StringComparison compareType = StringComparison.Ordinal)
        {
            for (int i = this.Lines.Count - 1; i >= 0; i--)
            {
                // Meh, Compare doesn't support StringComparison until .NET Standard 2.1
                if (this.Lines[i].IndexOf(pattern, 0, compareType) >= 0)
                {
                    this.Lines.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Removes a line if it contains with any of the supplied patterns.
        /// </summary>
        /// <param name="patterns"></param>
        /// <param name="compareType"></param>
        public void RemoveLineIfContains(string[] patterns, StringComparison compareType = StringComparison.Ordinal)
        {
            for (int i = this.Lines.Count - 1; i >= 0; i--)
            {
                foreach (string pattern in patterns)
                {
                    if (this.Lines[i].IndexOf(pattern, 0, compareType) >= 0)
                    {
                        // If the match is found, remove it, then break out of the inner loop.
                        this.Lines.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Removes a line if it equals the supplied pattern.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="compareType"></param>
        public void RemoveLineIfEquals(string pattern, StringComparison compareType = StringComparison.Ordinal)
        {
            for (int i = this.Lines.Count - 1; i >= 0; i--)
            {
                if (this.Lines[i].Equals(pattern, compareType))
                {
                    this.Lines.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Removes a line if it equals with any of the supplied patterns.
        /// </summary>
        /// <param name="patterns"></param>
        /// <param name="compareType"></param>
        public void RemoveLineIfEquals(string[] patterns, StringComparison compareType = StringComparison.Ordinal)
        {
            for (int i = this.Lines.Count - 1; i >= 0; i--)
            {
                foreach (string pattern in patterns)
                {
                    if (this.Lines[i].Equals(pattern, compareType))
                    {
                        // If the match is found, remove it, then break out of the inner loop.
                        this.Lines.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Removes a line if it meets the Regex pattern.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="options">Regular Expression Options: Default is SingleLine only.</param>
        public void RemoveLineIfRegexMatches(string pattern, RegexOptions options = RegexOptions.Singleline)
        {
            for (int i = this.Lines.Count - 1; i >= 0; i--)
            {
                if (Regex.IsMatch(this.Lines[i], pattern, options))
                {
                    this.Lines.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Removes a line if it matches any of the supplied Regex patterns.
        /// </summary>
        /// <param name="patterns"></param>
        /// <param name="options">Regular Expression Options: Default is SingleLine only.</param>
        public void RemoveLineIfRegexMatches(string[] patterns, RegexOptions options = RegexOptions.Singleline)
        {
            for (int i = this.Lines.Count - 1; i >= 0; i--)
            {
                foreach (string pattern in patterns)
                {
                    if (Regex.IsMatch(this.Lines[i], pattern, options))
                    {
                        this.Lines.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Removes a line if it is null or whitespace.
        /// </summary>
        public void RemoveIfNullOrWhiteSpace()
        {
            for (int i = this.Lines.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrWhiteSpace(this.Lines[i]))
                {
                    this.Lines.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Removes a line if it is null or whitespace.
        /// </summary>
        public void RemoveLineIfIsNullOrEmpty()
        {
            for (int i = this.Lines.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(this.Lines[i]))
                {
                    this.Lines.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Removes a line if its word count exceeds the specified value.
        /// </summary>
        /// <param name="words"></param>
        public void RemoveLineIfWordCountEquals(int words)
        {
            for (int i = this.Lines.Count - 1; i >= 0; i--)
            {
                if (WordCount(this.Lines[i]) == words)
                {
                    this.Lines.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Removes a line if its word count exceeds the specified value.
        /// </summary>
        /// <param name="words"></param>
        public void RemoveLineIfWordCountGreaterThan(int words)
        {
            for (int i = this.Lines.Count - 1; i >= 0; i--)
            {
                if (WordCount(this.Lines[i]) > words)
                {
                    this.Lines.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Removes a line if its word count is less than the specified value.
        /// </summary>
        /// <param name="words"></param>
        public void RemoveLineIfWordCountLessThan(int words)
        {
            for (int i = this.Lines.Count - 1; i >= 0; i--)
            {
                if (WordCount(this.Lines[i]) < words)
                {
                    this.Lines.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Removes duplicate lines keeping the first instance.
        /// </summary>
        public void RemoveDuplicateLines()
        {
            this.Lines = (List<string>)this.Lines.Distinct();
        }

        /// <summary>
        /// Removes two blank lines in a row and replaces them with one.
        /// </summary>
        public void RemoveDoubleBlankLines()
        {
            // Get rid of lines with only white space.
            for (int i = this.Lines.Count - 1; i >= 0; i--)
            {
                bool onlySpace = true;
                for (int c = 0; c < this.Lines[i].Length; c++)
                {
                    if (!char.IsWhiteSpace(this.Lines[i][c]))
                    {
                        onlySpace = false;
                        break;
                    }
                }

                if (onlySpace)
                {
                    this.Lines[i] = "";
                }
            }

            BuildString();

            string text = _sb.ToString();
            text = text.Replace("\r\n", "\n");
            bool containsPattern = text.Contains("\n\n\n");

            while (containsPattern)
            {
                text = text.Replace("\n\n\n", "\n\n");
                containsPattern = text.Contains("\n\n\n");
            }

            text = text.Replace("\n", "\r\n");

            _sb.Clear();
            _sb.Append(text);

            BuildLines();
        }

        /// <summary>
        /// Replaces all occurrences of a string with another string in the <see cref="Lines"/>.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="replaceWith"></param>
        public void Replace(string pattern, string replaceWith)
        {
            BuildString();
            _sb.Replace(pattern, replaceWith);
            BuildLines();
        }

        /// <summary>
        /// Replaces all occurrences of a char with another char in the <see cref="Lines"/> list.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="replaceWith"></param>
        public void Replace(char c, char replaceWith)
        {
            BuildString();
            _sb.Replace(c, replaceWith);
            BuildLines();
        }

        /// <summary>
        /// Replaces a regular expression match with the provided string run against each line.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="replaceWith"></param>
        public void ReplaceRegex(string pattern, string replaceWith)
        {
            BuildString();
            string temp = Regex.Replace(_sb.ToString(), pattern, replaceWith, RegexOptions.Singleline);
            BuildLines(temp);
        }

        /// <summary>
        /// Replaces a regular expression match with the provided string run against each line.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="replaceWith"></param>
        public void ReplaceRegexPerLine(string pattern, string replaceWith)
        {
            for (int i = 0; i < this.Lines.Count - 1; i++)
            {
                this.Lines[i] = Regex.Replace(this.Lines[i], pattern, replaceWith);
            }
        }

        /// <summary>
        /// Appends the provided text to each line.
        /// </summary>
        /// <param name="text"></param>
        public void AppendToLine(string text)
        {
            for (int i = 0; i < this.Lines.Count - 1; i++)
            {
                this.Lines[i] = $"{this.Lines[i]}{text}";
            }
        }

        /// <summary>
        /// Prepends the provided text to each line.
        /// </summary>
        /// <param name="text"></param>
        public void PrependToLine(string text)
        {
            for (int i = 0; i < this.Lines.Count - 1; i++)
            {
                this.Lines[i] = $"{text}{this.Lines[i]}";
            }
        }

        /// <summary>
        /// Wraps each line with the specified before and after text.
        /// </summary>
        /// <param name="before"></param>
        /// <param name="after"></param>
        public void WrapLine(string before, string after)
        {
            for (int i = 0; i < this.Lines.Count - 1; i++)
            {
                this.Lines[i] = $"{before}{this.Lines[i]}{after}";
            }
        }

        /// <summary>
        /// Returns the word count in the current string accounting for whitespace.
        /// </summary>
        /// <param name="text"></param>
        private int WordCount(string text)
        {
            int wordCount = 0;
            int index = 0;

            // Skip whitespace until first word.
            while (index < text.Length && char.IsWhiteSpace(text[index]))
            {
                index++;
            }

            while (index < text.Length)
            {
                // Check if current char is part of a word.
                while (index < text.Length && !char.IsWhiteSpace(text[index]))
                {
                    index++;
                }

                wordCount++;

                // Skip whitespace until next word.
                while (index < text.Length && char.IsWhiteSpace(text[index]))
                {
                    index++;
                }
            }

            return wordCount;
        }

        /// <summary>
        /// Returns a string built from the current contents of <see cref="Lines"/>.
        /// </summary>
        public override string ToString()
        {
            this.BuildString();
            return _sb.ToString();
        }

        /// <summary>
        /// Returns a StringBuilder with the filtered output.
        /// </summary>
        public StringBuilder ToStringBuilder()
        {
            this.BuildString();
            return _sb;
        }
    }
}
