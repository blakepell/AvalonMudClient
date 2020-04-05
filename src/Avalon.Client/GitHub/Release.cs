using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalon.GitHub
{
    public class Release
    {

        [JsonProperty("tag_name")]
        public string TagName { get; set; } = "";

        public List<Asset> Assets { get; set; } = new List<Asset>();

    }
}
