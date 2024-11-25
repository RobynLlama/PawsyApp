using System.Text.Json.Serialization;

namespace MeowBoard.Settings;

internal class MeowBank
{
    [JsonInclude]
    public ulong MeowMoney = 0;

}
