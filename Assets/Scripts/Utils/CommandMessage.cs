using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class CommandMessage
{
    [JsonProperty("command")]
    public string Command { get; set; }

    [JsonProperty("parameter")]
    public Dictionary<string, JToken> Parameter { get; set; }
}
