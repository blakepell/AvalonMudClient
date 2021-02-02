using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Avalon.Utilities
{
    /// <summary>
    /// The ability to send toast notifications through the NotifyIcon Windows Forms class.
    /// </summary>
    public class Toast : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;

        public Toast()
        {
            _notifyIcon = new NotifyIcon {Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location)};
            _notifyIcon.BalloonTipClosed += (s, e) => _notifyIcon.Visible = false;
        }

        public void Dispose()
        {
            _notifyIcon?.Icon?.Dispose();
            _notifyIcon?.Dispose();
        }

        /// <summary>
        /// Shows a notification message with a title that lasts for 3 seconds.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        public void ShowNotification(string title, string msg)
        {
            this.ShowNotification(title, msg, ToolTipIcon.Info);
        }

        /// <summary>
        /// Shows a notification message with a title and a type icon that lasts for 3 seconds.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        /// <param name="iconType"></param>
        public void ShowNotification(string title, string msg, ToolTipIcon iconType)
        {
            // Apparently the timeout is deprecated as of Vista and no longer works, it defaults to a system
            // setting in the accessibility section.
            _notifyIcon.Visible = true;
            _notifyIcon.ShowBalloonTip(5000, title, msg, iconType);
        }

    }
}