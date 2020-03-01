using System.Diagnostics;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{
    /// <summary>
    /// The ability to turn scraping on and off.
    /// </summary>
    /// <remarks>
    /// Since commands are sent async turning scraping on, issuing a command and then turning it off results
    /// in not data being scraped.  To be effective this will need to have a delay after the command is issued
    /// but before the command to turn scraping off to give the command time to be sent and data to be sent back.
    /// It's not an exact science because there's no way to know what should be sent back unless we allow scraping
    /// to be turned off by finding an end marker which is a neat idea.  We'll call this scrape v1.
    ///
    /// TODO - Temporary variable to store this in that isn't saved in the settings file.
    /// </remarks>
    public class Scrape : HashCommand
    {
        public Scrape(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#scrape";

        public override string Description { get; } = "The ability to turn scraping on and off.  Syntax: #scrape <on|off|echo>";

        public override void Execute()
        {
            if (string.Equals(this.Parameters, "ON", System.StringComparison.OrdinalIgnoreCase))
            {
                this.Interpreter.Conveyor.ScrapeEnabled = true;
            }
            else if (string.Equals(this.Parameters, "OFF", System.StringComparison.OrdinalIgnoreCase))
            {
                this.Interpreter.Conveyor.ScrapeEnabled = false;
            }
            else if (string.Equals(this.Parameters, "ECHO", System.StringComparison.OrdinalIgnoreCase))
            {
                this.Interpreter.EchoText(this.Interpreter.Conveyor.Scrape.ToString());
            }
            else if (string.Equals(this.Parameters, "CLEAR", System.StringComparison.OrdinalIgnoreCase))
            {
                this.Interpreter.Conveyor.ScrapeEnabled = false;
                this.Interpreter.Conveyor.Scrape.Clear();
            }
            else
            {
                this.Interpreter.EchoText("--> Syntax: #scrape <on|off|echo|clear>", AnsiColors.Yellow);
            }
        }
    }
}
