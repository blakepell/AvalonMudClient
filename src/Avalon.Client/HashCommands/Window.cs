/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Argus.Extensions;
using Avalon.Common.Interfaces;
using CommandLine;
using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Avalon.Common;

namespace Avalon.HashCommands
{

    /// <summary>
    /// Options to change attributes about the current window.
    /// </summary>
    public class Window : HashCommand
    {
        public Window(IInterpreter interp) : base(interp)
        {
        }

        public override string Name { get; } = "#window";

        public override string Description { get; } = "Options to change attributes about the current window";

        public override void Execute()
        {
            // If no parameters echo the help.
            if (string.IsNullOrWhiteSpace(this.Parameters))
            {
                this.Interpreter.Send("#window --help");
                return;
            }

            // Parse the arguments and append to the file.
            var result = Parser.Default.ParseArguments<Arguments>(CreateArgs(this.Parameters))
                .WithParsed(o =>
                {
                    // Close all windows
                    if (o.CloseAll)
                    {
                        int count = this.Interpreter.Conveyor.WindowList.Count;

                        // Step backwards through the list removing all the items.
                        for (int i = this.Interpreter.Conveyor.WindowList.Count - 1; i >= 0; i--)
                        {
                            this.Interpreter.Conveyor.WindowList[i].Close();
                        }

                        this.Interpreter.Conveyor.EchoLog($"{count.ToString()} {"window".IfCountPluralize(count, "windows")} {"was".IfCountPluralize(count, "were")} closed", Common.Models.LogType.Information);

                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(o.Name))
                    {
                        // This case is if they specified a window that might exist, we'll find it, edit that.
                        var win = this.Interpreter.Conveyor.WindowList.FirstOrDefault(x => x.Name.Equals(o.Name, StringComparison.Ordinal));
                        
                        if (win == null && o.Close)
                        {
                            // Window wasn't found, but close was specified, just exit.
                            return;
                        }
                        else if (win == null)
                        {
                            // Window wasn't found, create it.
                            win = new TerminalWindow {Name = o.Name};

                            // Add the terminal window to our list.
                            this.Interpreter.Conveyor.WindowList.Add(win);
                        }
                        else
                        {
                            // It existed at this point, but let's see if it was supposed to be closed.  Closing
                            // will trigger the Closed event on the Window which will remove the Window from the
                            // shared list of ITerminalWindows on the Conveyor.
                            if (o.Close)
                            {
                                win.Close();
                                return;
                            }
                        }

                        SetWindowProperties(win, o);

                        win.Show();

                        return;
                    }

                    // Shows a known system window.
                    if (!string.IsNullOrWhiteSpace(o.Show))
                    {
                        Utilities.WindowManager.ShellWindowAsync(o.Show);
                        return;
                    }

                    if (o.List)
                    {
                        // List info about all of the active windows.
                        if (this.Interpreter.Conveyor.WindowList.Count == 0)
                        {
                            this.Interpreter.Conveyor.EchoLog("No user created terminal windows currently exist.", Common.Models.LogType.Information);
                            return;
                        }

                        var tb = new TableBuilder(new[] {"Window Name", "Type", "Title", "Status Text"});
                        
                        foreach (var win in this.Interpreter.Conveyor.WindowList)
                        {
                            tb.AddRow(win.Name, win.WindowType.ToString(), win.Title, win.StatusText);
                        }

                        this.Interpreter.Conveyor.EchoText(tb.ToString());

                        return;
                    }

                    // Main window handling
                    if (o.Info)
                    {
                        this.Interpreter.Conveyor.EchoText($"Width = {{y{App.MainWindow.Width.ToString()}{{x,  Height = {{y{App.MainWindow.Height.ToString()}{{x\r\n");
                        this.Interpreter.Conveyor.EchoText($"Left = {{y{App.MainWindow.Left.ToString()}{{x,  Height = {{y{App.MainWindow.Top.ToString()}{{x\r\n");
                    }

                    if (o.Height > 0)
                    {
                        App.MainWindow.Height = o.Height;
                    }

                    if (o.Width > 0)
                    {
                        App.MainWindow.Width = o.Width;
                    }

                    if (o.Top >= 0)
                    {
                        App.MainWindow.Top = o.Top;
                    }

                    if (o.Left >= 0)
                    {
                        App.MainWindow.Left = o.Left;
                    }

                    if (o.Center)
                    {
                        double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                        double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                        double windowWidth = App.MainWindow.Width;
                        double windowHeight = App.MainWindow.Height;
                        App.MainWindow.Left = (screenWidth / 2) - (windowWidth / 2);
                        App.MainWindow.Top = (screenHeight / 2) - (windowHeight / 2);
                    }

                    if (!string.IsNullOrWhiteSpace(o.Title))
                    {
                        App.Settings.ProfileSettings.WindowTitle = o.Title;
                    }
                });

            // Display the help or error output from the parameter parsing.
            this.DisplayParserOutput(result);
        }

        /// <summary>
        /// Shared method to set the properties of an ITerminalWindow from a set of arguments.  Note,
        /// this will set everything but the Name.
        /// </summary>
        /// <param name="win"></param>
        /// <param name="args"></param>
        private void SetWindowProperties(IWindow win, Arguments args)
        {
            if (args.Left >= 0)
            {
                win.Left = args.Left;
            }

            if (args.Top >= 0)
            {
                win.Top = args.Top;
            }

            if (args.Height > 0)
            {
                win.Height = args.Height;
            }

            if (args.Width > 0)
            {
                win.Width = args.Width;
            }

            if (!string.IsNullOrWhiteSpace(args.Title))
            {
                win.Title = args.Title;
            }

            if (!string.IsNullOrWhiteSpace(args.Status))
            {
                win.StatusText = args.Status;
            }
        }

        /// <summary>
        /// The supported command line arguments for this hash command.
        /// </summary>
        public class Arguments
        {

            [Option('c', "center", Required = false, HelpText = "Centers the window in the middle of the current screen.")]
            public bool Center { get; set; }

            [Option("close", Required = false, HelpText = "Closes the window if found of the window specified with the --name switch.")]
            public bool Close { get; set; } = false;


            [Option("closeall", Required = false, HelpText = "Closes all open windows that are tracked excluding the main game window.  This ignores all other switches when used.")]
            public bool CloseAll { get; set; } = false;

            [Option('h', "height", Required = false, HelpText = "The height of the window.")]
            public int Height { get; set; } = 0;

            [Option('w', "width", Required = false, HelpText = "The width of the window")]
            public int Width { get; set; } = 0;

            [Option('l', "left", Required = false, HelpText = "The left pixel location of the window.")]
            public int Left { get; set; } = -1;

            [Option('t', "top", Required = false, HelpText = "The top pixel location of the window.")]
            public int Top { get; set; } = -1;

            [Option('i', "info", Required = false, HelpText = "Echos information about the properties of the window.")]
            public bool Info { get; set; }

            [Option("title", Required = false, HelpText = "The title of the window.")]
            public string Title { get; set; }

            [Option("list", Required = false, HelpText = "Will list all of the active user created windows and their names.")]
            public bool List { get; set; } = false;

            [Option("status", Required = false, HelpText = "Sets the text on the status bar of the window.")]
            public string Status { get; set; }

            [Option('n', "name", Required = false, HelpText = "The name of the of the new terminal window that is required to echo to it.")]
            public string Name { get; set; } = "";

            [Option('s', "show", Required = false, HelpText = "Shows a known system window.")]
            public string Show { get; set; }

        }

    }
}
