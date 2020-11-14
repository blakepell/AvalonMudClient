namespace Avalon.Common.Models
{
    /// <summary>
    /// Represents a small subset of metadata about installed packages.
    /// </summary>
    public class InstalledPackage
    {
        public string PackageId { get; set; }

        public int Version { get; set; }
    }
}
