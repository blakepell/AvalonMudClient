using Argus.Extensions;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Avalon.HashCommands;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Avalon.Plugins.DarkAndShatteredLands.HashCommands
{
    public class ConCard : HashCommand
    {
        public ConCard(IInterpreter interp) : base(interp)
        {
            this.IsAsync = true;
        }

        public ConCard()
        {
            this.IsAsync = true;
        }

        public override string Name { get; } = "#con-card";

        public override string Description { get; } = "Checks the status of a con card.";

        public override void Execute()
        {
        }

        public override async Task ExecuteAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Parameters))
            {
                this.Interpreter.Conveyor.EchoLog("You must enter a con card value.", Common.Models.LogType.Error);
                return;
            }

            if (this.Parameters.Length > 45)
            {
                this.Interpreter.Conveyor.EchoLog("The value you provided is too long.", Common.Models.LogType.Error);
                return;
            }

            // Clean it up.
            string conCard = this.Parameters.Trim().RemoveSpecialCharacters().HtmlEncode();

            this.Interpreter.Conveyor.EchoText("\r\n");
            this.Interpreter.Conveyor.EchoLog($"Checking the status of the con card '{conCard}' from dsl-mud.org", Common.Models.LogType.Information);

            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("CardCode", conCard)
            });

            try
            {
                // The HttpClient is unique.. it implements IDisposable but DO NOT call Dispose.  It's meant to be used throughout
                // the life of your application and will be re-used by the framework.  Odd but that's what it is.
                var http = new HttpClient();
                var response = await http.PostAsync("http://www.dsl-mud.org/inside/checkcard.asp", form);
                string html = await response.Content.ReadAsStringAsync();
                html = html.HtmlDecode();

                if (html.Contains("has not been submitted"))
                {
                    var line = new Line
                    {
                        ReverseColors = true,
                        ForegroundColor = AnsiColors.Green,
                        IgnoreLastColor = true
                    };

                    line.FormattedText.AppendLine("\r\nCard not yet submitted.");

                    this.Interpreter.Conveyor.EchoText(line, TerminalTarget.Main);

                    string buf = Between(html, "<font size=\"5\" face=\"Verdana\">", "</font></b>");
                    this.Interpreter.Conveyor.EchoText($"{buf.Replace("<br>", "\r\nThe card is a ")}\r\n");
                }
                else if (html.Contains("500 - Internal server error."))
                {
                    var line = new Line
                    {
                        ReverseColors = true,
                        ForegroundColor = AnsiColors.Red,
                        IgnoreLastColor = true
                    };

                    line.FormattedText.AppendLine("\r\n500 - Internal server error.");

                    this.Interpreter.Conveyor.EchoText(line, TerminalTarget.Main);
                    this.Interpreter.Conveyor.EchoText("There is a problem with the resource you are looking for, and it cannot be displayed.", TerminalTarget.Main);
                }
                else if (html.Contains("card is invalid"))
                {
                    var line = new Line
                    {
                        ReverseColors = true,
                        ForegroundColor = AnsiColors.Yellow,
                        IgnoreLastColor = true
                    };

                    line.FormattedText.AppendLine("\r\nInvalid Card");

                    this.Interpreter.Conveyor.EchoText(line, TerminalTarget.Main);
                    this.Interpreter.Conveyor.EchoText("The Abhorrant says \"That card is invalid.Please check your cards code and try again.\"", TerminalTarget.Main);
                }
                else if (html.Contains("That card has already been processed"))
                {
                    var line = new Line
                    {
                        ReverseColors = true,
                        ForegroundColor = AnsiColors.Green,
                        IgnoreLastColor = true
                    };

                    line.FormattedText.AppendLine("\r\nCard Processed");

                    this.Interpreter.Conveyor.EchoText(line, TerminalTarget.Main);

                    string buf = Between(html, "<br>", "</font></b>");
                    this.Interpreter.Conveyor.EchoText($"{buf}\r\n");
                }
                else
                {
                    var line = new Line
                    {
                        ReverseColors = true,
                        ForegroundColor = AnsiColors.Yellow,
                        IgnoreLastColor = true
                    };

                    line.FormattedText.Append("\r\nUnknown State\r\n");

                    this.Interpreter.Conveyor.EchoText(line, TerminalTarget.Main);

                    string buf = Between(html, "<br>", "</font></b>");
                    this.Interpreter.Conveyor.EchoText($"{buf}\r\n");
                }

            }
            catch (Exception ex)
            {
                this.Interpreter.Conveyor.EchoLog(ex.ToFormattedString(), LogType.Error);
            }

        }

        /// <summary>
        /// Returns a string between two markers.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="beginMarker"></param>
        /// <param name="endMarker"></param>
        private string Between(ReadOnlySpan<char> span, ReadOnlySpan<char> beginMarker, ReadOnlySpan<char> endMarker)
        {
            int pos1 = span.IndexOf(beginMarker) + beginMarker.Length;
            int pos2 = span.Slice(pos1).IndexOf(endMarker);
            return span.Slice(pos1, pos2).ToString();
        }

    }

}