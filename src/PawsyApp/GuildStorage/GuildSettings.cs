using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using PawsyApp.Settings;
using PawsyApp.Utils;
using System.Collections.Generic;

namespace PawsyApp.GuildStorage;

public class GuildSettings
{
    public ulong LoggingChannelID { get; set; } = 0;
    public ulong ID { get; set; }
    public string ServerName { get; set; }
    [JsonInclude]
    internal MeowBoard MeowBoard { get; set; } = new();
    internal readonly object AccessLock = new();

    [JsonInclude]
    internal List<RuleBundle> RuleList { get; private set; } = [];

    public ConcurrentDictionary<string, bool> EnabledModules { get; set; } = [];

    [JsonConstructor]
    public GuildSettings(ulong ID, string ServerName)
    {
        this.ID = ID;
        this.ServerName = ServerName;
    }

    internal void Save()
    {
        using StreamWriter writer = new(GuildFile.Get(ID));
        writer.Write(JsonSerializer.Serialize(this, AllSettings.options));
        writer.Flush();
    }
}
