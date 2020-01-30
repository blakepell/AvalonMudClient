using Avalon.Common.Colors;
using System;

namespace Avalon.Common.Models
{
    public class EchoEventArgs : EventArgs
    {

        public string Text { get; set; } = "";

        public AnsiColor ForegroundColor { get; set; }

        public bool ReverseColors { get; set; }

        public bool UseDefaultColors { get; set; } = true;

    }
}
