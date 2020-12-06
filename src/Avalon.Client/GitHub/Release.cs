using Newtonsoft.Json;
using System.Collections.Generic;

namespace Avalon.GitHub
{
    public class Release
    {

        [JsonProperty("tag_name")]
        public string TagName { get; set; } = "";

        public List<Asset> Assets { get; set; } = new List<Asset>();

    }
}
