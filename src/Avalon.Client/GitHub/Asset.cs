using Newtonsoft.Json;

namespace Avalon.GitHub
{
    public class Asset
    {
        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = "";
    }
}
