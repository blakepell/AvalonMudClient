/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Newtonsoft.Json;

namespace Avalon.GitHub
{
    public class Asset
    {
        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = "";
    }
}
