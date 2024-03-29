﻿/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using CommandLine;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Command that allow for interaction with a trigger.
    /// </summary>
    public class Trigger : HashCommand
    {
        public Trigger(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#trigger";

        public override string Description { get; } = "Command that allows for interactions with a specific trigger.";

        public override void Execute()
        {
            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                .WithParsed(o =>
                {
                    if (o.Enable && o.Disable)
                    {
                        this.Interpreter.Conveyor.EchoLog("You cannot both enable and disable a trigger at the same time.", Common.Models.LogType.Error);
                        return;
                    }

                    // System triggers that match the ID.
                    foreach (var t in App.InstanceGlobals.SystemTriggers.Where(x => x.Identifier == o.Id))
                    {
                        if (o.Enable)
                        {
                            t.Enabled = true;

                            if (o.Verbose)
                            {
                                this.Interpreter.Conveyor.EchoLog($"A trigger with the ID of '{o.Id}' was enabled.", Common.Models.LogType.Success);
                            }
                        }

                        if (o.Disable)
                        {
                            t.Enabled = false;

                            if (o.Verbose)
                            {
                                this.Interpreter.Conveyor.EchoLog($"A trigger with the ID of '{o.Id}' was disabled.", Common.Models.LogType.Success);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(o.StopProcessing))
                        {
                            t.StopProcessing = bool.Parse(o.StopProcessing);

                            if (o.Verbose)
                            {
                                this.Interpreter.Conveyor.EchoLog($"A trigger with the ID of '{o.Id}' had stop processing set to {t.StopProcessing.ToString()}.", Common.Models.LogType.Success);
                            }
                        }

                        // The pattern has to be something
                        if (!string.IsNullOrWhiteSpace(o.Pattern))
                        {
                            t.Pattern = o.Pattern;

                            if (o.Verbose)
                            {
                                this.Interpreter.Conveyor.EchoLog($"A trigger with the ID of '{o.Id}' had its pattern set to {o.Pattern}.", Common.Models.LogType.Success);
                            }
                        }

                        // The command only has to not be null so it can be cleared.
                        if (o.Command != null)
                        {
                            t.Command = o.Command;

                            if (o.Verbose)
                            {
                                this.Interpreter.Conveyor.EchoLog($"A trigger with the ID of '{o.Id}' had its command set to {o.Command}.", Common.Models.LogType.Success);
                            }
                        }

                    }

                    bool found = false;

                    // Update - User created triggers that match the ID.
                    foreach (var t in App.Settings.ProfileSettings.TriggerList.Where(x => x.Identifier == o.Id))
                    {
                        found = true;

                        if (o.Enable)
                        {
                            t.Enabled = true;

                            if (o.Verbose)
                            {
                                this.Interpreter.Conveyor.EchoLog($"A trigger with the ID of '{o.Id}' was enabled.", Common.Models.LogType.Success);
                            }
                        }

                        if (o.Disable)
                        {
                            t.Enabled = false;

                            if (o.Verbose)
                            {
                                this.Interpreter.Conveyor.EchoLog($"A trigger with the ID of '{o.Id}' was disabled.", Common.Models.LogType.Success);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(o.StopProcessing))
                        {
                            t.StopProcessing = bool.Parse(o.StopProcessing);

                            if (o.Verbose)
                            {
                                this.Interpreter.Conveyor.EchoLog($"A trigger with the ID of '{o.Id}' had stop processing set to {t.StopProcessing.ToString()}.", Common.Models.LogType.Success);
                            }
                        }

                        // The pattern has to be something
                        if (!string.IsNullOrWhiteSpace(o.Pattern))
                        {
                            t.Pattern = o.Pattern;
                            
                            if (o.Verbose)
                            {
                                this.Interpreter.Conveyor.EchoLog($"A trigger with the ID of '{o.Id}' had its pattern set to {o.Pattern}.", Common.Models.LogType.Success);
                            }
                        }

                        // The command only has to not be null so it can be cleared.
                        if (o.Command != null)
                        {
                            t.Command = o.Command;

                            if (o.Verbose)
                            {
                                this.Interpreter.Conveyor.EchoLog($"A trigger with the ID of '{o.Id}' had its command set to {o.Command}.", Common.Models.LogType.Success);
                            }
                        }

                    }

                    // Create
                    if (!found)
                    {
                        // Create the trigger.
                        var t = new Common.Triggers.Trigger(o.Pattern, o.Command ?? "", "", false, o.Id)
                        {
                            Group = o.Group ?? "", VariableReplacement = o.VariableReplacement
                        };

                        if (!string.IsNullOrWhiteSpace(o.StopProcessing))
                        {
                            t.StopProcessing = bool.Parse(o.StopProcessing);
                        }

                        // Enabled is default, they can force it to be created disabled.
                        if (o.Disable)
                        {
                            t.Enabled = false;
                        }

                        // Add it to the list.  BUG: This could cause a bug if a trigger is added and this has
                        // been called from the trigger loop itself (e.g. you're adding to a collection that is being
                        // currently enumerated.
                        App.Settings.ProfileSettings.TriggerList.Add(t);

                        if (o.Verbose)
                        {
                            this.Interpreter.Conveyor.EchoLog($"A trigger with the ID of '{o.Id}' has been created.", Common.Models.LogType.Success);
                        }
                    }
                });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// The supported command line arguments.
        /// </summary>
        private class Arguments
        {

            [Option('i', "id", Required = true, HelpText = "The ID of the trigger.")]
            public string Id { get; set; } = "";

            [Option('d', "disable", Required = false, HelpText = "Whether the trigger should be disabled.")]
            public bool Disable { get; set; }

            [Option('e', "enable", Required = false, HelpText = "Whether the trigger should be enabled.")]
            public bool Enable { get; set; }

            [Option('v', "verbose", Required = false, HelpText = "Whether echo to the terminal even on success.")]
            public bool Verbose { get; set; }

            [Option('r', "replacement", Required = false, HelpText = "Whether variables should be replaced in the trigger.")]
            public bool VariableReplacement { get; set; } = false;

            [Option('p', "pattern", Required = false, HelpText = "Sets the pattern of the trigger.")]
            public string Pattern { get; set; }

            [Option('c', "command", Required = false, HelpText = "Sets the command of the trigger.")]
            public string Command { get; set; }

            [Option('g', "group", Required = false, HelpText = "Sets the group of the trigger.")]
            public string Group { get; set; }

            [Option('s', "stop", Required = false, HelpText = "[true|false] Whether the trigger should stop the processing the remaining lower priority triggers.")]
            public string StopProcessing { get; set; }

        }
    }
}
