/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.GitHub
{
    public class Release
    {

        [JsonProperty("tag_name")]
        public string TagName { get; set; } = "";

        public List<Asset> Assets { get; set; } = new();

    }
}
