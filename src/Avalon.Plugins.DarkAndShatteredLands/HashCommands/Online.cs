/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using Avalon.HashCommands;
using System.Net.Http;

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
                buf = buf.Replace("[ Bloodlust ]", "[ {rBloodlust{x ]");
                buf = buf.Replace("[ White Robes ]", "[ {WWhite Robes{x ]");
                buf = buf.Replace("[ Red Robes ]", "[ {RRed Robes{x ]");
                buf = buf.Replace("[ Black Robes ]", "[ {DBlack Robes{x ]");
                buf = buf.Replace("[ Shalonesti ]", "[ {gShalonesti{x ]");
                buf = buf.Replace("[ Wargar ]", "[ {CWargar{x ]");
                buf = buf.Replace("[ Justice ]", "[ {bJustice{x ]");
                buf = buf.Replace("[ Knighthood ]", "[ {BKnighthood{x ]");
                buf = buf.Replace("[ Shadow ]", "[ {wShadow{x ]");
                buf = buf.Replace("[ Chaos ]", "[ {DChaos{x ]");
                buf = buf.Replace("[ Slayers ]", "[ {YSlayers{x ]");
                buf = buf.Replace("(WANTED)", "({RWANTED{x)");
                buf = buf.Replace("(Hostile)", "({WHostile{x)");
                buf = buf.Replace("( Dragon )", "( {GDragon{x )");
                buf = buf.Replace("(THAX)", "{C({cTHAX{C){x");
                buf = buf.Replace("(Marauders)", "{C({cMarauders{C){x");
                buf = buf.Replace("(NT)", "{C({cNT{C){x");
                buf = buf.Replace("(AR)", "{C({cAR{C){x");
                buf = buf.Replace("(SH)", "{C({cSH{C){x");
                buf = buf.Replace("(AL)", "{C({cAL{C){x");
                buf = buf.Replace("(Darkonin)", "{C({cDarkonin{C){x");
                buf = buf.Replace("(Abaddon)", "{C({cAbaddon{C){x");
                //buf = buf.Replace("()", "{C({c{C){x");

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