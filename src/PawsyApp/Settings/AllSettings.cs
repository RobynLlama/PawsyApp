using PawsyApp.GuildStorage;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace PawsyApp.Settings;

public partial class AllSettings
{
    internal static ConcurrentDictionary<ulong, GuildSettings> GuildSettingsStorage = [];
    internal static readonly JsonSerializerOptions options = new() { WriteIndented = true };
    internal static async void SaveAll()
    {
        List<Task> tasks = [];

        foreach (var item in GuildSettingsStorage.Keys)
        {
            tasks.Add(GuildSettingsStorage[item].Save());
        }

        await Task.WhenAll(tasks);
    }
}
