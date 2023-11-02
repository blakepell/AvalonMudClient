/*
 * Lua Automation IDE
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2022 All rights reserved.
 * @license           : Closed Source
 */

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Application = System.Windows.Application;
using Mouse = Argus.Windows.Mouse;

namespace MoonSharp.Interpreter.Wpf.Modules
{
    [MoonSharpModule(Namespace = "mouse")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class MouseScriptCommands
    {
        /// <summary>
        /// Sets the x and y position of the mouse pointer.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [MoonSharpModuleMethod(Description = "Sets the x and y position of the mouse pointer.",
                               ParameterCount = 2)]
        public void SetPosition(int x, int y)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => this.SetPosition(x, y)));
                return;
            }

            Mouse.SetMousePos(x, y);
        }

        [MoonSharpModuleMethod(Description = "Left click the mouse button.")]
        public void LeftClick()
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                _ = Application.Current.Dispatcher.BeginInvoke(new Action(this.LeftClick));
                return;
            }

            Mouse.LeftClick();
        }

        [MoonSharpModuleMethod(Description = "Double click the left mouse button.")]
        public void LeftDoubleClick()
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                _ = Application.Current.Dispatcher.BeginInvoke(new Action(this.LeftDoubleClick));
                return;
            }

            Mouse.LeftClick();
            Thread.Sleep(20);
            Mouse.LeftClick();
        }

        [MoonSharpModuleMethod(Description = "Press the left mouse button down.")]
        public void LeftDown()
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                _ = Application.Current.Dispatcher.BeginInvoke(new Action(this.LeftDown));
                return;
            }

            Mouse.LeftDown();
        }

        [MoonSharpModuleMethod(Description = "Release the left mouse button.")]
        public void LeftUp()
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                _ = Application.Current.Dispatcher.BeginInvoke(new Action(this.LeftUp));
                return;
            }

            Mouse.LeftUp();
        }

        [MoonSharpModuleMethod(Description = "Right click the mouse button.")]
        public void RightClick()
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                _ = Application.Current.Dispatcher.BeginInvoke(new Action(this.RightClick));
                return;
            }

            Mouse.RightClick();
        }

        [MoonSharpModuleMethod(Description = "Press the right mouse button down.")]
        public void RightDown()
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                _ = Application.Current.Dispatcher.BeginInvoke(new Action(this.RightDown));
                return;
            }

            Mouse.RightDown();
        }

        [MoonSharpModuleMethod(Description = "Release the right mouse button.")]
        public void RightUp()
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                _ = Application.Current.Dispatcher.BeginInvoke(new Action(this.RightUp));
                return;
            }

            Mouse.RightUp();
        }

        [MoonSharpModuleMethod(Description = "Middle click the mouse button.")]
        public void MiddleClick()
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                _ = Application.Current.Dispatcher.BeginInvoke(new Action(this.MiddleClick));
                return;
            }

            Mouse.MiddleClick();
        }

        [MoonSharpModuleMethod(Description = "Press the middle mouse button down.")]
        public void MiddleDown()
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                _ = Application.Current.Dispatcher.BeginInvoke(new Action(this.MiddleDown));
                return;
            }

            Mouse.MiddleDown();
        }

        [MoonSharpModuleMethod(Description = "Release the middle mouse button.")]
        public void MiddleUp()
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                _ = Application.Current.Dispatcher.BeginInvoke(new Action(this.MiddleUp));
                return;
            }
            Mouse.MiddleUp();
        }

        [Description("The X coordinate of the mouse's current position.")]
        public int X
        {
            get => Mouse.X();
            set
            {
                int y = Mouse.Y();
                Mouse.SetMousePos(value, y);
            }
        }

        [Description("The X coordinate of the mouse's current position.")]
        public int Y
        {
            get => Mouse.Y();
            set
            {
                int x = Mouse.X();
                Mouse.SetMousePos(x, value);
            }
        }

        [MoonSharpModuleMethod(Description = "Scrolls up the specified number of clicks.",
                               ParameterCount = 1)]
        public void ScrollUp(int clicks)
        {
            Mouse.ScrollUp(clicks);
        }

        [MoonSharpModuleMethod(Description = "Scrolls down the specified number of clicks.",
                               ParameterCount = 1)]
        public void ScrollDown(int clicks)
        {
            Mouse.ScrollDown(clicks);
        }
    }
}
