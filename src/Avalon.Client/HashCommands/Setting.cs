/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using CommandLine;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Hash command that can set profile settings values that have been pre-defined.
    /// </summary>
    public class Setting : HashCommand
    {
        public Setting(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#setting";

        public override string Description { get; } = "Allows interaction with traditional variables.";

        public override void Execute()
        {
            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                .WithParsed(o =>
                {
                    if (o.ClientSetting)
                    {
                        var p = App.Settings.AvalonSettings.GetType().GetProperty(o.Name);

                        switch (p.PropertyType.Name)
                        {
                            case "Int32":
                                p.SetValue(App.Settings.AvalonSettings, Convert.ToInt32(o.Value), null);
                                break;
                            case "String":
                                p.SetValue(App.Settings.AvalonSettings, o.Value, null);
                                break;
                            case "Boolean":
                                p.SetValue(App.Settings.AvalonSettings, Convert.ToBoolean(o.Value), null);
                                break;
                        }
                    }

                    if (o.ProfileSetting)
                    {
                        var p = App.Settings.ProfileSettings.GetType().GetProperty(o.Name);

                        switch (p.PropertyType.Name)
                        {
                            case "Int32":
                                p.SetValue(App.Settings.ProfileSettings, Convert.ToInt32(o.Value), null);
                                break;
                            case "String":
                                p.SetValue(App.Settings.ProfileSettings, o.Value, null);
                                break;
                            case "Boolean":
                                p.SetValue(App.Settings.ProfileSettings, Convert.ToBoolean(o.Value), null);
                                break;
                        }
                    }
                });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// The supported command line arguments for this hash command.
        /// </summary>
        public class Arguments
        {
            /// <summary>
            /// Adds the number to the variable if the variable is a number.
            /// </summary>
            [Option('s', "setting-name", Required = true, HelpText = "The name of the profile setting.")]
            public string Name { get; set; }


            [Option('v', "value", Required = true, HelpText = "The value to set.")]
            public string Value { get; set; }

            [Option('c', "client-setting", Required = false, HelpText = "If the setting is part of the client settings.")]
            public bool ClientSetting { get; set; }

            [Option('p', "profile-setting", Required = false, HelpText = "If the setting is part of the current profile.")]
            public bool ProfileSetting { get; set; }

        }

    }
}