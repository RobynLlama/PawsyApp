using System.Text.Json.Serialization;
using PawsyApp.PawsyCore.Modules.Settings;

namespace ModderRoleChecker.Settings;

[method: JsonConstructor]
internal class ModderRoleCheckerSettings() : ISettings
{
    [JsonInclude]
    public ulong ModdingChannel { get; set; } = 0;
    public ulong AlertChannel { get; set; } = 0;
    public ulong ModderRoleID { get; set; } = 0;
}
