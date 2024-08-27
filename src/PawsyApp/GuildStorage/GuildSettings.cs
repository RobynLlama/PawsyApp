using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using PawsyApp.Settings;
using PawsyApp.Utils;

namespace PawsyApp.GuildStorage;

public class GuildSettings
{
    public ulong LoggingChannelID { get; set; } = 0;
    public ulong ID { get; set; }
    public List<RuleBundle> rules { get; set; } = [];

    [JsonConstructor]
    public GuildSettings(ulong LoggingChannelID, ulong ID, List<RuleBundle> rules)
    {
        this.LoggingChannelID = LoggingChannelID;
        this.rules = rules;
        this.ID = ID;
    }

    public GuildSettings(ulong ID)
    {
        this.ID = ID;
    }

    internal void Save()
    {
        using StreamWriter writer = new(GuildFile.Get(ID));
        writer.Write(JsonSerializer.Serialize(this, AllSettings.options));
        writer.Flush();
    }
}
