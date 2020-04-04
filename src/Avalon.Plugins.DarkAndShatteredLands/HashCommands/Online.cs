using Avalon.Common.Interfaces;
using Avalon.HashCommands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Argus.Extensions;
using System.Net.Http;
using System.Diagnostics;

namespace Avalon.Plugins.DarkAndShatteredLands.HashCommands
{
    public class Online : HashCommand
    {
        public Online(IInterpreter interp) : base(interp)
        {
            this.IsAsync = true;
        }

        public Online()
        {
            this.IsAsync = true;
        }

        public override string Name { get; } = "#online";

        public override string Description { get; } = "Shows the online characters as per the dsl-mud.org webpage.";

        public override void Execute()
        {
        }

        public override async Task ExecuteAsync()
        {
            this.Interpreter.Conveyor.EchoText("\r\n");
            this.Interpreter.Conveyor.EchoLog($"Checking the online players via http://www.dsl-mud.org/players_online.asp", Common.Models.LogType.Information);

            try
            {
                // The HttpClient is unique.. it implements IDisposable but DO NOT call Dispose.  It's meant to be used throughout
                // the life of your application and will be re-used by the framework.  Odd but that's what it is.
                var http = new HttpClient();

                var sw = new Stopwatch();
                sw.Start();

                var response = await http.GetAsync("http://www.dsl-mud.org/players_online.asp");
                string html = await response.Content.ReadAsStringAsync();

                sw.Stop();

                this.Interpreter.Conveyor.EchoLog($"Results returned in {sw.ElapsedMilliseconds}ms.", Common.Models.LogType.Success);

                html = html.HtmlDecode();
                string buf = Between(html, "<font face=\"Verdana\" size=\"5\">Players Online</font>", "The most we've ever had on was");
                buf = Argus.Data.Formatting.StripHtml2(buf);

                this.Interpreter.Conveyor.EchoText($"{buf}\r\n");
            }
            catch (Exception ex)
            {
                this.Interpreter.Conveyor.EchoLog(ex.ToFormattedString(), Common.Models.LogType.Error);
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