using System;
using System.Media;
using Argus.Extensions;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;
using CommandLine;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Makes the default system sound.
    /// </summary>
    public class Beep : HashCommand
    {
        public Beep(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#beep";

        public override string Description { get; } = "Plays the system sound for a beep.";

        public override void Execute()
        {
            // Default beep
            if (this.Parameters.IsNullOrEmptyOrWhiteSpace())
            {
                SystemSounds.Beep.Play();
                return;
            }

            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<BeepArguments>(CreateArgs(this.Parameters))
                               .WithParsed(o =>
                               {
                                   switch (o.BeepType)
                                   {
                                       case BeepType.Beep:
                                           SystemSounds.Beep.Play();
                                           return;
                                       case BeepType.Asterisk:
                                           SystemSounds.Asterisk.Play();
                                           return;
                                       case BeepType.Exclamation:
                                           SystemSounds.Exclamation.Play();
                                           return;
                                       case BeepType.Hand:
                                           SystemSounds.Hand.Play();
                                           return;
                                       case BeepType.Question:
                                           SystemSounds.Question.Play();
                                           return;
                                       case BeepType.Alert:
                                           // Special type, this will play even if other system sounds are muted.
                                           if (!System.IO.File.Exists(@"Media\alert.wav"))
                                           {
                                               this.Interpreter.Conveyor.EchoLog("Media\\alert.wav does not exist.", Common.Models.LogType.Error);
                                               return;
                                           }

                                           App.Beep.Play();

                                           return;
                                       default:
                                           SystemSounds.Beep.Play();
                                           return;
                                   }
                               });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);

        }

        /// <summary>
        /// The types of beeps/alerts that are supported via the system.
        /// </summary>
        public enum BeepType
        {
            Beep,
            Asterisk,
            Exclamation,
            Hand,
            Question,
            Alert
        }

        /// <summary>
        /// The supported command line arguments the #beep hash command.
        /// </summary>
        public class BeepArguments
        {
            [Option('t', "type", Required = false, HelpText = "The type of system beep: [Beep|Asterisk|Exclamation|Hand|Question|Alert]")]
            public BeepType BeepType { get; set; }
        }

    }
}
