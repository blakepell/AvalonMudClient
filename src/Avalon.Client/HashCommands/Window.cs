﻿using Avalon.Common.Interfaces;
using CommandLine;

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
                    // First, new window handling.
                    if (o.NewWindow)
                    {
                        if (string.IsNullOrWhiteSpace(o.Name))
                        {
                            this.Interpreter.Conveyor.EchoLog("You must specify the 'name' switch with -x or --name when creating a new terminal window.", Common.Models.LogType.Error);
                            return;
                        }

                        var win = new TerminalWindow();
                        win.Name = o.Name;

                        if (o.Left >= 0)
                        {
                            win.Left = o.Left;
                        }

                        if (o.Top >= 0)
                        {
                            win.Top = o.Top;
                        }

                        if (o.Height > 0)
                        {
                            win.Height = 0;
                        }

                        if (o.Width > 0)
                        {
                            win.Width = 0;
                        }

                        if (!string.IsNullOrWhiteSpace(o.Title))
                        {
                            win.Title = o.Title;
                        }

                        // Add the terminal window to our list.
                        this.Interpreter.Conveyor.TerminalWindowList.Add(win);

                        win.Show();

                    }

                    if (o.Info)
                    {
                        this.Interpreter.Conveyor.EchoText($"Width = {App.MainWindow.Width},  Height = {App.MainWindow.Height}");
                        this.Interpreter.Conveyor.EchoText($"Left = {App.MainWindow.Left},  Height = {App.MainWindow.Top}");

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
                        App.MainWindow.Title = o.Title;
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

            [Option('c', "center", Required = false, HelpText = "Centers the window in the middle of the current screen.")]
            public bool Center { get; set; }

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

            [Option('n', "new", Required = false, HelpText = "Spawns a new terminal window and applies the additional settings to that window.")]
            public bool NewWindow { get; set; } = false;

            [Option('x', "name", Required = false, HelpText = "The name of the of the new terminal window that is required to echo to it.")]
            public string Name { get; set; }

        }

    }
}
