using System.Text.Json.Serialization;

namespace PawsyApp.PawsyCore.Modules.Settings;

internal class MeowBank
{
    [JsonInclude]
    public ulong MeowMoney = 0;

}
