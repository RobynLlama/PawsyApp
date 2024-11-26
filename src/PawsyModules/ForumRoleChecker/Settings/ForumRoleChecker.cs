using System.Text.Json.Serialization;
using PawsyApp.PawsyCore.Modules;

namespace ForumRoleChecker.Settings;

[method: JsonConstructor]
public class ForumRoleCheckerSettings() : ISettings
{
    [JsonInclude]
    public ulong ModdingChannel { get; set; } = 0;
    public ulong AlertChannel { get; set; } = 0;
    public ulong ModderRoleID { get; set; } = 0;
}
