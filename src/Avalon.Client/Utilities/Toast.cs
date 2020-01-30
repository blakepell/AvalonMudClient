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
            _notifyIcon = new NotifyIcon();
            // Extracts your app's icon and uses it as notify icon
            _notifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            // Hides the icon when the notification is closed
            _notifyIcon.BalloonTipClosed += (s, e) => _notifyIcon.Visible = false;
        }

        public void Dispose()
        {
            _notifyIcon?.Icon?.Dispose();
            _notifyIcon?.Dispose();
        }

        public void ShowNotification(string msg)
        {
            this.ShowNotification(Assembly.GetExecutingAssembly().GetName().Name, msg, ToolTipIcon.Info, 3000);
        }

        public void ShowNotification(string title, string msg)
        {
            this.ShowNotification(title, msg, ToolTipIcon.Info);
        }

        public void ShowNotification(string title, string msg, ToolTipIcon iconType)
        {
            this.ShowNotification(title, msg, iconType, 3000);
        }

        public void ShowNotification(string title, string msg, ToolTipIcon iconType, int millisecondsTimeout)
        {
            _notifyIcon.Visible = true;
            _notifyIcon.ShowBalloonTip(millisecondsTimeout, title, msg, iconType);
        }
    }
}