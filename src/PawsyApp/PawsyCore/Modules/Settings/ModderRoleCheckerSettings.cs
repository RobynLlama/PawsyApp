using System.Text.Json.Serialization;

namespace PawsyApp.PawsyCore.Modules.Settings;

[method: JsonConstructor]
internal class ModderRoleCheckerSettings() : ISettings
{
    [JsonInclude]
    public ulong ModdingChannel { get; set; } = 0;
    public ulong AlertChannel { get; set; } = 0;
    public ulong ModderRoleID { get; set; } = 0;
}
