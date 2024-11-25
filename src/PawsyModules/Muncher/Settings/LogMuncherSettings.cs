using System.Text.Json.Serialization;
using PawsyApp.PawsyCore.Modules.Settings;

namespace MuncherModule.Settings;

[method: JsonConstructor]
public class LogMuncherSettings() : ISettings
{
    [JsonInclude]
    public ulong MunchingChannel { get; set; } = 0;
}
