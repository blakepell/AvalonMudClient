using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalon.GitHub
{
    public class Asset
    {
        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = "";
    }
}
