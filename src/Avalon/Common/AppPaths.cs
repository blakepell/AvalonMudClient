using System.IO;
using Argus.Memory;
using Avalon.Models;

namespace Avalon.Common;

/// <summary>
/// The folders Avalon reads and writes.
/// </summary>
public static class AppPaths
{
    /// <summary>
    /// Gets the application data folder (settings, artwork, the script database).
    /// </summary>
    public static string AppDataFolder
    {
        get
        {
            var folder = AppServices.IsRegistered<AppSettings>() ? AppServices.GetRequiredService<AppSettings>().ApplicationDataFolder : null;
            return string.IsNullOrWhiteSpace(folder)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Apps", "AvalonMudClient")
                : folder;
        }
    }

    /// <summary>
    /// Gets the documents folder for files the user works with (packages, saved scripts and logs).
    /// </summary>
    public static string DocumentsFolder
    {
        get
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Avalon");
            Directory.CreateDirectory(folder);
            return folder;
        }
    }

    /// <summary>
    /// Gets the folder compiled packages are written to.
    /// </summary>
    public static string PackagesFolder
    {
        get
        {
            var folder = Path.Combine(DocumentsFolder, "Packages");
            Directory.CreateDirectory(folder);
            return folder;
        }
    }
}
