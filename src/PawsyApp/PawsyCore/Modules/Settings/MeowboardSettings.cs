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
    internal ConcurrentDictionary<ulong, MeowBank> Records { get; set; } = [];

    [JsonInclude]
    internal int MeowBoardDisplayLimit = 5;

    [JsonInclude]
    internal ulong GameChannelID = 0;
}
