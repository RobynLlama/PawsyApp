using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using PawsyApp.Settings;
using PawsyApp.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PawsyApp.GuildStorage;

public class GuildSettings
{
    [JsonInclude]
    private ulong NextCreatedID = 0;
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

    internal ulong GetNextID()
    {
        return NextCreatedID++;
    }

    internal async Task Save()
    {
        List<Task> tasks = [];

        using StreamWriter writer = new(GuildFile.Get(ID));
        tasks.Add(writer.WriteAsync(JsonSerializer.Serialize(this, AllSettings.options)));
        tasks.Add(writer.FlushAsync());

        await Task.WhenAll(tasks);
    }
}
