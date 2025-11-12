using System.Text.Json.Serialization;

namespace MeowBoard.Settings;

public class MeowBank
{
  [JsonInclude]
  public ulong MeowMoney = 0;

}
