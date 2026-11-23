using System.IO;
using System.Windows.Media.Imaging;
using Argus.Memory;
using Avalon.Models;

namespace Avalon.Common;

/// <summary>
/// Loads and imports game artwork without serializing WPF image objects.
/// </summary>
internal static class GameArtwork
{
    private static string Root => AppServices.GetRequiredService<AppSettings>().ApplicationDataFolder
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Apps", "AvalonMudClient");

    /// <summary>
    /// Loads a detached thumbnail, returning null for missing or unsupported artwork.
    /// </summary>
    internal static BitmapSource? Load(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        try
        {
            using var stream = File.OpenRead(Path.IsPathRooted(path) ? path : Path.Combine(Root, path));
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.DecodePixelWidth = 720;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException or FormatException)
        {
            return null;
        }
    }

    /// <summary>
    /// Copies selected artwork into managed storage and returns its relative path.
    /// </summary>
    internal static string Import(string source, Guid id)
    {
        if (Load(source) is null)
        {
            throw new IOException("Choose a readable PNG, JPEG, BMP, or GIF image.");
        }

        var relativePath = Path.Combine("GameImages", $"{id:N}{Path.GetExtension(source).ToLowerInvariant()}");
        var destination = Path.Combine(Root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(source, destination, overwrite: false);
        return relativePath;
    }
}
