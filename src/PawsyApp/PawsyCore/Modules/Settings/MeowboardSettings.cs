using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace PawsyApp.PawsyCore.Modules.Settings;

internal class MeowBoardSettings() : ISettings
{
    [JsonInclude]
    internal ConcurrentDictionary<ulong, int> Records { get; set; } = [];

    [JsonInclude]
    internal int MeowBoardDisplayLimit = 5;
}
