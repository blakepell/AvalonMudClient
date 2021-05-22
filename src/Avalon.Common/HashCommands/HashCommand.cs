/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;
using System.Text;
using System.Threading.Tasks;
using Avalon.Common.Interfaces;
using CommandLine;
using CommandLine.Text;

namespace Avalon.HashCommands
{
    /// <summary>
    /// Base class for a hash command.
    /// </summary>
    public abstract class HashCommand : IHashCommand
    {
        protected HashCommand(IInterpreter interp)
        {
            this.Interpreter = interp;
        }

        protected HashCommand()
        {

        }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public string Parameters { get; set; }

        public virtual void Execute()
        {
        }

        public virtual Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }

        public IInterpreter Interpreter { get; set; }

        public bool IsAsync { get; set; }


        /// <summary>
        /// Displays the parser output from the model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        public void DisplayParserOutput<T>(ParserResult<T> result)
        {
            if (result.Tag == ParserResultType.NotParsed)
            {
                var helpText = HelpText.AutoBuild(result, h =>
                {
                    h.AutoVersion = false;
                    h.Copyright = "";
                    h.AdditionalNewLineAfterOption = false;
                    h.Heading = $"{this.Name}: {this.Description}";
                    return h;
                });

                this.Interpreter.EchoText(helpText);
            }
        }

        /// <summary>
        /// Command line argument parser.
        /// </summary>
        /// <param name="commandLine">Command line string with arguments.</param>
        public static string[] CreateArgs(string commandLine)
        {
            var sb = new StringBuilder(commandLine);
            bool inQuote = false;

            // Character 30 is a record separator.
            const char splitter = (char)30;

            // Convert the spaces to a record separator sign so we can split at newline
            // later on.  Only convert spaces which are outside the boundaries of quoted text.
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i].Equals('"'))
                {
                    inQuote = !inQuote;
                }

                if (sb[i].Equals(' ') && !inQuote)
                {
                    sb[i] = splitter;
                }
            }

            // Split to args array
            var args = sb.ToString().Split(new[] { splitter }, StringSplitOptions.RemoveEmptyEntries);

            // Clean the '"' signs from the args as needed.
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = ClearQuotes(args[i]);
            }

            return args;
        }

        /// <summary>
        /// Cleans quotes from the arguments.
        ///   - All single quotes will be removed.
        ///   - All single double quotes (") will be removed.
        ///   - Every pair of double double quotes ("") will transform to a single quote.
        /// </summary>
        /// <param name="stringWithQuotes">A string with quotes.</param>
        /// <returns>The same string if its without quotes, or a clean string if its with quotes.</returns>
        private static string ClearQuotes(string stringWithQuotes)
        {
            int quoteIndex;

            if ((quoteIndex = stringWithQuotes.IndexOf('"')) == -1)
            {
                // String is without quotes..
                return stringWithQuotes;
            }

            // Linear sb scan is faster than string assignment if quote count is 2 or more (=always)
            var sb = new StringBuilder(stringWithQuotes);

            for (int i = quoteIndex; i < sb.Length; i++)
            {
                if (sb[i].Equals('"'))
                {
                    // If we are not at the last index and the next one is '"', we need to jump one to preserve one
                    if (i != sb.Length - 1 && sb[i + 1].Equals('"'))
                    {
                        i++;
                    }

                    // We remove and then set index one backwards.
                    // This is because the remove itself is going to shift everything left by 1.
                    sb.Remove(i--, 1);
                }
            }

            return sb.ToString();
        }

    }
}
