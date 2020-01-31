using CommandLine;

namespace Avalon
{
    /// <summary>
    /// The supported command line arguments for this application.
    /// </summary>
    public class CommandLineArguments
    {
        [Option('p', "profile", Required = false, HelpText = "The name of the profile to load from the applications data folder.")]
        public string Profile { get; set; }
    }
}
