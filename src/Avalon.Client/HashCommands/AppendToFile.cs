using System;
using System.IO;
using Argus.Extensions;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Appends text to a file.
    /// </summary>
    public class AppendToFile : HashCommand
    {
        public AppendToFile(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#append-to-file";

        public override string Description { get; } = "Syntax: #append-to-file <filename> <text>";

        public override void Execute()
        {
            // TODO - FirstArgument needs to account for the argument being in quotes, or an overload for one with quotes needs to exist.
            var argOne = this.Parameters.FirstArgument();
            string argTwo = argOne.Item2;
            File.AppendAllText(argOne.Item1, argTwo);
        }

    }
}