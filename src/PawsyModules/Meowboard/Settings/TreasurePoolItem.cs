using System.Text.Json.Serialization;

namespace MeowBoard.Settings;

public class TreasurePoolItem
{
  [JsonInclude]
  public string BoxName { get; set; } = "Loose Change";

  [JsonInclude]
  public ulong Payout { get; set; } = 50;
}
