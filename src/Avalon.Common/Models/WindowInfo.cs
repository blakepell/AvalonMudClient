/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;

namespace Avalon.Common.Models
{
    /// <summary>
    /// A model that represents the metadata in an <see cref="IWindow"/>.
    /// </summary>
    public class WindowInfo : IWindow
    {
        public void Show()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Activate()
        {
            throw new NotImplementedException();
        }

        public string Name { get; set; }

        public string Title { get; set; }

        public double Top { get; set; }

        public double Left { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public double Opacity { get; set; }

        public string StatusText { get; set; }

        public WindowType WindowType { get; set; }
    }
}
