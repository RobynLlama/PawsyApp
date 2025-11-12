using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using PawsyApp.PawsyCore.Modules;

namespace MeowBoard.Settings;

public class MeowBoardSettings() : ISettings
{
  [JsonInclude]
  internal ConcurrentDictionary<ulong, MeowBank> Records { get; set; } = [];

  [JsonInclude]
  internal int MeowBoardDisplayLimit = 5;

  [JsonInclude]
  internal ulong GameChannelID = 0;
}
