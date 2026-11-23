using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Argus.Memory;
using Avalon.Controls;
using Avalon.Models;
using Mosaic.UI.Wpf;
using Mosaic.UI.Wpf.Controls;
using Mosaic.UI.Wpf.Controls.Scripting;
using Mosaic.UI.Wpf.Themes;

namespace Avalon.Scripting;

/// <summary>
/// The <c>ui</c> object: Mosaic's dialogs and toasts plus the window helpers Continuum scripts use.
/// </summary>
[ScriptModule(Name = "ui", Description = "Dialogs, toasts and window helpers.")]
public sealed class UiScriptCommands : Mosaic.UI.Wpf.Controls.Scripting.ScriptCommands.UiScriptCommands
{
    /// <summary>
    /// Shows an information toast.
    /// </summary>
    public void Toast(string title, string msg) => ShowToast(title, msg, "Info");

    /// <summary>
    /// Shows an information toast.
    /// </summary>
    public void ToastInfo(string title, string msg) => ShowToast(title, msg, "Info");

    /// <summary>
    /// Shows a warning toast.
    /// </summary>
    public void ToastWarning(string title, string msg) => ShowToast(title, msg, "Warning");

    /// <summary>
    /// Shows an error toast.
    /// </summary>
    public void ToastError(string title, string msg) => ShowToast(title, msg, "Error");

    /// <summary>
    /// Shows a success toast.
    /// </summary>
    public void ToastSuccess(string title, string msg) => ShowToast(title, msg, "Success");

    /// <summary>
    /// Flashes the main window on the task bar until it is activated.
    /// </summary>
    public void FlashWindow() => WindowFlasher.Flash(Application.Current.MainWindow);

    /// <summary>
    /// Sets the size of the main window.
    /// </summary>
    public void SetWindowDimensions(int height, int width)
    {
        if (Application.Current.MainWindow is { } window)
        {
            window.Height = height;
            window.Width = width;
        }
    }

    /// <summary>
    /// Moves the main window.
    /// </summary>
    public void SetWindowPosition(int top, int left)
    {
        if (Application.Current.MainWindow is { } window)
        {
            window.Top = top;
            window.Left = left;
        }
    }

    /// <summary>
    /// Gets or sets the main window's status bar text.
    /// </summary>
    public string StatusText
    {
        get => AppServices.GetRequiredService<AppViewModel>().StatusText;
        set => AppServices.GetRequiredService<AppViewModel>().StatusText = value;
    }

    /// <summary>
    /// Updates a resource in the named vitals panel, adding the row if needed.
    /// </summary>
    /// <param name="panelName">The name given to the dock panel.</param>
    /// <param name="label">The resource label.</param>
    /// <param name="current">The current resource value.</param>
    /// <param name="maximum">The positive maximum resource value.</param>
    /// <returns><see langword="true" /> if the reading was accepted; otherwise, <see langword="false" />.</returns>
    public bool SetVital(string panelName, string label, double current, double maximum) =>
        WithVitalsPanel(panelName, panel => panel.SetVital(label, current, maximum));

    /// <summary>
    /// Removes a resource row from the named vitals panel for this session.
    /// </summary>
    /// <param name="panelName">The name given to the dock panel.</param>
    /// <param name="label">The resource label.</param>
    /// <returns><see langword="true" /> if a row was removed; otherwise, <see langword="false" />.</returns>
    public bool RemoveVital(string panelName, string label) =>
        WithVitalsPanel(panelName, panel => panel.RemoveVital(label));

    private static bool WithVitalsPanel(string panelName, Func<VitalsPanel, bool> action)
    {
        if (Application.Current is not { } app)
        {
            return false;
        }

        return app.Dispatcher.Invoke(() => app.MainWindow is MainWindow main && main.FindVitalsPanel(panelName) is { } panel && action(panel));
    }

    /// <summary>
    /// Sets the application theme: Light, Dark, Black, Blue or HighContrast.
    /// </summary>
    public void SetTheme(string themeName)
    {
        if (Enum.TryParse<MosaicThemeMode>(themeName, true, out var mode))
        {
            AppServices.GetRequiredService<ThemeManager>().Theme = mode;
            AppServices.GetRequiredService<AppSettings>().Theme = mode;
        }
    }
}

/// <summary>
/// Flashes a window on the task bar, used by ui.FlashWindow and #flash.
/// </summary>
public static class WindowFlasher
{
    [StructLayout(LayoutKind.Sequential)]
    private struct FlashInfo
    {
        public uint Size;
        public IntPtr Handle;
        public uint Flags;
        public uint Count;
        public uint Timeout;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FlashWindowEx(ref FlashInfo info);

    /// <summary>
    /// Flashes <paramref name="window"/> until it comes to the foreground.
    /// </summary>
    public static void Flash(Window? window)
    {
        if (window is null || window.IsActive)
        {
            return;
        }

        var info = new FlashInfo
        {
            Handle = new WindowInteropHelper(window).Handle,
            Flags = 3 | 12, // FLASHW_ALL | FLASHW_TIMERNOFG
            Count = uint.MaxValue,
            Timeout = 0
        };
        info.Size = (uint)Marshal.SizeOf(info);
        FlashWindowEx(ref info);
    }
}
