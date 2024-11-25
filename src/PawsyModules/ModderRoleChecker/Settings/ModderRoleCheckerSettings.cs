using System.Text.Json.Serialization;
using PawsyApp.PawsyCore.Modules;

namespace ModderRoleChecker.Settings;

[method: JsonConstructor]
public class ModderRoleCheckerSettings() : ISettings
{
    [JsonInclude]
    public ulong ModdingChannel { get; set; } = 0;
    public ulong AlertChannel { get; set; } = 0;
    public ulong ModderRoleID { get; set; } = 0;
}
