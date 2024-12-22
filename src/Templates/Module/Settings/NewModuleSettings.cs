using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using PawsyApp.PawsyCore.Modules;

namespace NewModule.Settings;

public class NewModuleSettings() : ISettings
{
    [JsonInclude]
    internal int MeowLimit = 0;
}
