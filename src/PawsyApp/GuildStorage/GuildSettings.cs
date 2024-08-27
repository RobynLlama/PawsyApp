using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PawsyApp.Settings;
using PawsyApp.Utils;

namespace PawsyApp.GuildStorage;

public class GuildSettings
{
    public ulong LoggingChannelID { get; set; } = 0;
    public List<RuleBundle> rules { get; set; } = [];
    public GuildSettings(ulong LoggingChannelID, List<RuleBundle> rules)
    {
        this.LoggingChannelID = LoggingChannelID;
        this.rules = rules;
    }

    public GuildSettings()
    {

    }

    internal void Save(ulong MyID)
    {
        using StreamWriter writer = new(GuildFile.Get(MyID));
        writer.Write(JsonSerializer.Serialize(this, AllSettings.options));
        writer.Flush();
    }
}
