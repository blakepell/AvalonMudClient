/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

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
